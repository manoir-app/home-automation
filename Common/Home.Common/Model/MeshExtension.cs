using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Model
{

    public enum MeshExtensionTechStack
    {
        WebSite,
        AzureFunction,
        Agent,
        Batch
    }

    public class MeshExtension
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string DockerImageName { get; set; }

        public MeshExtensionTechStack TechStack { get; set; }

        public bool IsInstalled { get; set; }

    }
}
