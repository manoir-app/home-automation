using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Home.Common;
using Home.Common.Model;
using Home.Graph.Common;
using Home.Graph.Server.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using SixLabors.ImageSharp;

namespace Home.Graph.Server.Controllers
{
    partial class DevicesController
    {
        [Route("register/{agentId}"), HttpPost(), Authorize(Roles = "Agent")]
        public IEnumerable<Device> RegisterDevices(string agentId, [FromBody] Device[] devices)
        {
            var coll = MongoDbHelper.GetClient<Device>();

            List<Device> dInsert = new List<Device>();

            foreach (var dev in devices)
            {
                if (string.IsNullOrEmpty(dev.MeshId))
                    dev.MeshId = "local";

                if (dev.DeviceRoles != null)
                {
                    for (int i = 0; i < dev.DeviceRoles.Count; i++)
                    {
                        dev.DeviceRoles[i] = dev.DeviceRoles[i].ToLowerInvariant();
                    }
                }

                if (!string.IsNullOrEmpty(dev.Id))
                {
                    var existing = coll.Find(x => x.Id == dev.Id).FirstOrDefault();
                    // on enregistre pas un device qui existe déjà
                    if (existing != null)
                    {
                        var upd = Builders<Device>.Update
                                   .Set("DevicePlatform", dev.DevicePlatform)
                                   .Set("DeviceAddresses", dev.DeviceAddresses)
                                   .Set("SupportPrivacyMode", dev.SupportPrivacyMode)
                                   .Set("DeviceRoles", dev.DeviceRoles);

                        if (string.IsNullOrEmpty(existing.MeshId))
                            upd.Set("MeshId", dev.MeshId);

                        // si on reçoit un nom alors qu'on en avait aucun
                        // mais ne pas mettre à jour si il y a déjà un nom setté
                        if (dev.DeviceGivenName != null &&
                            (string.IsNullOrEmpty(existing.DeviceGivenName)
                                || dev.DeviceInternalName == dev.DeviceGivenName))
                            upd.Set("DeviceGivenName", dev.DeviceGivenName);

                        coll.UpdateOne(x => x.Id == existing.Id, upd);

                        Console.WriteLine($"Device {dev.DeviceInternalName}/{dev.Id} already registered with this ID");
                        continue;
                    }
                }
                else if (!string.IsNullOrEmpty(dev.DeviceInternalName))
                {
                    var existing = coll.Find(x => x.DeviceInternalName == dev.DeviceInternalName).FirstOrDefault();
                    // on enregistre pas un device qui existe déjà
                    if (existing != null)
                    {
                        var upd = Builders<Device>.Update
                                   .Set("DevicePlatform", dev.DevicePlatform)
                                   .Set("DeviceAddresses", dev.DeviceAddresses)
                                   .Set("SupportPrivacyMode", dev.SupportPrivacyMode)
                                   .Set("DeviceRoles", dev.DeviceRoles);

                        if (string.IsNullOrEmpty(existing.MeshId))
                            upd.Set("MeshId", dev.MeshId);

                        // si on reçoit un nom alors qu'on en avait aucun
                        // mais ne pas mettre à jour si il y a déjà un nom setté
                        if (dev.DeviceGivenName != null &&
                            (existing.DeviceGivenName == null
                                || dev.DeviceInternalName == dev.DeviceGivenName))
                            upd.Set("DeviceGivenName", dev.DeviceGivenName);

                        coll.UpdateOne(x => x.Id == existing.Id, upd);

                        Console.WriteLine($"Device {dev.DeviceInternalName} already registered as id : {existing.Id}");
                        continue;
                    }
                }

                if (string.IsNullOrEmpty(dev.Id))
                {
                    if (!string.IsNullOrEmpty(dev.DeviceInternalName))
                        dev.Id = dev.DeviceInternalName;
                    else
                        dev.Id = Guid.NewGuid().ToString("D");
                }

                dInsert.Add(dev);
            }

            if (dInsert.Count > 0)
                coll.InsertMany(dInsert);

            return dInsert;
        }


        [Route("find"), HttpGet()]
        public IEnumerable<Device> FindDevice(string agentId = null, string kind = null, string role = null, string id = null, bool returnIgnored = false)
        {
            var coll = MongoDbHelper.GetClient<Device>();
            var fl = Builders<Device>.Filter;

            var lesFiltres = fl.Empty;

            if (!returnIgnored)
                lesFiltres &= fl.Eq("IsIgnored", false);

            if (!string.IsNullOrEmpty(kind))
                lesFiltres &= fl.Eq("DeviceKind", kind);

            if (!string.IsNullOrEmpty(agentId))
                lesFiltres &= fl.Eq("DeviceAgentId", agentId);

            if (!string.IsNullOrEmpty(id))
                lesFiltres &= fl.Eq("Id", id);

            if (!string.IsNullOrEmpty(role))
                lesFiltres &= fl.AnyEq("DeviceRoles", role);

            return coll.Find(lesFiltres & fl.Eq("MeshId", "local")).ToList();
        }

