using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Messages
{
    public class IntegrationConfigurationMessage : BaseMessage
    {
        private static readonly string _messageFormat = "{0}.integration.configure";

        public IntegrationConfigurationMessage() : base("")
        {

        }

        public IntegrationConfigurationMessage(string agent, Integration integration, IntegrationInstance instance) : base("")
        {
            this.Topic = string.Format(_messageFormat, agent);
            Instance = instance;
            Integration = integration;
        }

        private string _agent = null;
        public string Agent
        {
            get
            {
                return this._agent;
            }

            set
            {
                this._agent = value;
                this.Topic = string.Format(_messageFormat, value);
            }
        }

        public Integration Integration { get; set; }
        public IntegrationInstance Instance { get; set; }
    }

    public class IntegrationConfigurationResponse : MessageResponse
    {
        public IntegrationConfigurationResponse() : base()
        {

        }

        public string ConfigurationCard { get; set; }
        public string ConfigurationCardFormat { get; set; }

        public bool IsFinalStep { get; set; }
    }
}
