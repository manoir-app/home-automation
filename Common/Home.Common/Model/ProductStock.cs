using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Model
{
    public class ProductStock
    {
        public ProductStock()
        {
            Reservations = new List<ReservedProduct>();
        }

        public string Id { get; set; }
        public string ProductId { get; set; }
        public string StorageUnitId { get; set; }
        public ProductMetaType ProductMetaType { get; set; }
        public bool IsMainStorage { get; set; }


        public DateTimeOffset DateAdded { get; set; }

        public DateTimeOffset? DateExpiration { get; set; }

        public decimal QuantityNew { get; set; }
        public decimal QuantityOpened { get; set; }
        public decimal OriginalQuantity { get; set; }
        public string UnitId { get; set; }

        public string PackagingId { get; set; }
        public ProductPackagingType PackagingType { get; set; }

        public Dictionary<string, string> AssociatedTags { get; set; }

        public List<ReservedProduct> Reservations { get; set; }
    }


    public enum ProductReservationKind
    {
        ForRecipe,
        ForPlannedMeal
    }
    public class ReservedProduct
    {
        public ProductReservationKind Kind { get; set; }

        public string ReservationId { get; set; }
        public string AssociatedId { get; set; }

        public decimal Quantity { get; set; }
    }


}
