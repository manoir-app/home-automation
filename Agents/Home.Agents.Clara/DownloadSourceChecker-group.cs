using Home.Common;
using Home.Common.Model;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;
using System.Text.RegularExpressions;
using Home.Common.Messages;

namespace Home.Agents.Clara
{
    public static partial class DownloadSourceChecker
    {
        public static void ClassifyNewFromServer()
        {
            try
            {
                List<DownloadItem> list = null;
                using (var cli = new MainApiAgentWebClient("clara"))
                {
                    list = cli.DownloadData<List<DownloadItem>>("v1.0/downloads/new");
                }

                if (list == null || list.Count == 0)
                    return;

                foreach (var it in list)
                    UpdateExistingItem(it, list);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        public static void UpdateExistingItem(DownloadItem it, List<DownloadItem> allItems)
        {
            if (it == null)
                return;

            // on regroupe les downloads identiques
            if (!string.IsNullOrEmpty(it.Id))
            {
                DownloadItem itStd = null, it720p = null, it1080p = null, it4k = null;
                // si c'est le nom original (pas trouvé de méta) : on ignore
                if (it.VideoMetadata != null && !it.Title.Equals(it.VideoMetadata.LocalTitle))
                {
                    if (it.VideoMetadata.Resolution == null)
                        itStd = it;
                    else if (it.VideoMetadata.Resolution.Equals("720p"))
                        it720p = it;
                    else if (it.VideoMetadata.Resolution.Equals("1080p"))
                        it1080p = it;
                    else if (it.VideoMetadata.Resolution.Equals("4k"))
                        it4k = it;
                    foreach (var autreItem in allItems)
                    {
                        if (string.IsNullOrEmpty(autreItem.Id))
                            continue;

                        if (autreItem.VideoMetadata == null || autreItem.VideoMetadata.LocalTitle == null)
                            continue;
                        // si c'est le même titre
                        if (autreItem.VideoMetadata.LocalTitle.Equals(it.VideoMetadata.LocalTitle, StringComparison.InvariantCultureIgnoreCase))
                        {
                            // on check pour être sûr que ce n'est pas une série
                            if (it.VideoMetadata.Kind == autreItem.VideoMetadata.Kind
                                && it.VideoMetadata.Kind == VideoKind.TvShow)
                            {
                                continue;
                            }

                            if (itStd == null && autreItem.VideoMetadata.Resolution == null)
                                itStd = autreItem;
                            else if (autreItem.VideoMetadata.Resolution != null)
                            {
                                if (it720p == null && autreItem.VideoMetadata.Resolution.Equals("720p"))
                                    it720p = autreItem;
                                else if (it1080p == null && autreItem.VideoMetadata.Resolution.Equals("1080p"))
                                    it1080p = autreItem;
                                else if (it4k == null && autreItem.VideoMetadata.Resolution.Equals("4k"))
                                    it4k = autreItem;
                            }

                        }
                    }
                }

                DownloadItem mainItem; DownloadItem[] otherItems;
                SortItems(itStd, it720p, it1080p, it4k, out mainItem, out otherItems);
                if (mainItem != null)
                {
                    foreach (var tmp in otherItems)
                    {
                        if (tmp.LinkedDownloadId == mainItem.Id)
                            continue;

                        using (var cli = new MainApiAgentWebClient("clara"))
                        {
                            if (cli.DownloadData<bool>($"v1.0/downloads/{tmp.Id}/linkTo/{mainItem.Id}"))
                                tmp.LinkedDownloadId = mainItem.Id;
                        }
                    }
                }
            }

            if (IsAlreadyAvailable(it))
            {
                it.Status = DownloadItemStatus.DontDownload;
                return;
            }
            else
            {
                // reste à calculer la pertinence
                if (ScoreDownload(it))
                {
                    // mettre à jour sur le serveur
                }
            }
        }

        private static void SortItems(DownloadItem itStd, DownloadItem it720p, DownloadItem it1080p, DownloadItem it4k, out DownloadItem mainItem, out DownloadItem[] otherItems)
        {
            List<DownloadItem> itRet = new List<DownloadItem>();
            mainItem = it720p;
            if (mainItem == null)
                mainItem = itStd;
            if (mainItem == null)
                mainItem = it1080p;
            if (mainItem == null)
                mainItem = it4k;

            if (itStd != null && itStd != mainItem)
                itRet.Add(itStd);
            if (it720p != null && it720p != mainItem)
                itRet.Add(it720p);
            if (it1080p != null && it1080p != mainItem)
                itRet.Add(it1080p);
            if (it4k != null && it4k != mainItem)
                itRet.Add(it4k);

            otherItems = itRet.ToArray();
        }
        private static bool IsAlreadyAvailable(DownloadItem it)
        {
            return false;
        }


        public static bool HandleCancel(DownloadItemProgressMessage msg)
        {
            using(var cli = new MainApiAgentWebClient("clara"))
            {
                var lst = cli.DownloadData<List<DownloadItem>>($"/v1.0/downloads/{msg.DownloadId}/linked");
                if(lst!=null && lst.Count>0)
                {
                    var ids = (from z in lst
                               where z.Status == DownloadItemStatus.New
                               select z.Id).ToArray();
                    if(ids.Length>0)
                    {
                        Console.WriteLine("Abandon des downloads d'id : " + (string.Join(",", ids)));
                        cli.UploadData<int, string[]>($"/v1.0/downloads/all/abandon/byids", "POST", ids);
                    }
                }
            }
            return true;
        }

        public static bool HandleStartDownload(DownloadItemProgressMessage msg)
        {
            // pour l'instant c'est la même chose pour les deux
            return HandleCancel(msg);
        }

    }
}