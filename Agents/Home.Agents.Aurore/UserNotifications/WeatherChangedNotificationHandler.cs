using Home.Common.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Home.Agents.Aurore.UserNotifications
{
    internal class WeatherChangedNotificationHandler
    {
        public static MessageResponse HandleMessage(MessageOrigin origin, string topic, string messageBody)
        {
            if (!topic.Equals(WeatherChangeMessage.TopicName))
                return MessageResponse.GenericFail;

            var data = JsonConvert.DeserializeObject<WeatherChangeMessage>(messageBody);
            if (data != null && data.CurrentWeather != null)
            {
                JournalHelper.UpdateProperties("https://manoir.app/agents/aurore#weather", (upd) =>
                {
                    if (upd.Section.Properties == null)
                        upd.Section.Properties = new Dictionary<string, string>();

                    //upd.Section.Data = $"Méteo : {data.CurrentWeather.Label} : {data.CurrentWeather.Temperature}°C";

                    upd.Section.Properties["WEATHER"] = data.CurrentWeather.Label;
                    upd.Section.Properties["TEMPERATURE"] = $"{data.CurrentWeather.Temperature}°C";

                    return true;
                });
            }
            return MessageResponse.OK;
        }
    }
}
