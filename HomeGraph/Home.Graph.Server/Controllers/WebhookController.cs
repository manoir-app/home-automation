using Home.Common.Model;
using Home.Graph.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Home.Graph.Server.Controllers
{
    [Route("v1.0/webhooks")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        [Route("raise/{hookid}"), HttpGet, AllowAnonymous]
        public bool RaiseEventFromWebhook(string hookid, string source=null)
        {
            var coll = MongoDbHelper.GetClient<Trigger>();

            var tmp = coll.Find(x => x.Id == hookid && x.Kind == TriggerKind.Webhook).FirstOrDefault();
            if(tmp!=null)
            {
                DateTimeOffset runDate = DateTimeOffset.Now;
                coll.UpdateOne(x => x.Id == hookid, Builders<Trigger>.Update
                    .Set("LatestOccurence", runDate));

                MqttHelper.PublishTriggerActivated(hookid, runDate, tmp.ToString());

                foreach (var t in tmp.RaisedMessages)
                {
                    // on raise un event correspondant au webhook
                    if (!string.IsNullOrEmpty(t.MessageTopic))
                    {
                        MessagingHelper.PushToLocalAgent(t.MessageTopic,
                            tmp.ReplaceContent(t.MessageContent, source));
                    }
                }

                return true;
            }

            return false;

        }


    }
}
