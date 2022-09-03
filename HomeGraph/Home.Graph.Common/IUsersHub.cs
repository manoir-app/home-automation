using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common
{
    public interface IUsersHub
    {
        void NewNotification(string user, UserNotification notification);
    }
}
