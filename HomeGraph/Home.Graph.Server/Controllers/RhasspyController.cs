using Home.Common.Messages;
using Home.Graph.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using static Home.Graph.Server.Controllers.RhasspyController;

namespace Home.Graph.Server.Controllers
{
    [Route("v1.0/voice/rhasspy")]
    [ApiController]
    public class RhasspyController : ControllerBase
    {
        public class RhasspyIntentRequest
        {
            public RhasspyIntent intent { get; set; }
            public object[] entities { get; set; }
            public Dictionary<string, string> slots { get; set; }
            public string text { get; set; }
            public string raw_text { get; set; }
            public string[] tokens { get; set; }
            public string[] raw_tokens { get; set; }
            public string wakeword_id { get; set; }
            public string siteId { get; set; }
            public string sessionId { get; set; }
            public string customData { get; set; }
            public string wakewordId { get; set; }
            public object lang { get; set; }

            public VoiceIntentSource ToIntentSource()
            {
                var ret = new VoiceIntentSource()
                {
                    Topic = intent?.name,
                    Tokens = tokens
                };

                if (slots != null)
                    ret.Datas = slots;

                return ret;
            }
        }

        public class RhasspyIntent
        {
            public string name { get; set; }
            public int confidence { get; set; }
        }

        public class RhassySpeechResponse : RhasspyIntentRequest
        {
            public RhassySpeechResponseData Speech { get; set; }
        }

        public class RhassySpeechResponseData
        {
            public string Text { get; set; }
        }


        [Route("{id}/intent/handle"), HttpPost]
        public IActionResult HandleIntent(string id, [FromBody] RhasspyIntentRequest request)
        {
            // vérifier l'id
            if (request == null)
                return new BadRequestResult();

            Console.WriteLine("RHASSPY : " + JsonConvert.SerializeObject(request));

            var parsed = VoiceIntentsHelper.Parse(request.ToIntentSource());

            if (parsed == null)
            {
                var resp = JsonConvert.DeserializeObject<RhassySpeechResponse>(JsonConvert.SerializeObject(request));
                resp.Speech = new RhassySpeechResponseData()
                {
                    Text = "Désolé, je ne sais pas encore faire cela"
                };
                return new OkObjectResult(resp);
            }

            return new OkObjectResult(request);
        }

    }
}
