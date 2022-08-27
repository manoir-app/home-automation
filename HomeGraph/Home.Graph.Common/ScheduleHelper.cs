using Cronos;
using Home.Common.Model;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Graph.Common
{
    public static class ScheduleHelper
    {
        public static DateTimeOffset GetNextOccurence(SchedulingRule rule) => GetNextOccurence(rule, DateTimeOffset.Now);

        public static DateTimeOffset GetNextOccurence(SchedulingRule rule, DateTimeOffset from)
        {
            switch(rule.Kind)
            {
                case SchedulingRuleKind.CronExpression:
                    return GetNextCronOccurence(rule, from);
                case SchedulingRuleKind.TimeSpanIncrement:
                    return GetNextTimespanOccurence(rule, from);
            }

            return DateTimeOffset.MinValue;
        }

        private static DateTimeOffset GetNextTimespanOccurence(SchedulingRule rule, DateTimeOffset from)
        {
            TimeSpan ts;
            if (!TimeSpan.TryParse(rule.Expression, out ts))
                return DateTimeOffset.MinValue;
            return from.Add(ts);
        }

        private static DateTimeOffset GetNextCronOccurence(SchedulingRule rule, DateTimeOffset from)
        {
            var expression = CronExpression.Parse(rule.Expression, CronFormat.IncludeSeconds);

            var mcoll = MongoDbHelper.GetClient<AutomationMesh>();
            var local = mcoll.Find(x => x.Id == "local").FirstOrDefault();


            TimeZoneInfo tz = null;
            if (!string.IsNullOrEmpty(local.TimeZoneId))
                tz = TimeZoneInfo.FindSystemTimeZoneById(local.TimeZoneId);
            else
            {
                tz = TimeZoneInfo.Utc;
                from = from.ToUniversalTime();
            }

            return expression.GetNextOccurrence(from, tz).GetValueOrDefault();
        }
    }
}
