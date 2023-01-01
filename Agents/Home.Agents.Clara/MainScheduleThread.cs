using Home.Common;
using Home.Common.Model;
using Home.Graph.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Home.Agents.Clara
{
    public static class MainScheduleThread
    {
        public static bool _stop = false;
        public static void Start()
        {
            var t = new Thread(() => MainScheduleThread.Run());
            t.Name = "Schedule Thread";
            t.Start();
        }

        public static void Stop()
        {
            _stop = true;
        }


        private static DateTimeOffset _nextSunUpdate = DateTimeOffset.Now;
        private static AutomationMesh _mesh = null;
        private static Location _meshLocation = null;


        private static void Run()
        {
            _mesh = AgentHelper.GetLocalMesh("clara");
            _meshLocation = AgentHelper.GetLocalMeshLocation("clara");
            while (!_stop)
            {
                RefreshSun();

                Thread.Sleep(250);
            }
        }

        private static void RefreshSun()
        {
            if (_nextSunUpdate > DateTimeOffset.Now)
                return;

            try
            {
                _nextSunUpdate = DateTimeOffset.Now.AddMinutes(5);

                Entity sun = GetSun();
                if (sun == null)
                {
                    Console.WriteLine("Unable to get data from server");
                    return;
                }
                DateTimeOffset dtSunRise = SunCalculator.CalculateSunRise((double)_meshLocation.Coordinates.Latitude,
                    (double)_meshLocation.Coordinates.Longitude, DateTime.Today);
                DateTimeOffset dtSunSet = SunCalculator.CalculateSunSet((double)_meshLocation.Coordinates.Latitude,
                    (double)_meshLocation.Coordinates.Longitude, DateTime.Today);
                DateTimeOffset dtNextSunRise = dtSunRise;
                DateTimeOffset dtNextSunSet = dtSunSet;

                if (dtSunRise < DateTimeOffset.Now)
                {
                    dtNextSunRise = SunCalculator.CalculateSunRise((double)_meshLocation.Coordinates.Latitude,
                    (double)_meshLocation.Coordinates.Longitude, DateTime.Today.AddDays(1));
                    sun.CurrentImageUrl = HomeServerHelper.GetPublicGraphUrl($"/v1.0/services/files/common/images/sun_daylight.png");
                }
                else
                {
                    sun.CurrentImageUrl = HomeServerHelper.GetPublicGraphUrl($"/v1.0/services/files/common/images/sun_night.png");
                }
                if (dtSunSet < DateTimeOffset.Now)
                {
                    dtSunSet = SunCalculator.CalculateSunSet((double)_meshLocation.Coordinates.Latitude,
                    (double)_meshLocation.Coordinates.Longitude, DateTime.Today.AddDays(1));
                    sun.CurrentImageUrl = HomeServerHelper.GetPublicGraphUrl($"/v1.0/services/files/common/images/sun_night.png");
                }

                sun.Datas["TodaySunRise"] = new EntityData(dtSunRise);
                sun.Datas["TodaySunSet"] = new EntityData(dtSunSet);
                sun.Datas["NextSunRise"] = new EntityData(dtNextSunRise);
                sun.Datas["NextSunSet"] = new EntityData(dtNextSunSet);

                PushSun(sun);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static Entity GetSun()
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("clara"))
                    {
                        var t = cli.DownloadData<Entity>($"v1.0/entities/all/sun");
                        if (t == null || string.IsNullOrEmpty(t.Id))
                        {
                            t = new Entity()
                            {
                                Id = "sun",
                                EntityKind = Entity.EntityKindSun,
                                MeshId = "local",
                                Name = "Sun",
                                DefaultImageUrl = HomeServerHelper.GetPublicGraphUrl($"/v1.0/services/files/common/images/sun_daylight.png")
                            };
                        }
                        return t;
                    }
                }
                catch (WebException ex)
                {
                    var rsp = ex.Response as HttpWebResponse;
                    if(rsp!=null && rsp.StatusCode == HttpStatusCode.NoContent)
                    {
                        return new Entity()
                        {
                            Id = "sun",
                            EntityKind = Entity.EntityKindSun,
                            MeshId = "local",
                            Name = "Sun",
                            DefaultImageUrl = HomeServerHelper.GetPublicGraphUrl($"/v1.0/services/files/common/images/sun_daylight.png")
                        };
                    }
                    Thread.Sleep(1000);
                    Console.WriteLine(ex.Message);
                }
            }

            return null;
        }

        private static Entity PushSun(Entity sun)
        {
            // on push sur le serveur
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("clara"))
                    {
                        var t = cli.UploadData<Entity, Entity>($"v1.0/entities/all/", "POST", sun);
                        return t;
                    }
                }
                catch (WebException ex)
                {
                    Thread.Sleep(1000);
                    Console.WriteLine(ex.Message);
                }
            }

            // on push aussi sur MQTT
            MqttHelper.PublishEntity(sun);

            return null;
        }



    }
}
