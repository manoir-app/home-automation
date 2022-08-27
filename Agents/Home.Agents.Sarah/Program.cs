using Home.Agents.Sarah.Devices.ZipatoBox;
using Home.Common;
using Home.Common.Model;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Home.Common.Messages;
using Home.Graph.Common.Scripting;
using System.Diagnostics;

namespace Home.Agents.Sarah
{
    class Program
    {
        public static bool _stop = false;

        static void Main(string[] args)
        {
            Console.WriteLine("Starting Sarah");

#if DEBUG
            var t = ScriptingHelper.Execute(@"var t = Devices[""pouet""];
                return t.On;", "sarah");
#endif
            Console.WriteLine("Starting Sarah");
            string cn = Environment.GetEnvironmentVariable("APPCONFIG_CNSTRING");

            AgentHelper.SetupLocaleFromServer("sarah");
            AgentHelper.ReportStart("sarah", "home-automation");

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

            var ag = AgentHelper.GetAgent("sarah");
            if (ag == null || string.IsNullOrEmpty(ag.ConfigurationData))
                return;

            SarahIntegrationsProvider.InitIntegrations();

            var cfg = JsonConvert.DeserializeObject<SarahConfigurationData>(ag.ConfigurationData);

            // running scriptinghelper to load all assemblies :)
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var t = ScriptingHelper.Execute(@"var mesh = Mesh.Local; return mesh.PrivacyModeLabel;", "sarah");
            sw.Stop();
            Console.WriteLine($"Waked scripting module : privacy mode = {t} in {sw.ElapsedMilliseconds}ms");

            SarahMessageHandler.Start();
            Scenes.ScenesHelper.Start();
            TriggersChecker.Start();
            HomeAutomationCleanup.Start();
            DeviceManager.Start();

            Devices.NetworkDeviceHelper.Start(cfg);

            if (SupportDeviceFamily("shelly", cfg))
                Devices.Shelly.ShellyDeviceHelper.Start();
            if (SupportDeviceFamily("wled", cfg))
                Devices.Wled.WledHelper.Start();
            if (SupportDeviceFamily("hue", cfg))
                Devices.Hue.HueHelper.Start();
            if (SupportDeviceFamily("divoom", cfg))
                Devices.Divoom.DivoomDeviceHelper.Start();

            while (!_stop)
            {
                Thread.Sleep(500);
                AgentHelper.Ping("sarah");
            }
            Devices.NetworkDeviceHelper.Stop();
            Devices.Hue.HueHelper.Stop();
            Devices.Shelly.ShellyDeviceHelper.Stop();
            Devices.Wled.WledHelper.Stop();
            Devices.Divoom.DivoomDeviceHelper.Stop();
            DeviceManager.Stop();
            HomeAutomationCleanup.Stop();
            TriggersChecker.Stop();
            Scenes.ScenesHelper.Stop();
            SarahMessageHandler.Stop();
        }

        internal static MessageResponse ConfigureIntegration(IntegrationConfigurationMessage integrationConfigurationMessage)
        {
            if (integrationConfigurationMessage == null
                || integrationConfigurationMessage.Integration == null
                || integrationConfigurationMessage.Instance == null)
            {
                return MessageResponse.GenericFail;
            }

            switch(integrationConfigurationMessage.Integration.Id.ToLowerInvariant())
            {
                case "sarah.shelly.local":
                    return Devices.Shelly.ShellyDeviceHelper.HandleConfigurationMessage(integrationConfigurationMessage);
            }

            return MessageResponse.GenericFail;
        }

        internal static bool SupportDeviceFamily(string family, SarahConfigurationData data)
        {
            if (data == null)
                return true;

            if (data.DeviceFamilies == null)
                return true;

            return data.DeviceFamilies.Contains(family);
        }

        internal static string GetPassword()
        {
            var passwordToGet = Environment.GetEnvironmentVariable("HOMEAUTOMATION_APIKEY");
            return passwordToGet;
        }



        

    }
}
