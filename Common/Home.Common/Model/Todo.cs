using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Model
{
    public enum TodoItemType
    {
        TodoItem,
        EventItem,
        FullDayEventItem
    }

    public enum TodoItemStatus
    {
        Creating = 0,

        Todo = 1,

        InProgress = 2,

        Done = 16
    }

    public class TodoItem
    {
        public TodoItem()
        {
            SyncDatas = new List<TodoItemSyncData>();
            Categories = new List<string>();
            Schedule = new List<SchedulingRule>();
            AssociatedUsers = new List<UserForTodo>();
            Reminders = new List<ReminderForTodo>();
        }

        public string Id { get; set; }

        public TodoItemType Type { get; set; }

        public string UserId { get; set; }

        public string Label { get; set; }

        public string Description { get; set; }

        public PrivacyLevel PrivacyLevel { get; set; }

        public List<TodoItemSyncData> SyncDatas { get; set; }

        public string Origin { get; set; }

        public string OriginItemData { get; set; }

        public TodoItemStatus Status { get; set; }

        public string ParentId { get; set; }

        public string ListId { get; set; }

        public DateTimeOffset? DueDate { get; set; }
        public TimeSpan? Duration { get; set; }

        public List<string> Categories { get; set; }

        public string SourceItemId { get; set; }

        public List<SchedulingRule> Schedule { get; set; }

        public List<ReminderForTodo> Reminders { get; set; }

        public List<UserForTodo> AssociatedUsers { get; set; }

        public string ScenarioOnStart { get; set; }
        public string ScenarioOnEnd { get; set; }
        public bool AutoActivate { get; set; }
    }

    public enum ReminderKind
    {
        GreetingMessage,
        Todo,
    }

    public class ReminderForTodo
    {
        public ReminderForTodo()
        {
            AssociatedUserIds = new List<string>();
        }

        public ReminderKind Kind { get; set; }

        public TimeSpan? OffsetFromDueDate { get; set; }

        public List<string> AssociatedUserIds { get; set; }

        public string ListIdForTodo { get; set; }

        public string Label { get; set; }
        public string Description { get; set; }
    }

    public class UserForTodo
    {
        public string UserId { get; set; }

        public string GuestName { get; set; }
        public string GuestEmail { get; set; }

        public bool SharedWith { get; set; }

        public bool ShouldUpdatePresence { get; set; }
    }

    public enum SchedulingRuleKind
    {
        CronExpression,
        TimeSpanIncrement
    }

    public class SchedulingRule
    {
        public SchedulingRuleKind Kind { get; set; }

        public string Expression { get; set; }
    }


    public class TodoItemSyncData
    {
        public string ForUserId { get; set; }
        public string ExternalServiceId { get; set; }
        public string ItemId { get; set; }

        public DateTimeOffset? LastSync { get; set; }
    }


    public class TodoList
    {
        public TodoList()
        {
            SyncDatas = new List<TodoItemSyncData>();
        }

        public string Id { get; set; }

        public string ListType { get; set; }

        public string UserId { get; set; }

        public string Label { get; set; }

        public PrivacyLevel? PrivacyLevel { get; set; }

        public bool IsDeleted { get; set; }

        public List<TodoItemSyncData> SyncDatas { get; set; }
    }



}
