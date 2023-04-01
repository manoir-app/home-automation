using Home.Common.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Graph.Common
{
    public partial class ConditionHelper
    {
        private string _agent = null;
        public static ConditionHelper GetForAgent(string agentId)
        {
            return new ConditionHelper(agentId);
        }

        public static ConditionHelper GetForServer()
        {
            return new ConditionHelper(null);
        }


        protected ConditionHelper(string agentId)
        {
            _agent = agentId;

        }
        public bool Evaluate(Condition condition)
        {
            ClearCaches();
            return DoEvaluate(condition);
        }

        private Dictionary<string, object> _cache = new Dictionary<string, object>();

        private void ClearCaches()
        {
            _cache = new Dictionary<string, object>();
        }

        public bool DoEvaluate(Condition condition)
        {

            if (condition == null)
                return true;

            switch (condition.Kind)
            {
                case ConditionKind.And:
                    if (condition.SubConditions == null)
                        return true;
                    foreach (var sub in condition.SubConditions)
                    {
                        if (!DoEvaluate(sub))
                            return false;
                    }
                    return true;
                case ConditionKind.Or:
                    if (condition.SubConditions == null)
                        return true;
                    foreach (var sub in condition.SubConditions)
                    {
                        if (DoEvaluate(sub))
                            return true;
                    }
                    return false;
                case ConditionKind.UserCheck:
                    return EvaluateForUser(condition);
            }

            return false;
        }
    }
}
