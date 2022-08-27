using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Model
{
    public class RoutineData
    {
        public DateTimeOffset? NextWakeUpTime { get; set; }


        public RoutineData()
        {

        }

        public RoutineData(RoutineDataWithUser data)
        {
            this.NextWakeUpTime = data.NextWakeUpTime;
        }
    }

    public class RoutineDataWithUser : RoutineData
    {
        public string UserId { get; set; }
        public string UserName { get; set; }

        public RoutineDataWithUser()
        {

        }

        public RoutineDataWithUser(RoutineData data, User user)
        {
            this.NextWakeUpTime = data.NextWakeUpTime;
            this.UserId = user.Id;
            this.UserName = user.Name;
        }

        public RoutineDataWithUser(User user)
        {
            this.NextWakeUpTime = user?.Routine.NextWakeUpTime;
            this.UserId = user.Id;
            this.UserName = user.Name;
        }

    }

}
