using Home.Common;
using Home.Common.Model;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Home.Agents.Aurore.Search
{
    partial class TypeSenseSearchProvider
    {
        public void SyncProducts()
        {
            // on récupère tous les produits
            var prdsServer = GetServerProducts();
            if (prdsServer == null) return;

            // puis on récupère les ids existant coté typesense
            var ids = GetTypeSenseProducts();
            if (ids == null) ids = new List<string>();

            // on trouve quels produits seront à supprimer
            var lesIdsServer = (from z in prdsServer select z.Id).ToList();
            var idsASuppr = (from z in ids where !lesIdsServer.Contains(z) select z).ToList();
            DeleteProductsOnTypeSense(idsASuppr);

            // on upsert tous les autres
            ExportProductToTypeSense(prdsServer);
        }

        private void ExportProductToTypeSense(List<Product> prdsServer)
        {
            
        }

        private void DeleteProductsOnTypeSense(List<string> idsASuppr)
        {
            
        }

        private List<Product> GetServerProducts()
        {
            using(var cli = new MainApiAgentWebClient("aurore"))
            {

            }

            return null;
        }

        private List<string> GetTypeSenseProducts()
        {
            return null;
        }
    }
}
