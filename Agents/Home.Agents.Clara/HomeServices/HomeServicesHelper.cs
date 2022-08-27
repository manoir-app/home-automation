using Home.Common;
using Home.Common.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Home.Agents.Clara.HomeServices
{
    public static class HomeServicesHelper
    {
        public static List<TodoItem> GetNextScheduledItems(DateTimeOffset maxDate)
        {
            List<TodoItem> todoItems = new List<TodoItem>();

            var ag = AgentHelper.GetAgent("clara");
            if (ag == null || string.IsNullOrEmpty(ag.ConfigurationData))
                return todoItems;

            var cfg = JsonConvert.DeserializeObject<ClaraConfigurationData>(ag.ConfigurationData);
            if (cfg.HomeServices == null || cfg.HomeServices.Count == 0)
                return todoItems;

            foreach (var k in cfg.HomeServices.Keys)
            {
                var inte = GetProvider(k, cfg.HomeServices[k]);
                if (inte == null || !(inte is IHomeServiceScheduler))
                    continue;
                Console.WriteLine("Obtention des items depuis " + k);
                var sch = (inte as IHomeServiceScheduler).GetNextScheduledItems(maxDate);
                todoItems.AddRange(sch);
            }

            return todoItems;
        }

        private static IHomeServiceProvider GetProvider(string k, HomeServicesConfig config)
        {
            IHomeServiceProvider integration = null;
            switch (k.ToLowerInvariant())
            {
                case "azae": 
                    integration =  new Providers.HouseKeeping.AzaeDotComHomeServiceIntegration();
                    break;
            }

            if (integration != null)
                integration.Init(config);

            return integration;
        }
    }
}
