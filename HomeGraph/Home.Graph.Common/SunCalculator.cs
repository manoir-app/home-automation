using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Graph.Common
{
    /// <summary>
    /// base for this code from Patrick Kalkman
    /// http://www.codeproject.com/Articles/78486/Solar-Calculator-Calculate-Sunrise-Sunset-and-Maxi
    /// with a few changes :)
    /// </summary>
    public class SunCalculator
    {
        public static DateTimeOffset CalculateSunRise(double latitude, double longitude, DateTime date)
        {

            var tz = GeoTimeZone.TimeZoneLookup.GetTimeZone(latitude, longitude);
            var t = TimeZoneConverter.TZConvert.GetTimeZoneInfo(tz.Result);

            date = date.Date;

            SunCalculator s = new SunCalculator(longitude, latitude, t.IsDaylightSavingTime(date));
            var dt = s.CalculateSunRise(date);
            return new DateTimeOffset(dt, t.GetUtcOffset(date));
        }
        public static DateTimeOffset CalculateSunSet(double latitude, double longitude, DateTime date)
        {
            var tz = GeoTimeZone.TimeZoneLookup.GetTimeZone(latitude, longitude);
            var t = TimeZoneConverter.TZConvert.GetTimeZoneInfo(tz.Result);

            date = date.Date;

            SunCalculator s = new SunCalculator(longitude, latitude, t.IsDaylightSavingTime(date));
            var dt = s.CalculateSunSet(date);
            return new DateTimeOffset(dt,t.GetUtcOffset(date));
        }

        private readonly double longitude;
        private readonly double latituteInRadians;
        private readonly double longituteTimeZone;
        private readonly bool useSummerTime;

        private SunCalculator()
        {
        }

        private SunCalculator(double longitude, double latitude, bool useSummerTime)
        {
            this.longitude = longitude;
            latituteInRadians = ConvertDegreeToRadian(latitude);
            this.longituteTimeZone = CalculateTimeZone(longitude);
            this.useSummerTime = useSummerTime;
        }

        private static double CalculateTimeZone(double longitude)
        {
            if (longitude > 0) return (Math.Ceiling(longitude / 15)) * 15;
            return (Math.Floor(longitude / 15)) * 15;
        }

        private DateTime CalculateSunRise(DateTime dateTime)
        {
            int dayNumberOfDateTime = ExtractDayNumber(dateTime);
            double differenceSunAndLocalTime = CalculateDifferenceSunAndLocalTime(dayNumberOfDateTime);
            double declanationOfTheSun = CalculateDeclination(dayNumberOfDateTime);
            double tanSunPosition = CalculateTanSunPosition(declanationOfTheSun);
            int sunRiseInMinutes = CalculateSunRiseInternal(tanSunPosition, differenceSunAndLocalTime);
            return CreateDateTime(dateTime, sunRiseInMinutes);
        }

        private DateTime CalculateSunSet(DateTime dateTime)
        {
            int dayNumberOfDateTime = ExtractDayNumber(dateTime);
            double differenceSunAndLocalTime = CalculateDifferenceSunAndLocalTime(dayNumberOfDateTime);
            double declanationOfTheSun = CalculateDeclination(dayNumberOfDateTime);
            double tanSunPosition = CalculateTanSunPosition(declanationOfTheSun);
            int sunSetInMinutes = CalculateSunSetInternal(tanSunPosition, differenceSunAndLocalTime);
            return CreateDateTime(dateTime, sunSetInMinutes);
        }

        private double CalculateMaximumSolarRadiation(DateTime dateTime)
        {
            int dayNumberOfDateTime = ExtractDayNumber(dateTime);
            double differenceSunAndLocalTime = CalculateDifferenceSunAndLocalTime(dayNumberOfDateTime);
            int numberOfMinutesThisDay = GetNumberOfMinutesThisDay(dateTime, differenceSunAndLocalTime);
            double declanationOfTheSun = CalculateDeclination(dayNumberOfDateTime);
            double sinSunPosition = CalculateSinSunPosition(declanationOfTheSun);
            double cosSunPosition = CalculateCosSunPosition(declanationOfTheSun);
            double sinSunHeight = sinSunPosition + cosSunPosition * Math.Cos(2.0 * Math.PI * (numberOfMinutesThisDay + 720.0) / 1440.0) + 0.08;
            double sunConstantePart = Math.Cos(2.0 * Math.PI * dayNumberOfDateTime);
            double sunCorrection = 1370.0 * (1.0 + (0.033 * sunConstantePart));
            return CalculateMaximumSolarRadiationInternal(sinSunHeight, sunCorrection);
        }

        private double CalculateDeclination(int numberOfDaysSinceFirstOfJanuary)
        {
            return Math.Asin(-0.39795 * Math.Cos(2.0 * Math.PI * (numberOfDaysSinceFirstOfJanuary + 10.0) / 365.0));
        }

        private static int ExtractDayNumber(DateTime dateTime)
        {
            return dateTime.DayOfYear;
        }

        private static DateTime CreateDateTime(DateTime dateTime, int timeInMinutes)
        {
            int hour = timeInMinutes / 60;
            int minute = timeInMinutes - (hour * 60);
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, hour, minute, 00);
        }

        private static int CalculateSunRiseInternal(double tanSunPosition, double differenceSunAndLocalTime)
        {
            int sunRise = (int)(720.0 - 720.0 / Math.PI * Math.Acos(-tanSunPosition) - differenceSunAndLocalTime);
            sunRise = LimitSunRise(sunRise);
            return sunRise;
        }


        private static int CalculateSunSetInternal(double tanSunPosition, double differenceSunAndLocalTime)
        {
            int sunSet = (int)(720.0 + 720.0 / Math.PI * Math.Acos(-tanSunPosition) - differenceSunAndLocalTime);
            sunSet = LimitSunSet(sunSet);
            return sunSet;
        }

        private double CalculateTanSunPosition(double declanationOfTheSun)
        {
            double sinSunPosition = CalculateSinSunPosition(declanationOfTheSun);
            double cosSunPosition = CalculateCosSunPosition(declanationOfTheSun);
            double tanSunPosition = sinSunPosition / cosSunPosition;
            tanSunPosition = LimitTanSunPosition(tanSunPosition);
            return tanSunPosition;
        }

        private double CalculateCosSunPosition(double declanationOfTheSun)
        {
            return Math.Cos(latituteInRadians) * Math.Cos(declanationOfTheSun);
        }

        private double CalculateSinSunPosition(double declanationOfTheSun)
        {
            return Math.Sin(latituteInRadians) * Math.Sin(declanationOfTheSun);
        }

        private double CalculateDifferenceSunAndLocalTime(int dayNumberOfDateTime)
        {
            double ellipticalOrbitPart1 = 7.95204 * Math.Sin((0.01768 * dayNumberOfDateTime) + 3.03217);
            double ellipticalOrbitPart2 = 9.98906 * Math.Sin((0.03383 * dayNumberOfDateTime) + 3.46870);

            double differenceSunAndLocalTime = ellipticalOrbitPart1 + ellipticalOrbitPart2 + (longitude - longituteTimeZone) * 4;

            if (useSummerTime)
                differenceSunAndLocalTime -= 60;
            return differenceSunAndLocalTime;
        }

        private static double LimitTanSunPosition(double tanSunPosition)
        {
            if (((int)tanSunPosition) < -1)
            {
                tanSunPosition = -1.0;
            }
            if (((int)tanSunPosition) > 1)
            {
                tanSunPosition = 1.0;
            }
            return tanSunPosition;
        }

        private static int LimitSunSet(int sunSet)
        {
            if (sunSet > 1439)
            {
                sunSet -= 1439;
            }
            return sunSet;
        }

        private static int LimitSunRise(int sunRise)
        {
            if (sunRise < 0)
            {
                sunRise += 1440;
            }
            return sunRise;
        }

        private static double ConvertDegreeToRadian(double degree)
        {
            return degree * Math.PI / 180;
        }

        private static double CalculateMaximumSolarRadiationInternal(double sinSunHeight, double sunCorrection)
        {
            double maximumSolarRadiation;
            if ((sinSunHeight > 0.0) && Math.Abs(0.25 / sinSunHeight) < 50.0)
            {
                maximumSolarRadiation = sunCorrection * sinSunHeight * Math.Exp(-0.25 / sinSunHeight);
            }
            else
            {
                maximumSolarRadiation = 0;
            }
            return maximumSolarRadiation;
        }

        private static int GetNumberOfMinutesThisDay(DateTime dateTime, double differenceSunAndLocalTime)
        {
            return dateTime.Hour * 60 + dateTime.Minute + (int)differenceSunAndLocalTime;
        }
    }

}
