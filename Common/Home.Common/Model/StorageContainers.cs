using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Model
{
    public class StorageContainer
    {
        public StorageContainer()
        {
        }

        public string Id { get; set; }

        public string CurrentStorageUnitId { get; set; }
        public string CurrentStorageUnitSubId { get; set; }
        public string Label { get; set; }
    }

}
