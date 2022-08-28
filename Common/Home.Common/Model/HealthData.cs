using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Model
{
    public class HealthData
    {
        public HealthData()
        {
            WeightDatas = new List<WeightData>();
        }

        public List<WeightData> WeightDatas { get; set; }
    }

    public class WeightData
    {
        public decimal Value { get; set; }
        public DateTimeOffset Date { get; set; }
    }

}
