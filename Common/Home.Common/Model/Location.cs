using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Home.Common.Model
{
    public enum LocationKind
    {
        Home = 0, 
        Work = 1,
        Family = 2,
        Friends = 3
    }

    public class Location
    {
        public Location()
        {
            Zones = new List<LocationZone>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        
        public bool HasAutomationsMesh { get; set; }

        public LocationKind Kind { get; set; }

        public GeoCoordinates Coordinates { get; set; }

        public string StreetAddress { get; set; }

        public string ZipCode { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }

        public List<LocationZone> Zones { get; set; }
    }

    public class LocationZone
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public List<LocationRoom> Rooms { get; set; }

        public LocationZone()
        {
            Rooms = new List<LocationRoom>();
        }
    }

    public enum RoomKind
    {
        Generic,
        Corridor,
        Bedroom,
        Bathroom,
        Kitchen,
        LivingRoom,
        DiningRoom,
        Office,
        ReadingRoom,
        Pool
    }

    public class LocationRoom
    {
        public LocationRoom()
        {
            RoomMappingForServices = new Dictionary<string, List<string>>();
            GroupMappingForServices = new Dictionary<string, List<string>>();
            Shape = new List<LocationPoint>();
            Walls = new List<LocationWall>();
            Properties = new LocationRoomProperties();
        }

        public string Id { get; set; }
        public string Name { get; set; }

        public Dictionary<string, List<string>> RoomMappingForServices { get; set; }
        public Dictionary<string, List<string>> GroupMappingForServices { get; set; }

        public RoomKind RoomKind { get; set; }

        public int FloorLevel { get; set; }

        public LocationRoomProperties Properties { get; set; }

        public List<LocationPoint> Shape { get; set; }
        public List<LocationWall> Walls { get; set; }
    }

    public class LocationRoomProperties
    {
        public decimal? Temperature { get; set; }
        public decimal? Humidity { get; set; }

        public Dictionary<string, string> MoreProperties { get; set; }
    }
    public class LocationPoint
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class LocationWall
    {
        public LocationWall()
        {
            Points = new List<LocationPoint>();
            Thickness = 1;
        }

        public List<LocationPoint> Points { get; set; }

        public int Thickness { get; set; }
    }


}
