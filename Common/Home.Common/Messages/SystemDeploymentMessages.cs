using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Messages
{
    public enum SystemDeploymentAction
    {
        Restart,
        DeployGeneric,
        DeployAgent,
        DeployWebApp,
    }
    public class SystemDeploymentMessage : BaseMessage
    {
        public const string TopicName = "gaia.deployments";

        public SystemDeploymentMessage() : base(TopicName)
        {
        }

        public string DeploymentName { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public SystemDeploymentAction Action { get; set; }

        public string SourceFileContent { get; set; }
    }
}
