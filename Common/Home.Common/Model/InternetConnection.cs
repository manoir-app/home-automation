using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Model
{
    public enum ConnectionStatus
    {
        Up,
        Restarting,
        Failing,
        Down
    }


    public class InternetConnectionStatusRefresh
    {
        public string ConnectionId { get; set; }
        public string ConnectionType { get; set; }

        public ConnectionStatus Status { get; set; }

        public long DownloadBandwith { get; set; }
        public long UploadBandwith { get; set; }

        public long UsedDownloadBandwith { get; set; }
        public long UsedUploadBandwith { get; set; }

        public string Message { get; set; }
        public string[] Ssids { get; set; }
    }


    public class InternetConnection
    {
        public string Id { get; set; }
        public string ConnectionType { get; set; }

        public ConnectionStatus Status { get; set; }

        public bool IsMain { get; set; }

        public long DownloadBandwith { get; set; }
        public long UploadBandwith { get; set; }

        public long UsedDownloadBandwith { get; set; }
        public long UsedUploadBandwith { get; set; }

        public string LastMessage { get; set; }
        public DateTimeOffset LastUpdate { get; set; }
    }
}
