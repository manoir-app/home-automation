using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Messages
{
    public class UserChangeMessage : BaseMessage
    {
        public const string DeletedTopic = "users.accounts.delete";
        public const string GuestDeletedTopic = "users.accounts.delete.guest";
        public const string UpdatedTopic = "users.accounts.update";
        public const string GuestUpdatedTopic = "users.accounts.update.guest";
        public const string CreatedTopic = "users.accounts.create";
        public const string GuestCreatedTopic = "users.accounts.create.guest";

        public UserChangeMessage() : base(UpdatedTopic)
        {

        }

        public UserChangeMessage(string topic) : base(topic)
        {

        }

        public UserChangeMessage(string topic, string userId) : this(topic)
        {
            UserId = userId;
        }

        public string UserId { get; set; }
    }
}
