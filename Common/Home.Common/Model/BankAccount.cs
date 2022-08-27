using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Model
{
    public enum BankAccountKind
    {
        Standard,
        Savings,
        Debt,
        ShortTermCredit
    }

    public class BankAccount
    {
        public BankAccount()
        {
            AssociatedUserIds = new List<string>();
        }

        public string Id { get; set; }

        public string Label { get; set; }
        public string BankName { get; set; }

        public BankAccountKind AccountKind { get; set; }

        public decimal CurrentAmount { get; set; }

        public string AccountNumber { get; set; }

        public string AccountIdInFiles { get; set; }

        public List<string> AssociatedUserIds { get; set; }
    }


    public class BankRecord
    {
        public string Id { get; set; }
        public string BankAccountId { get; set; }
        public string RecordNumber { get; set; }

        public DateTime Date { get; set; }
        
        public string DownloadedLabel { get; set; }
        public string DownloadedMoreInfo { get; set; }

        public decimal Amount { get; set; }
        public string BudgetCategoryId { get; set; }

        public bool Validated { get; set; }
        public string Label { get; set; }
        
        public string LinkedBankAccountId { get; set; }
    }
}
