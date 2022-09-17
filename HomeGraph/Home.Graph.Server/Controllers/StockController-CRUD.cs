using Home.Common.Model;
using Home.Graph.Common;
using Home.Graph.Server.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Collections.Generic;
using static Microsoft.Azure.Amqp.Serialization.SerializableType;

namespace Home.Graph.Server.Controllers
{
    public partial class StockController
    {
        [Route("content/find"), HttpGet]
        public List<ProductStock> FindStock(string storageUnitId = null, string productId = null, string packagingID = null)
        {
            var collection = MongoDbHelper.GetClient<ProductStock>();

            var bldr = Builders<ProductStock>.Filter.Empty;

            if (!string.IsNullOrEmpty(productId))
                bldr &= Builders<ProductStock>.Filter.Eq("ProductId", productId);

            if (!string.IsNullOrEmpty(storageUnitId))
                bldr &= Builders<ProductStock>.Filter.Eq("StorageUnitId", storageUnitId);

            if (!string.IsNullOrEmpty(storageUnitId))
                bldr &= Builders<ProductStock>.Filter.Eq("PackagingId", packagingID);

            var lst = collection.Find(bldr).ToList();
            return lst;
        }

        [Route("content/{id}"), HttpGet]
        public ProductStock Get(string id)
        {
            id = id.ToLowerInvariant();
            var collection = MongoDbHelper.GetClient<ProductStock>();
            return collection.Find(x => x.Id.Equals(id)).FirstOrDefault();
        }

        [Route("content/upsert"), HttpPost]
        public IActionResult Upsert([FromBody] ProductStock st)
        {
            if (st.Id == null) st.Id = Guid.NewGuid().ToString("D");

            st.Id = st.Id.ToLowerInvariant();

            var collection = MongoDbHelper.GetClient<ProductStock>();
            var old = collection.Find(x => x.Id.Equals(st.Id)).FirstOrDefault();

            if (old != null)
            {
                st.DateAdded = old.DateAdded;
                if (old.Reservations != null && old.Reservations.Count > 0)
                {
                    if (st.Reservations == null)
                        st.Reservations = new List<ReservedProduct>();
                    MergeReservations(old.Reservations, st.Reservations);
                }
            }
            else
            {
                st.DateAdded = DateTimeOffset.Now;
            }

            if (st.ProductId != null) st.ProductId = st.ProductId.ToLowerInvariant();

            var prd = MongoDbHelper.GetClient<Product>().Find(x => x.Id.Equals(st.ProductId)).FirstOrDefault();
            if (prd == null)
                return NotFound();

            if (prd.CommonDuration.HasValue && !st.DateExpiration.HasValue)
                st.DateExpiration = (DateTimeOffset.Now).AddDays(prd.CommonDuration.Value);

            if (old == null)
                collection.InsertOne(st);
            else
                collection.ReplaceOne(x => x.Id.Equals(st.Id), st);

            return Ok(st);
        }

        private void MergeReservations(List<ReservedProduct> source, List<ReservedProduct> dest)
        {
            if (source == null || source.Count == 0)
                return;

            foreach (var rS in source)
            {
                var ligD = (from z in dest
                            where z.ReservationId != null && z.ReservationId.Equals(rS.ReservationId)
                            select z).FirstOrDefault();
                if (ligD == null)
                    continue;

                ligD = (from z in dest
                        where z.AssociatedId != null && z.AssociatedId.Equals(rS.AssociatedId)
                        select z).FirstOrDefault();
                if (ligD == null)
                    continue;

                dest.Add(rS);
            }
        }

