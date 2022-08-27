using Home.Common;
using Home.Common.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace Home.Agents.Aurore.Greetings
{
    internal static class GreetingsHandler
    {
        public static MessageResponse GetGreetings(GreetingsMessage request)
        {
            if (request == null)
                return null;

            switch(request.Destination)
            {
                case GreetingsMessage.GreetingsDestination.Screen:
                    return CommonScreenGreetings.GetGreetings(request, false);
                default:
                    if (request.Users.Count != 1)
                        return null;
                    return SingleUserGreetings.GetGreetings(request, true);
            }

        }

        internal static void NotifyChangeInGreetings(string userName, GreetingsMessageResponse greetings)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback((a) => {

                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        using (var cli = new MainApiAgentWebClient("aurore"))
                        {
                            bool t = cli.UploadData<bool, GreetingsMessageResponse>($"v1.0/users/interactions/greetings/all/{userName}", "POST", greetings);
                            break;
                        }
                    }
                    catch (WebException ex)
                    {
                        Thread.Sleep(1000);
                    }
                }
            }), null);
        }

        internal static HomeStatusItem[] AddMeshStatus(GreetingsMessageResponse ret)
        {
            StringBuilder blr = new StringBuilder();

            blr.Append("Nous sommes le **");
            blr.Append(DateTime.Today.ToString("dd MMMM"));
            blr.Append("**.");
            ret.Items.Add(new GreetingsMessageResponseItem()
            {
                Content = blr.ToString(),
                ContentKind = GreetingsMessageResponseItem.GreetingsMessageResponseItemKind.DateContent
            });

            blr = new StringBuilder();
            var tmp = AuroreNewsItemService.GetGlobalMeshItems();
            if (tmp != null && tmp.Length > 0)
            {
                foreach (var r in tmp)
                {
                    blr.Append(" ");
                    blr.Append(r.Message);
                }
            }
            if (blr.Length > 0)
            {
                ret.Items.Add(new GreetingsMessageResponseItem()
                {
                    Content = blr.ToString(),
                    ContentKind = GreetingsMessageResponseItem.GreetingsMessageResponseItemKind.MainContent
                });
            }
            return tmp;
        }

        internal static HomeStatusItem[] AddUserStatus(GreetingsMessageResponse ret, Common.Model.User user, bool withName)
        {
            StringBuilder blr = new StringBuilder();
            var tmp = AuroreNewsItemService.GetUserItems(user.Id);
            if (withName)
            {
                blr.Append(user.CommonName);
                blr.Append(" : ");
            }

            if (tmp != null && tmp.Length > 0)
            {
                foreach (var r in tmp)
                    blr.Append(r.Message);
            }
            ret.Items.Add(new GreetingsMessageResponseItem()
            {
                Content = blr.ToString(),
                ContentKind = GreetingsMessageResponseItem.GreetingsMessageResponseItemKind.MainContent
            });
            return tmp;
        }
    }
}
