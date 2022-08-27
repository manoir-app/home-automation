using Home.Common;
using Home.Common.Messages;
using Home.Common.Model;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;

namespace Home.Agents.Erza
{
    partial class PresenceHelper
    {
        private static MessageResponse HandleActivity(PresenceNotificationMessage msg)
        {
            var usrid = msg.Data?.AssociatedUser;
            if (usrid == null)
                return MessageResponse.GenericFail;

            User usr = AgentHelper.GetUser("erza", usrid);
            if (usr == null)
                return MessageResponse.GenericFail;

            DecoterLesPresences(usr);

            // on ignore ce qui a plus de 2h
            usr.Presence.LatestActivities = (from z in usr.Presence.LatestActivities
                                             where z.Date.GetValueOrDefault() > DateTimeOffset.Now.AddHours(-2)
                                             select z).ToList();

            if (string.IsNullOrEmpty(msg.Data.ActivityKind))
                msg.Data.ActivityKind = "notification";

            var pud = new PresenceUpdateData();
            var pad = new PresenceActivityData();
            pad.ActivityKind = msg.Data.ActivityKind;
            pad.Date = msg.Data.Date.GetValueOrDefault(DateTimeOffset.Now);
            pad.LocationId = msg.Data.LocationId;
            pad.Status = msg.Data.Status;
            pad.DeviceId = msg.Data.DeviceId;
            if (ShouldTrace(usr, pad))
                pud.ActivityToLog = pad;
            Console.WriteLine("Computing Location probabilty for : " + JsonConvert.SerializeObject(msg.Data));
            PresenceLocationData loc = FindLocation(usr, msg.Data);
            if (loc != null)
                pud.Location = loc;

            UploadPresenceData(usr, pud);

            return MessageResponse.OK;

        }

