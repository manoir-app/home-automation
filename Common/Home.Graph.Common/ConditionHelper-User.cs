using Home.Common;
using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Graph.Common
{
    partial class ConditionHelper
    {
        private bool EvaluateForUser(Condition condition)
        {
            if (condition == null)
                return true;
            if (string.IsNullOrEmpty(condition.PropertyName))
                return true;
            switch (condition.PropertyName.ToLowerInvariant())
            {
                case "ispresent":
                case "is-present":
                    return CheckUserPresence(condition);
            }

            return false;
        }



        private bool CheckUserPresence(Condition condition)
        {
            if (condition.InValues != null && condition.InValues.Length > 0)
                throw new InvalidOperationException("InValues not usable with User.IsPresent");

            condition.Normalize();

            condition.Value = condition.Value.ToLowerInvariant();
            if (!condition.Value.Equals("true") && !condition.Value.Equals("false"))
                throw new InvalidOperationException("Value must be either true ou false with User.IsPresent");
            if (!condition.Operator.Equals("=="))
                throw new InvalidOperationException("Operator must be == or != with User.IsPresent");

            bool isPresent = IsPresent(condition.ElementId);
            if (condition.Value.Equals("true"))
                return isPresent;
            else
                return !isPresent;
        }

        private bool IsPresent(string userName)
        {
            lock (this)
            {
                List<User> tmp = GetLocalPresentUsers();

                // on teste si au moins une personne est là
                if (string.IsNullOrEmpty(userName)
                    || "*".Equals(userName))
                {
                    return tmp.Count > 0;
                }

                // ou on cherche une personne en particulier
                foreach (var usr in tmp)
                {
                    if (usr.Id.Equals(userName))
                        return true;
                }
            }

            return false;
        }

        private List<User> GetLocalPresentUsers()
        {
            List<User> tmp = null;
            if (_cache.TryGetValue("LocalPresentUsers", out object fromCache))
                tmp = fromCache as List<User>;
            if (tmp == null)
            {
                tmp = UserHelper.GetLocalPresentUsers();
                _cache.Add("LocalPresentUsers", tmp);
            }

            return tmp;
        }
    }
}
