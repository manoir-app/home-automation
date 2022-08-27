using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Home.Common.Messages;
using Newtonsoft.Json;
using System.Threading;
using Home.Common.Model;
using Home.Common;

namespace Home.Agents.Aurore
{
    public static class AuroreNewsItemService
    {
        private static List<Common.Model.TodoItem> _allEvents = new List<Common.Model.TodoItem>();
        private static List<HomeStatusItem> _global = new List<HomeStatusItem>();
        private static Dictionary<string, List<HomeStatusItem>> _users = new Dictionary<string, List<HomeStatusItem>>();

        public static HomeStatusItem[] GetGlobalMeshItems()
        {
            _global.Sort((a, b) => b.Date.CompareTo(a.Date));
            return _global.ToArray();
        }

        private static void RefreshGlobalMeshItems(List<HomeStatusItem> global)
        {
            global.RemoveAll(z => z.Date.AddHours(12) < DateTimeOffset.Now);

            if (Math.Abs((DateTimeOffset.Now - _lastEventsRefresh).TotalMinutes) > 2)
                _allEvents = GetTodoItems();
            var scheduledEvents = (from z in _allEvents where z.UserId.Equals("#MESH#") select z).ToList();

            var allItems = ParseScheduledEvents(scheduledEvents);
            foreach (var st in allItems)
            {
                var exists = (from z in global where z.MessageKind.Equals(st.MessageKind) select z).FirstOrDefault();
                if (exists == null)
                {
                    Console.WriteLine("Adding item for greetings :" + st.Message);
                    _global.Add(st);
                }
            }
        }

        private static List<HomeStatusItem> ParseScheduledEvents(List<TodoItem> scheduledEvents)
        {
            List<HomeStatusItem> ret = new List<HomeStatusItem>();
            foreach (var td in scheduledEvents)
            {
                if (!td.DueDate.HasValue)
                    continue;
                var rms = (from z in td.Reminders where z.Kind == ReminderKind.GreetingMessage select z).ToList();
                foreach(var rm in rms)
                {
                    var dt = td.DueDate.Value;
                    if(rm.OffsetFromDueDate.HasValue)
                    {
                        dt = dt.Add(rm.OffsetFromDueDate.Value);
                    }

                    if (dt.Date == DateTime.Today)
                    {
                        var cat = td.Categories.FirstOrDefault();
                        if (string.IsNullOrEmpty(cat))
                            cat = "home.events." + td.Id;

                        ret.Add(new HomeStatusItem()
                        {
                            Date = DateTime.Now,
                            Message = rm.Label,
                            MessageKind = cat
                    });

                    }
                }
            }

            return ret;
        }

        private static List<TodoItem> GetTodoItems()
        {

            List<TodoItem> items = null;
            // on obtient la liste des items schedulés
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("sarah"))
                    {
                        items = cli.DownloadData<List<TodoItem>>("/v1.0/todos/events?nextDays=30");
                        _lastEventsRefresh = DateTimeOffset.Now;
                        break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Err gettings scheduled items : " + e);
                }
            }
            return items;
        }

        public static HomeStatusItem[] GetUserItems(string userName)
        {
            return new HomeStatusItem[0];
        }

        public static void Start()
        {
            var t = new Thread(() => AuroreNewsItemService.Run());
            t.Name = "NewsNotifications";
            t.Start();
        }

        public static void Stop()
        {
            _stop = true;
        }


        public static bool _stop = false;
        public static DateTimeOffset _lastEventsRefresh = DateTime.MinValue;

        private static void Run()
        {
            while (!_stop)
            {
                RefreshGlobalMeshItems(_global);

                Thread.Sleep(15000);
            }
        }


        internal static MessageResponse HandleMessage(MessageOrigin origin, string topic, string messageBody)
        {
            var t = JsonConvert.DeserializeObject<HomeStatusItemsMessage>(messageBody);

            HomeStatusItemsMessageResponse resp = new HomeStatusItemsMessageResponse();
            resp.Response = "OK";
            resp.Items.AddRange(GetGlobalMeshItems());
            if (!string.IsNullOrEmpty(t.UserId))
                resp.Items.AddRange(GetUserItems(t.UserId));
            return resp;
        }
    }
}
