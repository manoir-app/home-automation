using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Model
{
    public class ApplicationShortcut
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public bool IsManoirApp { get; set; }

        public string Name { get; set; }

        public ApplicationShortcutTile TileData { get; set; }

        /// <summary>
        /// si non <c>null</c> l'id de l'intégration ayant
        /// créé ce shortcut
        /// </summary>
        public string IntegrationId { get; set; }
        public string IntegrationInstanceId { get; set; }
    }

    public class ApplicationShortcutTile
    {
        public string GroupName { get; set; }
        public string Html { get; set; }
    }
}
