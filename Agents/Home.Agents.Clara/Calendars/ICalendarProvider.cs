using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Home.Agents.Clara.Calendars
{
    public interface ICalendarProvider
    {
        IEnumerable<TodoItem> GetNextScheduledItems(DateTimeOffset maxDate);

    }
}
