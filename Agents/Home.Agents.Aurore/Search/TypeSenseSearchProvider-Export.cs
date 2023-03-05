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
                    CreateProductCollection();
            }
        }

        private void CreateProductCollection()
        {
            Collection c = new Collection()
            {
                name = "products",
                enable_nested_fields = true
            };
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
                name = "category",
                type = "string",
                optional = false,
                sort = false,
                index = true,
                facet = true
            });

        }

        private Collection GetCollection(Collection[] all, string name)
        {
            return (from z in all where z.name != null && z.name.Equals(name) select z).FirstOrDefault();
        }
    }
}
