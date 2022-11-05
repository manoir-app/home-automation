using Markdig.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Model
{
    /// <summary>
    /// Données de "crm perso" pour les utilisateurs
    /// (les données que <see cref="DataOwnerUserId"/> veut
    /// conserver sur <see cref="SubjectContactId"/>)
    /// </summary>
    public class UserPrmData
    {


        public UserPrmData()
        {
            Images = new Dictionary<string, UserPrmImageCollection>();
            Tags = new List<string>();
            IsMain = true;
            MoreAdresses = new List<ContactAddress>();
            MoreCoordinates = new List<ContactCoordinates>();
        }

        public string Id { get; set; }

        public string DataOwnerUserId { get; set; }
        public string SubjectContactId { get; set; }




        public string RelationshipKindWithUser { get; set; }
        public string RelationshipSubKindWithUser { get; set; }
        
        public bool IsMain { get; set; }

        public string Summary { get; set; }

        public Dictionary<string, UserPrmImageCollection> Images { get; set; }

        public DateTimeOffset? DateOfBirth { get; set; }

        public List<string> Tags { get; set; }
        public List<UserPrmRelationship> Relationships { get; set; }

        public List<ContactCoordinates> MoreCoordinates { get; set; }
        public List<ContactAddress> MoreAdresses { get; set; }

        public UserPrmMeetingPreferences MeetingPreferences { get; set; }

        public Dictionary<string, UserPrmTodoListAssociationCollection> Reminders { get; set; }
        public UserPrmTodoListAssociation TasksTodoListId { get; set; }

        
    }

    public class UserPrmTodoListAssociationCollection : List<UserPrmTodoListAssociation>
    {
        public string NoteCategoryKind { get; set; }
        public string RelationshipKind { get; set; }

        public string AssociatedUserPrmId { get; set; }
        public string AssociationRelationship { get; set; }
    }

    public class UserPrmTodoListAssociation
    {
        public UserPrmTodoListAssociation()
        {
            MandatoryTasks = new List<UserPrmPlannedTask>();
            SuggestedTasks = new List<UserPrmPlannedTask>();
        }

        public string TodoListId { get; set; }
        public string EventListId { get; set; }

        public List<UserPrmPlannedTask> MandatoryTasks { get; set; }
        public List<UserPrmPlannedTask> SuggestedTasks { get; set; }
    }


    public class UserPrmImageCollection : List<UserPrmImage>
    {
        public string Name { get; set; }
        public bool IsPublic { get; set; }

    }
    public class UserPrmImage
    {
        public string Uri { get; set; }
        public DateTimeOffset Date { get; set; }

        public string Comment { get; set; }
        public string ImageSource { get; set; }
    }

    public class UserPrmRelationship
    {
       

        public string ContactId { get; set; }
        public string RelationKind { get; set; }
        public string RelationTypeId { get; set; }
        public string Comment { get; set; }
    }

    public class UserPrmMeetingPreferences
    {
        public UserPrmMeetingPreferences()
        {
            MandatoryTasks = new List<UserPrmPlannedTask>();
            SuggestedTasks = new List<UserPrmPlannedTask>();
        }

        public List<UserPrmPlannedTask> MandatoryTasks { get; set; }
        public List<UserPrmPlannedTask> SuggestedTasks { get; set; }
    }

    public class UserPrmPlannedTask
    {
        public string TaskName { get; set; }
        public TimeSpan OffsetFromEvent { get; set; }
    }

    public class UserPrmNote
    {
        public string UserPrmDataId { get; set; }
        public DateTimeOffset Date { get; set; }

        public string NoteCategoryKind { get; set; }
        public string NoteCategoryId { get; set; }
        public string NoteCode { get; set; }
        public string NoteContent { get; set; }
        public string NoteLink { get; set; }
        public UserPrmImageCollection Images { get; set; }
    }

    public class UserPrmInteractionLog
    {
        public string UserPrmDataId { get; set; }
        public DateTimeOffset Date { get; set; }

        public string LogCategory { get; set; }
        public string LogCode { get; set; }
        public string LogContent { get; set; }
    }
}
