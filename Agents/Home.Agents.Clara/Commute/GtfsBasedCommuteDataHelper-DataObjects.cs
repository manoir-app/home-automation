using Org.BouncyCastle.Crypto.Tls;
using System;
using System.Globalization;

namespace Home.Agents.Clara.Commute
{
    partial class GtfsBasedCommuteDataHelper
    {
        private class Calendar
        {
            public string ServiceId { get; set; }
            public bool Monday { get; set; }
            public bool Tuesday { get; set; }
            public bool Wednesday { get; set; }
            public bool Thursday { get; set; }
            public bool Friday { get; set; }
            public bool Saturday { get; set; }
            public bool Sunday { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }

            public Calendar()
            {

            }
            public Calendar(string[] parts)
            {
                if (parts.Length != 10)
                    return;

                bool btemp;
                DateTime dttemp;

                ServiceId = parts[0];
                if (bool.TryParse(parts[1], out btemp)) Monday = btemp;
                if (bool.TryParse(parts[2], out btemp)) Tuesday = btemp;
                if (bool.TryParse(parts[3], out btemp)) Wednesday = btemp;
                if (bool.TryParse(parts[4], out btemp)) Thursday = btemp;
                if (bool.TryParse(parts[5], out btemp)) Friday = btemp;
                if (bool.TryParse(parts[6], out btemp)) Saturday = btemp;
                if (bool.TryParse(parts[7], out btemp)) Sunday = btemp;

                if (DateTime.TryParseExact(parts[8], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dttemp)) StartDate = dttemp;
                if (DateTime.TryParseExact(parts[9], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dttemp)) EndDate = dttemp;
            }
        }

        private class CalendarDate
        {
            public string ServiceId { get; set; }
            public DateTime Date { get; set; }
            public CalendarDateExceptionType ExceptionType { get; set; }

            public CalendarDate()
            {

            }

            public CalendarDate(string[] parts)
            {
                if (parts.Length != 3)
                    return;

                int itemp;
                DateTime dttemp;

                ServiceId = parts[0];
                if (DateTime.TryParseExact(parts[1], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dttemp)) Date = dttemp;
                if (int.TryParse(parts[2], out itemp)) ExceptionType = (CalendarDateExceptionType)itemp;
            }

        }

        private enum CalendarDateExceptionType
        {
            DateAdded = 1,
            DateRemoved = 2
        }

        private enum StopLocationType
        {
            Stop = 0,
            Station = 1,
            EntranceOrExit=2,
            Generic=3,
            BoardingArea = 4
        }

        private class Stop
        {
            public string StopId { get; set; }
            public string StopCode { get; set; }
            public string StopName { get; set; }
            public string StopDescription { get; set; }
            public decimal Latitude { get; set; }
            public decimal Longitude { get; set; }
            public string ZoneId { get; set; }
            public string Url { get; set; }
            public StopLocationType LocationType { get; set; }
            public string ParentStationStopId { get; set; }
            public string StopTimezone { get; set; }
            public int WheelchairBoarding { get; set; }
            public string LevelId { get; set; }
            public string PlatformCode { get; set; }

            public Stop()
            {

            }
            public Stop(string[] parts)
            {
                if (parts.Length != 14)
                    return;

                bool btemp;
                decimal dtemp;
                int itemp;

                StopId = parts[0];
                StopCode = parts[1];
                StopName = parts[2];
                StopDescription = parts[3];

                if (decimal.TryParse(parts[4], NumberStyles.Number, CultureInfo.InvariantCulture, out dtemp)) Latitude = dtemp;
                if (decimal.TryParse(parts[5], NumberStyles.Number, CultureInfo.InvariantCulture, out dtemp)) Longitude = dtemp;

                ZoneId = parts[6];
                if (string.IsNullOrEmpty(ZoneId)) ZoneId = null;
                Url = parts[7];
                if (string.IsNullOrEmpty(Url)) Url = null;

                if (int.TryParse(parts[8], out itemp)) LocationType = (StopLocationType)itemp;
                ParentStationStopId = parts[9];
                if (string.IsNullOrEmpty(ParentStationStopId)) ParentStationStopId = null;
                StopTimezone = parts[10];
                if (int.TryParse(parts[11], out itemp)) WheelchairBoarding = itemp;

                LevelId = parts[12];
                if (string.IsNullOrEmpty(LevelId)) LevelId = null;

                PlatformCode = parts[13];
            }
        }

        private class Agency
        {
            public string AgencyId { get; set; }
            public string AgencyName { get; set; }
            public string Url { get; set; }

            public string TimeZone { get; set; }
            public string Language { get; set; }
            public string PhoneNumber { get; set; }
            public string FareUrl { get; set; }
            public string AgencyEmail { get; set; }

