using Home.Common;
using Home.Common.Model;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Home.Graph.Common;
using Newtonsoft.Json;
using static Home.Agents.Clara.NewsItems.YoutubeHelper;

namespace Home.Agents.Clara
{
    public static class PimSchedulerThread
    {
        public static bool _stop = false;
        public static void Start()
        {
            var t = new Thread(() => PimSchedulerThread.Run());
            t.Name = "PIM Thread";
            t.Start();
        }

        public static void Stop()
        {
            _stop = true;
        }

        private static DateTimeOffset _lastTodoRefresh = DateTimeOffset.Now;
        private static DateTimeOffset _lastInProgressRefresh = DateTimeOffset.MinValue;
        private static DateTimeOffset _lastHomeServicesRefresh = DateTimeOffset.MinValue;
        private static DateTimeOffset _lastGlobalCalendarRefresh = DateTimeOffset.MinValue;

        public static void Run()
        {
            while (!_stop)
            {
                DoRoutines();
                DoInProgessEvents();
                DoScheduledEventsMaintenace();
                DoTodoSync();
                DoHomeServicesEvents();
                DoCalendarEvents();
                Thread.Sleep(5000);
            }
        }

        private static List<TodoItem> _eventsInNextDay = new List<TodoItem>();

        private static void DoInProgessEvents()
        {
            RefreshForInProgress();
            bool hasDoneSomething = false;
            foreach (var t in _eventsInNextDay)
            {
                if (!t.AutoActivate)
                    continue;
                if (!t.DueDate.HasValue)
                    continue;

                if (t.DueDate < DateTimeOffset.Now && (
                    t.Status == TodoItemStatus.Todo
                    || t.Status == TodoItemStatus.Creating))
                {
                    Console.WriteLine($"Scheduled at : {t.DueDate} for {t.Duration} => should start");

                    // a démarrrer
                    for (int i = 0; i < 3; i++)
                    {
                        try
                        {
                            using (var cli = new MainApiAgentWebClient("clara"))
                            {
                                cli.DownloadData<bool>($"v1.0/todos/events/{t.Id}/start");
                                break;
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Err start event : " + e);
                        }
                    }
                    hasDoneSomething = true;
                }

                if (t.Duration.HasValue
                    && t.DueDate.Value.Add(t.Duration.Value) < DateTimeOffset.Now
                    && (t.Status == TodoItemStatus.Todo || t.Status == TodoItemStatus.InProgress || t.Status == TodoItemStatus.Creating))
                {
                    Console.WriteLine($"Scheduled at : {t.DueDate} for {t.Duration} => should end");

                    // a terminer
                    for (int i = 0; i < 3; i++)
                    {
                        try
                        {
                            using (var cli = new MainApiAgentWebClient("clara"))
                            {
                                cli.DownloadData<bool>($"v1.0/todos/events/{t.Id}/end");
                                break;
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Err end event : " + e);
                        }
                    }
                    hasDoneSomething = true;

                }
            }

            if (hasDoneSomething)
                _lastInProgressRefresh = DateTime.MinValue;
        }

        private static void RefreshForInProgress()
        {
            if (Math.Abs((DateTimeOffset.Now - _lastInProgressRefresh).TotalMinutes) < 5)
                return;

            _lastInProgressRefresh = DateTimeOffset.Now;

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("clara"))
                    {
                        _eventsInNextDay = cli.DownloadData<List<TodoItem>>("v1.0/todos/events?nextDays=2");
                        break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Err get events of the day : " + e);
                }
            }
        }

