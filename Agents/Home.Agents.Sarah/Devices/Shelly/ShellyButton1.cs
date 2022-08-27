using Home.Common.HomeAutomation;
using Home.Common.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Home.Agents.Sarah.Devices.Shelly
{
    public class ShellyButton1 : ShellyDeviceBase, IActionButton
    {
        public ShellyButton1(string ipv4, string deviceId) : base(ipv4, deviceId)
        {

        }

        public DeviceActionnable Configure(string name, DeviceActionnableActionType type, string parameter)
        {
            string url = DeviceActionnable.ToUrl(type, parameter);

            switch(name)
            {
                case "shortpush":
                    var acts = SetSettingsActionUrl("shortpush_url", url);
                    if (acts.actions.shortpush_url != null && acts.actions.shortpush_url.Length == 1)
                    {
                        var push = acts.actions.shortpush_url[0];
                        var act = new DeviceActionnable()
                        {
                            Name = "shortpush",
                            IsMainData = true,
                            StandardActionnableType = DeviceActionnable.ActionnableTypePushButton,
                        };
                        if (push.urls.Length == 1)
                            act.FillActionFromUrl(push.urls[0]);
                        else
                            act.ActionType = DeviceActionnableActionType.None;
                        return act;
                    }
                    return null;
                case "longpush":
                    acts = SetSettingsActionUrl("longpush_url", url);
                    if (acts.actions.longpush_url != null && acts.actions.longpush_url.Length == 1)
                    {
                        var push = acts.actions.longpush_url[0];
                        var act = new DeviceActionnable()
                        {
                            Name = "longpush",
                            IsMainData = false,
                            StandardActionnableType = DeviceActionnable.ActionnableTypePushButton,
                        };
                        if (push.urls.Length == 1)
                            act.FillActionFromUrl(push.urls[0]);
                        else
                            act.ActionType = DeviceActionnableActionType.None;
                        return act;
                    }
                    return null;
                case "doublepush":
                    acts = SetSettingsActionUrl("double_shortpush_url", url);
                    if (acts.actions.double_shortpush_url != null && acts.actions.double_shortpush_url.Length == 1)
                    {
                        var push = acts.actions.double_shortpush_url[0];
                        var act = new DeviceActionnable()
                        {
                            Name = "doublepush",
                            IsMainData = false,
                            StandardActionnableType = DeviceActionnable.ActionnableTypePushButton,
                        };
                        if (push.urls.Length == 1)
                            act.FillActionFromUrl(push.urls[0]);
                        else
                            act.ActionType = DeviceActionnableActionType.None;
                        return act;
                    }
                    return null;
                case "triplepush":
                    acts = SetSettingsActionUrl("triple_shortpush_url", url);
                    if (acts.actions.triple_shortpush_url != null && acts.actions.triple_shortpush_url.Length == 1)
                    {
                        var push = acts.actions.triple_shortpush_url[0];
                        var act = new DeviceActionnable()
                        {
                            Name = "triplepush",
                            IsMainData = false,
                            StandardActionnableType = DeviceActionnable.ActionnableTypePushButton,
                        };
                        if (push.urls.Length == 1)
                            act.FillActionFromUrl(push.urls[0]);
                        else
                            act.ActionType = DeviceActionnableActionType.None;
                        return act;
                    }
                    return null;
            }

            return new DeviceActionnable()
            {

            };
        }

        public IEnumerable<DeviceActionnable> GetActionables()
        {
            var acts = GetSettingsAction();

            var lst = new List<DeviceActionnable>();

            if (acts == null || acts.actions==null)
                return lst;

            if(acts.actions.shortpush_url!=null && acts.actions.shortpush_url.Length==1)
            {
                var push = acts.actions.shortpush_url[0];
                var act = new DeviceActionnable()
                {
                    Name = "shortpush",
                    IsMainData = true,
                    StandardActionnableType = DeviceActionnable.ActionnableTypePushButton,
                };
                if (push.urls.Length == 1)
                    act.FillActionFromUrl(push.urls[0]);
                else
                    act.ActionType = DeviceActionnableActionType.None;
                lst.Add(act);
            }
            if (acts.actions.longpush_url != null && acts.actions.longpush_url.Length == 1)
            {
                var push = acts.actions.longpush_url[0];
                var act = new DeviceActionnable()
                {
                    Name = "longpush",
                    IsMainData = false,
                    StandardActionnableType = DeviceActionnable.ActionnableTypePushButton,
                };
                if (push.urls.Length == 1)
                    act.FillActionFromUrl(push.urls[0]);
                else
                    act.ActionType = DeviceActionnableActionType.None;
                lst.Add(act);
            }
            if (acts.actions.double_shortpush_url != null && acts.actions.double_shortpush_url.Length == 1)
            {
                var push = acts.actions.double_shortpush_url[0];
                var act = new DeviceActionnable()
                {
                    Name = "doublepush",
                    IsMainData = false,
                    StandardActionnableType = DeviceActionnable.ActionnableTypePushButton,
                };
                if (push.urls.Length == 1)
                    act.FillActionFromUrl(push.urls[0]);
                else
                    act.ActionType = DeviceActionnableActionType.None;
                lst.Add(act);
            }
            if (acts.actions.triple_shortpush_url != null && acts.actions.triple_shortpush_url.Length == 1)
            {
                var push = acts.actions.triple_shortpush_url[0];
                var act = new DeviceActionnable()
                {
                    Name = "triplepush",
                    IsMainData = false,
                    StandardActionnableType = DeviceActionnable.ActionnableTypePushButton,
                };
                if (push.urls.Length == 1)
                    act.FillActionFromUrl(push.urls[0]);
                else
                    act.ActionType = DeviceActionnableActionType.None;
                lst.Add(act);
            }
            return lst;

        }

        private SettingsActionResponse SetSettingsActionUrl(string action, string urlAction)
        {
            // http://192.168.33.1/settings/actions?index=0&name=out_on_url&enabled=true&urls[]=http://192.168.1.4/on&urls[]=http://192.168.1.5/on
            string url = $"http://{IpV4}/settings/actions?index=0&name={action}&enabled=";
            if (urlAction == null)
                url += "false&urls[]=";
            else
                url += "true&urls[]=" + urlAction;

            using (var cli = new ShellyWebClient())
            {
                try
                {
                    string json = cli.DownloadString(url);
                    var sdi = JsonConvert.DeserializeObject<SettingsActionResponse>(json);
                    return sdi;
                }
                catch (WebException)
                {
                    return null;
                }
            }
        }

        private SettingsActionResponse GetSettingsAction()
        {
            string url = $"http://{IpV4}/settings/actions";
            using (var cli = new ShellyWebClient())
            {
                try
                {
                    string json = cli.DownloadString(url);
                    var sdi = JsonConvert.DeserializeObject<SettingsActionResponse>(json);
                    return sdi;
                }
                catch (WebException)
                {
                    return null;
                }
            }
        }



        public class SettingsActionResponse
        {
            public Actions actions { get; set; }
        }

        public class Actions
        {
            public Push_Url[] shortpush_url { get; set; }
            public Push_Url[] double_shortpush_url { get; set; }
            public Push_Url[] triple_shortpush_url { get; set; }
            public Push_Url[] longpush_url { get; set; }
        }

        public class Push_Url
        {
            public int index { get; set; }
            public string[] urls { get; set; }
            public bool enabled { get; set; }
        }



    }
}
