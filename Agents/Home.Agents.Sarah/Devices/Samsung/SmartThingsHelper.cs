using Home.Common;
using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Home.Agents.Sarah.Devices.Samsung
{
    internal static class SmartThingsHelper
    {
        static ExternalToken _token = null;


        public static ExternalToken GetToken()
        {
#if DEBUG
            return new ExternalToken()
            {
                Token = Environment.GetEnvironmentVariable("SMARTHINGS_TOKEN")
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
                            var exts = cli.DownloadData<List<ExternalToken>>("/v1.0/security/tokens/system/smartthings");
                            if (exts != null && exts.Count >= 1)
                            {
                                _token = exts[0];
                                Console.WriteLine("Smarthings - token is : " + _token.Token);

                            }
                            else
                            {

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
