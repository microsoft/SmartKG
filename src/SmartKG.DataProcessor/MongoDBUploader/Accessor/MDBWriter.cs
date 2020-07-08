// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Collections.Generic;
using MongoDB.Bson;
using System;
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

        /*public void AddDataStore(string datastoreName)
        {
            DatastoreItem item = new DatastoreItem();
            item.name = datastoreName;

            IMongoDatabase db = client.GetDatabase(this.mgmtDBName);
            IMongoCollection<DatastoreItem> collection = db.GetCollection<DatastoreItem>("DataStores");

            collection.InsertOne(item);
        }

        public void RemoveDataStore(string datastoreName)
        {
            IMongoDatabase db = client.GetDatabase(this.mgmtDBName);
            IMongoCollection<DatastoreItem> collection = db.GetCollection<DatastoreItem>("DataStores");

            var deleteFilter = Builders<DatastoreItem>.Filter.Eq("name", datastoreName);
           
            collection.DeleteOne(deleteFilter);
        }*/

        public void CreateKGCollections(string dbName, List<Vertex> vertexes, List<Edge> edges)
        {
            log.Here().Information("DatabaseName: " + dbName);
            IMongoDatabase db = client.GetDatabase(dbName);

            IMongoCollection<Vertex> vCollection = db.GetCollection<Vertex>("Vertexes");
            vCollection.DeleteMany(this.allFilter);

            if (vertexes != null && vertexes.Count > 0)
            {                 
                vCollection.InsertMany(vertexes);                
            }

            IMongoCollection<Edge> eCollection = db.GetCollection<Edge>("Edges");
            eCollection.DeleteMany(this.allFilter);

            
            if (edges != null && edges.Count > 0)
            {                 
                eCollection.InsertMany(edges);                
            }
        }

        public void CreateVisuliaztionConfigCollections(string dbName, List<VisulizationConfig> vcList)
        {
            log.Here().Information("DatabaseName: " + dbName);
            IMongoDatabase db = client.GetDatabase(dbName);

            IMongoCollection<VisulizationConfig> vcCollection = db.GetCollection<VisulizationConfig>("VisulizationConfigs");
            vcCollection.DeleteMany(this.allFilter);

            if (vcList != null && vcList.Count > 0)
            {
                vcCollection.InsertMany(vcList);
            }
        }

        public void CreateNLUCollections(string dbName, List<NLUIntentRule> intentRules, List<EntityData> entities, List<EntityAttributeData> entityAttributes)
        {
            log.Here().Information("DatabaseName: " + dbName);
            IMongoDatabase db = client.GetDatabase(dbName);

            IMongoCollection<NLUIntentRule> iCollection = db.GetCollection<NLUIntentRule>("IntentRules");
            
            iCollection.DeleteMany(this.allFilter);
            if (intentRules != null && intentRules.Count > 0)
            {
                iCollection.InsertMany(intentRules);
            }

            IMongoCollection<EntityData> eCollection = db.GetCollection<EntityData>("Entities");

            eCollection.DeleteMany(this.allFilter);
            if (entities != null && entities.Count > 0)
            {
                eCollection.InsertMany(entities);
            }

            IMongoCollection<EntityAttributeData> eaCollection = db.GetCollection<EntityAttributeData>("EntityAttributes");

            eaCollection.DeleteMany(this.allFilter);
            if (entityAttributes != null && entityAttributes.Count > 0)
            { 
                eaCollection.InsertMany(entityAttributes);
            }
        }
    }
}
