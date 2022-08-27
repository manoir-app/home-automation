using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Model
{
    public class UserInterestInfo
    {
        public string Id { get; set; }

        public string UserId { get; set; }

        public string DataType { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Language { get; set; }
        public string[] Authors { get; set; }
        
        public string[] Actors { get; set; }
        public string[] Directors { get; set; }
        public string[] Producers { get; set; }
        public string[] Editors { get; set; }

        public string[] Genres { get; set; }

        public string Resolution { get; set; }

    }
}
