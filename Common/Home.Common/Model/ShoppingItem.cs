using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Model
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ShoppingMode
    {
        Online,
        InStore,
    }

    public class ShoppingItem
    {
        public string Id { get; set; }

        public string ProductId { get; set; }

        public string Label { get; set; }

        public DateTimeOffset? DueDate { get; set; }

        public decimal Quantity { get; set; }

        public ShoppingMode ShoppingMode { get; set; }

        public string PreferredShoppindLocationId { get; set; }

        public bool InCart { get; set; }

        public bool Done { get; set; }

        public decimal? ExpectedPrice { get; set; }

        public decimal? Price { get; set; }
    }
}
