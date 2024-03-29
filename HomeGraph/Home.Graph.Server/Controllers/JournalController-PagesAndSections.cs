﻿using Home.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Collections.Generic;
using System;
using System.Linq;
using Home.Graph.Common;
using Home.Journal.Common.Model;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.AspNetCore.Http.Metadata;
using Home.Common.Messages;

namespace Home.Graph.Server.Controllers
{
    partial class JournalController
    {
        #region Pages
        [Route("pages"), HttpGet]
        public List<Page> GetPages()
        {
            var collection = MongoDbHelper.GetClient<Page>();
            var lst = collection.Find(x => true).ToList();
            return lst;
        }

        [Route("pages/{pageId}"), HttpGet]
        public Page GetPage(string pageId)
        {
            if (pageId == null)
                return null;

            pageId = pageId.ToLowerInvariant();
            var collection = MongoDbHelper.GetClient<Page>();
            var lst = collection.Find(x => x.Id == pageId).FirstOrDefault();
            return lst;
        }

        [Route("find/pages"), HttpGet]
        public Page GetPageByPath(string pagePath, string userId)
        {
            if (pagePath == null)
                return null;

            if (string.IsNullOrEmpty(userId))
                userId = "#MESH#";

            pagePath = pagePath.ToLowerInvariant();
            var collection = MongoDbHelper.GetClient<Page>();

            if (userId.Equals("#MESH#", StringComparison.InvariantCultureIgnoreCase))
            {
                var lst = collection.Find(x => x.Path == pagePath && x.IsPublic).FirstOrDefault();
                return lst;

            }
            else
            {
                userId = userId.ToLowerInvariant();
                var lst = collection.Find(x => x.Path == pagePath && !x.IsPublic && x.UserIds != null && x.UserIds.Contains(userId)).FirstOrDefault();
                return lst;
            }
        }

        [Route("pages"), HttpPost]
        public Page UpsertPage([FromBody] Page page)
        {
            if (page.Id == null)
                page.Id = Guid.NewGuid().ToString("D").ToLowerInvariant();
            else
                page.Id = page.Id.ToLowerInvariant();

            var collection = MongoDbHelper.GetClient<Page>();
            var lst = collection.Find(x => x.Id == page.Id).FirstOrDefault();

            if (lst == null)
            {
                collection.InsertOne(page);
            }
            else
            {
                collection.ReplaceOne(x => x.Id == lst.Id, page);
            }

            lst = collection.Find(x => x.Id == page.Id).FirstOrDefault();


            MessagingHelper.PushToLocalAgent(new JournalPageChangedMessage()
            {
                PageId = lst.Id,
                SectionId = null
            });

            return lst;
        }

        [Route("pages/{pageId}"), HttpDelete]
        public bool DeletePage(string pageId, string pageIdRemplacement)
        {
            pageId = pageId.ToLowerInvariant();
            if (pageIdRemplacement != null)
                pageIdRemplacement = pageIdRemplacement.ToLowerInvariant();
            var collection = MongoDbHelper.GetClient<Page>();
            var lst = collection.Find(x => x.Id == pageId).FirstOrDefault();

            if (lst == null)
                return true;

            var items = MongoDbHelper.GetClient<PageSection>();
            var arrayFilters = Builders<PageSection>.Filter.Eq("PageId", pageId);
            var count = items.CountDocuments(arrayFilters);
            if (/*string.IsNullOrEmpty(pageId) && */count > 0)
            {
                return false;
            }
            else
            {

            }

            collection.DeleteOne(x => x.Id == lst.Id);

            MessagingHelper.PushToLocalAgent(new JournalPageChangedMessage()
            {
                PageId = lst.Id,
                SectionId = null
            });;

            return true;
        }
        #endregion

        #region Sections

        [Route("sections/all"), HttpGet]
        public List<PageSection> GetAllSections()
        {
            var collection = MongoDbHelper.GetClient<PageSection>();
            var lst = collection.Find(x => true).ToList();
            return lst;
        }


        [Route("pages/{pageId}/sections"), HttpGet]
        public List<PageSection> GetPageSections(string pageId)
        {
            var collection = MongoDbHelper.GetClient<PageSection>();
            var lst = collection.Find(x => x.PageId.Equals(pageId)).ToList();
            return lst;
        }

        [Route("pages/{pageId}/sections/{sectionId}"), HttpGet]
        public PageSection GetPageSection(string pageId, string sectionId)
        {
            pageId = pageId.ToLowerInvariant();
            sectionId = sectionId.ToLowerInvariant();
            var collection = MongoDbHelper.GetClient<PageSection>();
            var lst = collection.Find(x => x.PageId == pageId && x.Id == sectionId).FirstOrDefault();
            return lst;
        }


