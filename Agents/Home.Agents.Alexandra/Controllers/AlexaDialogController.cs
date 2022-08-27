using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Home.Agents.Alexandra.Controllers
{
    [Route("alexa")]
    [ApiController]
    public class AlexaDialogController : ControllerBase
    {
        public AlexaDialogController()
        {

        }

        [HttpPost]
        public SkillResponse HandleRequest([FromBody] SkillRequest input)
        {
            var requestType = input.GetRequestType();

            // return a welcome message
            if (requestType == typeof(LaunchRequest))
                return HandleLaunchRequest(input, input.Request as LaunchRequest);
         
            throw new NotImplementedException();
        }

        private SkillResponse HandleLaunchRequest(SkillRequest input, LaunchRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
