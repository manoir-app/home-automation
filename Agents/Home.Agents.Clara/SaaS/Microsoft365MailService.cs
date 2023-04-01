using Home.Agents.Clara.Mails;
using Home.Common.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Home.Agents.Clara.SaaS
{
    internal class Microsoft365MailService : IMailService
    {
        private ExternalToken _tk = null;

        public Microsoft365MailService(string user, ExternalToken tk)
        {
            _tk = tk;
        }

        public List<MailServiceItem> CheckForMails()
        {
            List<MailServiceItem> ret = new List<MailServiceItem>();
            if (_tk.ExpiresAt < DateTimeOffset.Now.AddMinutes(1))
            {
                return null;
            }

            using (var cli = new WebClient())
            {
                cli.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + _tk.Token);
                var json = cli.DownloadString("https://graph.microsoft.com/v1.0/me/messages");
                if(json!=null)
                {
                    var items = JsonConvert.DeserializeObject<GetMailMessagesResponse>(json);
                    if(items!=null)
                    {
                        foreach(var mail in items.value)
                        {
                            ret.Add(ConvertMail(mail));
                        }
                    }
                }
            }

            return ret;
        }

        private MailServiceItem ConvertMail(GetMailMessagesValue mail)
        {
            MailServiceItem ret = new MailServiceItem();

            ret.From = mail.sender?.emailAddress?.address;
            ret.Subject = mail.subject;
            ret.Content = mail.body?.content;

            return ret;
        }

        public List<MailServiceItem> CheckForMails(string sender, DateTimeOffset since)
        {
            // https://graph.microsoft.com/v1.0/me/messages?$filter=(receivedDateTime%20gt%202023-04-01T08:15:00Z)
            throw new NotImplementedException();
        }






        public class GetMailMessagesResponse
        {
            public string odatacontext { get; set; }
            public string odatanextLink { get; set; }
            public GetMailMessagesValue[] value { get; set; }
        }

        public class GetMailMessagesValue
        {
            public string odataetag { get; set; }
            public string id { get; set; }
            public DateTime createdDateTime { get; set; }
            public DateTime lastModifiedDateTime { get; set; }
            public string changeKey { get; set; }
            public string[] categories { get; set; }
            public DateTime receivedDateTime { get; set; }
            public DateTime sentDateTime { get; set; }
            public bool hasAttachments { get; set; }
            public string internetMessageId { get; set; }
            public string subject { get; set; }
            public string bodyPreview { get; set; }
            public string importance { get; set; }
            public string parentFolderId { get; set; }
            public string conversationId { get; set; }
            public string conversationIndex { get; set; }
            public object isDeliveryReceiptRequested { get; set; }
            public bool isReadReceiptRequested { get; set; }
            public bool isRead { get; set; }
            public bool isDraft { get; set; }
            public string webLink { get; set; }
            public string inferenceClassification { get; set; }
            public Body body { get; set; }
            public Sender sender { get; set; }
            public From from { get; set; }
            public Torecipient[] toRecipients { get; set; }
            public Ccrecipient[] ccRecipients { get; set; }
            public object[] bccRecipients { get; set; }
            public Replyto[] replyTo { get; set; }
            public Flag flag { get; set; }
        }

        public class Body
        {
            public string contentType { get; set; }
            public string content { get; set; }
        }

        public class Sender
        {
            public Emailaddress emailAddress { get; set; }
        }

        public class Emailaddress
        {
            public string name { get; set; }
            public string address { get; set; }
        }

        public class From
        {
            public Emailaddress emailAddress { get; set; }
        }

        public class Flag
        {
            public string flagStatus { get; set; }
        }

        public class Torecipient
        {
            public Emailaddress emailAddress { get; set; }
        }

        public class Ccrecipient
        {
            public Emailaddress emailAddress { get; set; }
        }
        public class Replyto
        {
            public Emailaddress emailAddress { get; set; }
        }

    }
}
