using System;
using System.Collections.Generic;

namespace Home.Common.Model
{
    public enum TimeOffsetKind
    {
        FromMidnight = 0,
        FromSunrise = 1,
        FromSunset = 2,
        FromEarliestWakeup = 3,
        FromLatestWakeup = 4

    }

    public enum TriggerKind
    {
        Clock = 0,
        //Timer = 1,

        NetworkDeviceConnectionChanged = 8,

        Webhook = 15,
        MqttValue = 16,
    }

    public enum NetworkDeviceTriggerKind
    {
        Connection,
        Disconnection
    }

    public class Trigger
    {
        public Trigger()
        {
            RaisedMessages = new List<TriggerRaisedMessage>();
            ChangedProperties = new List<TriggerPropertyChange>();
        }

        public string Id { get; set; }
        public TriggerKind Kind { get; set; }

        public string Label { get; set; }

        public TimeSpan? Offset { get; set; }
        public TimeOffsetKind? OffsetKind { get; set; }

        public string NetworkDeviceName { get; set; }
        public NetworkDeviceTriggerKind? NetworkDeviceTriggerKind { get; set; }

        /// <summary>
        /// Le path de la propriété (pour MQTT par exemple)
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Si renseigné, la différence minimale pour
        /// que l'on mette à jour (pour éviter de faire
        /// des updates en permanence pour un détecteur
        /// un peu trop sensible)
        /// </summary>
        public decimal? ThredsholdForChange { get; set; }

        public List<TriggerRaisedMessage> RaisedMessages { get; set; }

        public List<TriggerPropertyChange> ChangedProperties { get; set; } 


        public DateTimeOffset? LatestOccurence { get; set; }

        public string ReplaceContent(string messageContent, string source, string data = null)
        {
            messageContent = messageContent.Replace("{{source}}", source == null ? "(inconnu)" : source);
            messageContent = messageContent.Replace("{{date}}", DateTimeOffset.Now.ToString("d"));
            messageContent = messageContent.Replace("{{time}}", DateTimeOffset.Now.ToString("HH:mm:ss.ffff"));
            messageContent = messageContent.Replace("{{networkdevice}}", NetworkDeviceName);
            messageContent = messageContent.Replace("{{rawdata}}", data ?? "-NODATA-");

            return messageContent;
        }

        public override string ToString()
        {
            switch (this.Kind)
            {
                case TriggerKind.Clock:
                    switch (this.OffsetKind.GetValueOrDefault(TimeOffsetKind.FromMidnight))
                    {
                        case TimeOffsetKind.FromMidnight:
                            return $"A {this.Offset.GetValueOrDefault().ToString()}";
                        case TimeOffsetKind.FromSunrise:
                            return $"Après {this.Offset.GetValueOrDefault().ToString()} après le lever du soleil";
                        case TimeOffsetKind.FromSunset:
                            return $"Après {this.Offset.GetValueOrDefault().ToString()} après le coucher du soleil";
                    }
                    break;
                case TriggerKind.NetworkDeviceConnectionChanged:
                    return $"Sur connexion/deconnexion de {this.NetworkDeviceName}";
                case TriggerKind.Webhook:
                    return $"Webhook";
            }
            return "inconnu";
        }
    }

    public class TriggerRaisedMessage
    {
        public string MessageTopic { get; set; }
        public string MessageContent { get; set; }
    }

    public enum TriggerPropertyChangeKind
    {
        GenericProperty,
        RoomProperty
    }

    public class TriggerPropertyChange
    {
        public string PropertyName { get; set; }

        public string RoomId { get; set; }

        public TriggerPropertyChangeKind Kind { get; set; }
    }

}
