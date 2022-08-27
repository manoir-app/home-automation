using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Model
{
    public enum StorageUnitRole
    {
        None,
        Fridge,
        Freezer
    }

    public class StorageUnit
    {
        public StorageUnit()
        {
            AllowedProductsMetaTypes = new List<ProductMetaType>();
            AssociatedDevices = new Dictionary<string, string>();
            SubElements = new List<StorageUnitSubElement>();
        }

        public string Id { get; set; }

        public StorageUnitRole Role { get; set; }
        public string RoomId { get; set; }
        public string Label { get; set; }
        public decimal CoordinateX { get; set; }
        public decimal CoordinateY { get; set; }

        public StorageUnitForDisplay ShapeOnFloorPlan { get; set; }
        public StorageUnitForDisplay ShapeDetails { get; set; }

        public List<ProductMetaType> AllowedProductsMetaTypes { get; set; }
        public List<StorageUnitSubElement> SubElements { get; set; }

        public Dictionary<string, string> AssociatedDevices { get; set; }
    }

    public class StorageUnitForDisplay
    {
        public StorageUnitForDisplay()
        {
            Shape = new List<LocationPoint>();
            Images = new Dictionary<string, string>();
        }

        public List<LocationPoint> Shape { get; set; }

        public Dictionary<string, string> Images { get; set; }
    }

    public class StorageUnitSubElement
    {
        public StorageUnitSubElement()
        {
            AllowedProductsMetaTypes = new List<ProductMetaType>();

        }

        public StorageUnitRole Role { get; set; }
        public string Id { get; set; }
        public string Label { get; set; }
        public StorageUnitForDisplay ShapeInUnit { get; set; }
        public decimal CoordinateX { get; set; }
        public decimal CoordinateY { get; set; }
        public List<ProductMetaType> AllowedProductsMetaTypes { get; set; }

    }

}
