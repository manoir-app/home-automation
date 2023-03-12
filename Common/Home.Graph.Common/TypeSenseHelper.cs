using Home.Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Home.Graph.Common
{
    public static class TypeSenseHelper
    {

        public class CollectionDefinition
        {
            public string name { get; set; }
            public Field[] fields { get; set; }
            public bool enable_nested_fields { get; set; }
        }

        public class Field
        {
            public string name { get; set; }
            public string type { get; set; }
        }



        public static void CreateCollectionIfNotExists<T>(string collectionName)
        {
            CreateCollectionIfNotExists(collectionName, typeof(T));
        }

        public static void CreateCollectionIfNotExists(string collectionName, Type dataType)
        {
            using (var cli = new TypeSenseWebClient())
            {
                try
                {
                    var t = cli.DownloadData($"collections/{collectionName}");
                }
                catch (WebException ex) when (ex.Response is HttpWebResponse
                                            && (ex.Response as HttpWebResponse).StatusCode == HttpStatusCode.NotFound)
                {
                    // ici, créer la collection à partir du type
                }
            }
        }
    }
}
