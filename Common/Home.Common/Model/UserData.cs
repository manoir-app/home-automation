using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Model
{
    /// <summary>
    /// Représente un jeu de data personnalisé
    /// pour un user. Comme par exemple des settings
    /// </summary>
    /// <remarks>A ne pas utiliser pour stocker des
    /// informations de sécurité, il y a 
    /// <see cref="ExternalToken"/> pour ça</remarks>
    public class UserCustomData
    {
        public string Id { get; set; }


        public string Code { get; set; }
        public string UserId { get; set; }

        // les données peuvent être encryptées
        // à l'appli cliente de faire le travail
        // pour encrypter/décrypter
        public string EncryptedData { get; set; }
        public string EncryptionMode { get; set; }

        // et/ou être mis sous forme de clef/valeurs
        public Dictionary<string, string> PropertyBag { get; set; }
    }
}
