using Home.Common;
using Home.Common.Model;
using Home.Graph.Common;
using Microsoft.Azure.Amqp.Framing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Home.Agents.Sarah
{
    public class TriggersChecker
    {
        public static void Start()
        {
            try
            {
                RefreshRoutines();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            var t = new Thread(() => TriggersChecker.Run());
            t.Name = "Triggers";
            t.Start();
        }

        private class MqttTriggerChecker
        {
            private Trigger _t;
            decimal _lastValue = decimal.MinValue;
            public void Init(Trigger t)
            {
                if (t.Kind != TriggerKind.MqttValue)
                    return;

                MqttHelper.Start("agents-sarah");
                if (_t != null)
                    MqttHelper.RemoveChangeHandler(_t.Path, this.Handle);

                _t = t;

                MqttHelper.AddChangeHandler(t.Path, this.Handle);
            }

            private void Handle(string topic, string value)
            {
                Console.WriteLine($"Received message on MQTT for topic {topic} : {value}");

                if (_t.JsonPathInValue != null)
                {
                    try
                    {
                        var tmp = JObject.Parse(value);
                        if (tmp != null)
                        {
                            var obj = tmp.SelectToken(_t.JsonPathInValue, false);
                            if (obj != null)
                            {
                                switch (obj.Type)
                                {
                                    case JTokenType.Float:
                                        value = ((float)obj).ToString("0.0000", CultureInfo.InvariantCulture);
                                        break;
                                    default:
                                        value = obj.ToString();
                                        break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }

                if (_t.ThredsholdForChange != null)
                {
                    decimal valDec = -1;
                    if (decimal.TryParse(value, out valDec))
                    {
                        decimal change = Math.Abs(_lastValue - valDec);

                        if (change < Math.Abs(_t.ThredsholdForChange.Value))
                            return;

                        _lastValue = valDec;
                    }
                }

                TriggersChecker.Raise(_t, value);
            }

            public void Stop()
            {
                if (_t != null)
                    MqttHelper.RemoveChangeHandler(_t.Path, this.Handle);
            }
        }

        public static void Stop()
        {
            _stop = true;
        }

        public static void RefreshRoutines()
        {
            Console.WriteLine("Refreshing wake-up time");

            for (int i = 0; i < 3; i++)
            {
                using (var cli = new MainApiAgentWebClient("sarah"))
                {
                    var lst = cli.DownloadData<List<RoutineDataWithUser>>("v1.0/pim/scheduler/wakeuptime/next");

                    nextminWakeUp = (from z in lst
                                     where z.NextWakeUpTime.HasValue
                                     && z.NextWakeUpTime.Value > DateTimeOffset.Now
                                     select z.NextWakeUpTime.Value).Min();
                    nextmaxWakeUp = (from z in lst
                                     where z.NextWakeUpTime.HasValue
                                     && z.NextWakeUpTime.Value > DateTimeOffset.Now
                                     select z.NextWakeUpTime.Value).Max();

                    break;
                }
            }

            if (todayminWakeUp == null || todayminWakeUp.Value.Date < DateTime.Today)
            {
                if (nextminWakeUp.HasValue && nextminWakeUp.Value.Date != nextminWakeUp.Value)
                {
                    todayminWakeUp = nextminWakeUp;
                    Console.Write($"Min wakeuptime : {todayminWakeUp.GetValueOrDefault().ToString("yyyy-MM-dd HH:mm:ss")}");
                }
            }
            if (todaymaxWakeUp == null || todaymaxWakeUp.Value.Date < DateTime.Today)
            {
                if (todaymaxWakeUp.HasValue && todaymaxWakeUp.Value.Date != nextminWakeUp.Value)
                {
                    todaymaxWakeUp = nextmaxWakeUp;
                    Console.Write($"Max wakeuptime : {todaymaxWakeUp.GetValueOrDefault().ToString("yyyy-MM-dd HH:mm:ss")}");
                }
            }
        }

        private static List<Trigger> _triggers = new List<Trigger>();
        private static Location _loc = null;

        internal static DateTimeOffset? nextminWakeUp = null;
        internal static DateTimeOffset? nextmaxWakeUp = null;

        internal static DateTimeOffset? todayminWakeUp = null;
        internal static DateTimeOffset? todaymaxWakeUp = null;


        internal static Location GetLocation()
        {
            if (_loc == null)
                Reload();
            return _loc;
        }

        public static bool _stop = false;

        static Dictionary<string, MqttTriggerChecker> _mqtt = new Dictionary<string, MqttTriggerChecker>();

        public static void Reload()
        {
            foreach (var k in _mqtt.Keys)
            {
                _mqtt[k].Stop();
            }
            _mqtt.Clear();

            using (var cli = new MainApiAgentWebClient("sarah"))
            {
                var lst = cli.DownloadData<List<Trigger>>("v1.0/system/mesh/local/triggers");

                _triggers = lst;
                foreach (var t in _triggers)
                {
                    if (t.Kind == TriggerKind.MqttValue)
                    {
                        var mqtt = new MqttTriggerChecker();
                        _mqtt.Add(t.Id, mqtt);
                        mqtt.Init(t);
                        Console.WriteLine($"Trigger {t.Id} - MQTT initialized : {JsonConvert.SerializeObject(t)}");
                    }
                    else
                        Console.WriteLine($"Trigger {t.Id} : {JsonConvert.SerializeObject(t)}");
                }

                var tmp = cli.DownloadData<Location>("v1.0/system/mesh/local/location");
                if (tmp != null)
                    _loc = tmp;
            }
        }

        internal static void Run()
        {
            Reload();

            while (!_stop)
            {
                try
                {
                    Thread.Sleep(100);
                    foreach (var t in _triggers)
                    {
                        if (t.Kind == TriggerKind.Clock)
                            TestClockTrigger(t);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        private static void TestClockTrigger(Trigger t)
        {
            var tzOffset = AgentHelper.UserTimeZone.GetUtcOffset(DateTime.Today);

            DateTime last = t.LatestOccurence.GetValueOrDefault(DateTime.MinValue).Date;
            // déjà arrivé aujourd'hui
            if (last >= DateTime.Today)
                return;

            var offset = t.Offset.GetValueOrDefault(TimeSpan.FromHours(48));

            switch (t.OffsetKind.GetValueOrDefault(TimeOffsetKind.FromMidnight))
            {
                case TimeOffsetKind.FromSunrise:
                    if (_loc != null && _loc.Coordinates != null)
                    {
                        DateTimeOffset sunrise = SunCalculator.CalculateSunRise((double)_loc.Coordinates.Latitude, (double)_loc.Coordinates.Longitude, DateTime.Today);
                        sunrise = sunrise.Add(offset);
                        if (sunrise < DateTimeOffset.Now && Math.Abs((sunrise - DateTimeOffset.Now).TotalMinutes) < 15)
                        {
                            t.LatestOccurence = DateTimeOffset.Now;
                            Raise(t, null);
                        }
                        else if (!t.ProbableNextOccurence.HasValue
                            || t.ProbableNextOccurence.Value != sunrise)
                        {
                            SetProbable(t, sunrise);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Un planification par rapport au lever du soleil a été définie, mais la localisation du device est inconnue");
                    }
                    break;
                case TimeOffsetKind.FromSunset:
                    if (_loc != null && _loc.Coordinates != null)
                    {
                        DateTimeOffset sunset = SunCalculator.CalculateSunSet((double)_loc.Coordinates.Latitude, (double)_loc.Coordinates.Longitude, DateTime.Today);
                        sunset = sunset.Add(offset);
                        if (sunset < DateTimeOffset.Now && Math.Abs((sunset - DateTimeOffset.Now).TotalMinutes) < 15)
                        {
                            t.LatestOccurence = DateTimeOffset.Now;
                            Raise(t, null);
                        }
                        else if (!t.ProbableNextOccurence.HasValue
                            || t.ProbableNextOccurence.Value != sunset)
                        {
                            SetProbable(t, sunset);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Un planification par rapport au coucher du soleil a été définie, mais la localisation du device est inconnue");
                    }
                    break;
                case TimeOffsetKind.FromMidnight:
                    var today = DateTime.Today;
                    var dt = new DateTimeOffset(today.Year, today.Month, today.Day, 0, 0, 0, tzOffset).Add(offset);
                    if (dt< DateTimeOffset.Now && Math.Abs((dt - DateTimeOffset.Now).TotalMinutes) < 15)
                    {
                        t.LatestOccurence = DateTimeOffset.Now;
                        Raise(t, null);
                    }
                    else if (!t.ProbableNextOccurence.HasValue
                            || t.ProbableNextOccurence.Value != dt)
                    {
                        SetProbable(t, dt);
                    }
                    break;
                case TimeOffsetKind.FromEarliestWakeup:
                    if (todayminWakeUp.HasValue)
                    {
                        var tmp = todayminWakeUp.Value.Add(offset);
                        if (tmp < DateTimeOffset.Now && Math.Abs((tmp - DateTimeOffset.Now).TotalMinutes) < 15)
                        {
                            t.LatestOccurence = DateTimeOffset.Now;
                            Raise(t, null);
                        }
                        else if (!t.ProbableNextOccurence.HasValue
                            || t.ProbableNextOccurence.Value != tmp)
                        {
                            SetProbable(t, tmp);
                        }

                    }
                    else
                    {
                        Console.WriteLine("Un planification par rapport au reveil (min) a été configuré, mais il n'y a pas d'heure de lever");
                    }
                    break;
                case TimeOffsetKind.FromLatestWakeup:
                    if (todaymaxWakeUp.HasValue)
                    {
                        var tmp = todaymaxWakeUp.Value.Add(offset);
                        if (tmp < DateTimeOffset.Now && Math.Abs((tmp - DateTimeOffset.Now).TotalMinutes) < 15)
                        {
                            t.LatestOccurence = DateTimeOffset.Now;
                            Raise(t, null);
                        }
                        else if (!t.ProbableNextOccurence.HasValue
                            || t.ProbableNextOccurence.Value != tmp)
                        {
                            SetProbable(t, tmp);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Un planification par rapport au reveil (max) a été configuré, mais il n'y a pas d'heure de lever");
                    }
                    break;

            }



        }

        private static void SetProbable(Trigger t, DateTimeOffset value)
        {
            if (!t.ProbableNextOccurence.HasValue
                   || t.ProbableNextOccurence.Value != value)
            {
                for (int i = 0; i < 3; i++)
                {
                    using (var cli = new MainApiAgentWebClient("sarah"))
                    {
                        string url = $"v1.0/system/mesh/local/triggers/{t.Id}/settings?probableNextOccurrence=";
                        url += value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");

                        var done = cli.DownloadData<bool>(url);
                        if (done)
                        {
                            break;
                        }
                    }
                }
                t.ProbableNextOccurence = value;
            }
        }

        private static void Raise(Trigger t, string data)
        {
            for (int i = 0; i < 3; i++)
            {
                using (var cli = new MainApiAgentWebClient("sarah"))
                {
                    string url = $"v1.0/system/mesh/local/triggers/{t.Id}/raise";
                    if (!string.IsNullOrEmpty(data))
                        url += "?data=" + Uri.EscapeDataString(data);

                    var done = cli.DownloadData<bool>(url);
                    if (done)
                    {
                        Console.WriteLine("Trigger : " + t.Id + " déclenché");
                        break;
                    }
                }
            }
        }
    }
}
