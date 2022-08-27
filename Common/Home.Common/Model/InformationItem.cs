using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Model
{
    public enum InformationItemStatus
    {
        New,
        Read,
        Kept
    }

    public enum InformationItemKind
    {
        Message,
        NewsItem
    }

    public enum InformationItemContentFormat
    {
        Html
    }     


    public class InformationItem
    {
        public InformationItem()
        {
            Tags = new List<string>();
        }

        public string Id { get; set; }
        public string Source { get; set; }
        public string SourceUrl { get; set; }

        public string UserId { get; set; }
        public DateTimeOffset Date { get; set; }

        public InformationItemStatus Status { get; set; }

        public string BucketId { get; set; }

        public InformationItemKind Kind { get; set; }

        public string Author { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public List<string> Tags { get; set; }

        public bool IsPrivate { get; set; }

        public InformationItemContentFormat ContentFormat { get; set; }
    }

    public class InformationItemsBucket
    {
        public InformationItemsBucket()
        {
            Tags = new List<string>();
        }

        public string Id { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }

        public bool IsPrivate { get; set; }
        public List<string> Tags { get; set; }

    }

}
