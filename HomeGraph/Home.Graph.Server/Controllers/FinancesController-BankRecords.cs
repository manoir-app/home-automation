using Home.Common.Model;
using Home.Graph.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Home.Graph.Server.Controllers
{
    partial class FinancesController
    {
        #region BankAccounts
        [Route("bank/records"), HttpGet]
        public IEnumerable<BankRecord> GetBankRecords(string accountId = null, DateTime? dtDebut = null, DateTime? dtFin = null)
        {
            if (!dtDebut.HasValue)
                dtDebut = DateTime.Today.AddDays(-30);
            if (!dtFin.HasValue)
                dtFin = DateTime.Today;

            dtFin = dtFin.Value.AddDays(1).AddSeconds(-1);

            var collection = MongoDbHelper.GetClient<BankRecord>();
            var lst = collection.Find(x => x.Date >= dtDebut.Value
                                    && x.Date <= dtFin.Value
                                    && (accountId == null || x.BankAccountId == accountId)).ToList();
            return lst;
        }

        [Route("bank/records"), HttpPost]
        public IEnumerable<BankRecord> UploadBankRecords(string accountId, [FromBody] BankRecord[] records)
        {
            var acc = GetBankAccount(accountId);
            if (acc == null)
                return null;

            List<BankRecord> ret = new List<BankRecord>();
            // TODO : tester ce système transaction
            var cli = MongoDbHelper.GetRootMongoClient();
            using (var sess = cli.StartSession())
            {
                List<BankRecord> toInsert = new List<BankRecord>();
                var collection = MongoDbHelper.GetClient<BankRecord>(cli);


                foreach (var r in records)
                {
                    r.BankAccountId = accountId;


                    if (r.DownloadedLabel != null)
                    {
                        // on va essayer de vérifier qu'il n'est pas déjà importé
                        var exists = collection.Find(x => x.BankAccountId == accountId
                                        && x.Date == r.Date
                                        && x.DownloadedLabel == r.DownloadedLabel
                                        && x.Amount == r.Amount).FirstOrDefault();
                        if (exists != null)
                        {
                            ret.Add(exists);
                            continue;
                        }
                    }

                    r.Id = Guid.NewGuid().ToString("N");
                    toInsert.Add(r);
                    ret.Add(r);
                }

                collection.InsertMany(toInsert);

                acc.CurrentAmount += (from z in toInsert
                                      select z.Amount).Sum();
                var collAcc = MongoDbHelper.GetClient<BankAccount>(cli);
                var upd = Builders<BankAccount>.Update.Set("CurrentAmount", acc.CurrentAmount);
                collAcc.UpdateOne(x => x.Id == acc.Id, upd);
            }
            return ret;
        }

        [Route("bank/records/upload/{format}/content"), HttpPost, DisableRequestSizeLimit]
        public IEnumerable<BankRecord> UploadBankRecords([FromBody] string content, string format, string accountId = null)
        {
            var acc = GetBankAccounts();

            BankRecord[] ret = Parse(format, content, acc);
            if (ret.Length > 0)
            {
                if (accountId != null)
                {
                    var c = GetBankAccount(accountId);
                    foreach (var r in ret)
                    {
                        if (string.IsNullOrEmpty(r.BankAccountId))
                            r.BankAccountId = accountId;
                    }
                }
                return UploadBankRecords(ret[0].BankAccountId, ret);
            }

            return new BankRecord[0];
        }

        [Route("bank/records/upload/{format}"), HttpPost, DisableRequestSizeLimit]
        public IEnumerable<BankRecord> UploadBankRecords([FromBody] IFormFile file, string format, string accountId = null)
        {
            string content = null;
            using (var st = file.OpenReadStream())
            using (var rdr = new StreamReader(st))
                content = rdr.ReadToEnd();

            return UploadBankRecords(content, format, accountId);
        }

        [Route("bank/records/{recordId}/categorize/{categoryId}"), HttpGet]
        public bool CategorizeBankRecord(string recordId, string categoryId)
        {
            var collection = MongoDbHelper.GetClient<BankRecord>();
            var lst = collection.Find(x => x.Id == recordId).FirstOrDefault();

            if (lst == null)
                return false;

            var up = Builders<BankRecord>.Update
                    .Set("BudgetCategoryId", categoryId);

            collection.UpdateOne(x => x.Id == recordId, up);
            return true;
        }
        #endregion


    }
}
