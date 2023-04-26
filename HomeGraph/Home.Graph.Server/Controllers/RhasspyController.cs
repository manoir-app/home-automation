using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;

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
            public Slots slots { get; set; }
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
        }

        public class RhasspyIntent
        {
            public string name { get; set; }
            public int confidence { get; set; }
        }

        public class Slots
        {
        }


        [Route("{id}/intent/handle"), HttpPost]
        public IActionResult HandleIntent(string id, [FromBody] RhasspyIntentRequest request)
        {
            // vérifier l'id

            if (request == null)
                return new BadRequestResult();

            Console.WriteLine("RHASSPY : " + JsonConvert.SerializeObject(request));

            return new OkObjectResult(request);
        }

    }
}
