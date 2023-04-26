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

        private static void ClearCache()
        {
            _allUsers = null;
            _allScenes = null;
        }

        private static void SetupCache()
        {
            if (_allScenes == null)
                _allScenes = GetAllScenes();
            if (_allUsers == null)
                _allUsers = GetAllUsers();
        }

        public static List<User> GetAllUsers()
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("aurore"))
                    {
                        var t = cli.DownloadData<List<User>>($"v1.0/users/all");
                        return (from z in t where !z.IsGuest select z).ToList();
                    }
                }
                catch (WebException ex)
                {
                    Thread.Sleep(1000);
                }
            }
            return null;
        }

        private static List<Scene> GetAllScenes()
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("aurore"))
                    {
                        var t = cli.DownloadData<List<Scene>>($"v1.0/homeautomation/scenes/scenes");
                        return t;
                    }
                }
                catch (WebException ex)
                {
                    Thread.Sleep(1000);
                }
            }
            return null;
        }
    }
}
