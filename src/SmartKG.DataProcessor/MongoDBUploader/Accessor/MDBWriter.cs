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
using CommonSmartKG.Common.Data.LU;
using SmartKG.Common.Data.LU;

namespace MongoDBUploader.DataProcessor.Accessor
{   
    public class MDBWriter
    {       
        private BsonDocument allFilter;
        
        private ILogger log;
        private IMongoDatabase db;

        public MDBWriter(IConfiguration config) 
        {
            log = Log.Logger.ForContext<MDBWriter>();

            this.allFilter = new BsonDocument();

            string connectionString = config.GetConnectionString("MongoDbConnection");
            string dbName = config.GetConnectionString("DatabaseName");

            Console.WriteLine("Database Name: " + dbName);

            MongoClient client = new MongoClient(connectionString);
            this.db = client.GetDatabase(dbName);

            log.Here().Information("connectionString: " + connectionString + ", databaseName: " + dbName);
        
        }

        public void CreateKGCollections(List<Vertex> vertexes, List<Edge> edges)
        {
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

        public void CreateNLUCollections(List<NLUIntentRule> intentRules, List<EntityData> entities, List<EntityAttributeData> entityAttributes)
        {
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
