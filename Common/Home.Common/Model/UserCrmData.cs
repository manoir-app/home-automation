using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Model
{
    /// <summary>
    /// Données de "crm perso" pour les utilisateurs
    /// (les données que <see cref="DataOwnerUserId"/> veut
    /// conserver sur <see cref="SubjectUserId"/>)
    /// </summary>
    public class UserCrmData
    {
        public string Id { get; set; }

        public string DataOwnerUserId { get; set; }
        public string SubjectUserId { get; set; }

        public string Summary { get; set; }

    }
}
