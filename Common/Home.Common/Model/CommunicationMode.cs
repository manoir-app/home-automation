using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Model
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CommunicationMode
    {
        RestApi
    }
}
