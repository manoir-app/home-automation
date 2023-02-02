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
using Colourful;

namespace Home.Agents.Sarah.Devices.Hue
{
    internal static partial class HueHelper
    {
        static Device _bridge = null;
        static ExternalToken _token = null;

        static HueHelper()
        {
            rgbToXyz = new ConverterBuilder().FromRGB().ToXYZ().Build();
            xyzToRgb = new ConverterBuilder().FromXYZ().ToRGB().Build();

        }

        static Colourful.IColorConverter<XYZColor, RGBColor> xyzToRgb = null;
        static Colourful.IColorConverter<RGBColor, XYZColor> rgbToXyz = null;
        public static XYZColor ToXy(Color g)
        {
            var rgb = RGBColor.FromColor(g);
            return rgbToXyz.Convert(rgb);
        }


        public static Color FromXyz(XYZColor g)
        {
            var tmp = xyzToRgb.Convert(g);
            return tmp.ToColor();
        }
        public static Color FromXyz(float[] values)
        {
            XYZColor g;
            if (values.Length == 3)
                g = new XYZColor(values[0], values[1], values[2]);
            else
                return Color.Transparent;

            return FromXyz(g);
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
