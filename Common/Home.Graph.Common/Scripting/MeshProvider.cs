using Home.Common.Model;
using Microsoft.CodeAnalysis;
using MongoDB.Driver;
using System;
using System.Linq;

namespace Home.Graph.Common.Scripting
{
    public class MeshProvider
    {
        public class DynamicMesh
        // : DynamicObject 
        // peut être qu'un jour ça fonctionnera :
        // https://github.com/dotnet/roslyn/issues/3194
        // mais après 7 ans...
        {
            private Home.Common.Model.AutomationMesh _mesh = null;
            private Type _t = null;

            public DynamicMesh(Home.Common.Model.AutomationMesh mesh)
            {
                _mesh = mesh;
            }

            public string Id { get { return _mesh.Id; } }
            public AutomationMeshPrivacyMode? PrivacyMode { get { return _mesh.CurrentPrivacyMode; } }
            public string PrivacyModeLabel { get { return _mesh.CurrentPrivacyMode.HasValue ? _mesh.CurrentPrivacyMode.Value.ToString() : "none"; } }

            public class ReadOnlyWeatherHazard
            {
                public WeatherHazardKind Kind { get; internal set; }
                public WeatherHazardSeverity Severity { get; internal set; }
                public string Label { get; internal set; }

                public ReadOnlyWeatherHazard(WeatherHazard orig)
                {
                    Kind = orig.Kind;
                    Severity = orig.Severity;
                    Label = orig.Label;
                }
            }

            public ReadOnlyWeatherHazard CurrentWeatherHazard
            {
                get
                {
                    var wt = (from z in _mesh.LocationInfo.WeatherHazards
                              where z.DateDebut <= DateTimeOffset.Now
                              && z.DateFin >= DateTimeOffset.Now
                              select new ReadOnlyWeatherHazard(z)).FirstOrDefault();
                    return wt;
                }
            }

            public class ReadOnlyWeatherInfo
            {
                public WeatherInfoKind Kind { get; internal set; }
                public int Temperature { get; internal set; }
                public int TemperatureFeltAs { get; internal set; }
                public bool RiskOfThumber { get; internal set; }

                public ReadOnlyWeatherInfo(WeatherInfo orig)
                {
                    Kind = orig.Kind;
                    Temperature = orig.Temperature;
                    TemperatureFeltAs = orig.TemperatureFeltAs;
                    RiskOfThumber = orig.RiskOfThumber;
                }
            }

            public ReadOnlyWeatherInfo CurrentWeather
            {
                get
                {
                    var wt = (from z in _mesh.LocationInfo.Weather
                              where z.DateDebut <= DateTimeOffset.Now
                              && z.DateFin >= DateTimeOffset.Now
                              select new ReadOnlyWeatherInfo(z)).FirstOrDefault();
                    return wt;
                }
            }
        }

        public DynamicMesh Local
        {
            get
            {
                var coll = MongoDbHelper.GetClient<Home.Common.Model.AutomationMesh>();
                var item = coll.Find(x => x.Id.Equals("local")).FirstOrDefault();

                if (item != null)
                {
                    var ret = new DynamicMesh(item);
                    return ret;
                }
                return null;
            }
        }
    }
}
