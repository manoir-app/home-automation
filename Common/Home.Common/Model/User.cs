using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Model
{
    public class User
    {
        public void MinimizeData()
        {
            this.HealthData = null;
            this.HashedPassword = null;
            this.Presence = null;
        }

        public void ForPresence()
        {
            this.HealthData = null;
            this.HashedPassword = null;
        }

        public User()
        {
            HealthData = new HealthData();
            Presence = new PresenceData();
            Avatar = new UserImageData();
        }

        public DateTimeOffset? DeleteAfter { get; set; }
        public bool IsGuest { get; set; }

        public string Id { get; set; }
        public bool IsMain { get; set; }

        public string Name { get; set; }
        public string FirstName { get; set; }

        public string CommonName { get; set; }
        public string SsmlTaggedName { get; set; }

        public string HashedPinCode { get; set; }
        public string HashedPassword { get; set; }

        public string MainEmail { get; set; }
        public string MainPhoneNumber { get; set; }

        public HealthData HealthData { get; set; }

        public PresenceData Presence { get; set; }

        public RoutineData Routine { get; set; }

        public UserImageData Avatar { get; set; }

    }

    public class UserImageData
    {
        public string UrlRoundBig { get; set; }
        public string UrlRoundSmall { get; set; }
        public string UrlRoundTiny { get; set; }
        public string UrlRoundSvg { get; set; }


        public string UrlSquareBig { get; set; }
        public string UrlSquareSmall { get; set; }
        public string UrlSquareTiny { get; set; }
        public string UrlSquareSvg { get; set; }
    }
}
