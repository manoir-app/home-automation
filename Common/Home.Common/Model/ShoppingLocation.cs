using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Model
{
    public class ShoppingLocation
    {
        public string Id { get; set; }


        public string Category { get; set; }

        public string Url { get; set; }
        public decimal? Lat { get; set; }
        public decimal? Long { get; set; }

        public string[] ProductCategoriesId { get; set; }
    }
}
