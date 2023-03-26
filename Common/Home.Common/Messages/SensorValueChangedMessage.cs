using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Messages
{
    public class SensorValueChangedMessage : BaseMessage
    {
        public SensorValueChangedMessage() : base(TopicTemperature)
        {

        }

        public SensorValueChangedMessage(string topic, string itemType, string itemId, decimal measure)
            :base(topic)
        {
            ItemType = itemType;
            ItemId = itemId;
            Measure = measure;
        }


        public const string TopicTemperature = "home.measures.temperature";
        public const string TopicHumidity = "home.measures.humidity";
        public const string TopicPressure = "home.measures.pressure";
        public const string TopicOccupancy = "home.measures.occupancy";

        public const string ItemTypeDevice = "device";
        public const string ItemTypeRoom = "room";
        public const string ItemTypeLevel = "level";
        public const string ItemTypeMesh = "mesh";

        public string ItemType { get; set; }
        public string ItemId { get; set; }
        public decimal Measure { get; set; }
    }
}
