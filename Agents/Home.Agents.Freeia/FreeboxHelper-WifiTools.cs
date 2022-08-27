using Newtonsoft.Json;
using QRCoder;
using System.Linq;
using System.Net;
using static QRCoder.PayloadGenerator;

namespace Home.Agents.Freeia
{
    partial class FreeboxHelper
    {
        public static byte[] GetWifiQr(string wifiName, string secret, int size = 20)
        {
            WiFi generator = new WiFi(wifiName, secret, WiFi.Authentication.WPA);
            string payload = generator.ToString();

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(size);
        }

        public static byte[] GetWifiConnectionInfoForMain(int size = 20)
        {
            var tk = GetToken();

            using (WebClient cli = new WebClient())
            {
                GetBaseUrl(cli);

                string sessTk = Login(tk, cli);

                try
                {
                    cli.Headers.Add("X-Fbx-App-Auth", sessTk);
                    BssConfigData wifiConfig = null;
                    string t = cli.DownloadString("v4/wifi/bss/");
                    var bss = JsonConvert.DeserializeObject<BssConfigResponse>(t);
                    wifiConfig = bss?.result.FirstOrDefault().config;
                    if (wifiConfig == null)
                        return null;

                    var qr = GetWifiQr(wifiConfig.ssid, wifiConfig.key, size);
                    return qr;
                }
                catch
                {
                    return null;

                }
            }
        }
    }
}
