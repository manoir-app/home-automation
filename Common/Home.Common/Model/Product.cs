using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Model
{
    [Flags]
    public enum ProductMetaType
    {
        Consumable = 1,
        Perishable = 2,
        Food = 16,
        Beverage = 32,
        HouseWare = 128,
        HouseKeeping= 64
    }

    public enum ProductOrigin
    {
        Unknown,
        Bought,
        SelfMade
    }


    public class ProductType
    {
        public string Id { get; set; }
        public string Label { get; set; }

        public ProductMetaType MetaType { get; set; }
        public Dictionary<string, ProductImages> Images { get; set; }
    }

    public class Product
    {
        public Product()
        {
            Categories = new List<string>();
            Images = new Dictionary<string, ProductImages>();
            Packagings = new List<ProductPackaging>();
            StorageInfo = new ProductStorageInfo();
        }

        public string Id { get; set; }
        public string Label { get; set; }
        public string GenericName { get; set; }
        public ProductMetaType MetaType { get; set; }
        public string ProductTypeId { get; set; }
        public Dictionary<string, ProductImages> Images { get; set; }

        public int? CommonDuration { get; set; }

        public UnitMetaType UnitMetaType { get; set; }
        public string UnitId { get; set; }
        public string UnitExplanation { get; set; }

        public List<string> Categories { get; set; }

        public List<ProductPackaging> Packagings { get; set; }

        public ProductStorageInfo StorageInfo { get; set; }

        public ProductConsumption ConsumptionInfo { get; set; }
    }

    public class ProductConsumption
    {
        public int? AutoDecrementAmount { get; set; }
        public TimeSpan? AutoDecrementPeriod { get; set; }
        public DateTimeOffset? LastAutoDecrementDate { get; set; }
    }

    public class ProductStorageInfo
    {
        public int? StandardExpirationDuration { get; set; }
        public int? StandardExpirationTolerance { get; set; }
        public string DefaultStorageIdWhenNew { get; set; }
        public string DefaultStorageSubElementIdWhenNew { get; set; }
        public string DefaultStorageCommentWhenNew { get; set; }
        public string DefaultStorageIdWhenOpened { get; set; }
        public string DefaultStorageSubElementIdWhenOpened { get; set; }
        public string DefaultStorageCommentWhenOpened { get; set; }
    }

    public class ProductImages
    {
        public string[] BigImageUrls { get; set; }
        public string[] SmallImageUrls { get; set; }

        public string[] IconUrls { get; set; }
    }

    public enum ProductPackagingType
    {
        Box,
        Bottle
    }

    public class ProductPackaging
    {
        public string Id { get; set; }
        public string Label { get; set; }

        public int ProductPackagingType { get; set; }

        public ProductOrigin OriginKind { get; set; }
        public string OriginId { get; set; }


        public string Ean { get; set; }
        public decimal? PackQuantity { get; set; }
        public Dictionary<string, ProductImages> Images { get; set; }

        public int? StandardExpirationDuration { get; set; }

    }


}