        [Route("all/{deviceId}"), HttpGet]
        public Device GetDeviceById(string deviceId)
        {
            deviceId = deviceId.ToLowerInvariant();
            var coll = MongoDbHelper.GetClient<Device>();
            return coll.Find(x => x.Id.Equals(deviceId)).FirstOrDefault();
        }


        [Route("all/{deviceId}/images/{imagecode}"), HttpDelete]
        public Device DeleteImage(string deviceId, string imagecode)
        {
            deviceId = deviceId.ToLowerInvariant();
            var coll = MongoDbHelper.GetClient<Device>();
            var dev = coll.Find(x => x.Id.Equals(deviceId)).FirstOrDefault();

            if (dev != null)
            {
                var pth = FileCacheHelper.GetLocalFilename("devices", $"images/{dev.Id}", imagecode + ".png");

                if (System.IO.File.Exists(pth))
                    System.IO.File.Delete(pth);

                if (dev.Images.ContainsKey(imagecode))
                    dev.Images.Remove(imagecode);

                coll.ReplaceOne(x => x.Id == dev.Id, dev);
            }

            return coll.Find(x => x.Id.Equals(deviceId)).FirstOrDefault();
        }


        [Route("all/{deviceId}/status"), HttpPatch]
        public bool ReplaceDataAndStatus(string deviceId, string status, List<DeviceData> datas)
        {
            var ctl = MongoDbHelper.GetClient<Device>();

            var upd = Builders<Device>.Update
                .Set("Datas", datas)
                .Set("MainStatusInfo", status);

            var t = ctl.UpdateOne(x => x.Id.Equals(deviceId), upd);

            return t.MatchedCount == 1;
        }

        [Route("all/{deviceId}/status/data"), HttpPost]
        public bool ChangeData(string deviceId, DeviceData data, string mainStatus=null)
        {
            var ctl = MongoDbHelper.GetClient<Device>();

            var dev = ctl.Find(x => x.Id.Equals(deviceId)).FirstOrDefault();
            if (dev == null)
                return false;

            var dt = (from z in dev.Datas
                      where z.Name.Equals(data.Name)
                      select z).FirstOrDefault();
            if (dt != null)
            {
                var arrayFilters = Builders<Device>.Filter.Eq("Id", deviceId)
                        & Builders<Device>.Filter.Eq("Datas.Name", data.Name);

                var upd = Builders<Device>.Update
                    .Set("Datas.$.Value", data.Value);

                if (!string.IsNullOrEmpty(mainStatus))
                    upd = upd.Set("MainStatusInfo", mainStatus);

                var t = ctl.UpdateOne(arrayFilters, upd, options: new UpdateOptions()
                {
                    IsUpsert = true
                });
                return t.IsAcknowledged;
            }
            else
            {
                dev.Datas.Add(data);
                return ReplaceDataAndStatus(deviceId, dev.MainStatusInfo, dev.Datas);
            }

        }

        [Route("all/{deviceId}/status/data"), HttpGet]
        public List<DeviceData> GetData(string deviceId)
        {
            var ctl = MongoDbHelper.GetClient<Device>();

            var dev = ctl.Find(x => x.Id.Equals(deviceId)).FirstOrDefault();
            if (dev != null)
                return dev.Datas;

            return null;
        }


        [Route("all/{deviceId}/images/{imagecode}"), HttpPost]
        public Device UpsertImage(string deviceId, string imagecode)
        {
            deviceId = deviceId.ToLowerInvariant();
            var coll = MongoDbHelper.GetClient<Device>();
            var dev = coll.Find(x => x.Id.Equals(deviceId)).FirstOrDefault();

            if (dev != null)
            {
                var pth = FileCacheHelper.GetLocalFilename("devices", $"images/{dev.Id}", imagecode + ".dat");
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

                string pubUrl = $"https://public.anzin.carbenay.manoir.app/v1.0/services/files/devices/images/{dev.Id}/{imagecode}.png"; ;
                if (dev.Images.ContainsKey(imagecode))
                    dev.Images[imagecode] = pubUrl;
                else
                    dev.Images.Add(imagecode, pubUrl);
                coll.ReplaceOne(x => x.Id == dev.Id, dev);
            }

            return coll.Find(x => x.Id.Equals(deviceId)).FirstOrDefault();

        }
    }
}