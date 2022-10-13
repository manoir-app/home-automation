using Home.Common;
using Home.Common.Model;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Home.Agents.Clara.HomeServices.Providers.HouseKeeping
{
    internal class AzaeDotComHomeServiceIntegration : IHomeServiceProvider, IHomeServiceScheduler
    {
        private static Dictionary<string, List<TodoItem>> _allUsersItems = new Dictionary<string, List<TodoItem>>();

        private static DateTimeOffset _lastUpdateTime = DateTimeOffset.MinValue;

        private static Dictionary<string, HomeServicesConfig> _allConfig = new Dictionary<string, HomeServicesConfig>();

        public IEnumerable<TodoItem> GetNextScheduledItems(DateTimeOffset maxDate)
        {
            if (Math.Abs((DateTimeOffset.Now - _lastUpdateTime).TotalHours) > 6)
                RefreshFromWebsite(maxDate.Date).Wait();

            var dt = DateTime.Today.AddDays(-1);

            List<TodoItem> items = new List<TodoItem>();
            foreach (var r in _allUsersItems.Values)
                items.AddRange((from z in r
                                where z.DueDate.HasValue && z.DueDate > dt
                                select z).ToArray());

            return items;
        }

        public void Init(HomeServicesConfig config)
        {
            _allConfig[config.UserId] = config;
        }

        private async Task RefreshFromWebsite(DateTime dtMax)
        {
            _lastUpdateTime = DateTimeOffset.Now;

            using (var playwright = await Playwright.CreateAsync())
            {
                BrowserTypeLaunchOptions opts = null;
#if DEBUG
                opts = new BrowserTypeLaunchOptions()
                {
                    Headless = false
                };
#endif

                var browser = await playwright.Chromium.LaunchAsync(opts);

                foreach (var val in _allConfig.Values)
                    await RefreshFromWebsite(playwright, browser, val.ServiceUsername, val.ServicePassword, dtMax, val);
            }
        }

        private async Task RefreshFromWebsite(IPlaywright playwright, IBrowser browser, string user, string pwd, DateTime dtMax, HomeServicesConfig cfg)
        {
            List<TodoItem> ret = new List<TodoItem>();

            var ctx = await browser.NewContextAsync(new BrowserNewContextOptions
            {
            });
            try
            {
                var page = await ctx.NewPageAsync();
                await page.GotoAsync("https://extranet.a2micile.com/login");
                await page.FillAsync("#username", user);
                await page.FillAsync("#password", pwd);
                await page.ClickAsync("button[type=submit]");
                var url = page.Url;
                if (!url.Equals("https://extranet.a2micile.com/usager", StringComparison.InvariantCulture))
                    throw new ApplicationException("Azae : Login error => url was " + url + " instead of https://extranet.a2micile.com/usager");
                var dt = DateTime.Today;
                while (dt < dtMax)
                {
                    await page.GotoAsync("https://extranet.a2micile.com/checklist");
                    await page.FillAsync("#datetimepicker_start_value", dt.ToString("dd/MM/yyyy"));
                    await page.FillAsync("#datetimepicker_end_value", dt.AddDays(7).ToString("dd/MM/yyyy"));
                    await page.ClickAsync("button.btn-arrow");

                    await page.WaitForTimeoutAsync(1000);

                    dt = dt.AddDays(7);

                    var ligs = page.Locator("#table_list .tr-validation");
                    var ligsCount = await ligs.CountAsync();
                    for (int i = 0; i < ligsCount; i++)
                    {
                        var lig = ligs.Nth(i);
                        var td = lig.Locator("td[tabindex='0']").First;
                        var div = td.Locator("div").First;
                        DateTimeOffset dtDebut = DateTimeOffset.MinValue, dtFin = DateTimeOffset.MinValue;
                        if (td != null && div != null)
                        {
                            var itemDateStr = await td.GetAttributeAsync("data-sort");
                            if (!DateTimeOffset.TryParseExact(itemDateStr, "yyyy-MM-dd HH:mm", CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal, out dtDebut))
                            {
                                Console.WriteLine("Unable to parse date for azae event : " + itemDateStr);
                                continue;
                            }
                            dtDebut = dtDebut.ToOffset(AgentHelper.UserTimeZone.GetUtcOffset(dtDebut.Date)).AddMilliseconds(0 - AgentHelper.UserTimeZone.GetUtcOffset(dtDebut.Date).TotalMilliseconds);

                            itemDateStr = await div.GetAttributeAsync("data-end");
                            if (itemDateStr.IndexOf("_") > -1)
                                itemDateStr = itemDateStr.Substring(0, itemDateStr.LastIndexOf("_"));
                            itemDateStr = itemDateStr.Replace("_", " ").Trim();
                            if (!DateTimeOffset.TryParseExact(itemDateStr, "yyyy-MM-dd HH:mm", CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal, out dtFin))
                            {
                                Console.WriteLine("Unable to parse date for azae event : " + itemDateStr);
                                continue;
                            }
                            dtFin = dtFin.ToOffset(AgentHelper.UserTimeZone.GetUtcOffset(dtFin.Date)).AddMilliseconds(0 - AgentHelper.UserTimeZone.GetUtcOffset(dtFin.Date).TotalMilliseconds);

                        }
                        td = lig.Locator("td").Nth(3);
                        var type = await td.InnerTextAsync();

                        Console.WriteLine($"Azae : {type} on {dtDebut.Date.ToString("dd/MM/yyyy")} from {dtDebut.ToString("HH:mm-zzz")} to {dtFin.ToString("HH:mm-zzz")}");

                        var tdItem = new TodoItem()
                        {
                            Label = type,
                            OriginItemData = "azae_" + type + "_" + dtDebut.ToString("yyyyMMdd_HHmm") + "-" + dtFin.ToString("yyyyMMdd_HHmm"),
                            DueDate = dtDebut,
                            Duration = (dtFin - dtDebut),
                            Type = TodoItemType.EventItem,
                            PrivacyLevel = PrivacyLevel.SharedWithUsers,
                            ListId = cfg.ListId,
                            Origin = "HomeServices_azae",
                            AutoActivate = true,
                            ScenarioOnEnd = cfg.ScenarioOnEnd,
                            ScenarioOnStart = cfg.ScenarioOnStart
                        };
                        tdItem.Categories.Add("home.events.housekeeping");
                        if(cfg.ExternalUserIds!=null && cfg.ExternalUserIds.Length>0)
                        {
                            foreach (var externalUser in cfg.ExternalUserIds)
                                tdItem.AssociatedUsers.Add(new UserForTodo()
                                {
                                    ShouldUpdatePresence = true,
                                    UserId = externalUser
                                });
                        }

                        ret.Add(tdItem);

                    }
                }

            }
            finally
            {
                try
                {
                    await ctx.DisposeAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unable to dispose Playwright context : " + ex.ToString());
                }
            }

            _allUsersItems[cfg.UserId] = ret;
        }
    }
}
