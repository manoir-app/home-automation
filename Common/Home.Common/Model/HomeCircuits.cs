using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Model
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum HomeCircuitKind
    {
        Electrical,
        Heating,
        Gaz,
        HVac,
        Water,
        Smoke,
        Ventilation
    }

    public class HomeCircuit
    {
        public string Id { get; set; }
        public HomeCircuitKind Kind { get; set; }

        public string LocationId { get; set; }

        public List<string> ZoneId { get; set; } = new List<string>();

        public List<HomeCircuitElement> Elements { get; set; } = new List<HomeCircuitElement>();

        public List<HomeCircuitLink> Links { get; set; } = new List<HomeCircuitLink>();

    }

    public class HomeCircuitElement
    {
        public int Id { get; set; }
        public string Kind { get; set; }
        public Point3D Position { get; set; }
        public Size3D Size { get; set; }

        public HomeCircuitElementSchematic Schematic { get; set; }

        public List<HomeCircuitElementDetail> Details { get; set; } = new List<HomeCircuitElementDetail>();
    }

    public class HomeCircuitElementSchematic
    {
        public string Id { get; set; }
        public List<Point3D> Shape { get; set; } = new List<Point3D>();
    }

    public class HomeCircuitElementDetail
    {
        public string Id { get; set; }
        public string Kind { get; set; }
        public string AssociatedShapeId { get; set; }
        public List<string> AssociatedDeviceIds { get; set; } = new List<string>();
        public string Brand { get; set; }
        public string Model { get; set; }

        public string UsageDescription { get; set; }

        public string Label { get; set; }

        public Dictionary<string, string> Properties { get; set; }
    }

    public class HomeCircuitElementInput
    {
        public string Kind { get; set; }
        public Point3D Position { get; set; }
        public string ID { get; set; }
    }

    public class HomeCircuitLink
    {
        public string Id { get; set; }
        public List<Point3D> Positions { get; set; }
        public string StartElementId { get; set; }
        public string StartElementDetailId { get; set; }
        public string StartElementInputId { get; set; }
        public string EndElementId { get; set; }
        public string EndElementDetailId { get; set; }
        public string EndElementInputId { get; set; }


        public string Color { get; set; }
        public string Type { get; set; }
        public string Capacity { get; set; }
        public string LabelAtStart { get; set; }
        public string LabelAtEnd { get; set; }
    }

}
