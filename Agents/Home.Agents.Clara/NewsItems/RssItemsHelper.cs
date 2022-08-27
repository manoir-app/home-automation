using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Xml;

namespace Home.Agents.Clara.NewsItems
{
    public static class RssItemsHelper
    {

        public static List<InformationItem> GetNewItems(string channelUrl, DateTimeOffset since)
        {
            List<InformationItem> ret = new List<InformationItem>();
            string content;

            try
            {
                using (var cli = new WebClient())
                {
                    content = cli.DownloadString(channelUrl);
                }
            }
            catch
            {
                content = "<rss version='2.0'><channel /></rss>";
            }

            var doc = new XmlDocument();
            doc.LoadXml(content);

            ParseV2_0(since, ret, doc);

            return ret;
        }

        private static void ParseV2_0(DateTimeOffset since, List<InformationItem> ret, XmlDocument doc)
        {
            var mgr = new XmlNamespaceManager(doc.NameTable);
            mgr.AddNamespace("atom", "http://www.w3.org/2005/Atom");
            mgr.AddNamespace("media", "http://search.yahoo.com/mrss/");

            foreach (XmlElement elm in doc.SelectNodes("/rss/channel/item", mgr))
            {
                InformationItem ite = new InformationItem();

                var elmTmp = elm.SelectSingleNode("pubDate") as XmlElement;
                if (elmTmp != null)
                {
                    ite.Date = XmlConvert.ToDateTimeOffset(elmTmp.InnerText);
                    if (ite.Date < since)
                        continue;
                }
                elmTmp = elm.SelectSingleNode("title") as XmlElement;
                ite.Title = elmTmp?.InnerText;
                elmTmp = elm.SelectSingleNode("description") as XmlElement;
                ite.Content = elmTmp?.InnerText;
                ite.ContentFormat = InformationItemContentFormat.Html;
                elmTmp = elm.SelectSingleNode("author") as XmlElement;
                ite.Author = elmTmp?.InnerText;
                elmTmp = elm.SelectSingleNode("link") as XmlElement;
                ite.SourceUrl = elmTmp?.InnerText;
                ite.Source = "rss";

                if (!string.IsNullOrEmpty(ite.Title))
                {
                    ret.Add(ite);
                }
            }
        }
    }
}