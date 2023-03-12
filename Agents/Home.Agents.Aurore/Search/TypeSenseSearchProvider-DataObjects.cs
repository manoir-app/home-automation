using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Home.Agents.Aurore.Search
{
    partial class TypeSenseSearchProvider
    {

        public class Collection
        {
            [JsonConverter(typeof(UnixDateTimeConverter))   ]
            public DateTimeOffset? created_at { get; set; }
            public string default_sorting_field { get; set; }
            public bool enable_nested_fields { get; set; }
            public List<CollectionField> fields { get; set; } = new List<CollectionField>();
            public string name { get; set; }
            public int num_documents { get; set; }
            public object[] symbols_to_index { get; set; }
            public object[] token_separators { get; set; }
        }

        //[JsonConverter(typeof(StringEnumConverter))]
        //public enum CollectionFieldType
        //{
        //    @string,
        //}

        public class CollectionField
        {
            public bool facet { get; set; }
            public bool index { get; set; }
            public bool infix { get; set; }
            public string locale { get; set; }
            public string name { get; set; }
            public bool optional { get; set; }
            public bool sort { get; set; }
            public string type { get; set; }
        }


    }
}
