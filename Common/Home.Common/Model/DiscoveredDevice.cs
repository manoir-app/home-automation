using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Model
{

    /// <summary>
    /// Représente un device non associé, qui vient juste
    /// d'être detecté OU qui n'a jamais été validé et intégré
    /// dans la mesh
    /// </summary>
    public class DiscoveredDevice
    {
        public DiscoveredDevice()
        {
            DeviceRoles = new List<string>();
        }

        public string DeviceCode { get; set; }

        public string Id { get; set; }
        public string MeshId { get; set; }

        public DateTimeOffset DiscoveryDate { get; set; }


        public string DeviceInternalName { get; set; }

        public string DeviceAgentId { get; set; }
        public string DevicePlatform { get; set; }

        public string DeviceKind { get; set; }
        public List<string> DeviceRoles { get; set; }

        public string DefaultConfigurationData { get; set; }
    }

}
