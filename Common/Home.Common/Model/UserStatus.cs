using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Model
{
    /// <summary>
    /// Représente l'état actuel d'un utilisateur
    /// </summary>
    /// <remarks>Pour éviter de polluer la classe
    /// <see cref="User"/> avec les données "techniques"
    /// comme où on en est de la lecture du chat, etc.</remarks>
    public class UserStatus
    {
        public UserStatus()
        {
            ChatsStatus = new Dictionary<string, UserStatusChat>();
        }

        public string Id { get; set; }

        public Dictionary<string, UserStatusChat> ChatsStatus { get; set; }
    }

    public class UserStatusChat
    {
        public DateTimeOffset LastRead { get; set; }
    }

}
