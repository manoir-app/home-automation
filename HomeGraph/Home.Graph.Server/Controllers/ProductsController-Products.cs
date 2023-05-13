using Home.Common.Model;
using Home.Graph.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Home.Graph.Server.Controllers
{ 
    partial class ProductsController 
    {
        public class ProductSearchResult
        {
            public List<Product> Items { get; set; }
            public long TotalResults { get; set; }
        }

        #region CRUD de base
        [Route("get/{productId}"), HttpGet]
        public Product GetProduct(string productId)
        {
            var collection = MongoDbHelper.GetClient<Product>();
            var lst = collection.Find(x => x.Id == productId).FirstOrDefault();
            return lst;
        }

        [Route("find/all"), HttpGet]
        public ProductSearchResult GetProducts(int page = 0, int pageSize = 250)
        {
            var ret = new ProductSearchResult();
            var collection = MongoDbHelper.GetClient<Product>();
            var qry = collection.Find(x => true);

            ret.TotalResults = qry.CountDocuments();

            if (page>0)
                qry = qry.Skip(page* pageSize);
            qry.Limit(pageSize);
            ret.Items = qry.ToList();

            return ret;
        }

        [Route("find/ean/{ean}"), HttpGet]
        public Product GetProductByEan(string ean)
        {
            var collection = MongoDbHelper.GetClient<Product>();
            var arrayFilters = Builders<Product>.Filter.Eq("Packagings.Ean", ean);
            var lst = collection.Find(arrayFilters).FirstOrDefault();
            return lst;
        }

        [Route("find/category/{categoryId}"), HttpGet]
        public ProductSearchResult GetProductsInCategory(string categoryId, int page = 0)
        {
            var ret = new ProductSearchResult();
            var collection = MongoDbHelper.GetClient<Product>();
            var arrayFilters = Builders<Product>.Filter.AnyEq("Categories", categoryId);
            var qry = collection.Find(arrayFilters);
            ret.TotalResults = qry.CountDocuments();

            if (page > 0)
                qry = qry.Skip(page * 250);
            qry.Limit(250);
            ret.Items = qry.ToList();

            return ret;
        }

        [Route("find/label"), HttpPost]
        public ProductSearchResult GetProductsByLabel([FromBody] string labelText)
        {
            var ret = new ProductSearchResult();

            var collection = MongoDbHelper.GetClient<Product>();
            var F = Builders<Product>.Filter.Text(labelText);
            var P = Builders<Product>.Projection.MetaTextScore("TextMatchScore");
            var S = Builders<Product>.Sort.MetaTextScore("TextMatchScore");
            var lst = collection.Find<Product>(F).Project<Product>(P).Sort(S).Limit(250).ToList();

            ret.TotalResults = lst.Count();
            ret.Items = lst;
            return ret;
        }

        [Route(""), HttpPost]
        public Product UpsertProduct([FromBody] Product product)
        {
            if (product.Id == null)
                product.Id = Guid.NewGuid().ToString("D").ToLowerInvariant();

            if (string.IsNullOrEmpty(product.Label))
                throw new ArgumentException("label");

            if(product.Packagings!=null)
            {
                foreach(var pack in product.Packagings)
                {
                    if (pack.Id == null)
                        pack.Id = Guid.NewGuid().ToString("D").ToLowerInvariant();
                    if (pack.Ean == "")
                        pack.Ean = null;
                }
            }
            
            if(product.StorageInfo!=null) // on nettoie les données provenant du formulaire html
            {
                if (string.IsNullOrEmpty(product.StorageInfo.DefaultStorageCommentWhenNew))
                    product.StorageInfo.DefaultStorageCommentWhenNew = null;
                if (string.IsNullOrEmpty(product.StorageInfo.DefaultStorageCommentWhenOpened))
                    product.StorageInfo.DefaultStorageCommentWhenOpened = null;
                if (string.IsNullOrEmpty(product.StorageInfo.DefaultStorageIdWhenNew))
                    product.StorageInfo.DefaultStorageIdWhenNew = null;
                if (string.IsNullOrEmpty(product.StorageInfo.DefaultStorageIdWhenOpened))
                    product.StorageInfo.DefaultStorageIdWhenOpened = null;
                if (string.IsNullOrEmpty(product.StorageInfo.DefaultStorageSubElementIdWhenNew))
                    product.StorageInfo.DefaultStorageSubElementIdWhenNew = null;
                if (string.IsNullOrEmpty(product.StorageInfo.DefaultStorageSubElementIdWhenOpened))
                    product.StorageInfo.DefaultStorageSubElementIdWhenOpened = null;
            }

            if (string.IsNullOrEmpty(product.GenericName))
                product.GenericName = product.Label;


            if (product.Categories!=null && product.Categories.Count>0)
            {
                var allCats = MongoDbHelper.GetClient<ProductCategory>()
                    .Find(x => true).ToList();
                var productCats = product.Categories.ToArray();
                foreach (var cat in productCats)
                {
                    FindAndAddParentCatIfNeeded(cat, allCats, product.Categories);
                }
                product.Categories = (from z in product.Categories
                                      select z).Distinct().ToList();
            }

            var collection = MongoDbHelper.GetClient<Product>();
            var lst = collection.Find(x => x.Id == product.Id).FirstOrDefault();

            if (lst == null)
            {
                collection.InsertOne(product);
            }
            else
            {
                collection.ReplaceOne(x => x.Id == lst.Id, product);
            }

            lst = collection.Find(x => x.Id == product.Id).FirstOrDefault();
            return lst;
        }

        private void FindAndAddParentCatIfNeeded(string catId, List<ProductCategory> allCats, List<string> productCategories)
        {
            var cat = (from z in allCats
                       where z.Id.Equals(catId, StringComparison.InvariantCultureIgnoreCase)
                       select z).FirstOrDefault();

            if(cat==null)
            {
                productCategories.Remove(catId);
                return;
            }
            if (string.IsNullOrEmpty(cat.ParentId))
                return;

            if(!productCategories.Contains(cat.ParentId, StringComparer.InvariantCultureIgnoreCase))
                productCategories.Add(cat.ParentId);

            FindAndAddParentCatIfNeeded(cat.ParentId, allCats, productCategories);
        }


        [Route("{productId}"), HttpDelete]
        public bool DeleteProduct(string productId)
        {
            var collection = MongoDbHelper.GetClient<Product>();
            var lst = collection.Find(x => x.Id == productId).FirstOrDefault();

            if (lst == null)
                return true;

            // ici il faudra check les shopping list et le garde manger
            // pour supprimer les articles de ces éléments là

            //var items = MongoDbHelper.GetClient<BankRecord>();
            //var count = items.CountDocuments(x => x.BankAccountId == lst.Id);
            //if (string.IsNullOrEmpty(productId) && count > 0)
            //{
            //    return false;
            //}
            //else
            //{
            //}

            collection.DeleteOne(x => x.Id == lst.Id);
            return true;
        }
        #endregion

    }
}
