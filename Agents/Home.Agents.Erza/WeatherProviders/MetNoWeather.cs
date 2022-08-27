using Home.Common;
using Home.Common.Model;
using Home.Graph.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Home.Agents.Erza.WeatherProviders
{
    internal class MetNoWeather : ITask<Location>
    {
        public bool Run(TaskContextKind contextKind, string contextName, Location loc)
        {
            if (contextKind != TaskContextKind.Agent)
                return false;


            var coord = loc.Coordinates;

            if (coord == null)
                return false;
            try
            {
                string url = $"https://api.met.no/weatherapi/locationforecast/2.0/compact.json?lat={coord.Latitude.ToString("0.0000", CultureInfo.InvariantCulture)}&lon={coord.Longitude.ToString("0.0000", CultureInfo.InvariantCulture)}";
                using (var cli = new WebClient())
                {
                    cli.Headers.Add(HttpRequestHeader.UserAgent, "manoir.app erza-weather-status-1.0 github.com/mcarbenay");
                    var response = cli.DownloadString(url);
                    var objResp = JsonConvert.DeserializeObject<MetNoResponse>(response);
                    ConvertAndUpload(objResp, contextName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }

            return true;
        }

        private void ConvertAndUpload(MetNoResponse objResp, string contextName)
        {
            var data = objResp.properties.timeseries.FirstOrDefault()?.data;
            if (data == null)
                return;

            var instant = data.instant;
            var next = data.next_1_hours;

            WeatherInfo wi = new WeatherInfo()
            {
                DateDebut = DateTimeOffset.Now,
                DateFin = DateTimeOffset.Now.AddHours(1),
                Kind = ConvertSummaryToWeatherKind(next),
                Temperature = (int)next.details.air_temperature.GetValueOrDefault(instant.details.air_temperature.GetValueOrDefault(-400))
            };
            wi.TemperatureFeltAs = wi.Temperature;
            wi.Label = WeatherInfo.GetLabelFromKind(wi.Kind);
            var symb = next.summary?.symbol_code;
            if (symb != null && symb.Contains("thunder", StringComparison.CurrentCultureIgnoreCase))
                wi.RiskOfThumber = true;

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient(contextName))
                    {
                        bool b = cli.UploadData<bool, WeatherInfo[]>("v1.0/system/mesh/local/location/infos/weather", "POST", new WeatherInfo[] { wi });
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        private WeatherInfoKind ConvertSummaryToWeatherKind(CompactForecast next1h)
        {
            if (string.IsNullOrEmpty(next1h.summary.symbol_code))
                return WeatherInfoKind.Sunny;

            switch (next1h.summary.symbol_code.ToLowerInvariant())
            {
                case "clearsky":
                    return WeatherInfoKind.Sunny;
                case "cloudy":
                    return WeatherInfoKind.Cloudy;
                case "fair":
                    return WeatherInfoKind.Fair;
                case "fog":
                    return WeatherInfoKind.Fog;
                case "lightrain":
                case "lightrainandthunder":
                case "lightrainshowers":
                case "lightrainshowersandthunder":
                case "rain":
                case "rainandthunder":
                case "rainshowers":
                case "rainshowersandthunder":
                    return WeatherInfoKind.Rain;
                case "heavyrain":
                case "heavyrainandthunder":
                case "heavyrainshowers":
                case "heavyrainshowersandthunder":
                    return WeatherInfoKind.HardRain;
                case "snow":
                case "snowandthunder":
                case "snowshowers":
                case "snowshowersandthunder":
                case "heavysnow":
                case "heavysnowandthunder":
                case "heavysnowshowers":
                case "heavysnowshowersandthunder":
                case "heavysleet":
                case "heavysleetandthunder":
                case "heavysleetshowers":
                case "heavysleetshowersandthunder":
                case "lightsnow":
                case "lightsnowandthunder":
                case "lightsnowshowers":
                case "lightsnowshowersandthunder":
                    return WeatherInfoKind.Snow;
            }

            return WeatherInfoKind.Sunny;
        }

        class MetNoResponse
        {
            public string type { get; set; }
            public Geometry geometry { get; set; }
            public Properties properties { get; set; }
        }

        class Geometry
        {
            public string type { get; set; }
            public float[] coordinates { get; set; }
        }

        class Properties
        {
            public Meta meta { get; set; }
            public Timesery[] timeseries { get; set; }
        }

        class Meta
        {
            public DateTime updated_at { get; set; }
            public Units units { get; set; }
        }


        public class Units
        {
            public string air_pressure_at_sea_level { get; set; }
            public string air_temperature { get; set; }
            public string air_temperature_max { get; set; }
            public string air_temperature_min { get; set; }
            public string cloud_area_fraction { get; set; }
            public string cloud_area_fraction_high { get; set; }
            public string cloud_area_fraction_low { get; set; }
            public string cloud_area_fraction_medium { get; set; }
            public string dew_point_temperature { get; set; }
            public string fog_area_fraction { get; set; }
            public string precipitation_amount { get; set; }
            public string precipitation_amount_max { get; set; }
            public string precipitation_amount_min { get; set; }
            public string probability_of_precipitation { get; set; }
            public string probability_of_thunder { get; set; }
            public string relative_humidity { get; set; }
            public string ultraviolet_index_clear_sky { get; set; }
            public string wind_from_direction { get; set; }
            public string wind_speed { get; set; }
            public string wind_speed_of_gust { get; set; }
        }


        class Timesery
        {
            public DateTime time { get; set; }
            public Data data { get; set; }
        }

        class Data
        {
            public Instant instant { get; set; }
            public CompactForecast next_12_hours { get; set; }
            public CompactForecast next_1_hours { get; set; }
            public CompactForecast next_6_hours { get; set; }
        }

        public class Instant
        {
            public Details details { get; set; }
        }

        public class Details
        {
            public float? precipitation_amount { get; set; }
            public float? precipitation_amount_max { get; set; }
            public float? precipitation_amount_min { get; set; }
            public float? probability_of_precipitation { get; set; }
            public float? probability_of_thunder { get; set; }
            public float? air_pressure_at_sea_level { get; set; }
            public float? air_temperature { get; set; }
            public float? air_temperature_min { get; set; }
            public float? air_temperature_max { get; set; }
            public float? cloud_area_fraction { get; set; }
            public float? relative_humidity { get; set; }
            public float? wind_from_direction { get; set; }
            public float? wind_speed { get; set; }
            public float? wind_speed_of_gust { get; set; }

        }

        public class Summary
        {
            public string symbol_code { get; set; }
        }

        public class CompactForecast
        {
            public Summary summary { get; set; }
            public Details details { get; set; }
        }

    }
}
