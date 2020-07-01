// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using MongoDB.Bson;
using MongoDB.Driver;
using Serilog;
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

        public MongoDataAccessor(string connectionString)
        {
            this.log = Log.Logger.ForContext<MongoDataAccessor>();

            this.client = new MongoClient(new MongoUrl(connectionString));

        }

        public (List<Vertex>, List<Edge>) LoadKG(string dbName)
        {
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
            Console.WriteLine("Database Name: " + dbName);

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
            Console.WriteLine("Database Name: " + dbName);

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

    }       
}
