using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Model
{
    public enum DownloadItemStatus
    {
        DontDownload = -1,
        New = 0,
        Queued = 1,
        InProgress = 2,
        Done = 9
    }
    public class DownloadItem
    {
        public DownloadItem()
        {
            Tags = new List<string>();
            Results = new List<DownloadItemResult>();
            Pertinences = new List<DownloadPertinence>();
        }

        public List<string> Tags { get; set; }

        public string Id { get; set; }
        public DownloadItemStatus Status { get; set; }
        public string Source { get; set; }
        public string SourceUrl { get; set; }
        public string Title { get; set; }
        public string DownloadPrivateId { get; set; }
        public bool? IsPrivate { get; set; }

        public DateTimeOffset DateAdded { get; set; }

        public string SourceAgent { get; set; }
        public string SourceAgentComment { get; set; }

        public string LinkedDownloadId { get; set; }

        public List<DownloadPertinence> Pertinences { get; set; }

        public List<DownloadItemResult> Results { get; set; }

        public VideoMetadata VideoMetadata { get; set; }
        public string Language { get; set; }
    }

    public class DownloadPertinence
    {
        public string UserId { get; set; }
        public decimal Pertinence { get; set; }
    }

    public class DownloadItemResult
    {
        public DownloadItemResult()
        {
            Links = new List<DownloadItemResultFileLink>();
        }
        public string Filename { get; set; }
        public List<DownloadItemResultFileLink> Links { get; set; }
    }

    public enum DownloadItemResultFileLinkKind
    {
        Ftp,
        Smb
    }

    public class DownloadItemResultFileLink
    {
        public DownloadItemResultFileLinkKind Kind { get; set; }
        public string Uri { get; set; }
    }
}
