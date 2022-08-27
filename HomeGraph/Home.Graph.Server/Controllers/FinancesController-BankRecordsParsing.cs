using Home.Common.Model;
using Home.Graph.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Home.Graph.Server.Controllers
{
    partial class FinancesController
    {
        private static BankRecord[] Parse(string format, string fileContent, List<BankAccount> accounts)
        {
            if(format.Equals("OFX", StringComparison.InvariantCultureIgnoreCase))
                return new OfxParser().Parse(fileContent, accounts);

            throw new InvalidDataException($"{format} is not a supported file format");
        }

        internal abstract class FichierBanqueParser
        {
            internal abstract BankRecord[] Parse(string fileContent, List<BankAccount> accounts);
        }

        internal class OfxParser : FichierBanqueParser
        {
            internal override BankRecord[] Parse(string content, List<BankAccount> accounts)
            {
                var lesEcritures = new List<BankRecord>();

                using (var s = new StringReader(content))
                {
                    var ec = ParseOFX(s, accounts);
                    if (ec != null)
                    {
                        lesEcritures.Add(ec);
                    }
                }
                return lesEcritures.ToArray();
            }


            private BankRecord ParseOFX(StringReader s, List<BankAccount> accounts)
            {

                var ec = new BankRecord();
                string q = "";


                while (q != "/STMTTRN")
                {
                    string num;
                    String r = s.ReadLine();
                    if (r == null)
                    {
                        return null;
                    }

                    if (r.Length == 0)
                        continue;

                    string w = r.Trim();

                    string[] t = w.Split(new char[] { '<', '>' },
                        StringSplitOptions.RemoveEmptyEntries);
                    q = t[0];
                    switch (q)
                    {
                        case "ACCTID":
                            num = t[1];

                            foreach (var p in accounts)
                            {
                                if (p.AccountNumber.Length > 5)
                                {
                                    string sDeb = p.AccountNumber.Substring(0, 3);
                                    string sFin = p.AccountNumber.Substring(p.AccountNumber.Length - 3);
                                    if (num.StartsWith(sDeb) && num.EndsWith(sFin))
                                    {
                                        ec.BankAccountId = p.Id;
                                    }
                                }
                            }
                            break;

                        case "DTUSER": ec.Date = DateTime.ParseExact(t[1], "yyyyMMdd", CultureInfo.InvariantCulture); break;
                        case "DTPOSTED": ec.Date = DateTime.ParseExact(t[1], "yyyyMMdd", CultureInfo.InvariantCulture); break;
                        case "TRNAMT": ec.Amount = Decimal.Parse(t[1], System.Globalization.CultureInfo.InvariantCulture); break;
                        case "FITID": ec.RecordNumber = t[1]; break;
                        case "NAME": ec.DownloadedLabel = t[1]; break;
                        case "MEMO": ec.DownloadedMoreInfo = t[1]; break;
                    }

                }
                return ec;
            }

        }


    }
}
