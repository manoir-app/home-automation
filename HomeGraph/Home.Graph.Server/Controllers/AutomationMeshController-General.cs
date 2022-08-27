using Home.Common;
using Home.Common.Messages;
using Home.Common.Model;
using Home.Graph.Common;
using Home.Graph.Server.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Home.Graph.Server.Controllers
{
    partial class AutomationMeshController
    {
        [Route("{name}/location"), HttpGet]
        public Location GetLocation(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            name = name.ToLowerInvariant();

            var collMesh = MongoDbHelper.GetClient<AutomationMesh>();
            var lst = collMesh.Find(x => x.Id == name).FirstOrDefault();

            if (lst == null || string.IsNullOrEmpty(lst.LocationId))
                return null;

            var collection = MongoDbHelper.GetClient<Location>();
            var loca = collection.Find(x => x.Id == lst.LocationId).FirstOrDefault();
            return loca;
        }

        public class MeshSettings
        {
            public string LanguageId { get; set; }
            public string TimeZoneId { get; set; }
        }

        public class SettingsValue
        {
            public string Id { get; set; }
            public string Label { get; set; }
        }

        [Route("settings/available/language"), HttpGet]
        public List<SettingsValue> GetLanguages()
        {
            var lst = new List<SettingsValue>();
            lst.Add(new SettingsValue() { Id = "fr-FR", Label = "Français (France)" });
            lst.Add(new SettingsValue() { Id = "en-US", Label = "English (US)" });
            return lst;
        }

        [Route("settings/available/timezone"), HttpGet]
        public List<SettingsValue> GetTimeZones()
        {
            var lst = new List<SettingsValue>();

            var t = new MeshInfoMessage();
            var ret = MessagingHelper.RequestFromLocalAgent<MeshInfoMessageResponse>(t);
            if (ret != null)
            {
                foreach (string k in ret.AvailableTimeZones.Keys)
                {
                    lst.Add(new SettingsValue() { Id = k, Label = ret.AvailableTimeZones[k] });
                }
            }

            return lst;
        }


        [Route("{name}/settings"), HttpPost]
        public bool SetSettings(string name, [FromBody] MeshSettings settings)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            name = name.ToLowerInvariant();

            var collMesh = MongoDbHelper.GetClient<AutomationMesh>();
            var lst = collMesh.Find(x => x.Id == name).FirstOrDefault();


            var ret = collMesh.UpdateOne(x => x.Id == lst.Id,
                Builders<AutomationMesh>.Update
                .Set("LanguageId", settings.LanguageId)
                .Set("TimeZoneId", settings.TimeZoneId));

            if (ret.IsAcknowledged && ret.ModifiedCount == 1)
            {
                var collAgt = MongoDbHelper.GetClient<Agent>();
                var allAgents = collAgt.Find(x => x.AgentMesh == "local").ToList();

                foreach (var t in allAgents)
                {
                    if (t.AgentName.Equals("gaia", StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    string service = "agents-" + t.AgentName.ToLowerInvariant();
                    SystemDeploymentMessage msg = new SystemDeploymentMessage()
                    {
                        Action = SystemDeploymentAction.Restart,
                        DeploymentName = service
                    };

                    MessagingHelper.PushToLocalAgent(msg);
                }

                return true;
            }

            return false;
        }

        [Route("{name}/location"), HttpPost]
        public bool SetLocation(string name, [FromBody] Location location)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            if (location == null || location.Id == null)
                return false;

            name = name.ToLowerInvariant();

            var collMesh = MongoDbHelper.GetClient<AutomationMesh>();
            var lst = collMesh.Find(x => x.Id == name).FirstOrDefault();

            var collection = MongoDbHelper.GetClient<Location>();
            var loca = collection.Find(x => x.Id == location.Id).FirstOrDefault();

            if (lst == null || loca == null)
                return false;

            var ret = collMesh.UpdateOne(x => x.Id == lst.Id,
                Builders<AutomationMesh>.Update
                .Set("LocationId", location.Id));

            return ret.IsAcknowledged && ret.ModifiedCount == 1;
        }

        [Route("{name}/location/set/{locationId}"), HttpGet]
        public bool SetLocation(string name, string locationId)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            if (locationId == null)
                return false;

            name = name.ToLowerInvariant();

            var collection = MongoDbHelper.GetClient<Location>();
            var loca = collection.Find(x => x.Id == locationId).FirstOrDefault();

            if (loca == null)
                return false;

            return SetLocation(name, loca);
        }




        [Route("local/graph/version"), HttpGet(), AllowAnonymous()]
        public string GetVersion()
        {
            var asm = typeof(AutomationMeshController).Assembly;
            var pth = new FileInfo(asm.Location);
            return pth.LastWriteTime.ToString("yy.MMdd.HHmm");
        }

        [Route("local/graph/check"), HttpGet(), AllowAnonymous()]
        public string CheckIfExists()
        {
            return "dev.carbenay.info:home-automation:graph";
        }


        [Route("local/privacymode/set"), HttpGet]
        public bool SetPrivacyMode(AutomationMeshPrivacyMode privacyMode = AutomationMeshPrivacyMode.High)
        {

            var collMesh = MongoDbHelper.GetClient<AutomationMesh>();

            var ret = collMesh.UpdateOne(x => x.Id == "local",
                    Builders<AutomationMesh>.Update
                    .Set("CurrentPrivacyMode", privacyMode));
            MqttHelper.PublishMeshProperty("privacyMode", privacyMode.ToString());

            if (ret.IsAcknowledged && ret.ModifiedCount == 1)
            {
                var mesh = Get("local");
                
                LogHelper.Log("mesh", "home-automation", $"Privacy mode switched to {privacyMode.ToString()}");

                MessagingHelper.PushToLocalAgent(new AgentGenericMessage("system.mesh.privacymode.changed"));
                AppAndDeviceHub.SendMeshChange(_appContext, "privacyMode", mesh);

            }

            return true;
        }

        [Route("local/privacymode/clear"), HttpGet]
        public bool ClearPrivacyMode()
        {
            var collMesh = MongoDbHelper.GetClient<AutomationMesh>();

            var ret = collMesh.UpdateOne(x => x.Id == "local",
                    Builders<AutomationMesh>.Update
                    .Set<AutomationMeshPrivacyMode?>("CurrentPrivacyMode", null));

            MqttHelper.PublishMeshProperty("privacyMode", "none");

            if (ret.IsAcknowledged && ret.ModifiedCount == 1)
            {
                var mesh = Get("local");

                LogHelper.Log("mesh", "home-automation", $"Privacy mode switched to none");

                MessagingHelper.PushToLocalAgent(new AgentGenericMessage("system.mesh.privacymode.changed"));
                AppAndDeviceHub.SendMeshChange(_appContext, "privacyMode", mesh);
            }

            return true;
        }

        [Route("local/privacymode/isenabled"), HttpGet]
        public bool GetPrivacyModeEnabled()
        {
            var collMesh = MongoDbHelper.GetClient<AutomationMesh>();
            var mesh = Get("local");
            return mesh.CurrentPrivacyMode.HasValue;
        }


        [Route("local/globalscenario/all"), HttpGet]
        public IEnumerable<AutomationMeshGlobalScenario> GetGlobalScenarios()
        {
            var collMesh = MongoDbHelper.GetClient<AutomationMesh>();
            var mesh = Get("local");
            return mesh.Scenarios;
        }

        [Route("local/globalscenario/all"), HttpPost]
        public IEnumerable<AutomationMeshGlobalScenario> UpsetGlobalScenarios([FromBody] AutomationMeshGlobalScenario scenario)
        {
            if (scenario.Code == null)
                BadRequest("code is mandatory on a scenario");

            scenario.Code = scenario.Code.ToLowerInvariant();

            var collMesh = MongoDbHelper.GetClient<AutomationMesh>();
            var mesh = Get("local");
            if (mesh.Scenarios == null)
                mesh.Scenarios = new List<AutomationMeshGlobalScenario>();
            var t = (from z in mesh.Scenarios
                     where z.Code.Equals(scenario.Code)
                     select z).FirstOrDefault();
            if (t != null)
            {
                if (t.Images != null && t.Images.Count > 0 && scenario.Images == null)
                    scenario.Images = t.Images;
                mesh.Scenarios.Remove(t);
            }
            mesh.Scenarios.Add(scenario);

            collMesh.UpdateOne(x => x.Id == "local",
            Builders<AutomationMesh>.Update.Set("Scenarios", mesh.Scenarios));


            return mesh.Scenarios;
        }

        [Route("local/globalscenario/all/{code}"), HttpDelete]
        public IEnumerable<AutomationMeshGlobalScenario> DeleteGlobalScenarios(string code)
        {
            if (code == null)
                BadRequest("code is mandatory on a scenario");

            code = code.ToLowerInvariant();

            var collMesh = MongoDbHelper.GetClient<AutomationMesh>();
            var mesh = Get("local");
            if (mesh.Scenarios == null)
                mesh.Scenarios = new List<AutomationMeshGlobalScenario>();
            var t = (from z in mesh.Scenarios
                     where z.Code.Equals(code)
                     select z).FirstOrDefault();
            if (t != null)
                mesh.Scenarios.Remove(t);
            collMesh.UpdateOne(x => x.Id == "local",
            Builders<AutomationMesh>.Update.Set("Scenarios", mesh.Scenarios));


            return mesh.Scenarios;
        }

        [Route("local/globalscenario/all/{code}/images/{imagecode}"), HttpDelete]
        public AutomationMeshGlobalScenario DeleteImage(string code, string imagecode)
        {
            code = code.ToLowerInvariant();
            var mesh = Get("local");
            if (mesh.Scenarios == null)
                mesh.Scenarios = new List<AutomationMeshGlobalScenario>();
            var t = (from z in mesh.Scenarios
                     where z.Code.Equals(code)
                     select z).FirstOrDefault();

            if (t != null)
            {
                var pth = FileCacheHelper.GetLocalFilename("home-automation", $"images/scenarios/{code}", imagecode + ".png");

                if (System.IO.File.Exists(pth))
                    System.IO.File.Delete(pth);

                if (t.Images.ContainsKey(imagecode))
                    t.Images.Remove(imagecode);

                var collMesh = MongoDbHelper.GetClient<AutomationMesh>();
                collMesh.UpdateOne(x => x.Id == "local",
                     Builders<AutomationMesh>.Update.Set("Scenarios", mesh.Scenarios));
            }

            return t;
        }


        [Route("local/globalscenario/all/{code}/images/{imagecode}"), HttpPost]
        public AutomationMeshGlobalScenario UpsertScenarioImage(string code, string imagecode)
        {
            code = code.ToLowerInvariant();
            var mesh = Get("local");
            if (mesh.Scenarios == null)
                mesh.Scenarios = new List<AutomationMeshGlobalScenario>();
            var t = (from z in mesh.Scenarios
                     where z.Code.Equals(code)
                     select z).FirstOrDefault();

            if (t != null)
            {
                var pth = FileCacheHelper.GetLocalFilename("home-automation", $"images/scenarios/{code}", imagecode + ".dat");
                using (var st = System.IO.File.Create(pth))
                    this.HttpContext.Request.BodyReader.CopyToAsync(st).Wait();

                try
                {
                    var fmt = Image.DetectFormat(pth);
                    if (fmt.DefaultMimeType.StartsWith("image/"))
                    {
                        switch (fmt.DefaultMimeType)
                        {
                            case "image/png":
                                System.IO.File.Move(pth, System.IO.Path.ChangeExtension(pth, ".png"));
                                pth = System.IO.Path.ChangeExtension(pth, ".png");
                                break;
                            default:
                                var img = Image.Load(pth);
                                img.SaveAsPng(System.IO.Path.ChangeExtension(pth, ".png"));
                                System.IO.File.Delete(pth);
                                pth = System.IO.Path.ChangeExtension(pth, ".png");
                                break;
                        }
                    }
                    else
                    {
                        System.IO.File.Delete(pth);
                        BadRequest("Invalid file format");
                    }
                }
                catch (Exception ex)
                {
                    if (System.IO.File.Exists(pth))
                        System.IO.File.Delete(pth);
                    Console.WriteLine(ex);
                    BadRequest("Invalid file format : " + ex.Message);
                }

                string pubUrl = $"https://public.anzin.carbenay.manoir.app/v1.0/services/files/home-automation/images/scenarios/{code}/{imagecode}.png"; ;
                if (t.Images.ContainsKey(imagecode))
                    t.Images[imagecode] = pubUrl;
                else
                    t.Images.Add(imagecode, pubUrl);

                var collMesh = MongoDbHelper.GetClient<AutomationMesh>();
                collMesh.UpdateOne(x => x.Id == "local",
                     Builders<AutomationMesh>.Update.Set("Scenarios", mesh.Scenarios));
            }

            return t;

        }

        [Route("local/globalscenario/all/{code}"), HttpGet]
        public AutomationMeshGlobalScenario GetGlobalScenarios(string code)
        {
            if (code == null)
                BadRequest("code is mandatory on a scenario");

            code = code.ToLowerInvariant();

            var collMesh = MongoDbHelper.GetClient<AutomationMesh>();
            var mesh = Get("local");
            if (mesh.Scenarios == null)
                mesh.Scenarios = new List<AutomationMeshGlobalScenario>();
            var t = (from z in mesh.Scenarios
                     where z.Code.Equals(code)
                     select z).FirstOrDefault();
            return t;
        }

        [Route("local/globalscenario/current"), HttpGet]
        public string GetGlobalScenario()
        {
            var collMesh = MongoDbHelper.GetClient<AutomationMesh>();
            var mesh = Get("local");
            return mesh.CurrentScenario;
        }

        [Route("local/globalscenario/current"), HttpPost]
        public AutomationMesh SetGlobalScenario([FromBody] string scenario)
        {
            var collMesh = MongoDbHelper.GetClient<AutomationMesh>();
            var mesh = Get("local");

            if (scenario != mesh.CurrentScenario)
            {
                var t = (from z in mesh.Scenarios
                         where z.Code.Equals(scenario, StringComparison.InvariantCultureIgnoreCase)
                         select z).FirstOrDefault();
                if (t == null)
                    NotFound();
                else
                {
                    mesh.CurrentScenario = scenario;
                    var upd = collMesh.UpdateOne(x => x.Id == "local",
                        Builders<AutomationMesh>.Update.Set("CurrentScenario", scenario));
                    if (upd.ModifiedCount > 0)
                        LogHelper.Log("mesh", "home-automation", $"Global Scenarion changed to {scenario}");
                    MessagingHelper.PushToLocalAgent(new MeshScenarioMessage(MeshScenarioMessage.ChangedTopic) { Scenario = scenario });
                    AppAndDeviceHub.SendMeshChange(_appContext, "globalScenario", mesh);
                }
            }

            return Get("local");
        }

        [Route("local/globalscenario/current"), HttpDelete]
        public AutomationMesh ClearGlobalScenarioByDelete()
        {
            return ClearGlobalScenario();
        }

        [Route("local/globalscenario/current/clear"), HttpGet]
        public AutomationMesh ClearGlobalScenario()
        {
            var collMesh = MongoDbHelper.GetClient<AutomationMesh>();
            var mesh = Get("local");

            if (mesh.CurrentScenario != null)
            {
                mesh.CurrentScenario = null;
                var upd = collMesh.UpdateOne(x => x.Id == "local",
                    Builders<AutomationMesh>.Update.Unset("CurrentScenario"));
                if (upd.ModifiedCount > 0)
                    LogHelper.Log("mesh", "home-automation", $"Global Scenarion cleared");

                MessagingHelper.PushToLocalAgent(new MeshScenarioMessage(MeshScenarioMessage.ChangedTopic) { Scenario = null });
                AppAndDeviceHub.SendMeshChange(_appContext, "globalScenario", mesh);
            }

            return Get("local");
        }

    }
}
