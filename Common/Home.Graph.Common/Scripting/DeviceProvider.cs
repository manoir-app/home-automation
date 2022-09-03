using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using Microsoft.CSharp.RuntimeBinder;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Home.Common.Messages;

namespace Home.Graph.Common.Scripting
{
    public class DeviceProvider
    {
        public class DynamicDevice
        // : DynamicObject 
        // peut être qu'un jour ça fonctionnera :
        // https://github.com/dotnet/roslyn/issues/3194
        // mais après 7 ans...
        {
            private Home.Common.Model.Device _device = null;
            private Type _t = null;

            public DynamicDevice(Home.Common.Model.Device device)
            {
                _device = device;
                _t = _device.GetType();
            }

            public string Id { get { return _device.Id; } }
            public string DeviceGivenName { get { return _device.DeviceGivenName; } }
            public string DeviceInternalName { get { return _device.DeviceInternalName; } }

            public string On
            {
                get
                {
                    return GetDataIfRole(Home.Common.Model.Device.HomeAutomationRoleSwitch, "on");
                }
                set
                {
                    if (value != null)
                    {
                        HomeAutomationMessage msg = new HomeAutomationMessage(
                            $"{_device.DeviceKind}.{_device.DevicePlatform}",
                            _device.Id,
                            Home.Common.Model.Device.HomeAutomationRoleSwitch,
                            "on",
                            value);
                        MessagingHelper.PushToLocalAgent(msg);
                    }
                }
            }

            public int Brightness
            {
                get
                {
                    int parsed = 0;
                    if (!int.TryParse(GetDataIfRole(Home.Common.Model.Device.HomeAutomationRoleDimmer, "brightness"), out parsed))
                        return -1;
                    return parsed;
                }

                set
                {

                    HomeAutomationMessage msg = new HomeAutomationMessage(
                        $"{_device.DeviceKind}.{_device.DevicePlatform}",
                        _device.Id,
                        Home.Common.Model.Device.HomeAutomationRoleDimmer,
                        "brightness",
                        value.ToString("0"));
                    MessagingHelper.PushToLocalAgent(msg);
                }
            }

            private string GetDataIfRole(string role, string dataName)
            {
                if (_device.DeviceRoles != null)
                {
                    if (_device.DeviceRoles.Contains(role))
                    {
                        var t = (from z in _device.Datas
                                 where z.Name.Equals(dataName)
                                 select z).FirstOrDefault();
                        if (t != null)
                            return t.Value;
                        else
                            return "off";
                    }
                }
                return null;
            }


        }

        public DynamicDevice this[string deviceId]
        {
            get
            {
                var coll = MongoDbHelper.GetClient<Home.Common.Model.Device>();
                Console.WriteLine($"Trying to find {deviceId}");
                var item = coll.Find(x => x.Id.Equals(deviceId)).FirstOrDefault();
                Console.WriteLine($"Trying to find {deviceId} : {item}");

                if (item != null)
                {
                    var ret = new DynamicDevice(item);
                    return ret;
                }
                return null;
            }
        }
    }
}
