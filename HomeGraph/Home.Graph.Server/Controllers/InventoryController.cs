using Home.Common.Model;
using Home.Graph.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using NodaTime.TimeZones;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Home.Graph.Server.Controllers
{
    [Route("v1.0/inventory")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        public class InventoryAddMovement
        {
            public decimal Quantity { get; set; }
            public string UnitId { get; set; }

            public string ContainerId { get; set; }
            public string StorageUnitId { get; set; }
            public string StorageUnitSubId { get; set; }
            
            public string PackagingId { get; set; }

        }

        [Route("product/{productId}"), HttpPost]
        public IActionResult AddToInventory(string productId, [FromBody] InventoryAddMovement movement)
        {
            if (string.IsNullOrEmpty(productId))
                return new BadRequestResult();
            if(movement== null || movement.Quantity<=0)
                return new BadRequestResult();


            var prodColl = MongoDbHelper.GetClient<Product>();
            var item= prodColl.Find(x => x.Id == productId).FirstOrDefault();

            if (item == null)
                return new NotFoundResult();

            var invColl = MongoDbHelper.GetClient<ProductStock>();
            var stocks = invColl.Find(x => x.ProductId == productId).ToList();

            ProductPackaging pack = null;
            if (!string.IsNullOrEmpty(movement.PackagingId))
            {
                pack = item.Packagings.Where(c => c.Id == movement.PackagingId).FirstOrDefault();
            }

            if (pack == null)
                pack = item.Packagings.FirstOrDefault();

            var st = FindBestMatch(item, stocks, movement, pack);
            if(st!=null)
            {
                decimal newQty = ConvertQuantity(movement.Quantity, movement.UnitId, item.UnitId, pack);
                st.QuantityNew += newQty;
                st.OriginalQuantity += newQty;
                invColl.ReplaceOne(x => x.Id == st.Id, st, new ReplaceOptions() { IsUpsert = true });

                return new OkObjectResult(invColl);
            }


            return new NotFoundResult();
        }

        private ProductStock FindBestMatch(Product item, List<ProductStock> stocks, InventoryAddMovement movement, ProductPackaging pack)
        {
            ProductStock best = null;

            

            if (best==null)
            {
                best = new ProductStock()
                {
                    DateAdded = DateTime.Now,
                    UnitId = item.UnitId,
                    OriginalQuantity = 0,
                    ProductId = item.Id,
                    Id = Guid.NewGuid().ToString("n").ToLowerInvariant(),
                    PackagingId = movement.PackagingId,
                    ContainerId = movement.ContainerId,
                    StorageUnitId = movement.StorageUnitId,
                    IsMainStorage = true,
                    ProductMetaType = item.MetaType,
                    PackagingType = (pack?.ProductPackagingType).GetValueOrDefault(ProductPackagingType.Unit),
                };
                best.QuantityNew = best.OriginalQuantity;
            }

            return best;
        }

        private decimal ConvertQuantity(decimal quantity, string unitId1, string unitId2, ProductPackaging pack)
        {
            return quantity;
        }
    }
}
