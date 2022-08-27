using Home.Common;
using Home.Common.Messages;
using Home.Common.Model;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Home.Common.HomeAutomation;
using System.Drawing;

namespace Home.Agents.Sarah.Devices.Hue
{
    internal static partial class HueHelper
    {
        static Device _bridge = null;
        static ExternalToken _token = null;

        public class XYLight
        {
            public float X { get; set; }
            public float Y { get; set; }

            public XYLight()
            {

            }

            public XYLight(float[] xy)
            {
                if(xy!=null && xy.Length==2)
                {
                    X = xy[0];
                    Y = xy[1];
                }
            }

            public XYLight(Color c)
            {
                float x = (c.R * 0.649926f) + (c.G * 0.103455f) + (c.B * 0.197109f);
                float y = (c.R * 0.234327f) + (c.G * 0.743075f) + (c.B * 0.022598f);
                float z = (c.R * 0.000000f) + (c.G * 0.053077f) + (c.B * 1.035763f);

                this.X = x / (x + y + z);
                this.Y = y / (x + y + z);
            }

            public Color ToColor(float brightness = 1)
            {
                float x1 = X; // the given x1 value

                float y1 = Y; // the given y1 value

                float z = 1.0f - x1 - y1;

                float y2 = brightness; // The given brightness value

                float x2 = (y2 / y1) * x1;

                float z2 = (y2 / y1) * z;

                float r = x2 * 1.4628067f - y2 * 0.1840623f - z2 * 0.2743606f;

                float g = -x2 * 0.5217933f + y2 * 1.4472381f + z2 * 0.0677227f;

                float b = x2 * 0.0349342f - y2 * 0.0968930f + z2 * 1.2884099f;

                r = (float)(r <= 0.0031308f ? 12.92f * r : (1.0f + 0.055f) * Math.Pow(r, (1.0f / 2.4f)) - 0.055f);

                g = (float)(g <= 0.0031308f ? 12.92f * g : (1.0f + 0.055f) * Math.Pow(g, (1.0f / 2.4f)) - 0.055f);

                b = (float)(b <= 0.0031308f ? 12.92f * b : (1.0f + 0.055f) * Math.Pow(b, (1.0f / 2.4f)) - 0.055f);

                return Color.FromArgb((int)(255 * r), (int)(255 * g), (int)(255 * b));
            }
        }




        #region Data Objects


        public class GetGroupsResponse : Dictionary<string, HueGroup>
        {
        }

        public class HueGroup
        {
            public string name { get; set; }
            public string[] lights { get; set; }
            public string[] sensors { get; set; }
            public string type { get; set; }
            public GroupState state { get; set; }
            public bool recycle { get; set; }
            public string _class { get; set; }
            public GroupAction action { get; set; }
        }

        public class GroupState
        {
            public bool all_on { get; set; }
            public bool any_on { get; set; }
        }

        public class GroupAction
        {
            public bool on { get; set; }
            public int bri { get; set; }
            public int hue { get; set; }
            public int sat { get; set; }
            public string effect { get; set; }
            public float[] xy { get; set; }
            public int ct { get; set; }
            public string alert { get; set; }
            public string colormode { get; set; }
        }


        public class GetHueLightResponse : Dictionary<string, HueLight>
        {
        }

        public class HueLight
        {
            public LightState state { get; set; }
            public Swupdate swupdate { get; set; }
            public string type { get; set; }
            public string name { get; set; }
            public string modelid { get; set; }
            public string manufacturername { get; set; }
            public string productname { get; set; }
            public LightCapabilities capabilities { get; set; }
            public LightConfig config { get; set; }
            public string uniqueid { get; set; }
            public string swversion { get; set; }
        }

        public class LightState
        {
            public bool on { get; set; }
            public int bri { get; set; }
            public int hue { get; set; }
            public int sat { get; set; }
            public string effect { get; set; }
            public float[] xy { get; set; }
            public int ct { get; set; }
            public string alert { get; set; }
            public string colormode { get; set; }
            public string mode { get; set; }
            public bool reachable { get; set; }
        }

        public class Swupdate
        {
            public string state { get; set; }
            public DateTime? lastinstall { get; set; }
        }

        public class LightCapabilities
        {
            public bool certified { get; set; }
            public LightControl control { get; set; }
            public LightStreaming streaming { get; set; }
        }

        public class LightControl
        {
            public int mindimlevel { get; set; }
            public int maxlumen { get; set; }
            public string colorgamuttype { get; set; }
            public float[][] colorgamut { get; set; }
            public Ct ct { get; set; }
        }

        public class Ct
        {
            public int min { get; set; }
            public int max { get; set; }
        }

        public class LightStreaming
        {
            public bool renderer { get; set; }
            public bool proxy { get; set; }
        }

        public class LightConfig
        {
            public string archetype { get; set; }
            public string function { get; set; }
            public string direction { get; set; }
            public LightConfigStartup startup { get; set; }
        }

        public class LightConfigStartup
        {
            public string mode { get; set; }
            public bool configured { get; set; }
        }


        public class ChangeResponse
        {
            public Success success { get; set; }
            public Error error { get; set; }
        }

        public class Error
        {
            public int type { get; set; }
            public string address { get; set; }
            public string description { get; set; }
        }


        public class Success
        {
            public int lights1statebri { get; set; }
            public bool lights1stateon { get; set; }
            public int lights1statehue { get; set; }
        }

        #endregion

        public static void Login()
        {
            var t = GetToken();
            if (t != null)
                return;


        }



        public static Dictionary<string, HueGroup> GetGroups()
        {
            var id = GetToken()?.Token;
            if (id == null)
                return new Dictionary<string, HueGroup>();

            var dev = GetHueBridge();
            if (dev == null)
                return new Dictionary<string, HueGroup>();

            string ipV4 = dev.DeviceAddresses.FirstOrDefault();
            using (var cli = new WebClient())
            {
                string json = cli.DownloadString($"http://{ipV4}/api/{id}/groups");
                var ret = JsonConvert.DeserializeObject<GetGroupsResponse>(json);
                return ret;
            }
        }

        public static ExternalToken GetToken()
        {
#if DEBUG
            return new ExternalToken()
            {
                Token = "xfGnvNa412hXasfuzDiWKrcOIa7bHn1EAi7BxhiQ"
            };
#endif
            if (_token == null)
            {
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        using (var cli = new MainApiAgentWebClient("sarah"))
                        {
                            var exts = cli.DownloadData<List<ExternalToken>>("/v1.0/security/tokens/system/hue");
                            if (exts != null && exts.Count >= 1)
                            {
                                _token = exts[0];
                                Console.WriteLine("Hue - token is : " + _token.Token);

                            }
                            else
                            {
                                Console.WriteLine("Hue - no token found, finding bridge");
                                var device = GetHueBridge();
                            }
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }

            }

            return _token;
        }
    }
}