        private static void DoScheduledEventsMaintenace()
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("clara"))
                    {
                        var lst = cli.DownloadData<bool>("v1.0/todos/events/clearInPast");
                        break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Err clear of past events : " + e);
                }
            }

            List<TodoItem> items = null;
            // on obtient la liste des items schedulés
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("clara"))
                    {
                        items = cli.DownloadData<List<TodoItem>>("/v1.0/todos/events?onlyScheduled=true");
                        break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Err gettings scheduled items : " + e);
                }
            }

            if (items != null)
            {
                foreach (TodoItem item in items)
                {
                    HandleSchedule(item);
                }
            }
        }

        internal static void DoCalendarEvents()
        {
            if (Math.Abs((DateTimeOffset.Now - _lastGlobalCalendarRefresh).TotalMinutes) < 5)
                return;

            _lastGlobalCalendarRefresh = DateTimeOffset.Now;

            try
            {
                // puis on récupère les éléments externes
                SyncItems(Calendars.SchoolPlanningCalendar.GetNextScheduledItems(DateTimeOffset.Now.AddMonths(18))
                , "SchoolPlanning_", 600);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in SchoolPlanning scheduler : " + e.ToString());
            }

        }

        private static void SyncItems(List<TodoItem> items, string type, int delai=31)
        {
            items = DedoublonnageParOriginId(items);

            var listIds = (from z in items
                           where z.ListId != null
                           select z.ListId).Distinct().ToArray();
            foreach (var listId in listIds)
            {
                var itemsInList = (from z in items
                                   where z.ListId != null && z.ListId.Equals(listId)
                                   select z).ToList();
                SyncItems(listId, itemsInList, type, delai);
            }
        }

        internal static void DoHomeServicesEvents()
        {
            if (Math.Abs((DateTimeOffset.Now - _lastHomeServicesRefresh).TotalMinutes) < 5)
                return;

            _lastHomeServicesRefresh = DateTimeOffset.Now;

            try
            {
                // puis on récupère les éléments externes
                SyncItems(HomeServices.HomeServicesHelper.GetNextScheduledItems(DateTimeOffset.Now.AddDays(31))
                , "HomeServices_");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in HomeServices scheduler : " + e.ToString());
            }
        }

        private static List<TodoItem> DedoublonnageParOriginId(List<TodoItem> items)
        {
            Dictionary<string, TodoItem> temp = new Dictionary<string, TodoItem>();
            foreach(var item in items)
            {
                if (item.Origin == null || string.IsNullOrEmpty(item.OriginItemData))
                    continue;

                if(!temp.ContainsKey(item.Origin + "/" + item.OriginItemData))
                    temp.Add(item.Origin + "/" + item.OriginItemData, item);
            }

            return temp.Values.ToList();
        }

        private static void SyncItems(string listId, List<TodoItem> newItems, string originPrefix, int delai = 31)
        {
            List<TodoItem> current = null;
            List<TodoItem> toDelete = new List<TodoItem>();
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("clara"))
                    {
                        current = cli.DownloadData<List<TodoItem>>("v1.0/todos/events?includeDone=true&nextDays=" + delai + "&listId=" + listId);
                        break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Err clear of past events : " + e);
                    current = null;
                }
            }
            if (current == null)
                return;

            current = (from z in current
                       where z.Origin != null 
                        && z.Origin.StartsWith(originPrefix, StringComparison.InvariantCultureIgnoreCase)
                       select z).ToList();

            Console.WriteLine($"CalendarSync : Sync in todo {listId}/{originPrefix} : server count = {current.Count}, local count={newItems.Count}");

            // on refresh les items existants
            foreach (var item in current)
            {
                if (item.Origin == null || !(item.Origin.StartsWith(originPrefix, StringComparison.InvariantCultureIgnoreCase)))
                    continue;
                bool found = false;
                for (int i = 0; i < newItems.Count; i++)
                {
                    if (newItems[i].OriginItemData == null)
                        continue;
                    if (newItems[i].OriginItemData.Equals(item.OriginItemData, StringComparison.InvariantCultureIgnoreCase))
                    {
                        item.Label = newItems[i].Label;
                        item.Description = newItems[i].Description;
                        item.DueDate = newItems[i].DueDate;
                        item.Duration = newItems[i].Duration;
                        item.Categories = newItems[i].Categories;
                        item.PrivacyLevel = newItems[i].PrivacyLevel;
                        item.ScenarioOnStart = newItems[i].ScenarioOnStart;
                        item.ScenarioOnEnd = newItems[i].ScenarioOnEnd;
                        item.AutoActivate = newItems[i].AutoActivate;
                        item.AssociatedUsers = newItems[i].AssociatedUsers;

                        newItems[i].Id = item.Id; 
                        // on met à jour les éléments
                        newItems.RemoveAt(i);
                        
                        UpsertItem(item);
                        found = true;
                        break;
                    }
                }
                if(!found)
                    toDelete.Add(item);
            }

            if (newItems.Count > 0)
                Console.WriteLine($"CalendarSync : adding {newItems.Count} local item(s) : " + JsonConvert.SerializeObject(newItems));

           
            foreach (var item in newItems)
                UpsertItem(item);

            if(toDelete!=null && toDelete.Count>0)
                Console.WriteLine($"CalendarSync : deleting {toDelete.Count} item(s) : " + JsonConvert.SerializeObject(toDelete));

            foreach (var item in toDelete)
                DeleteItem(item);
        }

        private static void UpsertItem(TodoItem item)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("sarah"))
                    {
                        var items = cli.UploadData<List<TodoItem>, TodoItem>($"/v1.0/todos/all", "POST", item);
                        break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Err updating scheduled items : " + e);
                }
            }
        }

        private static void DeleteItem(TodoItem item)
        {
            if (string.IsNullOrEmpty(item.Id))
                return;

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("sarah"))
                    {
                        var b = cli.UploadData<bool, TodoItem>((item.Type ==  TodoItemType.TodoItem ? $"/v1.0/todos/todoItems/" : $"/v1.0/todos/events/") + item.Id, "DELETE", item);
                        break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Err updating scheduled items : " + e);
                }
            }
        }

        private static void HandleSchedule(TodoItem item)
        {
            if (!item.DueDate.HasValue)
                return;

            // si on a déjà un évènement dans le futur "suffisant", on ne
            // créé pas de nouvel item
            if (item.DueDate.HasValue && DateTimeOffset.Now.AddDays(30) < item.DueDate.Value)
                return;

            Console.WriteLine("Determining next occurrence for event : " + item.Id + "/" + item.Label);

            List<DateTimeOffset> nexts = new List<DateTimeOffset>();
            foreach (var rule in item.Schedule)
            {
                var dtOffset = ScheduleHelper.GetNextOccurence(rule, item.DueDate.Value);
                if (dtOffset == DateTimeOffset.MinValue)
                    continue;
                if (dtOffset > DateTimeOffset.Now.AddDays(30))
                    continue;

                if (!nexts.Contains(dtOffset))
                    nexts.Add(dtOffset);
            }

            nexts.Sort();

            // si il n'y a rien à plannifier
            if (nexts.Count == 0)
                return;


            // ou si c'est déjà planifié : on ne fait rien
            if (nexts.Count >= 1 && Math.Abs((nexts.First() - item.DueDate.Value).TotalSeconds) < 10)
            {
                Console.WriteLine("Next occurrence for event : " + item.Id + "/" + item.Label + " is on : " + nexts.First() + " => ignoring");
                return;
            }
            else
                Console.WriteLine("Next occurrence for event : " + item.Id + "/" + item.Label + " is on : " + nexts.First() + " => creating event");

            List<TodoItem> scheduledItems = new List<TodoItem>();

            // d'abord on recopie l'item actuel dans une "veille" version
            var oldItem = JsonConvert.DeserializeObject<TodoItem>(JsonConvert.SerializeObject(item));
            oldItem.Id = null;
            oldItem.Schedule = null;
            oldItem.SourceItemId = item.Id;
            scheduledItems.Add(oldItem);

            // puis l'item actuel est défini à la prochaine date possible
            item.DueDate = nexts.First();
            scheduledItems.Add(item);

            // on met à jour les éléments
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("sarah"))
                    {
                        scheduledItems = cli.UploadData<List<TodoItem>, List<TodoItem>>($"/v1.0/todos/all/bulk", "POST", scheduledItems);
                        break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Err updating scheduled items : " + e);
                }
            }
        }

        private static void DoTodoSync()
        {
            if (Math.Abs((DateTimeOffset.Now - _lastTodoRefresh).TotalMinutes) < 2)
                return;

            Todos.MSTodosSync.Sync("mcarbenay");
        }

        private static void DoRoutines()
        {
            try
            {
                var routines = GetRoutines();

                // on met à jour les dates si elles sont dépassées
                foreach (var routine in routines)
                {
                    if (routine.NextWakeUpTime.HasValue
                        && routine.NextWakeUpTime.Value < DateTimeOffset.Now)
                    {
                        UpdateWakeUpTime(routine);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static Location _loc = null;

        internal static Location GetLocation()
        {
            if (_loc == null)
                Reload();
            return _loc;
        }

        public static void Reload()
        {
            using (var cli = new MainApiAgentWebClient("clara"))
            {
                var tmp = cli.DownloadData<Location>("v1.0/system/mesh/local/location");
                if (tmp != null)
                    _loc = tmp;
            }
        }

        private static void UpdateWakeUpTime(RoutineDataWithUser routine)
        {
            var loc = GetLocation();

            var tz = GeoTimeZone.TimeZoneLookup.GetTimeZone((double)loc.Coordinates.Latitude,
                (double)loc.Coordinates.Longitude);
            var t = TimeZoneConverter.TZConvert.GetTimeZoneInfo(tz.Result);


            if (routine.NextWakeUpTime.HasValue
                && routine.NextWakeUpTime.Value < DateTimeOffset.Now)
            {
                DayOfWeek dw = routine.NextWakeUpTime.Value.DayOfWeek;
                TimeSpan decal = routine.NextWakeUpTime.Value.Offset;
                while (routine.NextWakeUpTime.Value.AddDays(1) < DateTimeOffset.Now)
                    routine.NextWakeUpTime = routine.NextWakeUpTime.Value.AddDays(1);
                routine.NextWakeUpTime = routine.NextWakeUpTime.Value.AddDays(1);
                decal = decal - t.GetUtcOffset(routine.NextWakeUpTime.Value.Date);

                switch (routine.NextWakeUpTime.Value.DayOfWeek)
                {
                    case DayOfWeek.Sunday:
                        routine.NextWakeUpTime = routine.NextWakeUpTime.Value.Add(TimeSpan.FromSeconds(0 - routine.NextWakeUpTime.Value.TimeOfDay.TotalSeconds));
                        routine.NextWakeUpTime = routine.NextWakeUpTime.Value.Add(new TimeSpan(9, 30, 0));
                        routine.NextWakeUpTime = routine.NextWakeUpTime.Value.Add(decal);
                        break;
                    case DayOfWeek.Saturday:
                        routine.NextWakeUpTime = routine.NextWakeUpTime.Value.Add(TimeSpan.FromSeconds(0 - routine.NextWakeUpTime.Value.TimeOfDay.TotalSeconds));
                        routine.NextWakeUpTime = routine.NextWakeUpTime.Value.Add(new TimeSpan(8, 30, 0));
                        routine.NextWakeUpTime = routine.NextWakeUpTime.Value.Add(decal);
                        break;
                    default:
                        routine.NextWakeUpTime = routine.NextWakeUpTime.Value.Add(TimeSpan.FromSeconds(0 - routine.NextWakeUpTime.Value.TimeOfDay.TotalSeconds));
                        routine.NextWakeUpTime = routine.NextWakeUpTime.Value.Add(new TimeSpan(7, 30, 0));
                        routine.NextWakeUpTime = routine.NextWakeUpTime.Value.Add(decal);
                        break;
                }

                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        using (var cli = new MainApiAgentWebClient("clara"))
                        {
                            var lst = cli.UploadData<bool, RoutineDataWithUser>(
                                "v1.0/pim/scheduler/wakeuptime/next",
                                "POST",
                                 routine);
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Err refresh routine : " + e);
                    }
                }
            }
        }

        private static List<RoutineDataWithUser> GetRoutines()
        {
            using (var cli = new MainApiAgentWebClient("clara"))
            {
                var lst = cli.DownloadData<List<RoutineDataWithUser>>("v1.0/pim/scheduler/wakeuptime/next");
                return lst;
            }
        }
    }
}
