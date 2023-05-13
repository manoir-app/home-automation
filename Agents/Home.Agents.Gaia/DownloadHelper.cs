using Home.Common.Messages;
using k8s;
using System;

namespace Home.Agents.Gaia
{
    internal static class DownloadHelper
    {
        internal static MessageResponse TryDownload(DownloadItemMessage message)
        {
            if (message == null)
                return MessageResponse.GenericFail;

            var uri = new Uri(message.SourceUrl);
            switch (uri.Scheme.ToLowerInvariant())
            {
                case "http":
                case "https":
                    if (uri.Host == null)
                        throw new NotImplementedException();

                    // utiliser youtube-dl pour ça
                    if (uri.Host.Contains("youtube"))
                        break;
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
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
                var dl = StartYoutubeDlJob(message.SourceUrl);
                if (dl != null)
                {
                    retDownload.WasQueued = true;
                    retDownload.Identifier = "kubernetes:jobname:" + dl;
                }
            }
            return retDownload;
        }

        private static string StartYoutubeDlJob(string sourceUrl)
        {
            try
            {
                var config = KubernetesClientConfiguration.InClusterConfig();
                using (var client = new Kubernetes(config))
                {
                    JobRunnerHelper.CheckJobVolume(client, "/home-automation/youtubedl", "youtubedl-storage");

                    return JobRunnerHelper.RunYoutubeDlJob(client, sourceUrl);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return null;
        }
    }
}
