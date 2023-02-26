using Home.Common.Model;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Messages
{
    public class WeatherChangeMessage : BaseMessage
    {
        public const string TopicName = "system.mesh.weather.updated";

        public WeatherChangeMessage() : base(TopicName)
        {

        }

        public WeatherChangeMessage(IEnumerable<WeatherInfo> weather) : base(TopicName)
        {
            CurrentWeather = (from z in weather
                              where z.DateDebut <= DateTimeOffset.Now
                              && z.DateFin >= DateTimeOffset.Now
                              select z).FirstOrDefault();
            Forecast = (from z in weather
                        where z.DateDebut > DateTimeOffset.Now
                        select z).ToArray();
        }

        public WeatherInfo CurrentWeather { get; set; }
        public WeatherInfo[] Forecast { get; set; }
    }
}
