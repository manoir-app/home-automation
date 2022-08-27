using Home.Common.HomeAutomation;
using Home.Common.Messages;
using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Home.Agents.Sarah.Devices.Hue
{
    public abstract class HueDeviceBase : DeviceBase
    {
        public override string DeviceName
        {
            get
            {
                return GetLightDeviceName(_light);
            }
        }

        internal static string GetLightDeviceName(HueHelper.HueLight light)
        {
            return "hue-" + light.uniqueid.Replace(':', '-') + "-" + light.modelid;
        }

        internal HueHelper.HueLight _light = null;
        internal string _hueDeviceId = null;

        public override bool Equals(object obj)
        {
            if (obj != null && obj is HueDeviceBase)
            {
                return Equals((obj as HueDeviceBase)._light);
            }
            else if (obj != null && obj is HueHelper.HueLight)
            {
                var tocom = obj as HueHelper.HueLight;
                return tocom.uniqueid.Equals(_light.uniqueid);
            }
            return base.Equals(obj);
        }

        internal HueDeviceBase(string deviceId, HueHelper.HueLight light)
        {
            _hueDeviceId = deviceId;
            RefreshState(light);
            _light = light;
        }

        internal void RefreshState(HueHelper.HueLight light)
        {
            if (light != null)
            {
                string globalStatus = "Eteint";
                if(light.state.on)
                {
                    if (this is IIntensityGradientDevice)
                    {
                        int pct = (int)Math.Ceiling(light.state.bri / 2.54M);
                        globalStatus = $"Allumé à {pct} %";

                    }
                    else
                    {
                        globalStatus = "Allumé";
                    }
                }

                var changed = new List<DeviceStateChangedMessage.DeviceStateValue>();
                // on compare la nouvelle avec l'ancienne
                // pour lever un event si il y a des évolutions
                // de valeurs
                if (_light == null || !_light.state.Equals(light.state))
                    changed.Add(new DeviceStateChangedMessage.DeviceStateValue() { Name = "on", Value = light.state.on ? "on" : "off" });

                if (changed.Count > 0)
                    DeviceManager.OnDeviceStateChanged("Hue", GetLightDeviceName(light), Device.HomeAutomationRoleSwitch, globalStatus, changed.ToArray());

                changed.Clear();
                if (this is IIntensityGradientDevice)
                {
                    if (_light == null || _light.state.bri != light.state.bri)
                        changed.Add(new DeviceStateChangedMessage.DeviceStateValue() { Name = "brightness", Value = Math.Ceiling(light.state.bri / 2.54M).ToString("0") });

                    if (changed.Count > 0)
                        DeviceManager.OnDeviceStateChanged("Hue", GetLightDeviceName(light), Device.HomeAutomationRoleDimmer, globalStatus, changed.ToArray());
                }

                changed.Clear();
                if (this is IColorBoundDevice)
                {
                    try
                    {
                        switch (light.state.colormode)
                        {
                            default: // (x,y)
                                if (_light == null)
                                    changed.Add(new DeviceStateChangedMessage.DeviceStateValue() { Name = "color", Value = new HueHelper.XYLight(light.state.xy).ToColor(light.state.bri / 2.54f).ToString() });
                                else if (_light.state.xy == null && light.state.xy != null)
                                    changed.Add(new DeviceStateChangedMessage.DeviceStateValue() { Name = "color", Value = new HueHelper.XYLight(light.state.xy).ToColor(light.state.bri / 2.54f).ToString() });
                                else if (_light.state.xy.Length != light.state.xy?.Length)
                                    changed.Add(new DeviceStateChangedMessage.DeviceStateValue() { Name = "color", Value = new HueHelper.XYLight(light.state.xy).ToColor(light.state.bri / 2.54f).ToString() });
                                else if (_light.state.xy.Length > 0 && _light.state.xy[0] != light.state.xy[0])
                                    changed.Add(new DeviceStateChangedMessage.DeviceStateValue() { Name = "color", Value = new HueHelper.XYLight(light.state.xy).ToColor(light.state.bri / 2.54f).ToString() });
                                else if (_light.state.xy.Length > 1 && _light.state.xy[1] != light.state.xy[1])
                                    changed.Add(new DeviceStateChangedMessage.DeviceStateValue() { Name = "color", Value = new HueHelper.XYLight(light.state.xy).ToColor(light.state.bri / 2.54f).ToString() });
                                else if (_light.state.xy.Length > 2 && _light.state.xy[2] != light.state.xy[2])
                                    changed.Add(new DeviceStateChangedMessage.DeviceStateValue() { Name = "color", Value = new HueHelper.XYLight(light.state.xy).ToColor(light.state.bri / 2.54f).ToString() });
                                break;
                        }
                    }
                    catch
                    {

                    }
                    if (changed.Count > 0)
                        DeviceManager.OnDeviceStateChanged("Hue", GetLightDeviceName(light), Device.HomeAutomationRoleColorBound, globalStatus, changed.ToArray());
                }

            }
            _light = light;
        }

        protected internal virtual DeviceData ConvertOnOffState()
        {
            return new DeviceData()
            {
                IsMainData = true,
                Name = "on",
                StandardDataType = DeviceData.DataTypeSwitch,
                ValidValues = new string[] { "on", "off" },
                Value = _light.state.on ? "on" : "off"
            };
        }

        protected internal virtual DeviceData ConvertColor()
        {
            string colMode = _light?.state?.colormode;
            if (string.IsNullOrEmpty(colMode))
                colMode = "xy";

            Color c = Color.Black;

            switch (colMode.ToLowerInvariant())
            {

            }

            return new DeviceData()
            {
                IsMainData = true,
                Name = "on",
                StandardDataType = DeviceData.DataTypeColor,
            };
        }

    }
}
