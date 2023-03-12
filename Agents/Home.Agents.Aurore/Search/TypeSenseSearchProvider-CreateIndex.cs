using Home.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Home.Agents.Aurore.Search
{
    partial class TypeSenseSearchProvider
    {
        public void Prepare()
        {
            using(var cli = new TypeSenseWebClient())
            {
                var dt = cli.DownloadData<Collection[]>("/collections");
                var coll = GetCollection(dt, "products");
                if (coll == null)
                    CreateProductCollection(cli);
                coll = GetCollection(dt, "entities");
                if (coll == null)
                    CreateEntitiesCollection(cli);
                coll = GetCollection(dt, "recipes");
                if (coll == null)
                    CreateRecipesCollection(cli);
                coll = GetCollection(dt, "pages");
                if (coll == null)
                    CreatePagesCollection(cli);
            }
        }

        private void CreatePagesCollection(TypeSenseWebClient cli)
        {

        }

        private void CreateRecipesCollection(TypeSenseWebClient cli)
        {

        }

        private void CreateEntitiesCollection(TypeSenseWebClient cli)
        {
            
        }

        private void CreateProductCollection(TypeSenseWebClient cli)
        {
            Collection c = new Collection()
            {
                name = "products",
                enable_nested_fields = true
            };
            c.fields.Add(new CollectionField()
            {
                name = "id",
                type = "string",
                optional = false,
                sort = false,
                index = false
            });
            c.fields.Add(new CollectionField()
            {
                name = "name",
                type = "string",
                optional = false,
                sort = true,
                index = true
            });
            c.fields.Add(new CollectionField()
            {
                name = "image",
                type = "string",
                optional = true,
                sort = false,
                index = false
            });
            c.fields.Add(new CollectionField()
            {
                name = "category",
                type = "string",
                optional = false,
                sort = false,
                index = true,
                facet = true
            });
            c.fields.Add(new CollectionField()
            {
                name = "eans",
                type = "string[]",
                optional = true,
                sort = false,
                index = true,
                facet = false
            });
            c.fields.Add(new CollectionField()
            {
                name = "variations",
                type = "string[]",
                optional = true,
                sort = false,
                index = true,
                facet = false
            });
            c.fields.Add(new CollectionField()
            {
                name = "isAvailable",
                type = "bool",
                optional = false,
                sort = false,
                index = true,
                facet = true
            });
            c.fields.Add(new CollectionField()
            {
                name = "quantity",
                type = "float",
                optional = true,
                sort = false,
                index = false,
                facet = false
            });
            cli.UploadData<Collection, Collection>("/collections", "POST", c);

        }

        private Collection GetCollection(Collection[] all, string name)
        {
            return (from z in all where z.name != null && z.name.Equals(name) select z).FirstOrDefault();
        }
    }
}
