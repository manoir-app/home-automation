using Home.Common;
using Home.Graph.Common;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Playwright;
using MongoDB.Driver.Core.Operations;
using SharpCompress.Archives.Zip;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Home.Agents.Clara.Commute
{
    internal partial class GtfsBasedCommuteDataHelper
    {


        public static string DownloadDataFilesToFolder(string idPrefix, string gtfsZipFileUrl)
        {
            var localFileName = FileCacheHelper.GetLocalFilename("transport", idPrefix, "gtfs.zip");
            using(var cli = new WebClient())
            {
                if (File.Exists(localFileName))
                    File.Delete(localFileName);
                cli.DownloadFile(gtfsZipFileUrl, localFileName);
            }
            string localDir = GetLocalFolder(idPrefix, "gtfs");
            if (Directory.Exists(localDir))
                Directory.Delete(localDir, true);
            Directory.CreateDirectory(localDir);
            ZipFile.ExtractToDirectory(localFileName, localDir);
            return localDir;
        }

        private class GtfsDataStore
        {
            public GtfsDataStore()
            {
                LastRefresh = DateTimeOffset.Now; 
                Stops = new List<Stop>();
                Calendars = new List<Calendar>();
                CalendarDates = new List<CalendarDate>();
                Routes = new List<Route>();
            }


            public DateTimeOffset LastRefresh { get; set; }
            public Agency Agency { get; set; }
            public List<Stop> Stops { get; set; }
            public List<Calendar> Calendars { get; set; }
            public List<CalendarDate> CalendarDates { get; set; }
            public List<Route> Routes { get; set; }
            
        }

        private static Dictionary<string, GtfsDataStore> _dataStores = new Dictionary<string, GtfsDataStore>();

        private static GtfsDataStore GetMainInfos(string prefix, string gtfsFileUrl)
        {
            GtfsDataStore retValue = null;
            if (_dataStores.ContainsKey(prefix))
            {
                retValue = _dataStores[prefix];
                if (Math.Abs((retValue.LastRefresh - DateTimeOffset.Now).TotalHours)> 6)
                    ThreadPool.QueueUserWorkItem((a) =>
                    {
                        RefreshData(prefix, gtfsFileUrl);
                    });
                return retValue;
            }

            retValue = RefreshData(prefix, gtfsFileUrl);

            return retValue;
        }

        private static GtfsDataStore RefreshData(string prefix, string gtfsFileUrl)
        {
            string folder = DownloadDataFilesToFolder(prefix, gtfsFileUrl);
            GtfsDataStore st = new GtfsDataStore();

            st.Agency = ParseAgencyTxt(folder);


            _dataStores[prefix] = st;

            return st;
        }

        private static Agency ParseAgencyTxt(string folder)
        {
            string filename = Path.Combine(folder, "agency.txt");
            if (!File.Exists(filename))
                return null;
            using (var rdr = new CsvFileReader(filename))
            {
                while (true)
                {
                    var parts = rdr.Read(); 

                    if (parts == null)
                        break;

                    if (parts.Length == 8 && parts[0].Equals("agency_id")) // les entetes (normalement)
                        continue;

                    if (parts.Length == 8) // normalement il n'y a qu'une seule ligne
                    {
                        return new Agency(parts);
                    }    

                }
            }

            return null;
        }

        private static List<Stop> ParseStopsTxt(string folder)
        {
            string filename = Path.Combine(folder, "stops.txt");
            if (!File.Exists(filename))
                return null;
            List<Stop> ret = new List<Stop>();
            using (var rdr = new CsvFileReader(filename))
            {
                while (true)
                {
                    var parts = rdr.Read(); 

                    if (parts == null)
                        break;

                    if (parts.Length == 14 && parts[0].Equals("stop_id")) // les entetes (normalement)
                        continue;

                    if (parts.Length == 14) // normalement il n'y a qu'une seule ligne
                    {
                        ret.Add(new Stop(parts));
                    }

                }
            }

            return ret;
        }

        private static List<Calendar> ParseCalendarsTxt(string folder)
        {
            string filename = Path.Combine(folder, "calendar.txt");
            if (!File.Exists(filename))
                return null;
            var ret = new List<Calendar>();
            using (var rdr = new CsvFileReader(filename))
            {
                while (true)
                {
                    var parts = rdr.Read();

                    if (parts == null)
                        break;

                    if (parts.Length == 10 && parts[0].Equals("service_id")) // les entetes (normalement)
                        continue;

                    if (parts.Length == 10) // normalement il n'y a qu'une seule ligne
                    {
                        ret.Add(new Calendar(parts));
                    }

                }
            }

            return ret;
        }

        private static List<CalendarDate> ParseCalendarDatesTxt(string folder)
        {
            string filename = Path.Combine(folder, "calendar_dates.txt");
            if (!File.Exists(filename))
                return null;
            var ret = new List<CalendarDate>();
            using (var rdr = new CsvFileReader(filename))
            {
                while (true)
                {
                    var parts = rdr.Read();

                    if (parts == null)
                        break;

                    if (parts.Length == 3 && parts[0].Equals("service_id")) // les entetes (normalement)
                        continue;

                    if (parts.Length == 3) // normalement il n'y a qu'une seule ligne
                    {
                        ret.Add(new CalendarDate(parts));
                    }

                }
            }

            return ret;
        }

        private static List<Route> ParseRoutesTxt(string folder)
        {
            string filename = Path.Combine(folder, "routes.txt");
            if (!File.Exists(filename))
                return null;
            var ret = new List<Route>();
            using (var rdr = new CsvFileReader(filename))
            {
                while (true)
                {
                    var parts = rdr.Read();

                    if (parts == null)
                        break;

                    if (parts.Length == 10 && parts[0].Equals("route_id")) // les entetes (normalement)
                        continue;

                    if (parts.Length == 10) // normalement il n'y a qu'une seule ligne
                    {
                        ret.Add(new Route(parts));
                    }

                }
            }

            return ret;
        }

    }
}
