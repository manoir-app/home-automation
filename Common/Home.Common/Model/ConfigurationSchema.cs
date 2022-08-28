using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Model
{
    public class ConfigurationSchema
    {
        public ConfigurationSchema()
        {
            Items = new List<ConfigurationItem>();
        }

        public List<ConfigurationItem> Items { get; set; }
    }

    public class ConfigurationItem
    {
        public ConfigurationItem()
        {
            Choices = new List<ConfigurationItemChoice>();
        }

        public string Name { get; set; }
        public string HelpLabel { get; set; }
        public string HelpText { get; set; }
        public ConfigurationItemKind Kind { get; set; }
        public ConfigurationItem ObjectDescription { get; set; }
        public List<ConfigurationItemChoice> Choices { get; set; }
    }

    public enum ConfigurationItemKind
    {
        List,
        Object,
        Text,
        Choice,
        BucketChoice,
        UserId,
        DateTimeOffset
    }

    public class ConfigurationItemChoice
    {
        public string Value { get; set; }
        public string Label { get; set; }
    }

    [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class ConfigurationSchemaAttribute : Attribute
    {
        public ConfigurationSchemaAttribute()
        {
        }
    }
}
