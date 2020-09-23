// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;
using Serilog;
using Microsoft.Extensions.Configuration;
using SmartKG.Common.Data.KG;
using SmartKG.Common.Logger;
using SmartKG.Common.Data.LU;
using SmartKG.Common.Data.Visulization;
using SmartKG.Common.Data;

namespace MongoDBUploader.DataProcessor.Accessor
{   
    public class MDBWriter
    {       
        private BsonDocument allFilter;
        
        private ILogger log;
        private MongoClient client;
        private string mgmtDBName;

        public MDBWriter(IConfiguration config) 
        {
            log = Log.Logger.ForContext<MDBWriter>();

            this.allFilter = new BsonDocument();

            string connectionString = config.GetConnectionString("MongoDbConnection");
            this.mgmtDBName = config.GetConnectionString("DataStoreMgmtDatabaseName");
            this.client = new MongoClient(connectionString);
            
            log.Here().Information("connectionString: " + connectionString );        
        }

        public void CreateDataStoreMgmtDB(List<DatastoreItem> items)
        {
            string dbName = "DataStoreMgmt";
            log.Here().Information("DatabaseName: " + dbName);
            IMongoDatabase db = client.GetDatabase(dbName);

            IMongoCollection<DatastoreItem> collection = db.GetCollection<DatastoreItem>("DataStores");
            collection.DeleteMany(this.allFilter);

            collection.InsertMany(items);
        }

        public void CreateKGCollections(string dbName, List<Vertex> vertexes, List<Edge> edges, bool cleanCollection)
        {
            log.Here().Information("DatabaseName: " + dbName);
            IMongoDatabase db = client.GetDatabase(dbName);

            IMongoCollection<Vertex> vCollection = db.GetCollection<Vertex>("Vertexes");

            if (cleanCollection)
            {
                vCollection.DeleteMany(this.allFilter);
            }

            if (vertexes != null && vertexes.Count > 0)
            {                 
                vCollection.InsertMany(vertexes);                
            }

            IMongoCollection<Edge> eCollection = db.GetCollection<Edge>("Edges");
            if (cleanCollection)
            {
                eCollection.DeleteMany(this.allFilter);
            }
            
            if (edges != null && edges.Count > 0)
            {                 
                eCollection.InsertMany(edges);                
            }
        }

        public void CreateVisuliaztionConfigCollections(string dbName, List<VisulizationConfig> vcList, bool cleanCollection)
        {
            log.Here().Information("DatabaseName: " + dbName);
            IMongoDatabase db = client.GetDatabase(dbName);

            IMongoCollection<VisulizationConfig> vcCollection = db.GetCollection<VisulizationConfig>("VisulizationConfigs");

            if (cleanCollection)
            { 
                vcCollection.DeleteMany(this.allFilter);
            }

            if (vcList != null && vcList.Count > 0)
            {
                vcCollection.InsertMany(vcList);
            }
        }

        public void CreateNLUCollections(string dbName, List<NLUIntentRule> intentRules, List<EntityData> entities, List<EntityAttributeData> entityAttributes, bool cleanCollection)
        {
            log.Here().Information("DatabaseName: " + dbName);
            IMongoDatabase db = client.GetDatabase(dbName);

            IMongoCollection<NLUIntentRule> iCollection = db.GetCollection<NLUIntentRule>("IntentRules");
            
            if (cleanCollection)
            { 
                iCollection.DeleteMany(this.allFilter);
            }

            if (intentRules != null && intentRules.Count > 0)
            {
                iCollection.InsertMany(intentRules);
            }

            IMongoCollection<EntityData> eCollection = db.GetCollection<EntityData>("Entities");

            if (cleanCollection)
            {
                eCollection.DeleteMany(this.allFilter);
            }

            if (entities != null && entities.Count > 0)
            {
                eCollection.InsertMany(entities);
            }

            IMongoCollection<EntityAttributeData> eaCollection = db.GetCollection<EntityAttributeData>("EntityAttributes");

            if (cleanCollection)
            {
                eaCollection.DeleteMany(this.allFilter);
            }

            if (entityAttributes != null && entityAttributes.Count > 0)
            { 
                eaCollection.InsertMany(entityAttributes);
            }
        }
    }
}
