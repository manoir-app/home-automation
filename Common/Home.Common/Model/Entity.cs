using Home.Common.HomeAutomation;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Model
{
    public class Entity
    {
        public const string EntityKindSun = "manoirapp:sun";
        public const string EntityKindWeather = "manoirapp:weather";

        public const string EntityKindHomeAutomationDevice = "manoirapp:" + Device.DeviceKindHomeAutomation;
        public const string EntityKindMobileDevice = "manoirapp:" + Device.DeviceKindMobileDevice;


        public Entity()
        {
            Roles = new List<string>();
            Datas = new Dictionary<string, EntityData>();
            LastUpdate = DateTimeOffset.Now;
        }

        public Entity(Device device) : this()
        {
            Id = device.Id;
            EntityKind = "manoirapp:device/" + device.DeviceKind;
            Name = device.DeviceGivenName;
            MeshId = device.MeshId;
            Roles.Add(device.DeviceKind);

            if(device.DeviceRoles != null)
            {
                foreach(var rl in device.DeviceRoles)
                    Roles.Add($"{device.DeviceKind}:{rl}") ;
            }

            if(device.Datas!=null)
            {
                foreach (var data in device.Datas)
                {
                    EntityData newdt = new EntityData(data.Value);
                    this.Datas.Add(data.Name, newdt);
                }
            }
        }

        public Entity(AutomationMeshLocationInfo locationInfo) : this()
        {
            Id = "weather:local";
            EntityKind = EntityKindWeather;
            Name = "Weather";
            MeshId = "local";

            // info meteo

            if (locationInfo.Weather != null && locationInfo.Weather.Count > 0)
            {
                var wt = (from z in locationInfo.Weather
                           where z.DateDebut <= DateTimeOffset.Now
                           && z.DateFin >= DateTimeOffset.Now
                           select z).FirstOrDefault();
                if (wt == null)
                    wt = (from z in locationInfo.Weather
                          where z.DateFin >= DateTimeOffset.Now
                           orderby z.DateDebut ascending
                           select z).FirstOrDefault();
                this.Datas.Add("Weather", new EntityData(wt.Kind.ToString()));
                this.Datas.Add("Temperature", new EntityData(wt.Temperature));
                this.Datas.Add("Until", new EntityData(wt.DateFin));
            }


            // hazards
            if (locationInfo.WeatherHazards!=null && locationInfo.WeatherHazards.Count>0)
            {
                var cur = (from z in locationInfo.WeatherHazards
                           where z.DateDebut <= DateTimeOffset.Now
                           && z.DateFin >= DateTimeOffset.Now
                           orderby (int)z.Severity descending
                           select z).FirstOrDefault();
                if(cur==null)
                    cur = (from z in locationInfo.WeatherHazards
                           where z.DateFin >= DateTimeOffset.Now
                           orderby z.DateDebut ascending, (int)z.Severity descending
                           select z).FirstOrDefault();
                if(cur!=null)
                {
                    var haz = new EntityData("StartDate", new EntityData(cur.DateDebut));
                    haz.ComplexValue.Add("EndDate", new EntityData(cur.DateFin));
                    haz.ComplexValue.Add("Severity", new EntityData(cur.Severity.ToString()));
                    haz.ComplexValue.Add("Kind", new EntityData(cur.Kind.ToString()));
                    this.Datas.Add("Hazard", haz);
                }
            }
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string EntityKind { get; set; }
        public string DefaultImageUrl { get; set; }
        public string CurrentImageUrl { get; set; }
        
        public string MeshId { get; set; }
        public string LocationId { get; set; }

        public DateTimeOffset LastUpdate { get; set; }
        public List<string> Roles { get; set; }
        public Dictionary<string, EntityData> Datas { get; set; }
    }



    public class EntityData
    {
        public EntityData()
        {

        }

        public EntityData(string simpleValue)
        {
            SimpleValue = simpleValue;
            SimpleType = "System.String";
        }

        public EntityData(long simpleValue)
        {
            IntSimpleValue = simpleValue;
            SimpleType = "System.Int64";
        }

        public EntityData(Decimal simpleValue)
        {
            DecimalSimpleValue = simpleValue;
            SimpleType = "System.Decimal";
        }

        public EntityData(DateTimeOffset simpleValue)
        {
            DateSimpleValue = simpleValue;
            SimpleType = "System.DateTimeOffset";
        }

        public EntityData(string name, EntityData subItem)
        {
            ComplexValue = new Dictionary<string, EntityData>();
            ComplexValue.Add(name, subItem);
        }

        public bool IsComplex()
        {
            return ComplexValue != null && ComplexValue.Count > 0;
        }

        public string SimpleType { get; set; }
        public string SimpleValue { get; set; }
        public long? IntSimpleValue { get; set; }
        public decimal? DecimalSimpleValue { get; set; }
        public DateTimeOffset? DateSimpleValue { get; set; }
        public Dictionary<string, EntityData> ComplexValue { get; set; }

        public override string ToString()
        {
            return ToString("");
        }
        private string ToString(string indent)
        {
            if(ComplexValue!=null && ComplexValue.Count>0)
            {
                StringBuilder blr = new StringBuilder();
                foreach(var k in ComplexValue)
                {
                    blr.Append(indent);
                    blr.Append(k.Key);
                    blr.Append("=");
                    blr.AppendLine(k.Value.ToString("  "));
                }
                return blr.ToString();
            }

            if (SimpleValue != null)
                return indent + SimpleValue;
            if (IntSimpleValue.HasValue)
                return indent + IntSimpleValue.Value.ToString("0");
            if (DateSimpleValue.HasValue)
                return indent + DateSimpleValue.Value.ToUniversalTime().ToString("u");

            return "";
        }

    }
}
