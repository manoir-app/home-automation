using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Model
{
    public class PrmRelationship
    {
        public const string RelationshipKindLove = "LOVE";
        public const string RelationshipKindLoveEnded = "LOVE-EX";
        public const string RelationshipKindFamily = "PARENT";
        public const string RelationshipKindWork = "WORK";
        public const string RelationshipKindFriendship = "FRIEND";


        public string Id { get; set; }
        public string Kind { get; set; }

        public string Name { get; set; }

        public string ReciprocalKind { get; set; }
        public string ReciprocalId { get; set; }
    }
}
