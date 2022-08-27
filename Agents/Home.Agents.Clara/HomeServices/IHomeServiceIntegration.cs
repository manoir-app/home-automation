using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Home.Agents.Clara.HomeServices
{
    public interface IHomeServiceProvider
    {
        void Init(HomeServicesConfig config);
    }

    public interface IHomeServiceScheduler
    {
        IEnumerable<TodoItem> GetNextScheduledItems(DateTimeOffset maxDate);
    }
}
