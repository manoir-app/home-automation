using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Messages
{
    public class HomeAutomationMessage : BaseMessage
    {
        public List<HomeAutomationMessageOperation> Operations { get; set; }

        public HomeAutomationMessage() : base("homeautomation.global")
        {
            Operations = new List<HomeAutomationMessageOperation>();
        }

        public HomeAutomationMessage(string topic) : base(topic)
        {
            Operations = new List<HomeAutomationMessageOperation>();
        }

        public HomeAutomationMessage(string topic, string deviceName, string role, string elementName, string value) : this(topic)
        {
            if (role != null && elementName != null && value != null)
            {
                HomeAutomationMessageOperation ope = new HomeAutomationMessageOperation()
                {
                    ElementName = elementName,
                    DeviceName = deviceName,
                    Role = role,
                    Value = value
                };
                Operations.Add(ope);
            }
        }

        public HomeAutomationMessage(string topic, HomeAutomationMessageOperation operation) : this(topic)
        {
            if (operation != null)
                Operations.Add(operation);
        }
    }

    public class HomeAutomationMessageOperation
    {
        public string Role { get; set; }
        public string DeviceName { get; set; }
        public string InstanceId { get; set; }
        public string ElementName { get; set; }
        public string Value { get; set; }
    }

    public class HomeAutomationMessageResponse : MessageResponse
    {
        public List<HomeAutomationMessageOperation> SucceededOperations { get; set; }
        public List<HomeAutomationMessageOperation> FailedOperations { get; set; }

        public HomeAutomationMessageResponse() : base()
        {
            this.Response = "ok";
            this.SucceededOperations = new List<HomeAutomationMessageOperation>();
            this.FailedOperations = new List<HomeAutomationMessageOperation>();
        }
    }

}
