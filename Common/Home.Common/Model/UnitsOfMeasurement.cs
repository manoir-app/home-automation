using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Model
{
    public enum UnitMetaType
    {
        Mass,
        Volume,
        Discrete,
        Length,
        Temperature,
        Movement,
        Time,
        Light,
        Pressure,
    }


    public class UnitOfMeasurement
    {
        public UnitOfMeasurement()
        {
            ConvertRatios = new Dictionary<string, decimal>();
        }

        public string Id { get; set; }
        public string Label { get; set; }
        public UnitMetaType MetaType { get; set; }
        public string Symbol { get; set; }

        public Dictionary<string, decimal> ConvertRatios { get; set; }
    }
}
