using Home.Common;
using Home.Common.Messages;
using Home.Common.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.Converters;
using YamlDotNet.Serialization.NamingConventions;

namespace Home.Agents.Erza.SourceIntegration
{
    public static partial class GitSync
    {
        public class SyncFile
        {
            public GitFile GitFile { get; set; }
            public string Id { get; set; }
            public string ServerContent { get; set; }
            public string GitContent { get; set; }
        }


        public static void SyncFromSources(string messageBody)
        {
            try
            {
                SyncIntegrationsFromSources(messageBody);
                SyncSceneGroupsFromSources(messageBody);
                SyncTriggersFromSources(messageBody);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }


        private static ISerializer GetYamlSerializer()
        {
            return new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeConverter(new DateTimeConverter(DateTimeKind.Utc, CultureInfo.InvariantCulture, new[] { "yyyy-MM-dd HH:mm:ss" }))
                .Build();
        }

        private static IDeserializer GetYamlDeserializer()
        {
            return new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
        }



        private static IGitRepo GetGit()
        {
            var mesh = AgentHelper.GetLocalMesh("erza");
            if (mesh == null || mesh.SourceCodeIntegration == null)
                return null;

            var git = GetGit(mesh);
            if (git == null)
            {
                Console.WriteLine("No git handler for " + mesh.SourceCodeIntegration.GitRepoKind);
                return null;
            }

            return git;
        }

       

        private static IGitRepo GetGit(AutomationMesh mesh)
        {
            if (mesh.SourceCodeIntegration == null)
                return null;

            if (mesh.SourceCodeIntegration.GitRepoKind == null)
                return null;

            switch (mesh.SourceCodeIntegration.GitRepoKind.ToLowerInvariant())
            {
                case "gitea":
                    var t = new GiteaApi();
                    t.Init(mesh.SourceCodeIntegration);
                    return t;
                default:
                    return null;
            }
        }

    }
}
