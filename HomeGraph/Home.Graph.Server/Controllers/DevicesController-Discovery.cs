using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Home.Common.Messages;
using Home.Common.Model;
using Home.Graph.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace Home.Graph.Server.Controllers
{
    partial class DevicesController
    {
        private static DateTimeOffset? _dtDateDebutDiscovery = null;

        [Route("discovered"), HttpGet]
        public IEnumerable<DiscoveredDevice> GetDiscoveredDevices(string kind = null, string agent = null)
        {
            var coll = MongoDbHelper.GetClient<DiscoveredDevice>();

            var fl = Builders<DiscoveredDevice>.Filter;
            var filters = fl.Empty;

            if (!string.IsNullOrEmpty(kind))
                filters &= fl.Eq("DeviceKind", kind);

            if (!string.IsNullOrEmpty(agent))
                filters &= fl.Eq("DeviceAgentId", agent);

            return coll.Find(filters & fl.Eq("MeshId", "local")).ToList();
        }

        [Route("discovered/clearolds"), HttpGet()]
        public bool GetDiscoveredDevicesForAgent()
        {
            var coll = MongoDbHelper.GetClient<DiscoveredDevice>();
            var res = coll.DeleteMany(x => x.MeshId == "local" && x.DiscoveryDate < DateTimeOffset.Now.AddHours(-1));
            return res.IsAcknowledged;
        }

        [Route("discovery/enable"), HttpGet
#if DEBUG
            ,AllowAnonymous
#endif
]
        public void SetDiscoveryModeViaGet()
        {
            SetDiscoveryMode();
        }

        [Route("discovery/enabled"), HttpPost
#if DEBUG
            ,AllowAnonymous
#endif
]
        public void SetDiscoveryMode()
        {
#if !DEBUG
            if (!User.Identity.IsAuthenticated)
                throw new SecurityException();
#endif
            _dtDateDebutDiscovery = DateTimeOffset.Now;
        }

        [Route("discovery/enabled"), HttpGet
#if DEBUG
            ,AllowAnonymous
#endif
]
        public bool GetDiscoveryMode()
        {
            return CheckIfDiscovery();
        }


        public bool CheckIfDiscovery()
        {
            if (!_dtDateDebutDiscovery.HasValue)
                return false;

            if (_dtDateDebutDiscovery.Value.AddMinutes(1) < DateTimeOffset.Now)
            {
                _dtDateDebutDiscovery = null;
                return false;
            }

            return true;
        }


        public class AppDevice
        {
        }

        [Route("discovery/appdevice/{deviceId}/validate"), HttpPut()]

        public Device CreateDeviceForApp(string deviceId, string deviceName)
        {
            if (deviceId == null)
                throw new ArgumentNullException();

            deviceId = deviceId.ToLowerInvariant();

            var coll = MongoDbHelper.GetClient<Device>();
            var collDisc = MongoDbHelper.GetClient<DiscoveredDevice>();

            var disc = collDisc.Find(x => x.Id.Equals(deviceId)).FirstOrDefault();

            if (disc == null)
                this.NotFound();

            Device device = coll.Find(x => x.Id.Equals(deviceId)).FirstOrDefault();

            if (device == null)
            {
                device = new Device()
                {
                    Id = deviceId,
                    ConfigurationData = disc.DefaultConfigurationData,
                    DeviceKind = disc.DeviceKind,
                    DevicePlatform = disc.DevicePlatform,
                    DeviceRoles = disc.DeviceRoles,
                    DeviceInternalName = disc.DeviceInternalName,
                    DeviceGivenName = deviceName,
                    MeshId = disc.MeshId
                };
                coll.InsertOne(device);
            }

            return device;
        }

        [Route("discovery/appdevice/{deviceId}/check"), HttpGet(), AllowAnonymous]
        public bool CheckIfAssociated(string deviceId)
        {
            if (deviceId == null)
                return false;

            deviceId = deviceId.ToLowerInvariant();

            var coll = MongoDbHelper.GetClient<Device>();

            var dev = coll.Find(x => x.Id.Equals(deviceId)).FirstOrDefault();
            return dev != null;
        }

        /* les points apis pour la partie "non protégée" */

        [Route("discovery/appdevice"), HttpPost(), AllowAnonymous]
        public DiscoveredDevice CreateForAppDevice([FromBody] AppDevice device)
        {
            DiscoveredDevice d = new DiscoveredDevice()
            {
                DiscoveryDate = DateTimeOffset.Now,
                DefaultConfigurationData = null,
                DeviceAgentId = null,
                DeviceCode = MakeUniqueCode(6),
                DeviceInternalName = "",
                DevicePlatform = "WebApp",
                DeviceKind = Device.DeviceKindMobileDevice,
                MeshId = "local",
                Id = Guid.NewGuid().ToString().ToLowerInvariant()
            };

            var coll = MongoDbHelper.GetClient<DiscoveredDevice>();
            coll.InsertOne(d);

            return d;
        }



        private string MakeUniqueCode(int v)
        {
            var blr = new StringBuilder();
            var rnd = new Random();

            while (blr.Length < v)
            {
                switch (rnd.Next(2))
                {
                    case 0:
                        blr.Append((char)('0' + rnd.Next(9)));
                        break;
                    case 1:
                        blr.Append((char)('A' + rnd.Next(26)));
                        break;
                }
            }

            return blr.ToString().ToUpperInvariant();
        }

        [Route("discovery/multiple"), HttpPost(), AllowAnonymous]
        public bool UpsertDiscoveredDevice([FromBody] List<DiscoveredDevice> devices)
        {
            if (!CheckIfDiscovery())
                return false;

            var coll = MongoDbHelper.GetClient<DiscoveredDevice>();

            int iChanged = 0;
            foreach (var t in devices)
                iChanged += UpdateDiscoveredDevice(t, coll);

            return iChanged == devices.Count;
        }


        [Route("discovery"), HttpPost(), AllowAnonymous]
        public bool UpsetDiscoveredDevice([FromBody] DiscoveredDevice t)
        {
            if (!CheckIfDiscovery())
                return false;

            var coll = MongoDbHelper.GetClient<DiscoveredDevice>();
            return UpdateDiscoveredDevice(t, coll) > 0;
        }

        private static int UpdateDiscoveredDevice(DiscoveredDevice t, IMongoCollection<DiscoveredDevice> coll)
        {
            // si le device existe déjà du coté des devices
            // gérés, on ne le "découvre" plus :)
            var collExisting = MongoDbHelper.GetClient<Device>();
            var existing = collExisting.Find(x => x.DeviceInternalName == t.DeviceInternalName
                && x.DeviceAgentId == t.DeviceAgentId).FirstOrDefault();
            if (existing != null)
            {
                return 0;
            }

            if (t.DeviceKind == null)
                return 0;
            else
                t.DeviceKind = t.DeviceKind.ToLowerInvariant();

            if (t.DevicePlatform == null)
                return 0;
            else
                t.DevicePlatform = t.DevicePlatform.ToLowerInvariant();

            if (t.DeviceRoles != null)
            {
                for (int i = 0; i < t.DeviceRoles.Count; i++)
                    t.DeviceRoles[i] = t.DeviceRoles[i].ToLowerInvariant();
            }

            if (t.Id == null)
            {
                var deviold = coll.Find(x => x.DeviceInternalName == t.DeviceInternalName
                                && x.DeviceAgentId == t.DeviceAgentId).FirstOrDefault();
                if (deviold == null)
                    t.Id = Guid.NewGuid().ToString("N");
                else
                    t.Id = deviold.Id;
            }
            t.MeshId = "local";
            var res = coll.ReplaceOne(x => x.DeviceInternalName == t.DeviceInternalName
                                && x.DeviceAgentId == t.DeviceAgentId
                            , t, new ReplaceOptions() { IsUpsert = true });
            if (res.IsAcknowledged)
            {
                string topic = $"{t.DeviceKind}.discovery.{t.DevicePlatform}";
                MessagingHelper.PushToLocalAgent(new DeviceDiscoveredMessage(topic)
                {
                    Device = t,
                    DiscoveryTime = DateTimeOffset.Now
                });
                if (t.DeviceRoles != null)
                {
                    foreach (var role in t.DeviceRoles)
                    {
                        topic = $"{t.DeviceKind}.discovery.{t.DevicePlatform}.{role}";
                        MessagingHelper.PushToLocalAgent(new DeviceDiscoveredMessage(topic)
                        {
                            Device = t,
                            DiscoveryTime = DateTimeOffset.Now
                        });
                    }
                }
                return 1;
            }
            else
                return 0;
        }

        [Route("discovery/agent/{agentId}/devices"), HttpGet(), AllowAnonymous]
        public IEnumerable<DiscoveredDevice> GetDiscoveredDevicesForAgent(string agentId)
        {
            if (!CheckIfDiscovery())
                return new DiscoveredDevice[0];

            if (agentId != null)
                agentId = agentId.ToLowerInvariant();

            var coll = MongoDbHelper.GetClient<DiscoveredDevice>();
            return coll.Find(x => x.MeshId == "local" && x.DeviceAgentId == agentId).ToList();
        }


    }
}
