
using Home.Common;
using Home.Common.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.Serialization.TypeResolvers;

namespace Home.Agents.Aurore.Integrations.Rhasspy
{
    partial class RhasspyService
    {
        public static void RefreshSlots()
        {
            Dictionary<string, string[]> slots = new Dictionary<string, string[]>();

            GetLocationSlots(slots);
            GetUserSlots(slots);
            GetBaseSlots(slots);
            GetProductSlots(slots);
            GetScenesSlots(slots);

            using (var cli = new WebClient())
            {
                cli.BaseAddress = _mainRhasspyUrl;
                cli.Headers.Add("Content-Type", "application/json");
                cli.Headers.Add("Accept", "application/json");
                cli.UploadString("/api/slots?overwriteAll=false", "POST", JsonConvert.SerializeObject(slots));
            }

        }

        private static void GetProductSlots(Dictionary<string, string[]> slots)
        {
            SetupCache();
            var prods = _allProducts;
            var lines = new List<string>();
            foreach (var prod in prods)
            {
                if (!string.IsNullOrWhiteSpace(prod.GenericName))
                {
                    string tmp = $"{prod.GenericName}{{product:{prod.Id}}}";
                    if (!lines.Contains(tmp))
                        lines.Add(tmp);
                }
                if (!string.IsNullOrWhiteSpace(prod.Label))
                {
                    string tmp = $"{prod.Label}{{product:{prod.Id}}}";
                    if (!lines.Contains(tmp))
                        lines.Add(tmp);
                }
                if(prod.Packagings!=null)
                {
                    foreach(var pack in prod.Packagings)
                    {
                        if (!string.IsNullOrWhiteSpace(pack.Label))
                        {
                            string tmp = $"{pack.Label}{{product:{prod.Id}}}";
                            if (!lines.Contains(tmp))
                                lines.Add(tmp);
                        }
                    }
                }
            }

            slots.Add("manoir/products", lines.ToArray());
        }

        private static void GetUserSlots(Dictionary<string, string[]> slots)
        {
            SetupCache();
            var usrs = _allUsers;
            var lines = new List<string>();
            foreach (var user in usrs)
            {
                if (!string.IsNullOrWhiteSpace(user.FirstName))
                {
                    string tmp = $"{user.FirstName}{{user:{user.Id}}}";
                    if (!lines.Contains(tmp))
                        lines.Add(tmp);
                }
                if (!string.IsNullOrWhiteSpace(user.CommonName))
                {
                    string tmp = $"{user.CommonName}{{user:{user.Id}}}";
                    if (!lines.Contains(tmp))
                        lines.Add(tmp);
                }

            }

            slots.Add("manoir/users", lines.ToArray());

        }

        private static void GetScenesSlots(Dictionary<string, string[]> slots)
        {
            SetupCache();
            var scenes = _allScenes;
            var lines = new List<string>();
            foreach (var scene in scenes)
            {
                lines.Add($"{scene.Label}{{scene:{scene.Id}}}");
            }

            slots.Add("manoir/scenes", lines.ToArray());

        }

        private static void GetBaseSlots(Dictionary<string, string[]> slots)
        {
            var cld = CultureInfo.CurrentCulture.DateTimeFormat;
            slots.Add("rhasspy/days", cld.DayNames);
            slots.Add("rhasspy/months", cld.MonthNames);
        }

        private static void GetLocationSlots(Dictionary<string, string[]> slots)
        {
            var location = AgentHelper.GetLocalMeshLocation("aurore");
            if (location != null)
            {
                List<string> allrooms = new List<string>();
                foreach (var loc in location.Zones)
                {
                    var rooms = (from z in loc.Rooms
                                 select
                                 $"[(le | la):] {z.Name}{{room:{z.Id}}}").ToArray();
                    if (rooms != null)
                        allrooms.AddRange(rooms);
                }
                slots.Add("manoir/rooms", allrooms.ToArray());
                slots.Add("manoir/zones", (from z in location.Zones
                                           select $"[(le | la):] {z.Name}{{zone:{z.Id}}}").ToArray());
            }
        }
    }
}
