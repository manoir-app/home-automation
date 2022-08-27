using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Model
{
    public class SexLifeData
    {
        public string Id { get; set; }

        public List<string> PartnerIds { get; set; }

        public DateTimeOffset Date { get; set; }


    }

}
