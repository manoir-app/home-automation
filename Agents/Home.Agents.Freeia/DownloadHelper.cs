using Home.Common;
using Home.Common.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Home.Agents.Freeia
{
    internal static class DownloadHelper
    {
        internal static MessageResponse TryDownload(DownloadItemMessage message)
        {
            if (message == null)
                return MessageResponse.GenericFail;

            var uri = new Uri(message.SourceUrl);
            switch(uri.Scheme.ToLowerInvariant())
            {
                case "ftp":
                case "ftps":
                case "magnet":
                case "torrent":
                    break;

                case "http":
                case "https":
                    if (uri.Host == null)
                        throw new NatsNoResponseException();

                    // utiliser youtube-dl pour ça
                    if(uri.Host.Contains("youtube"))
                        throw new NatsNoResponseException();

                    break;
                default:
                    throw new NatsNoResponseException();
            }

            var retDownload = new DownloadItemMessageResponse()
            {
                Topic = DownloadItemMessage.TopicName,
                Response = "ok",
                SourceUrl = message.SourceUrl,
                WasQueued = false
            };
            if (message != null)
            {
                var dl = FreeboxHelper.StartDownload(message.SourceUrl);
                if (dl != null)
                {
                    retDownload.WasQueued = true;
                    retDownload.Identifier = "freebox:downloadmgr:" + dl.id;
                }
                else
                    throw new NatsNoResponseException();
            }
            return retDownload;
        }
    }
}
