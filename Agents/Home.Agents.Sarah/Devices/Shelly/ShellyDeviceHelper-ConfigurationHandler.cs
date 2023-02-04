using Home.Common;
using Home.Common.HomeAutomation;
using Home.Common.Messages;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Home.Common.Model;
using System.Threading;
using AdaptiveCards;

namespace Home.Agents.Sarah.Devices.Shelly
{
    partial class ShellyDeviceHelper
    {
        internal static IntegrationConfigurationResponse HandleConfigurationMessage(IntegrationConfigurationMessage message)
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 4));
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = "No configuration data required"
            });

            var tmp =  new IntegrationConfigurationResponse(message)
            {
                ConfigurationCard = card.ToJson(),
                ConfigurationCardFormat = "adaptivecard+json",
                IsFinalStep = true
            };

            tmp.Instance.IsSetup = true;
            return tmp;
        }

    }
}
