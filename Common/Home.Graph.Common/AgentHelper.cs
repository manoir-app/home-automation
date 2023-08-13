using Home.Common.Model;
using Home.Graph.Common;
using k8s;
using Newtonsoft.Json;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Threading;

namespace Home.Common
{
    public static class AgentHelper
    {

        public static TimeZoneInfo UserTimeZone { get; set; } = TimeZoneInfo.Local;

        public static void SetMeshPrivacyLevel(string agentName, AutomationMeshPrivacyMode? mode)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient(agentName))
                    {
                        if (mode.HasValue)
                            cli.DownloadData<bool>("v1.0/system/mesh/local/privacymode/set?privacyMode=" + mode.Value.ToString());
                        else
                            cli.DownloadData<bool>("v1.0/system/mesh/local/privacymode/clear");
                        break;
                    }
                }
                catch (WebException ex)
                {
                    Console.WriteLine("Failed to register : " + ex.Message);
                    Thread.Sleep(1000);
                }
            }
        }

        public static void SetupLocaleFromServer(string agentName)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient(agentName))
                    {
                        var t = cli.DownloadData<AutomationMesh>("v1.0/system/mesh/local/");

                        if (t != null)
                        {
                            if (!string.IsNullOrEmpty(t.LanguageId))
                            {
                                try
                                {
                                    var culture = new CultureInfo(t.LanguageId);
                                    if (culture != null)
                                    {
                                        Console.WriteLine("Culture : " + t.LanguageId);
                                        CultureInfo.DefaultThreadCurrentCulture = culture;
                                        Thread.CurrentThread.CurrentCulture = culture;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("Impossible de définir la culture : " + t.LanguageId + " : " + ex.ToString());
                                }
                            }

                            if (!string.IsNullOrEmpty(t.TimeZoneId))
                            {
                                try
                                {
                                    var tz = TimeZoneInfo.FindSystemTimeZoneById(t.TimeZoneId);
                                    if (tz != null)
                                    {
                                        Console.WriteLine("Timezone : " + t.TimeZoneId);
                                        UserTimeZone = tz;
                                    }
                                }
                                catch
                                {

                                }
                            }
                        }
                        break;
                    }
                }
                catch (WebException ex)
                {
                    Console.WriteLine("Failed to register : " + ex.Message);
                    Thread.Sleep(1000);
                }
            }
        }


        public static Integration GetIntegration(string agentName, string integrationId)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient(agentName))
                    {
                        var ret = cli.DownloadData<Integration>(
                            $"v1.0/system/mesh/local/integrations/{integrationId}");
                        return ret;
                    }
                }
                catch (WebException)
                {
                    Thread.Sleep(1000);
                }
            }
            return null;
        }

        public static List<Integration> GetMyIntegrations(string agentName, bool onlyActives)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient(agentName))
                    {
                        var ret = cli.DownloadData<List<Integration>>(
                            $"v1.0/system/mesh/local/integrations/byagent/{agentName}?onlyActives={onlyActives}");
                        return ret;
                    }
                }
                catch (WebException)
                {
                    Thread.Sleep(1000);
                }
            }
            return null;
        }

        public static Integration PushIntegration(string agentName, Integration integration)
        {
            integration.AgentId = agentName;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient(agentName))
                    {
                        var ret = cli.UploadData<Integration, Integration>(
                            $"v1.0/system/mesh/local/integrations", "POST", integration);
                        return ret;
                    }
                }
                catch (WebException)
                {
                    Thread.Sleep(1000);
                }
            }
            return null;
        }


        private static DateTimeOffset _lastPing = DateTimeOffset.MinValue;
        public static void Ping(string agentName)
        {
            if (Math.Abs((DateTimeOffset.Now - _lastPing).TotalSeconds) < 30)
                return;

            TimeDBHelper.Trace("system", "agents", "active", 1, new Dictionary<string, string>()
            {
                {"agentId", agentName }
            });

            MqttHelper.PublishAgentStatus(agentName, null, DateTime.Now);

            _lastPing = DateTimeOffset.Now;

            try
            {
                using (var cli = new MainApiAgentWebClient(agentName))
                {
                    string ret = cli.DownloadString($"v1.0/agents/{agentName}/ping");
                    return;
                }
            }
            catch (WebException ex)
            {
                Console.WriteLine("Failed to ping : " + ex.Message);
            }
        }

        public static void ReportStart(string agentName, params string[] agentRoles)
        {
            var reg = new AgentRegistration()
            {
                AgentMachineName = Environment.MachineName,
                AgentName = agentName,
                Roles = agentRoles
            };

            var kub = Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_HOST");
            if (!string.IsNullOrEmpty(kub))
                reg.AgentMachineName = "kubernetes";

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient(agentName))
                    {
                        Agent t = cli.UploadData<Agent, AgentRegistration>("v1.0/system/mesh/local/agents/register", "POST", reg);
                        Console.Write("Registred");
                        Console.Write(JsonConvert.SerializeObject(t));
                        break;
                    }
                }
                catch (WebException ex)
                {
                    Console.WriteLine("Failed to register : " + ex.Message);
                    Thread.Sleep(1000);
                }
            }
            LogHelper.Log("agent", agentName, "Agent started : " + agentName);
        }

        public static Agent GetAgent(string agentName)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient(agentName))
                    {
                        var ret = cli.DownloadData<Agent>($"v1.0/agents/{agentName}");
                        return ret;
                    }
                }
                catch (WebException)
                {
                    Thread.Sleep(1000);
                }
            }
            return null;
        }


        public static void UpdateConfig<T>(string agentName, T configData)
        {
            UpdateConfig(agentName, JsonConvert.SerializeObject(configData));
        }

        public static void UpdateConfig(string agentName, string configData)
        {

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient(agentName))
                    {
                        string ret = cli.UploadString($"v1.0/agents/{agentName}/configuration", "POST", JsonConvert.SerializeObject(configData));
                        Console.Write("Config updated");
                        return;
                    }
                }
                catch (WebException ex)
                {
                    Console.WriteLine("Failed to update config : " + ex.Message);
                    Thread.Sleep(1000);
                }
            }

        }

        public static void UpdateStatusWithInfo(string agentName, string status)
        {
            Console.WriteLine($"Update status for {agentName} : {status}");

            ThreadPool.QueueUserWorkItem(o =>
            {
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        using (var cli = new MainApiAgentWebClient(agentName))
                        {
                            string ret = cli.UploadString($"v1.0/agents/{agentName}/status", "POST", JsonConvert.SerializeObject(status));
                            return;
                        }
                    }
                    catch (WebException ex)
                    {
                        Console.WriteLine("Failed to update status : " + ex.Message);
                    }

                    Thread.Sleep(1000);
                }
            });
        }

        public static Location GetLocalMeshLocation(string agentId)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient(agentId))
                    {
                        var tmp = cli.DownloadData<Location>("v1.0/system/mesh/local/location");
                        if (tmp != null)
                            return tmp;
                    }
                }
                catch (WebException ex)
                {
                    Thread.Sleep(1000);
                }
            }
            return null;
        }

        public static AutomationMesh GetLocalMesh(string agentId)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient(agentId))
                    {
                        var tmp = cli.DownloadData<AutomationMesh>("v1.0/system/mesh/local");
                        if (tmp != null)
                            return tmp;
                    }
                }
                catch (WebException ex)
                {
                    Thread.Sleep(1000);
                }
            }
            return null;
        }

        public static List<User> GetLocalPresentUsers(string agentId)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient(agentId))
                    {
                        return cli.DownloadData<List<User>>("v1.0/users/presence/mesh/local/all");
                    }
                }
                catch (Exception ex)
                {

                }
            }

            return new List<User>();
        }
        public static User GetUser(string agentId, string userId)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient(agentId))
                    {
                        var t = cli.DownloadData<User>($"v1.0/users/all/{userId}");
                        return t;
                    }
                }
                catch (WebException ex)
                {
                    Thread.Sleep(1000);
                }
            }
            return null;
        }

        public static ExternalToken GetUserExternalToken(string agentId, string username, string platform)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient(agentId))
                    {
                        var exts = cli.DownloadData<List<ExternalToken>>($"/v1.0/security/tokens/{username}/{platform}");
                        if (exts != null && exts.Count >= 1)
                            return exts[0];
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

            return null;
        }

        public static List<User> GetMainUsers(string agentId)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient(agentId))
                    {
                        var t = cli.DownloadData<List<User>>($"v1.0/users/main");
                        return t;
                    }
                }
                catch (WebException ex)
                {
                    Thread.Sleep(1000);
                }
            }
            return null;
        }

        public static void WriteStartupMessage(string agentName, Assembly assembly)
        {
            Console.WriteLine($"Starting {agentName}");

            string cn = Environment.GetEnvironmentVariable("APPCONFIG_CNSTRING");

            if (cn != null)
            {
                Console.WriteLine("with config = " + cn);
                ConfigurationSettingsHelper.Init(cn);
            }
            else
                Console.WriteLine("without config");
            Console.WriteLine("-----------------------");
            var envs = Environment.GetEnvironmentVariables();
            Console.WriteLine("Env vars : ");
            foreach (var k in envs.Keys)
            {
                Console.Write(k.ToString().PadRight(35, ' '));
                Console.Write(" : ");
                Console.WriteLine(envs[k].ToString());
            }
            Console.WriteLine("-----------------------");

            try
            {
                var config = KubernetesClientConfiguration.InClusterConfig();
                Console.WriteLine($"host for k8s configuration : {config.Host}");
                Console.WriteLine("-----------------------");

            }
            catch
            {

            }

            try
            {
                DateTime dt = System.IO.File.GetLastWriteTime(assembly.Location);
                Console.WriteLine($"Build date : {dt.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")}");
                Console.WriteLine("-----------------------");
            }
            catch
            {

            }
        }

        private static string _agentName = null;

        public static void SetupReporting(string agentName)
        {
            _agentName = agentName;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exc = e.ExceptionObject as Exception;
            if (exc != null)
            {
                LogHelper.LogException("agent", _agentName, exc);
            }
            else
                LogHelper.LogException("agent", _agentName, new ApplicationException("Crash"));
        }

       
    }
}