        [Route("content/consume/reservation/{reservationId}")]
        public IActionResult ConsumeReservation(string reservationId, decimal? quantity = null, string stockId = null)
        {
            reservationId = reservationId.ToLower();
            var collection = MongoDbHelper.GetClient<ProductStock>();
            var arrayFilters = Builders<ProductStock>.Filter.Eq("Reservations.ReservationId", reservationId);
            var lesStksImpliques = collection.Find(arrayFilters).ToList();

            lesStksImpliques.Sort((a, b) => a.DateAdded.CompareTo(b.DateAdded));

            if (stockId != null)
            {
                stockId = stockId.ToLowerInvariant();
                var leSt = (from z in lesStksImpliques
                            where z.Id.Equals(stockId)
                            select z).FirstOrDefault();
                if (leSt == null)
                {
                    leSt = collection.Find(x => x.Id.Equals(stockId)).FirstOrDefault();
                }
                else
                {
                    lesStksImpliques.Remove(leSt);
                }
                if (leSt != null)
                    lesStksImpliques.Insert(0, leSt);
            }
            if (lesStksImpliques.Count < 1)
                return BadRequest();

            var laRes = (from z in lesStksImpliques[0].Reservations
                         where z.ReservationId.Equals(reservationId)
                         select z).First();

            if (!quantity.HasValue)
                quantity = laRes.Quantity;

            var lesStksModifies = new List<ProductStock>();
            int i = 0;
            while (quantity > 0 && i < lesStksImpliques.Count)
            {
                if (lesStksImpliques[i].QuantityOpened > 0)
                {
                    decimal aDeduire = quantity.Value;
                    quantity -= lesStksImpliques[i].QuantityOpened;
                    lesStksModifies.Add(lesStksImpliques[i]);
                    lesStksImpliques[i].QuantityOpened -= aDeduire;
                    lesStksImpliques[i].Reservations = (from z in lesStksImpliques[i].Reservations
                                                        where !z.ReservationId.Equals(reservationId)
                                                        select z).ToList();
                    if (lesStksImpliques[i].QuantityOpened < 0)
                    {
                        lesStksImpliques[i].QuantityOpened = 0;
                    }
                }
                if (quantity < 0)
                    break;
                if (lesStksImpliques[i].QuantityNew > 0)
                {
                    decimal aDeduire = quantity.Value;
                    quantity -= lesStksImpliques[i].QuantityNew;
                    lesStksModifies.Add(lesStksImpliques[i]);
                    lesStksImpliques[i].Reservations = (from z in lesStksImpliques[i].Reservations
                                                        where !z.ReservationId.Equals(reservationId)
                                                        select z).ToList();
                    lesStksImpliques[i].QuantityNew -= aDeduire;
                    if (lesStksImpliques[i].QuantityNew < 0)
                    {
                        lesStksImpliques[i].QuantityNew = 0;
                    }
                    var tmp = Math.Floor(lesStksImpliques[i].QuantityNew);
                    if (tmp < lesStksImpliques[i].QuantityNew)
                    {
                        lesStksImpliques[i].QuantityOpened += lesStksImpliques[i].QuantityNew - tmp;
                        lesStksImpliques[i].QuantityNew = tmp;
                    }
                }

                i++;
            }

            var cli = MongoDbHelper.GetRootMongoClient();
            using (var sess = cli.StartSession())
            {
                collection = MongoDbHelper.GetClient<ProductStock>();
                foreach (var st in lesStksModifies)
                {
                    // pas de suppr auto pour l'instant
                    collection.UpdateOne(x => x.Id.Equals(st.Id),
                        Builders<ProductStock>.Update.Set("Reservations", st.Reservations)
                        .Set("QuantityNew", st.QuantityNew)
                        .Set("QuantityOpened", st.QuantityOpened));
                }
                sess.CommitTransaction();
            }


            return Ok(lesStksModifies);
        }


        [Route("content/{id}/consume")]
        public IActionResult ConsumeById(string id, decimal? qty = null, bool fromNewOnly = false)
        {
            id = id.ToLower();
            var collection = MongoDbHelper.GetClient<ProductStock>();

            var stk = collection.Find(x => x.Id == id).FirstOrDefault();
            if (stk == null)
                return NotFound();

            if (!qty.HasValue)
            {
                if (fromNewOnly)
                    qty = stk.QuantityNew;
                else
                    qty = stk.QuantityOpened + stk.QuantityNew;
            }

            if (fromNewOnly)
            {
                if (qty > (stk.QuantityNew + 0.1M))
                    return BadRequest();
                stk.QuantityNew -= qty.Value;


            }

            if (!fromNewOnly && qty > (stk.QuantityOpened + stk.QuantityNew + 0.1M))
                return BadRequest();

            var tmp = Math.Floor(stk.QuantityNew);
            if (tmp < stk.QuantityNew)
            {
                stk.QuantityOpened += stk.QuantityNew - tmp;
                stk.QuantityNew = tmp;
            }

            if (stk.QuantityNew <= 0 && stk.QuantityOpened <= 0)
                collection.DeleteOne(x => x.Id == id);
            else
            {
                collection.UpdateOne(x => x.Id == id,
                    Builders<ProductStock>.Update
                        .Set("QuantityNew", stk.QuantityNew)
                        .Set("QuantityOpened", stk.QuantityOpened));
            }

            return Ok(stk);
        }

        [Route("content/{id}/open")]
        public IActionResult OpenById(string id, decimal? qty = null)
        {
            id = id.ToLower();
            var collection = MongoDbHelper.GetClient<ProductStock>();

            var stk = collection.Find(x => x.Id == id).FirstOrDefault();
            if (stk == null)
                return NotFound();

            if (!qty.HasValue)
            {
                qty = stk.QuantityNew;
            }

            if (qty > (stk.QuantityNew + 0.1M))
                return BadRequest();
            stk.QuantityNew -= qty.Value;
            stk.QuantityOpened += qty.Value;

            var tmp = Math.Floor(stk.QuantityNew);
            if (tmp < stk.QuantityNew)
            {
                stk.QuantityOpened += stk.QuantityNew - tmp;
                stk.QuantityNew = tmp;
            }

            if (stk.QuantityNew <= 0 && stk.QuantityOpened <= 0)
                collection.DeleteOne(x => x.Id == id);
            else
            {
                collection.UpdateOne(x => x.Id == id,
                    Builders<ProductStock>.Update
                        .Set("QuantityNew", stk.QuantityNew)
                        .Set("QuantityOpened", stk.QuantityOpened));
            }

            return Ok(stk);
        }


    }
}
