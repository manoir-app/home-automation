using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Messages
{
    public class HomeAutomationPlatformCommandMessage : BaseMessage
    {
        public List<HomeAutomationPlatformDirectCommand> Operations { get; set; }

        public HomeAutomationPlatformCommandMessage() : base("")
        {
            Operations = new List<HomeAutomationPlatformDirectCommand>();
        }

        public HomeAutomationPlatformCommandMessage(string topic) : base(topic)
        {
            Operations = new List<HomeAutomationPlatformDirectCommand>();
        }

        public HomeAutomationPlatformCommandMessage(string topic, string deviceName, string command, string data) : this(topic)
        {
            if (command != null)
            {
                HomeAutomationPlatformDirectCommand ope = new HomeAutomationPlatformDirectCommand()
                {
                    DeviceName = deviceName,
                    Command = command,
                    Data = data
                };
                Operations.Add(ope);
            }
        }

        public HomeAutomationPlatformCommandMessage(string topic, HomeAutomationPlatformDirectCommand operation) : this(topic)
        {
            if (operation != null)
                Operations.Add(operation);
        }
    }

    public class HomeAutomationPlatformDirectCommand
    {
        public string DeviceName { get; set; }
        public string Command { get; set; }
        public string Data { get; set; }
    }

    public class HomeAutomationPlatformDirectCommandMessageResponse : MessageResponse
    {
        public List<HomeAutomationPlatformDirectCommand> SucceededOperations { get; set; }
        public List<HomeAutomationPlatformDirectCommand> FailedOperations { get; set; }

        public HomeAutomationPlatformDirectCommandMessageResponse() : base()
        {
            this.Response = "ok";
            this.SucceededOperations = new List<HomeAutomationPlatformDirectCommand>();
            this.FailedOperations = new List<HomeAutomationPlatformDirectCommand>();
        }
    }

}