            public Agency()
            {

            }
            public Agency(string[] parts)
            {
                AgencyId = parts[0];
                AgencyName = parts[1];
                Url = parts[2];
                if (string.IsNullOrEmpty(Url)) Url = null;

                TimeZone = parts[3];
                Language = parts[4];
                PhoneNumber = parts[5];
                if (string.IsNullOrEmpty(PhoneNumber)) PhoneNumber = null;
                FareUrl = parts[6];
                if (string.IsNullOrEmpty(FareUrl)) FareUrl = null;
                AgencyEmail = parts[7];
                if (string.IsNullOrEmpty(AgencyEmail)) AgencyEmail = null;
            }

        }

        public enum RouteType
        {
            Tram=0,
            Subway=1,
            Rail=2,
            Bus=3,
            Ferry=4,
            CableTram=5,
            SuspendedTram=6,
            Funicular=7,
            TrolleyBus=11,
            Monorail=12
        }

        private class Route
        {
            public string RouteId { get; set; }
            public string AgencyId { get; set; }
            public string RouteShortName { get; set; }
            public string RouteName { get; set; }
            public string RouteDescription { get; set; }
            public RouteType RouteType { get; set; }
            public string Url { get; set; }
            public string Color { get; set; }
            public string TextColor { get; set; }
            public int SortOrder { get; set; }

            public Route()
            {

            }

            public Route(string[] parts)
            {
                RouteId = parts[0];
                AgencyId = parts[1];
                RouteShortName = parts[2];
                RouteName = parts[2];
                RouteDescription = parts[4];
                if (string.IsNullOrEmpty(Url)) Url = null;

                int itemp;
                if (int.TryParse(parts[5], out itemp)) RouteType = (RouteType)itemp;
                Url = parts[6];
                Color = parts[7];
                if (string.IsNullOrEmpty(Color)) Color = null;
                else if (!Color.StartsWith("#")) Color = "#" + Color;
                TextColor = parts[8];
                if (string.IsNullOrEmpty(TextColor)) TextColor = null;
                else if (!TextColor.StartsWith("#")) TextColor = "#" + Color;
                if (int.TryParse(parts[9], out itemp)) SortOrder = itemp;
            }
        }

        private class Trip
        {
            public string RouteId { get; set; }
            public string ServiceId { get; set; }
            public string TripId { get; set; }
            public string Headsign { get; set; }
            public string ShortName { get; set; }
            public int DirectionId { get; set; }
            public string BlockId { get; set; }
            public string ShapeId { get; set; }
            public int WheelchairAccesible { get; set; }
            public int BikesAllowed { get; set; }

            public Trip()
            {

            }
            public Trip(string[] parts)
            {
                RouteId = parts[0];
                ServiceId = parts[1];
                TripId = parts[2];
                Headsign = parts[3];
                ShortName = parts[4];

                int itemp;
                if (int.TryParse(parts[5], out itemp)) DirectionId = itemp;
                BlockId = parts[6];
                ShapeId = parts[7];
                if (int.TryParse(parts[8], out itemp)) WheelchairAccesible = itemp;
                if (int.TryParse(parts[9], out itemp)) BikesAllowed = itemp;

            }
        }


        public enum StopTimeMethodType
        {
            Normal = 0,
            NoPickup = 1,
            CallForArrangement = 2,
            DriverArrangement = 3,
        }

        private class StopTime
        {
            public string TripId { get; set; }
            public TimeSpan ArrivalTime { get; set; }
            public TimeSpan DepartureTime { get; set; }
            public int StopSequence { get; set; }
            public StopTimeMethodType PickupType { get; set; }
            public StopTimeMethodType DropOffType { get; set; }

            public StopTime()
            {

            }

            public StopTime(string[] parts)
            {
                TripId = parts[0];
                TimeSpan ts;
                if (TimeSpan.TryParseExact(parts[1], "HH:mm:ss", CultureInfo.InvariantCulture, TimeSpanStyles.None, out ts)) ArrivalTime = ts;
                if (TimeSpan.TryParseExact(parts[2], "HH:mm:ss", CultureInfo.InvariantCulture, TimeSpanStyles.None, out ts)) DepartureTime = ts;
                int itemp;
                if (int.TryParse(parts[3], out itemp)) StopSequence = itemp;
                if (int.TryParse(parts[4], out itemp)) PickupType = (StopTimeMethodType)itemp; else PickupType = StopTimeMethodType.Normal;
                if (int.TryParse(parts[5], out itemp)) DropOffType = (StopTimeMethodType)itemp; else DropOffType = StopTimeMethodType.Normal;
            }
        }
    }
}
