using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Model
{
    public class Contact
    {
        public Contact()
        {
            SyncInfos = new List<ContactSyncInfo>();
        }
        public string Id { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }

        public string CommonName { get; set; }
        public string SsmlTaggedName { get; set; }

        public string MainEmail { get; set; }
        public string MainPhoneNumber { get; set; }

        public List<ContactSyncInfo> SyncInfos { get; set; }
    }

    public class ContactSyncInfo
    {
        public string SyncSource { get; set; }

        public string SyncId { get; set; }
        public string SyncStatus { get; set; }
        public DateTimeOffset LastSync { get; set; }
    }

}
