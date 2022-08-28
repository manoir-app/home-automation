using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Model
{
    public enum BudgetMetaCategory
    {
        Housing,
        Transportation,
        Food,
        OtherGroceries,
        Utilities,
        Services,
        Healthcare,
        Personal,
        Entertainment,
        Taxes
    }

    public class BudgetCategory
    {
        public string Id { get; set; }
        public string Label { get; set; }

        public BudgetMetaCategory MetaCategory { get; set; }
        public string IconUrl { get; set; }

        public string AssociatedUserId { get; set; }
        public bool IsPrivateOnly { get; set; }
    }
}
