using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Linq;


namespace Home.Graph.Common
{
    public static class MongoDbHelper
    {
        public static IMongoCollection<T> GetClient<T>() where T : class
        {
            string name = typeof(T).Name;
            if (!name.EndsWith("s"))
                name = name + "s";
            return GetClient<T>(name);
        }

        public static IMongoCollection<T> GetClient<T>(string name) where T : class
        {
            IMongoDatabase database = GetDatabaseClient();

            var collection = database.GetCollection<T>(name);
            return collection;
        }

        public static IMongoCollection<T> GetClient<T>(MongoClient client) where T : class
        {
            string name = typeof(T).Name;
            if (!name.EndsWith("s"))
                name = name + "s";
            IMongoDatabase database = GetDatabaseClient(client);
            
            var collection = database.GetCollection<T>(name);
            return collection;
        }

        private static IMongoDatabase GetDatabaseClient()
        {
            MongoClient client = GetRootMongoClient();
            return GetDatabaseClient(client);
        }

        private static IMongoDatabase GetDatabaseClient(MongoClient client)
        {
            return client.GetDatabase("home-automation");
        }

        public static MongoClient GetRootMongoClient()
        {
            string srv = null;
            var port = 27017;

            var tmp = Environment.GetEnvironmentVariable("MONGODB_SERVICE_HOST");
            if (!string.IsNullOrEmpty(tmp))
                srv = tmp;

            tmp = LocalDebugHelper.GetLocalServiceHost();
            if (!string.IsNullOrEmpty(tmp))
                srv = tmp;


            tmp = Environment.GetEnvironmentVariable("MONGODB_SERVICE_PORT");
            if (!string.IsNullOrEmpty(tmp))
            {
                if (!int.TryParse(tmp, out port))
                    port = 27017;
            }

            var client = new MongoClient($"mongodb://{srv}:{port}");
            return client;
        }

        public static void CreateCollection<T>()
        {
            string name = typeof(T).Name;
            if (!name.EndsWith("s"))
                name = name + "s";
            CreateCollection(name);
        }

        public static void CreateCollection(string collectionName)
        {
            var client = GetRootMongoClient();
            var database = client.GetDatabase("home-automation");
            using (var names = database.ListCollections(new ListCollectionsOptions()
            {
                Filter = new BsonDocument("name", collectionName)
            }))
            {
                if (names.Any())
                    return;
            }

            Console.WriteLine("Création de la collection " + collectionName);
            database.CreateCollection(collectionName, new CreateCollectionOptions()
            {

            });
            Console.WriteLine("Création de la collection " + collectionName + " terminée");

        }
    }
}