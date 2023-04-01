using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Home.Agents.Clara.Mails
{
    public class MailServiceItem
    {
        public string From { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }

        public List<MailServiceItemAttachment> Attachments { get; set; } = new List<MailServiceItemAttachment>();
    }

    public class MailServiceItemAttachment
    {
        public string Name { get; set; }
        public byte[] Content { get; set; }
    }


    public interface IMailService
    {
        public List<MailServiceItem> CheckForMails();
        public List<MailServiceItem> CheckForMails(string sender, DateTimeOffset since);
    }
}
