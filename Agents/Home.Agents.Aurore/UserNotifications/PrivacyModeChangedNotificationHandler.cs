using Home.Common;
using Home.Common.Messages;
using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace Home.Agents.Aurore.UserNotifications
{
    internal static class PrivacyModeChangedNotificationHandler
    {
        private static AutomationMesh _mesh = null;
        private static DateTimeOffset _lastMeshSync = DateTimeOffset.MinValue;
        private static List<User> _userPresent = null;
        private static DateTimeOffset _lastPresentSync = DateTimeOffset.MinValue;

        internal static AutomationMesh GetMesh()
        {
            if(_mesh == null)
                _mesh = AgentHelper.GetLocalMesh("aurore");

            if (Math.Abs((DateTimeOffset.Now - _lastMeshSync).TotalSeconds) > 5)
                _mesh = AgentHelper.GetLocalMesh("aurore");

            _lastMeshSync = DateTimeOffset.UtcNow;

            return _mesh;
        }

        internal static List<User> GetLocalPresentUsers()
        {
            if (_userPresent == null)
                _userPresent = GetLocalPresentUsersFromServer();

            if (Math.Abs((DateTimeOffset.Now - _lastPresentSync).TotalSeconds) > 5)
                _userPresent = GetLocalPresentUsersFromServer();

            _lastPresentSync = DateTimeOffset.UtcNow;

            return _userPresent;
        }

        private static List<User> GetLocalPresentUsersFromServer()
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var cli = new MainApiAgentWebClient("erza"))
                    {
                        return cli.DownloadData<List<User>>("v1.0/users/presence/mesh/local/all");
                    }
                }
                catch (Exception ex)
                {

                }
            }

            return new List<User>();
        }

        public static MessageResponse HandleMessage(MessageOrigin origin, string topic, string messageBody)
        {
            if (!topic.Equals("system.mesh.privacymode.changed"))
                return MessageResponse.GenericFail;

            _mesh = AgentHelper.GetLocalMesh("aurore");
            _lastPresentSync = DateTime.MinValue;
            var usrs = AgentHelper.GetMainUsers("aurore");
            foreach (var usr in usrs)
            {
                if (!usr.IsMain)
                    continue;

                UserNotification not = new UserNotification()
                {
                    Date = DateTimeOffset.Now,
                    Description = _mesh.CurrentPrivacyMode.HasValue ? "Votre domicile est passé en mode privé" : "Votre domicile est de nouveau en mode normal",
                    Title = _mesh.CurrentPrivacyMode.HasValue?"Mode privé activé":"Mode privé désactivé",
                    Id = Guid.NewGuid().ToString(),
                    UserId = usr.Id,
                    Importance = UserNotificationImportance.High
                };

                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        using (var cli = new MainApiAgentWebClient("aurore"))
                        {
                            var t = cli.UploadData<bool, UserNotification>($"v1.0/users/{usr.Id}/notify?sendToMobile=true",
                                "POST", not);
                            break;
                        }
                    }
                    catch (WebException ex)
                    {
                        Thread.Sleep(1000);
                    }
                }
            }

            return MessageResponse.OK;
        }
    }
}
