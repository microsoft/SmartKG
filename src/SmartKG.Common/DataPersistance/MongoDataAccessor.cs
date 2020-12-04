// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using MongoDB.Bson;
using MongoDB.Driver;
using Serilog;
using SmartKG.Common.Data;
using SmartKG.Common.Data.KG;
using SmartKG.Common.Data.LU;
using SmartKG.Common.Data.Visulization;
using SmartKG.Common.DataPersistence;
using SmartKG.Common.Logger;
using System;
using System.Collections.Generic;


namespace SmartKG.Common.DataPersistance
{
    public class MongoDataAccessor : IDataAccessor
    {

        private ILogger log;

        private MongoClient client;

        private string mgmtDatabaseName;

        private string connectionString;

        public MongoDataAccessor(string connectionString, string mgmtDatabaseName)
        {
            this.log = Log.Logger.ForContext<MongoDataAccessor>();

            this.connectionString = connectionString;
            this.client = new MongoClient(new MongoUrl(this.connectionString));

            this.mgmtDatabaseName = mgmtDatabaseName;
        }

        public (List<Vertex>, List<Edge>) LoadKG(string dbName)
        {            
            if (string.IsNullOrWhiteSpace(dbName))
            {
                log.Here().Warning("The database: " + dbName + " doesn't exist.");
                return (null, null);
            }

            try
            {
                BsonDocument allFilter = new BsonDocument();

                IMongoDatabase db = this.client.GetDatabase(dbName);

                Console.WriteLine("Database Name: " + dbName);

                IMongoCollection<Vertex> vCollection = db.GetCollection<Vertex>("Vertexes");

                List<Vertex> vList = vCollection.Find(allFilter).ToList();

                IMongoCollection<Edge> eCollection = db.GetCollection<Edge>("Edges");

                List<Edge> eList = eCollection.Find(allFilter).ToList();

                log.Here().Information("KG data has been parsed from MongoDB.");

                return (vList, eList);
            }
            catch (Exception e)
            {
                log.Here().Warning("Fail to parse KG data.\n" + e.Message);
                return (null, null);
            }
        }

        public List<VisulizationConfig> LoadConfig(string dbName)
        {
            if (string.IsNullOrWhiteSpace(dbName))
            {
                log.Here().Warning("The database: " + dbName + " doesn't exist.");
                return null;
            }

            try
            {
                IMongoDatabase db = this.client.GetDatabase(dbName);
                IMongoCollection<VisulizationConfig> vcCollection = db.GetCollection<VisulizationConfig>("VisulizationConfigs");
                BsonDocument allFilter = new BsonDocument();

                List<VisulizationConfig> vcList = vcCollection.Find(allFilter).ToList();
                log.Here().Information("Visulization Config data has been parsed from MongoDB.");

                return vcList;
            }
            catch (Exception e)
            {
                log.Here().Warning("Fail to parse Visulization Config data.\n" + e.Message);
                return null;
            }
        }

        public (List<NLUIntentRule>, List<EntityData>, List<EntityAttributeData>) LoadNLU(string dbName)
        {
            if (string.IsNullOrWhiteSpace(dbName))
            {
                log.Here().Warning("The database: " + dbName + " doesn't exist.");
                return (null, null, null);
            }

            try
            {
                BsonDocument allFilter = new BsonDocument();
                IMongoDatabase db = client.GetDatabase(dbName);

                IMongoCollection<NLUIntentRule> iCollection = db.GetCollection<NLUIntentRule>("IntentRules");
                List<NLUIntentRule> iList = iCollection.Find(allFilter).ToList();

                IMongoCollection<EntityData> eCollection = db.GetCollection<EntityData>("Entities");
                List<EntityData> eList = eCollection.Find(allFilter).ToList();

                IMongoCollection<EntityAttributeData> eaCollection = db.GetCollection<EntityAttributeData>("EntityAttributes");
                List<EntityAttributeData> eaList = eaCollection.Find(allFilter).ToList();

                log.Here().Information("NLU Data has been parsed from MongoDB.");

                if (iList != null && iList.Count == 0)
                    iList = null;

                if (eList != null && eList.Count == 0)
                    eList = null;

                if (eaList != null && eaList.Count == 0)
                    eaList = null;

                return (iList, eList, eaList);
            }
            catch (Exception e)
            {
                log.Here().Warning("Fail to parse NLU data.\n" + e.Message);
                return (null, null, null);
            }
        }