        private static void UploadPresenceData(User usr, PresenceUpdateData pud)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("erza"))
                    {
                        PresenceData t = cli.UploadData<PresenceData, PresenceUpdateData>($"v1.0/users/all/{usr.Id}/presence", "POST", pud);
                        Console.Write("Updated presence for : " + usr.Id);
                        break;
                    }
                }
                catch (WebException ex)
                {
                    Console.Write("Failed to update presence for : " + usr.Id);
                    Thread.Sleep(1000);
                }
            }
        }

        private static bool ShouldTrace(User usr, PresenceActivityData pad)
        {
            bool hasAlready = false;

            if (usr.Presence != null &&
                usr.Presence.LatestActivities != null)
            {
                var rootReq = (from z in usr.Presence.LatestActivities
                               where z.ActivityKind.Equals(pad.ActivityKind)
                               select z).AsEnumerable();
                if (pad.LocationId != null)
                    rootReq = rootReq.Where(x => x.LocationId != null && x.LocationId.Equals(pad.LocationId, StringComparison.CurrentCultureIgnoreCase));
                else
                    rootReq = rootReq.Where(x => x.LocationId == null);

                if(pad.Status != null)
                    rootReq = rootReq.Where(x => x.Status !=null && x.Status.Equals(pad.Status, StringComparison.CurrentCultureIgnoreCase));
                else
                    rootReq = rootReq.Where(x => x.Status == null);

                hasAlready = rootReq.Count() > 0;
            }

            switch (pad.ActivityKind)
            {
                // la plupart des notifications ne devraient compter qu'une fois 
                default:
                    return !hasAlready;
            }
        }

        private static PresenceLocationData FindLocation(User usr, PresenceNotificationData data)
        {
            switch (data.ActivityKind.ToLowerInvariant())
            {
                case "mobileappusage":
                    return FindLocationForMobileAppUsage(usr, data);
                case "forcelocation":
                    if (data.Status.Equals("in"))
                        return new PresenceLocationData()
                        {
                            LocationId = data.LocationId,
                            Probability = 100,
                            LatestUpdate = DateTimeOffset.UtcNow
                        };
                    else if (data.Status.Equals("out"))
                        return new PresenceLocationData()
                        {
                            LocationId = data.LocationId,
                            Probability = 0,
                            LatestUpdate = DateTimeOffset.UtcNow
                        };
                    return null;
                case "fromevent":
                    if (data.Status.Equals("start"))
                        return new PresenceLocationData()
                        {
                            LocationId = data.LocationId,
                            Probability = 100,
                            LatestUpdate = DateTimeOffset.UtcNow
                        };
                    else if (data.Status.Equals("end"))
                        return new PresenceLocationData()
                        {
                            LocationId = data.LocationId,
                            Probability = 0,
                            LatestUpdate = DateTimeOffset.UtcNow
                        };
                    return null;
                case "personaldevicedetection":
                    if (data.Status.Equals("in"))
                        return new PresenceLocationData()
                        {
                            LocationId = data.LocationId,
                            Probability = CalculateProbability(usr.Presence.Location, data.LocationId, 60),
                            LatestUpdate = DateTimeOffset.UtcNow
                        };
                    else if (data.Status.Equals("out"))
                        return new PresenceLocationData()
                        {
                            LocationId = data.LocationId,
                            Probability = CalculateProbability(usr.Presence.Location, data.LocationId, -75),
                            LatestUpdate = DateTimeOffset.UtcNow
                        };
                    return null;
                default:
                    return null;
            }

        }

        private static PresenceLocationData FindLocationForMobileAppUsage(User usr, PresenceNotificationData data)
        {
            if (data.Status == null)
                data.Status = "info";
            switch(data.Status.ToLowerInvariant())
            {
                case "homeautomation":
                    string locId = data.LocationId;
                    if (string.IsNullOrEmpty(locId))
                        locId = _localLoc.Id;
                    return new PresenceLocationData()
                    {
                        LocationId = locId,
                        Probability = CalculateProbability(usr.Presence.Location, locId, 10),
                        LatestUpdate = DateTimeOffset.UtcNow
                    };
            }

            return null;
        }

        private static int CalculateProbability(List<PresenceLocationData> locations, string locationId, int delta)
        {
            var l = (from z in locations
                     where z.LocationId.Equals(locationId, StringComparison.InvariantCultureIgnoreCase)
                     select z).FirstOrDefault();

            if (l != null)
                return Math.Min(100, l.Probability + delta);

            return Math.Max(5, delta);
        }

        //public static void UpdateStatus(Guid deviceGuid, Guid userGuid, PresenceStatus status)
        //{
        //    if (deviceGuid.Equals(Guid.Empty) || userGuid.Equals(Guid.Empty))
        //        return;

        //    UserPresence presence = null;
        //    lock (_allPresences)
        //    {
        //        if (!_allPresences.ContainsKey(userGuid))
        //        {
        //            presence = new UserPresence();
        //            presence.UserGuid = userGuid;
        //            _allPresences.Add(userGuid, presence);
        //        }
        //        else
        //            presence = _allPresences[userGuid];
        //    }

        //    bool bFound = false;

        //    lock (presence)
        //    {
        //        foreach (PresenceDetails det in presence.Details)
        //        {
        //            if (det.DeviceGuid.Equals(deviceGuid))
        //            {
        //                det.Status = status;
        //                // si on recoit un message "inactif", on ne met pas
        //                // à jour la date de derniere notification : en effet, si
        //                // c'est inactif, c'est probablement qu'on est plus derriere l'écran
        //                // voir si cela marche, si ce n'est pas le cas, on remettra
        //                // à jour mais en le valorisant moins dans la partie "FindMostProbable"
        //                if (status != PresenceStatus.Inactive)
        //                {
        //                    det.LastPing = DateTime.Now;
        //                }
        //                FillPresenceDetailsFromDevice(det, deviceGuid);
        //                bFound = true;
        //                Console.Out.WriteLine(string.Format("PRESENCE - Updated presence-info for {0} on {1} : {2}",
        //                    userGuid, det.DeviceGuid, det.Status));
        //                break;
        //            }
        //        }
        //        if (!bFound)
        //        {
        //            PresenceDetails det = new PresenceDetails();
        //            det.Status = status;
        //            det.LastPing = DateTime.Now;
        //            FillPresenceDetailsFromDevice(det, deviceGuid);
        //            presence.Details.Add(det);
        //            Console.Out.WriteLine(string.Format("PRESENCE - Added presence-info for {0} on {1} : {2}",
        //                userGuid, det.DeviceGuid, det.Status));
        //        }
        //        presence.MostProbable = FindMostPropable(presence);
        //    }

        //    PresistPresences(_allPresences);
        //}

        //private static void FillPresenceDetailsFromDevice(PresenceDetails det, Guid deviceGuid)
        //{
        //    BaseDeviceInfo inf = null;
        //    det.DeviceGuid = deviceGuid;
        //    lock (ServerInstance.KnownLocations)
        //    {
        //        //foreach (DeviceInfo di in _devices)
        //        //{
        //        //    if (di.Guid == deviceGuid)
        //        //    {
        //        //        inf = di;
        //        //        break;
        //        //    }
        //        //}

        //        if (inf != null && inf.LocationGuid.HasValue)
        //        {
        //            foreach (Location li in ServerInstance.KnownLocations)
        //            {
        //                if (li.Guid == inf.LocationGuid.Value)
        //                {
        //                    det.LocationGuid = inf.LocationGuid;
        //                    det.Latitude = li.Latitude;
        //                    det.Longitude = li.Longitude;
        //                    break;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            det.LocationGuid = null;
        //            det.Longitude = null;
        //            det.Latitude = null;
        //        }
        //        //if (inf != null && inf.RoomGuid.HasValue)
        //        //    det.RoomGuid = inf.RoomGuid.Value;
        //        //else
        //        //    det.RoomGuid = null;
        //    }
        //}

        //private static PresenceDetails FindMostPropable(UserPresence presence)
        //{
        //    if (presence.Details.Count == 0)
        //        return null;

        //    if (presence.Details.Count == 1)
        //    {
        //        PresenceDetails d = presence.Details[0];
        //        TimeSpan depuis = DateTime.Now - d.LastPing;
        //        if (d.Status == PresenceStatus.Announced && depuis.TotalHours <= 1)
        //            return d;
        //        if (d.Status == PresenceStatus.Active && depuis.TotalMinutes < 15)
        //            return d;
        //    }
        //    else
        //    {
        //        PresenceDetails dMax = null;
        //        int maxScore = 0;

        //        foreach (PresenceDetails d in presence.Details)
        //        {
        //            int score = 0;
        //            TimeSpan depuis = DateTime.Now - d.LastPing;
        //            if (d.Status == PresenceStatus.Announced)
        //                score = (int)(100 / (1 + depuis.TotalHours));
        //            else if (d.Status == PresenceStatus.Active)
        //            {
        //                score = (int)(50 / (1 + (depuis.TotalSeconds / 10)));
        //            }
        //            else
        //            {
        //                score = (int)(5 / (1 + (depuis.TotalSeconds / 10))); ;
        //            }

        //            if (score > maxScore)
        //            {
        //                maxScore = score;
        //                dMax = d;
        //            }
        //        }

        //        return dMax;
        //    }
        //    return null;
        //}

        //private static Dictionary<Guid, UserPresence> _allPresences = new Dictionary<Guid, UserPresence>();


        //public static void UpdateStatusEx(Guid deviceGuid, Guid userGuid, PresenceStatus status, decimal latitude, decimal longitude)
        //{
        //    if (userGuid.Equals(Guid.Empty))
        //        return;

        //    UserPresence presence = null;
        //    if (!_allPresences.ContainsKey(userGuid))
        //    {
        //        presence = new UserPresence();
        //        presence.UserGuid = userGuid;
        //        _allPresences.Add(userGuid, presence);
        //    }
        //    else
        //        presence = _allPresences[userGuid];
        //    bool bFound = false;

        //    lock (presence)
        //    {

        //        foreach (PresenceDetails det in presence.Details)
        //        {
        //            if (det.DeviceGuid.Equals(deviceGuid))
        //            {
        //                det.LastPing = DateTime.Now;
        //                det.Status = status;
        //                FillPresenceDetailsFromGeoPosition(det, deviceGuid, latitude, longitude);
        //                bFound = true;
        //                break;
        //            }
        //        }
        //        if (!bFound)
        //        {
        //            PresenceDetails det = new PresenceDetails();
        //            det.Status = status;
        //            det.LastPing = DateTime.Now;
        //            FillPresenceDetailsFromDevice(det, deviceGuid);
        //            presence.Details.Add(det);
        //        }
        //        var tmp = FindMostPropable(presence);
        //        if (tmp != presence.MostProbable)
        //        {
        //            presence.MostProbable = tmp;
        //            try
        //            {
        //                if (tmp != null && tmp.LocationGuid.HasValue)
        //                {
        //                    var loc = (from z in ServerInstance.KnownLocations
        //                               where z.Guid.Equals(tmp.LocationGuid.Value)
        //                               select z).FirstOrDefault();
        //                    if (loc != null)
        //                    {
        //                        Logger.LogInfo(string.Format("Settings most probable presence settings for {0} to location : {1}", userGuid, loc.Name), "Presence");
        //                    }
        //                    else
        //                    {
        //                        Logger.LogInfo(string.Format("Settings most probable presence settings for {0} to location : unknown", userGuid), "Presence");
        //                    }
        //                }
        //                else
        //                    Logger.LogInfo(string.Format("Settings most probable presence settings for {0} to unknown", userGuid), "Presence");
        //            }
        //            catch
        //            {

        //            }
        //        }

        //    }

        //    PresistPresences(_allPresences);
        //}

        //static int nbUpdateStatusPresence = 0;


        //private static void PresistPresences(Dictionary<Guid, UserPresence> allPresences)
        //{
        //    nbUpdateStatusPresence++;
        //    if (nbUpdateStatusPresence % 50 != 1)
        //        return;
        //    string file = Path.Combine(Path.GetTempPath(), "aurore\\cache", "presence.cache");
        //    XmlDocument doc = new XmlDocument();
        //    doc.AppendChild(doc.CreateElement("Cache"));
        //    lock (_allPresences)
        //    {
        //        foreach (Guid g in allPresences.Keys)
        //        {
        //            UserPresence p = allPresences[g];
        //            XmlElement elmUsr = doc.CreateElement("User");
        //            elmUsr.SetAttribute("guid", g.ToString());
        //            foreach (PresenceDetails det in p.Details)
        //            {
        //                XmlElement elmDet = doc.CreateElement("Details");
        //                if (det == p.MostProbable)
        //                    elmDet.SetAttribute("probability", "most");
        //                elmDet.SetAttribute("deviceGuid", det.DeviceGuid.ToString());
        //                if (det.Latitude.HasValue)
        //                    elmDet.SetAttribute("latitude", det.Latitude.Value.ToString());
        //                if (det.Longitude.HasValue)
        //                    elmDet.SetAttribute("longitude", det.Longitude.Value.ToString());
        //                elmDet.SetAttribute("lastPing", det.LastPing.ToString("R"));
        //                if (det.LocationGuid.HasValue)
        //                    elmDet.SetAttribute("locationGuid", det.LocationGuid.Value.ToString());
        //                if (det.RoomGuid.HasValue)
        //                    elmDet.SetAttribute("roomGuid", det.RoomGuid.Value.ToString());
        //                elmDet.SetAttribute("status", det.Status.ToString());
        //                elmUsr.AppendChild(elmDet);
        //            }

        //            doc.DocumentElement.AppendChild(elmUsr);
        //        }
        //    }

        //    doc.Save(file);
        //}

        //private static void FillPresenceDetailsFromGeoPosition(PresenceDetails det, Guid deviceGuid, decimal latitude, decimal longitude)
        //{
        //    det.DeviceGuid = deviceGuid;
        //    det.Longitude = longitude;
        //    det.Latitude = latitude;
        //    det.LocationGuid = null;
        //}

        //internal static void UpdateStatusFromAurore(Guid userGuid, Guid locationGuid)
        //{
        //    UserPresence presence = null;
        //    lock (_allPresences)
        //    {
        //        if (!_allPresences.ContainsKey(userGuid))
        //        {
        //            presence = new UserPresence();
        //            presence.UserGuid = userGuid;
        //            _allPresences.Add(userGuid, presence);
        //        }
        //        else
        //            presence = _allPresences[userGuid];
        //    }

        //    bool bFound = false;

        //    lock (presence)
        //    {
        //        foreach (PresenceDetails det in presence.Details)
        //        {
        //            if (det.Status == PresenceStatus.Announced)
        //            {
        //                det.LastPing = DateTime.Now;
        //                FillPresenceDetailsFromLocation(det, locationGuid);
        //                bFound = true;
        //                Console.Out.WriteLine(string.Format("PRESENCE - Updated presence-info for {0} on {1} : {2}",
        //                    userGuid, det.DeviceGuid, det.Status));
        //                break;
        //            }
        //        }
        //        if (!bFound)
        //        {
        //            PresenceDetails det = new PresenceDetails();
        //            det.Status = PresenceStatus.Announced;
        //            det.LastPing = DateTime.Now;
        //            FillPresenceDetailsFromLocation(det, locationGuid);
        //            presence.Details.Add(det);
        //            Logger.LogInfo(string.Format("PRESENCE - Added presence-info for {0} on {1} : {2}",
        //                userGuid, det.DeviceGuid, det.Status));
        //        }
        //        presence.MostProbable = FindMostPropable(presence);
        //    }

        //    PresistPresences(_allPresences);
        //}

        //private static void FillPresenceDetailsFromLocation(PresenceDetails det, Guid locationGuid)
        //{
        //    lock (ServerInstance.KnownLocations)
        //    {
        //        foreach (Location li in ServerInstance.KnownLocations)
        //        {
        //            if (li.Guid == locationGuid)
        //            {
        //                det.LocationGuid = locationGuid;
        //                det.Latitude = li.Latitude;
        //                det.Longitude = li.Longitude;
        //                break;
        //            }
        //        }
        //    }
        //}

        //internal static string GetUserLocationForAurore(Guid userGuid)
        //{
        //    UserPresence pres = null;
        //    if (!_allPresences.TryGetValue(userGuid, out pres))
        //        return null;
        //    if (pres.MostProbable != null)
        //    {
        //        return ConvertLocationForAurore(pres.MostProbable.LocationGuid.GetValueOrDefault());
        //    }

        //    else if (pres.Details.Count == 1)
        //    {
        //        return ConvertLocationForAurore(pres.Details[0].LocationGuid.GetValueOrDefault());
        //    }

        //    return null;
        //}

        //internal static string ConvertLocationForAurore(Guid guid)
        //{
        //    foreach (Location l in ServerInstance.KnownLocations)
        //    {
        //        if (l.Guid.Equals(guid))
        //        {
        //            if (!string.IsNullOrWhiteSpace(l.CommonName))
        //                return l.CommonName;
        //            else
        //                return l.Name;
        //        }
        //    }

        //    return null;
        //}


    }
}
