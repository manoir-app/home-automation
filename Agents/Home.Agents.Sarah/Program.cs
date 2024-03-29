﻿using Home.Agents.Sarah.Devices.ZipatoBox;
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
using Home.Graph.Common;
using Home.Agents.Sarah.Devices.Zigbee2Mqtt;

namespace Home.Agents.Sarah
{
    class Program
    {
        public static bool _stop = false;

        static void Main(string[] args)
        {

            AgentHelper.WriteStartupMessage("Sarah", typeof(Program).Assembly);
#if DEBUG
            //MqttHelper.Start("agents-sarah");
            //Z2MqttHelper.Start();

            //Condition cond = new Condition()
            //{
            //    Kind = ConditionKind.And,
            //    SubConditions = new Condition[] {
            //        new Condition()
            //        {
            //            PropertyName = "IsPresent",
            //            ElementId = "mwaroquier",
            //            Kind = ConditionKind.UserCheck,
            //            Operator = "!=",
            //            Value = "true"
            //        },
            //        new Condition()
            //        {
            //            PropertyName = "IsPresent",
            //            ElementId = "mcarbenay",
            //            Kind = ConditionKind.UserCheck,
            //            Operator = "!=",
            //            Value = "true"
            //        }
            //    }
            //};
            //Console.WriteLine(cond);

            //Console.WriteLine("result = " + ConditionHelper.GetForAgent("sarah").Evaluate(cond));

            TriggersChecker.RefreshRoutines();
            TriggersChecker.Run();

            Console.ReadLine();
            return;
#endif

            AgentHelper.SetupReporting("sarah");
            AgentHelper.SetupLocaleFromServer("sarah");
            AgentHelper.ReportStart("sarah", "home-automation");

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
            MqttHelper.Start("agents-sarah");
            Devices.NetworkDeviceHelper.Start(cfg);

            if (SupportDeviceFamily("shelly", cfg))
                Devices.Shelly.ShellyDeviceHelper.Start();
            if (SupportDeviceFamily("wled", cfg))
                Devices.Wled.WledHelper.Start();
            if (SupportDeviceFamily("hue", cfg))
                Devices.Hue.HueHelper.Start();
            if (SupportDeviceFamily("divoom", cfg))
                Devices.Divoom.DivoomDeviceHelper.Start();
            if (SupportDeviceFamily("zigbee2mqtt", cfg))
                Devices.Zigbee2Mqtt.Z2MqttHelper.Start();




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
            Devices.Zigbee2Mqtt.Z2MqttHelper.Stop();
            MqttHelper.Stop();
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

            switch (integrationConfigurationMessage.Integration.Id.ToLowerInvariant())
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

    }
}