        public (List<Vertex>, List<Edge>, List<VisulizationConfig>, List<NLUIntentRule>, List<EntityData>, List<EntityAttributeData>) Load(string dbName)
        {            
            (var vList, var eList) = this.LoadKG(dbName);

            var vcList = this.LoadConfig(dbName);

            (var iList, var enList, var eaList) = this.LoadNLU(dbName);

            return (vList, eList, vcList, iList, enList, eaList);
        }

        public List<string> GetDataStoreList()
        {
            BsonDocument allFilter = new BsonDocument();
            IMongoDatabase db = client.GetDatabase(this.mgmtDatabaseName);

            IMongoCollection<DatastoreItem> collection = db.GetCollection<DatastoreItem>("DataStores");
            List<DatastoreItem> list = collection.Find(allFilter).ToList();

            List<string> result = new List<string>();

            foreach(DatastoreItem item in list)
            {
                result.Add(item.name);
            }

            return result;
        }

        private bool IsDataStoreExist(string datastoreName)
        {
            IMongoDatabase db = client.GetDatabase(this.mgmtDatabaseName);
            IMongoCollection<DatastoreItem> collection = db.GetCollection<DatastoreItem>("DataStores");

            var searchFilter = Builders<DatastoreItem>.Filter.Eq("name", datastoreName);

            var results = collection.Find(searchFilter).ToList();

            if (results.Count > 0)
                return true;
            else
                return false;
        }

        public bool AddDataStore(string user, string datastoreName)
        {
            if (IsDataStoreExist(datastoreName))
                return false;

            DatastoreItem item = new DatastoreItem();
            item.name = datastoreName;
            item.creator = user;

            IMongoDatabase db = client.GetDatabase(this.mgmtDatabaseName);
            IMongoCollection<DatastoreItem> collection = db.GetCollection<DatastoreItem>("DataStores");

            collection.InsertOne(item);

            return true;
        }

        private bool IsDataStoreCreatedByUser(string user, string datastoreName)
        {
            if (!IsDataStoreExist(datastoreName))
                return false;

            IMongoDatabase db = client.GetDatabase(this.mgmtDatabaseName);
            IMongoCollection<DatastoreItem> collection = db.GetCollection<DatastoreItem>("DataStores");

            var deleteFilter = Builders<DatastoreItem>.Filter.Eq("name", datastoreName);

            var results = collection.Find(deleteFilter).ToList();

            if (results.Count == 0)
                return false;

            if (string.IsNullOrWhiteSpace(user))
                return true;

            if (string.IsNullOrWhiteSpace(results[0].creator))
                return true;

            if (results[0].creator != user)
                return false;
            else
                return true;
        }

        public bool DeleteDataStore(string user, string datastoreName)
        {
            if (!IsDataStoreCreatedByUser(user, datastoreName))
                return false;
            
            var deleteFilter = Builders<DatastoreItem>.Filter.Eq("name", datastoreName);

            IMongoDatabase db = client.GetDatabase(this.mgmtDatabaseName);
            IMongoCollection<DatastoreItem> collection = db.GetCollection<DatastoreItem>("DataStores");
            collection.DeleteOne(deleteFilter);
            client.DropDatabase(datastoreName);
            return true;
        }

        public bool UpdateColorConfig(string user, string dsName, string scenarioName, List<ColorConfig> colorConfigs)
        {
            if (!IsDataStoreExist(dsName))
                return false;

            IMongoDatabase db = client.GetDatabase(dsName);
            IMongoCollection<VisulizationConfig> collection = db.GetCollection<VisulizationConfig>("VisulizationConfigs");

            var deleteFilter = Builders<VisulizationConfig>.Filter.Eq("scenario", scenarioName);
            collection.DeleteOne(deleteFilter);

            VisulizationConfig vConfig = new VisulizationConfig();
            vConfig.scenario = scenarioName;
            vConfig.labelsOfVertexes = colorConfigs;

            collection.InsertOne(vConfig);

            return true;
        }
    }       
}