        [Route("pages/{pageId}/sections/all"), HttpPatch]
        public List<PageSection> UpsertPageSection(string pageId, [FromBody] PageSection[] sections)
        {
            List<PageSection> ret = new List<PageSection>();
            bool hasId = (from z in sections where !string.IsNullOrEmpty(z.Id) select z).Count() > 0;

            var collection = MongoDbHelper.GetClient<PageSection>();

            var allSections = collection.Find(x => x.PageId.Equals(pageId)).ToList();
            var allproperties = new Dictionary<string, Dictionary<string, string>>();

            foreach(var section in allSections)
            {
                if(section.Source!=null && section.Properties !=null && section.Properties.Count>0)
                {
                    allproperties[section.Source] = section.Properties;
                }
            }

            if (!hasId) // on remplace tout, vu qu'aucun id dans les sections
                collection.DeleteMany(x => x.PageId.Equals(pageId));

            foreach (var section in sections)
            {
                ret.Add(UpdatePageSection(pageId, section, collection, allproperties));
            }

            MessagingHelper.PushToLocalAgent(new JournalPageChangedMessage()
            {
                PageId = pageId,
                SectionId = null
            });

            return ret;
        }

        [Route("pages/{pageId}/sections"), HttpPost]
        public PageSection UpsertPageSection(string pageId, [FromBody] PageSection section)
        {
            var collection = MongoDbHelper.GetClient<PageSection>();

            var tmp = UpdatePageSection(pageId, section, collection, null);

            MessagingHelper.PushToLocalAgent(new JournalPageChangedMessage()
            {
                PageId = pageId,
                SectionId = tmp.Id
            });


            return tmp;
        }

        private static PageSection UpdatePageSection(string pageId, PageSection section,
            IMongoCollection<PageSection> collection, Dictionary<string, Dictionary<string, string>> allproperties)
        {
            if (section.Id == null)
                section.Id = Guid.NewGuid().ToString("D").ToLowerInvariant();
            else
                section.Id = section.Id.ToLowerInvariant();

            if (pageId != null)
                section.PageId = pageId;

            if (section.PageId == null)
                throw new ApplicationException("Données invalides");
            else
                section.PageId = section.PageId.ToLowerInvariant();


            var lst = collection.Find(x => x.Id == section.Id).FirstOrDefault();

            if (lst == null)
            {
                if((section.Properties==null || section.Properties.Count == 0)
                    && !string.IsNullOrEmpty(section.Source)
                    && allproperties!=null)
                {
                    if(allproperties.TryGetValue(section.Source, out var props))
                        section.Properties = props;
                }
                collection.InsertOne(section);
            }
            else
            {
                section.Properties = lst.Properties;
                collection.ReplaceOne(x => x.Id == lst.Id, section);
            }

            lst = collection.Find(x => x.Id == section.Id).FirstOrDefault();
            return lst;
        }

        [Route("pages/{pageId}/sections/{sectionId}"), HttpDelete]
        public bool DeletePageSection(string pageId, string sectionId)
        {
            pageId = pageId.ToLowerInvariant();
            sectionId = sectionId.ToLowerInvariant();

            var collection = MongoDbHelper.GetClient<PageSection>();
            var lst = collection.Find(x => x.PageId == pageId && x.Id == sectionId).FirstOrDefault();

            if (lst == null)
                return true;

            collection.DeleteOne(x => x.Id == lst.Id);


            MessagingHelper.PushToLocalAgent(new JournalPageChangedMessage()
            {
                PageId = lst.PageId,
                SectionId = lst.Id
            });

            return true;
        }

        #endregion

        [Route("find/sections"), Route("find/section")]
        public ActionResult FindSection(string pagePath, string userId, int order)
        {
            if (string.IsNullOrEmpty(userId))
                userId = "#MESH#";

            var pg = GetPageByPath(pagePath, userId);
            if (pg == null)
                return new NotFoundResult();

            var collection = MongoDbHelper.GetClient<PageSection>();
            var lst = collection.Find(x => x.PageId == pg.Id && x.Order == order).FirstOrDefault();
            if (lst == null)
                return new NotFoundResult();

            return new OkObjectResult(lst);
        }

        public class FindSectionBySourceResult
        {
            public Page Page { get; set; }
            public PageSection Section { get; set; }
        }

        [Route("find/bysource")]
        public ActionResult FindSection(string source)
        {
            var collection = MongoDbHelper.GetClient<PageSection>();
            var lst = collection.Find(x => x.Source == source).ToList();
            if (lst == null)
                return new NotFoundResult();

            List<string> pageIds = (from z in lst select z.PageId).Distinct().ToList();

            var pgcollection = MongoDbHelper.GetClient<Page>();
            var lstPg = pgcollection.Find(x => pageIds.Contains(x.Id)).ToList();
            var ret = new List<FindSectionBySourceResult>();
            foreach (var r in lst)
            {
                var pg = (from z in lstPg where z.Id.Equals(r.PageId) select z).FirstOrDefault();
                ret.Add(new FindSectionBySourceResult()
                {
                    Page = pg,
                    Section = r,
                });
            }

            return new OkObjectResult(ret);
        }
    }
}
