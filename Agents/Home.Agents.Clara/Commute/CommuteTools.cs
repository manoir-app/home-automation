using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Home.Agents.Clara.Commute
{
    internal class CommuteTools
    {
        public class CommuteStation
        {
            public string Id { get; set; }

            public string Name { get; set; }
        }

        public class CommuteDataSet
        {
            public CommuteDataSet()
            {
                DatasetFormats = new List<string>();
            }

            public string DatasetUri { get; set; }
            public List<string> DatasetFormats { get; set; }
            public string DatasetProvider { get; set; }
        }

        public class CommuteTrip
        {

        }

        public class TripToMonitor : CommuteTrip
        {
            public TripToMonitor()
            {
                Schedule = new List<CommuteSchedule>();
            }

            public CommuteStation StartStation { get; set; }
            public CommuteStation EndStation { get; set; }

            public CommuteStation NextStation { get; set; }
            public CommuteStation LatestStation { get; set; }


            public List<CommuteSchedule> Schedule { get; set; }
        }

        public class StationToMonitor : CommuteStation
        {
            public StationToMonitor() : base()
            {
                StationIdentifiers = new List<string>();
                Lines = new List<CommuteLine>();
            }

            public List<string> StationIdentifiers { get; set; }

            public List<CommuteLine> Lines { get; set; }

        }

        public class CommuteLine
        {
            public CommuteLine()
            {
                ScheduleForNext24h = new List<CommuteSchedule>();
            }

            public string Id { get; set; }
            public CommuteStation LineStart { get; set; }
            public CommuteStation LineEnd { get; set; }
            public CommuteStation PreviousStation { get; set; }
            public CommuteStation NextStation { get; set; }

            public List<CommuteSchedule> ScheduleForNext24h { get; set; }
        }

        public class CommuteSchedule
        {
            public CommuteSchedule()
            {
                RealtimeEvents = new List<CommuteLineScheduleEvent>();
            }

            public string TripId { get; set; }
            public string StationId { get; set; }

            public DateTimeOffset TheoricalSchedule { get; set; }

            public DateTimeOffset RealtimeSchedule { get; set; }
            public bool RealtimeCancelled { get; set; }

            public List<CommuteLineScheduleEvent> RealtimeEvents { get; set; }

        }

        public class CommuteLineScheduleEvent
        {
            public string EventId { get; set; }
            public string Code { get; set; }

            public string Message { get; set; }
        }


        //public CommuteStation[] SearchForStation(string prefix)


    }
}
