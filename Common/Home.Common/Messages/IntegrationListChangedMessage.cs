using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Messages
{
    public class IntegrationListChangedMessage : BaseMessage
    {
        public const string ListChangedTopic = "system.integration.list.changed";

        public IntegrationListChangedMessage() : base(ListChangedTopic)
        {

        }
    }

    public class IntegrationInstancesListChangedMessage : BaseMessage
    {
        public const string InstancesChangedTopic = "system.integration.instances.changed";

        public IntegrationInstancesListChangedMessage() : base(InstancesChangedTopic)
        {

        }
    }
}
