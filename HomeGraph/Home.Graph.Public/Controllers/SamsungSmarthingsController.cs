using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace Home.Graph.Public.Controllers
{
    [Route("hooks/samsung")]
    [ApiController]
    public class SamsungSmarthingsController : ControllerBase
    {
        private class LifeCycleBaseMessage
        {
            public string lifecycle { get; set; }
            public string executionId { get; set; }
            public string locale { get; set; }
            public string version { get; set; }
        }

        [Route("smarthings"), HttpPost]
        public IActionResult HandleMessage()
        {
            string body = "";
            using (var rqBdRdr = new StreamReader(this.Request.Body, Encoding.UTF8))
                body = rqBdRdr.ReadToEndAsync().Result;

            Console.WriteLine(body);

            var lifeMsg = JsonConvert.DeserializeObject<LifeCycleBaseMessage>(body);
            if (lifeMsg == null || string.IsNullOrEmpty(lifeMsg.lifecycle))
                return BadRequest();

            switch (lifeMsg.lifecycle.ToUpperInvariant())
            {
                case "CONFIRMATION":
                    return HandleConfirmation(body);
            }

            return BadRequest();
        }



        class LifeCycleConfirmationMessage : LifeCycleBaseMessage
        {
            public string appId { get; set; }
            public Confirmationdata confirmationData { get; set; }
            public Settings settings { get; set; }
        }

        class Confirmationdata
        {
            public string appId { get; set; }
            public string confirmationUrl { get; set; }
        }

        public class Settings
        {
        }

        class LifeCycleConfirmationResponse
        {
            public string targetUrl { get; set; }
        }


        private IActionResult HandleConfirmation(string body)
        {
            var msg = JsonConvert.DeserializeObject<LifeCycleConfirmationMessage>(body);
            string url = msg.confirmationData.confirmationUrl;

            using (var cli = new WebClient())
                cli.DownloadString(url);

            return new OkObjectResult(new LifeCycleConfirmationResponse()
            {
                targetUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}"
            });
        }
    }
}
