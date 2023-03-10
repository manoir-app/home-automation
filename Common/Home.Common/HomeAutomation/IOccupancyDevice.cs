using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.HomeAutomation
{
    public interface IOccupancyDevice
    {
    }

    public enum OccupancyState
    {
        ActivePresence,
        RecentPresent,
        NoPresence
    }

}
