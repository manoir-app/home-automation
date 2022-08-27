using Home.Agents.Aurore.UserNotifications;
using Home.Common.Messages;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Home.Agents.Aurore.Greetings
{
    internal static class CommonScreenGreetings
    {

        public static MessageResponse GetGreetings(GreetingsMessage request, bool withNotify)
        {
            var mesh = PrivacyModeChangedNotificationHandler.GetMesh();
            var usrs = PrivacyModeChangedNotificationHandler.GetLocalPresentUsers();

            if (mesh == null)
                return GreetingsMessageResponse.GenericFail;

            request.Users = usrs;

            if (usrs.Count == 0)
                return GetNoOneHomeMessage();
            else if (!mesh.CurrentPrivacyMode.HasValue)
            {
                if (usrs.Count == 1)
                    return SingleUserGreetings.GetGreetings(request, false);

                var usrMain = (from z in usrs where z.IsMain select z).ToList();
                if (usrMain.Count == usrs.Count)
                {
                    // normalement, ca devrait toujours être le cas, sinon le mode privé serait activé
                    request.Users = usrMain;
                    return MultipleUserGreetings.GetGreetings(request);
                }

                return GreetingsMessageResponse.GenericFail;
            }
            else
                return GetPrivacyModeMessage(mesh.CurrentPrivacyMode.Value);

        }

        private static MessageResponse GetPrivacyModeMessage(Common.Model.AutomationMeshPrivacyMode value)
        {
            return GetNoOneHomeMessage();
        }

        private static GreetingsMessageResponse GetNoOneHomeMessage()
        {
            var ret = new GreetingsMessageResponse();
            ret.Response = "OK";

            StringBuilder blr = new StringBuilder();
            blr.Append("Bonjour,");

            ret.Items.Add(new GreetingsMessageResponseItem()
            {
                Content = blr.ToString(),
                ContentKind = GreetingsMessageResponseItem.GreetingsMessageResponseItemKind.HeaderContent
            });

            blr = new StringBuilder();
            blr.Append("Nous sommes le **");
            blr.Append(DateTime.Today.ToString("dd MMMM"));
            blr.Append("**.");
            ret.Items.Add(new GreetingsMessageResponseItem()
            {
                Content = blr.ToString(),
                ContentKind = GreetingsMessageResponseItem.GreetingsMessageResponseItemKind.DateContent
            });

            return ret;

        }
    }
}
