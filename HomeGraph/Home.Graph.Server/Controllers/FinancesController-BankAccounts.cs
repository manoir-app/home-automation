using Home.Common.Model;
using Home.Graph.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Home.Graph.Server.Controllers
{
    partial class FinancesController
    {
        #region BankAccounts
        [Route("bank/accounts"), HttpGet]
        public List<BankAccount> GetBankAccounts()
        {
            var collection = MongoDbHelper.GetClient<BankAccount>();
            var lst = collection.Find(x => true).ToList();
            return lst;
        }

        [Route("bank/accounts/{accountId}"), HttpGet]
        public BankAccount GetBankAccount(string accountId)
        {
            var collection = MongoDbHelper.GetClient<BankAccount>();
            var lst = collection.Find(x => x.Id == accountId).FirstOrDefault();
            return lst;
        }

        [Route("bank/accounts"), HttpPost]
        public BankAccount UpsertBankAccount([FromBody] BankAccount account)
        {
            if (account.Id == null)
                account.Id = Guid.NewGuid().ToString("D").ToLowerInvariant();

            var collection = MongoDbHelper.GetClient<BankAccount>();
            var lst = collection.Find(x => x.Id == account.Id).FirstOrDefault();

            if (lst == null)
            {
                collection.InsertOne(account);
            }
            else
            {
                collection.ReplaceOne(x => x.Id == lst.Id, account);
            }

            lst = collection.Find(x => x.Id == account.Id).FirstOrDefault();
            return lst;
        }

        [Route("bank/accounts/{accountId}"), HttpDelete]
        public bool DeleteBankAccount(string accountId, string accountIdRemplacement)
        {
            var collection = MongoDbHelper.GetClient<BankAccount>();
            var lst = collection.Find(x => x.Id == accountId).FirstOrDefault();

            if (lst == null)
                return true;

            var items = MongoDbHelper.GetClient<BankRecord>();
            var count = items.CountDocuments(x => x.BankAccountId == lst.Id);
            if (string.IsNullOrEmpty(accountIdRemplacement) && count > 0)
            {
                return false;
            }
            else
            {
                var upd = Builders<BankRecord>.Update.Set("BankAccountId", accountIdRemplacement);
                items.UpdateMany(x => x.BudgetCategoryId == lst.Id, upd);
            }

            collection.DeleteOne(x => x.Id == lst.Id);
            return true;
        }
        #endregion


    }
}
