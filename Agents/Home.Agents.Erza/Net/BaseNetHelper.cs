using Home.Common;
using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace Home.Agents.Erza.Net
{
    internal abstract class BaseNetHelper
    {
        private static List<User> _users = null;
        private static DateTimeOffset _lastRefresh = DateTimeOffset.MinValue;
        private static User _chosenUser = null;
        private static string _token = null;


        protected bool ConvertToClassicDnsDomainNames(ref string domain, ref string subdomain )
        {
            StringBuilder realDomain = new StringBuilder();
            var parts = domain.Split('.');
            if (parts.Length < 2)
                return false;

            for (int i = parts.Length - 2; i < parts.Length; i++)
            {
                if (realDomain.Length > 0)
                    realDomain.Append(".");
                realDomain.Append(parts[i]);
            }

            StringBuilder realSubDomain = new StringBuilder();
            realSubDomain.Append(subdomain);
            if (parts.Length > 2)
            {
                for (int i = 0; i < parts.Length - 2; i++)
                {
                    realSubDomain.Append(".");
                    realSubDomain.Append(parts[i]);
                }
            }

            domain = realDomain.ToString();
            if(domain.StartsWith("."))
                domain = domain.Substring(1);  
            subdomain = realSubDomain.ToString();
            if(subdomain.StartsWith("."))
                subdomain = subdomain.Substring(1);     

            return true;
        }
        protected string GetToken()
        {
            if (_users == null || Math.Abs((DateTimeOffset.Now - _lastRefresh).TotalMinutes) > 5)
                _users = AgentHelper.GetMainUsers("erza");

            foreach(var user in _users)
            {
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        using (var cli = new MainApiAgentWebClient("erza"))
                        {
                            var t = cli.DownloadData<string>($"oauth/microsoft/token/" + user.Id);
                            _chosenUser = user;
                            _token = t;
                        }
                    }
                    catch (WebException ex)
                    {
                        Thread.Sleep(1000);
                    }
                }
            }

            return _token;
        }

        public abstract bool SetTxtValue(string domain, string subdomain, string txtValue);

    }
}
