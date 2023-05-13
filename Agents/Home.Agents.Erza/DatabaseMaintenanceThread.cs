using Home.Common.Model;
using Home.Graph.Common;
using Home.Journal.Common.Model;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Home.Agents.Erza
{
    public static class DatabaseMaintenanceThread
    {

        public static void Start()
        {
            var t = new Thread(() => DatabaseMaintenanceThread.Run());
            t.Name = "Db Maintenance";
            t.Start();
        }

        public static void Stop()
        {
            _stop = true;
        }


        public static bool _stop = false;
        public static DateTimeOffset _lastcheck = DateTimeOffset.MinValue;
        public static DateTimeOffset _lastcleanup = DateTimeOffset.MinValue;

        private static void Run()
        {
            while (!_stop)
            {
                if (Math.Abs((DateTimeOffset.Now - _lastcheck).TotalHours) > 20)
                {
                    try
                    {
                        CheckDb();
                        CheckDefaults();
                        _lastcheck = DateTimeOffset.Now;
                    }
                    catch
                    {

                    }
                }
                Thread.Sleep(30000);
            }

        }

        private static void CheckDefaults()
        {
            try
            {
                CheckUnits();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void CheckUnits()
        {
            var cli = MongoDbHelper.GetClient<UnitOfMeasurement>();
            var units = cli.Find(x => true).ToList();
            UnitOfMeasurement newUnit;
            if(!CheckIfUnitExists("m", UnitMetaType.Length, "meter", "m", units, out newUnit)) Save(cli, newUnit);
            if(!CheckIfUnitExists("l", UnitMetaType.Volume, "litre", "l", units, out newUnit)) Save(cli, newUnit);
            if(!CheckIfUnitExists("degCel", UnitMetaType.Temperature, "°Celcius", "°C", units, out newUnit)) Save(cli, newUnit);
            if(!CheckIfUnitExists("pa", UnitMetaType.Pressure, "pascal", "pa", units, out newUnit)) Save(cli, newUnit);
        }

        private static void Save(IMongoCollection<UnitOfMeasurement> cli, UnitOfMeasurement newUnit)
        {
            var opt = new UpdateOptions() { IsUpsert = true };
            var res = cli.ReplaceOne(w => w.Id.Equals(newUnit.Id), newUnit);
            if (res.ModifiedCount == 0)
                cli.InsertOne(newUnit);
        }

        private static bool CheckIfUnitExists(string id, UnitMetaType meta, string name, string symbol, List<UnitOfMeasurement> units, out UnitOfMeasurement unit)
        {
            unit = (from z in units where z.Id.Equals(id) select z).FirstOrDefault();
            if(unit == null)
            {
                unit = new UnitOfMeasurement()
                {
                    Id = id,
                    Label = name,
                    Symbol = symbol,
                    MetaType = meta
                };
                units.Add(unit);
                return false;
            }
            return true;
        }

        public static void CheckDb()
        {
            try
            {
                MongoDbHelper.CreateCollection<Location>();
                MongoDbHelper.CreateCollection<ExternalToken>();
                MongoDbHelper.CreateCollection<AutomationMesh>();
                MongoDbHelper.CreateCollection<Agent>();
                MongoDbHelper.CreateCollection<Contact>();
                MongoDbHelper.CreateCollection<Common.Model.User>();
                MongoDbHelper.CreateCollection<Device>();
                MongoDbHelper.CreateCollection<InformationItem>();
                MongoDbHelper.CreateCollection<InformationItemsBucket>();
                MongoDbHelper.CreateCollection<DownloadItem>();
                MongoDbHelper.CreateCollection<UserNotification>();
                MongoDbHelper.CreateCollection<Trigger>();
                MongoDbHelper.CreateCollection<UserInterestInfo>();
                MongoDbHelper.CreateCollection<UserCustomData>();
                MongoDbHelper.CreateCollection<MeshExtension>();
                MongoDbHelper.CreateCollection<DiscoveredDevice>();
                MongoDbHelper.CreateCollection<TodoItem>();
                MongoDbHelper.CreateCollection<TodoList>();
                MongoDbHelper.CreateCollection<ProductCategory>();
                MongoDbHelper.CreateCollection<ProductType>();
                MongoDbHelper.CreateCollection<Product>();
                MongoDbHelper.CreateCollection<ChatChannel>();
                MongoDbHelper.CreateCollection<ChatMessage>();

                MongoDbHelper.CreateCollection<UnitOfMeasurement>();
                MongoDbHelper.CreateCollection<StorageUnit>();
                MongoDbHelper.CreateCollection<ProductStock>();

                MongoDbHelper.CreateCollection<Integration>();
                MongoDbHelper.CreateCollection<ApplicationShortcut>();
                MongoDbHelper.CreateCollection<UserStatus>();
                MongoDbHelper.CreateCollection<UserCrmData>();

                MongoDbHelper.CreateCollection<Entity>();

                MongoDbHelper.CreateCollection<Recipe>();
                MongoDbHelper.CreateCollection<RecipeCategory>();
                MongoDbHelper.CreateCollection<RecipeCuisine>();


                MongoDbHelper.CreateCollection<ShoppingItem>();
                MongoDbHelper.CreateCollection<ShoppingLocation>();
                MongoDbHelper.CreateCollection<ShoppingSession>();


                // les pages de journalApp
                MongoDbHelper.CreateCollection<Page>();
                MongoDbHelper.CreateCollection<PageSection>();
                MongoDbHelper.CreateCollection<PageTheme>();

                // les éléments de structure de la maison
                MongoDbHelper.CreateCollection<HomeCircuit>();

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }
    }
}
