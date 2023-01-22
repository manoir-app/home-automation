using Home.Common;
using Home.Common.Model;
using Home.Graph.Common;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Org.BouncyCastle.Asn1.Cmp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Home.Agents.Clara.Calendars.SchoolPlanning
{
    public class FranceSchoolPlanning : ICalendarProvider
    {
        private const string UrlCorrespondace = "https://www.data.gouv.fr/fr/datasets/r/b363e051-9649-4879-ae78-71ef227d0cc5";

        private static readonly ReadOnlyDictionary<string, string> _zonesFiles = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
            {
            { "Zone A", "https://www.data.gouv.fr/fr/datasets/r/ee16d126-af0f-4b3b-84d3-080ef8bc0abd" },
            { "Zone B", "https://www.data.gouv.fr/fr/datasets/r/c03b7373-6698-4e44-b5f1-9408b4b2cfe8" },
            { "Zone C", "https://www.data.gouv.fr/fr/datasets/r/c594ee20-e694-4f30-810d-752acdf69d70" }
            });

        private static DateTime _lastCheck = DateTime.MinValue;

        private List<TodoItem> _todoItems = new List<TodoItem>();

        public IEnumerable<TodoItem> GetNextScheduledItems(DateTimeOffset maxDate)
        {
            if (Math.Abs((DateTime.Now - _lastCheck).TotalDays) < 10)
                return _todoItems;

            Console.WriteLine($"Getting school planning for FRA");


            var msg = SchoolPlanningCalendar._meshLocation;
            if (msg==null || !msg.Country.Equals("FRA"))
                return _todoItems;

            var zone = GetZoneFromZip(msg.ZipCode);
            if (zone == null)
                return _todoItems;

            Console.WriteLine($"Getting school planning for FRA : {zone}");


            var refreshed = GetFromZone(zone, maxDate);
            _lastCheck = DateTime.Now;
            _todoItems = refreshed;
            return _todoItems;
        }

        private List<TodoItem> GetFromZone(string zone, DateTimeOffset maxDate)
        {
            var refreshed = new List<TodoItem>();

            string url;
            if (!_zonesFiles.TryGetValue(zone, out url))
                return refreshed;

            var pthZone = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            try
            {
                using (WebClient cli = new ManoirWebClient())
                {
                    cli.DownloadFile(url, pthZone);
                }

                var cals = CalendarCollection.Load(File.ReadAllText(pthZone));
                foreach(var cal in cals)
                {
                    var occs = cal.GetOccurrences(DateTime.Now.AddDays(-90), maxDate.AddDays(90).DateTime)
                        .Distinct().ToList();
                    foreach(var occ in occs)
                    {
                        try
                        {
                            var evt = occ.Source as CalendarEvent;
                            if (evt == null) { continue; }
                            if (evt.Start.Value < DateTime.Today)
                            {
                                if (evt.End.Value > DateTime.Today)
                                    AddItem(refreshed, evt);
                            }
                            else if (evt.Start.Value < maxDate.Date)
                                AddItem(refreshed, evt);
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine("Error in FranceSchoolPlanning : " + ex.ToString());
                        }
                    }
                }

            }
            finally
            {
                if (File.Exists(pthZone))
                    File.Delete(pthZone);
            }

            return refreshed;
        }

        private void AddItem(List<TodoItem> refreshed, CalendarEvent occ)
        {
            TodoItem item = new TodoItem();
            DateTime start = occ.Start.Value;
            DateTime end = occ.End.Value.Date.AddMinutes(-1);

            string name = occ.Summary;
            if(!string.IsNullOrEmpty(name)
                && name.Contains("enseignant", StringComparison.InvariantCultureIgnoreCase))
            {
                // on ignore les "prérentrée enseignants"
                return;
            }
            
            Console.WriteLine($"Getting school planning for FRA : found {name}");

            var tdItem = new TodoItem()
            {
                Label = name,
                OriginItemData = "schoolplanning_fr_" + start.ToString("yyyyMMdd_HHmm") + "-" + end.ToString("yyyyMMdd_HHmm"),
                DueDate = start,
                Duration = (end - start),
                Type = TodoItemType.EventItem,
                PrivacyLevel = PrivacyLevel.SharedWithUsers,
                ListId = "e2a06223-67ea-48f8-93b0-0cb7755591f7",
                Origin = "SchoolPlanning_fr"
            };
            tdItem.Categories.Add("family.events.schoolplanning");

            refreshed.Add(tdItem);
        }

        private string GetZoneFromZip(string zipCode)
        {
            if (zipCode == null || zipCode.Length < 5)
                return null;

            var dep = zipCode.Substring(0, 2);
            if (dep.Equals("97"))
                dep = zipCode.Substring(0, 3);

            var pthZones = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            try
            {
                using (WebClient cli = new ManoirWebClient())
                {
                    cli.DownloadFile(UrlCorrespondace, pthZones);
                }

                using (var st = File.OpenRead(pthZones))
                using (CsvFileReader rdr = new CsvFileReader(st,
                    ',', enc: Encoding.UTF8))
                {
                    while (true)
                    {
                        string[] ligne = rdr.Read();
                        if (ligne == null)
                            break;
                        if (ligne.Length == 6)
                        {
                            if (ligne[3].Equals(dep))
                                return ligne[2];
                        }
                    }
                }
            }
            finally
            {
                if (File.Exists(pthZones))
                    File.Delete(pthZones);
            }

            return null;
        }
    }
}
