using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Model
{
    public enum MeasureAggregationRuleKind
    {
        Average,
        Min,
        Max
    }

    public class MeasureAggregationRule
    {
        public List<MeasureAggregationRuleSource> Sources { get; set; }
        public MeasureAggregationRuleKind Kind { get; set; } = MeasureAggregationRuleKind.Average;

    }

    public enum MeasureAggregationRuleSourceKind
    {
        Device,
        Mqtt,
        Room,
        Level
    }

    public class MeasureAggregationRuleSource
    {

        public string Type { get; set; }
        public string Id { get; set; }
        public decimal Weight { get; set; } = 1M;
    }
}
