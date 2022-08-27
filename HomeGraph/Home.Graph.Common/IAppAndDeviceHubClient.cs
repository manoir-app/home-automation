using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common
{
    public interface IAppAndDeviceHubClient
    {
        void NotifyUserChange(string changeType, User user);
        void NotifyMeshChange(string changeType, AutomationMesh mesh);
    }
}
