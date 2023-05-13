using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Model
{
    public enum ShoppingSessionState
    {
        New,
        InProgress,
        WaitingReception,
        Done
    }

    public class ShoppingSession
    {
        public string Id { get; set; }
        public DateTimeOffset StartDate { get; set; }

        public ShoppingSessionState Status { get; set; }

        public string ShoppingLocationId { get; set; }
        public string[] ShoppingItemIds { get; set; }

        public string ReceiptMediaType { get; set; }
        public string ReceiptDataInBase64 { get; set; }
    }
}
