// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using Serilog;
using SmartKG.Common.Data.Configuration;
using SmartKG.Common.Data.KG;
using SmartKG.Common.Data.Visulization;
using SmartKG.Common.Importer;
using SmartKG.Common.Logger;
using SmartKG.KGManagement.DataPersistence;
using System;
using System.Collections.Generic;
using System.IO;

namespace SmartKG.KGManagement.DataPersistance
{
    public class MongoDataAccessor : KGDataAccessor
    {
        
        private ILogger log;

        private MongoClient client;

        public MongoDataAccessor(string connectionString)
        {
            this.log = Log.Logger.ForContext<MongoDataAccessor>();

            this.client = new MongoClient(new MongoUrl(connectionString));

        }

        public override void Load(string dbName)
        {
            BsonDocument allFilter = new BsonDocument();
            
            IMongoDatabase db = this.client.GetDatabase(dbName);

            Console.WriteLine("Database Name: " + dbName);

            IMongoCollection<Vertex> vCollection = db.GetCollection<Vertex>("Vertexes");

            this.vList = vCollection.Find(allFilter).ToList();

            IMongoCollection<Edge> eCollection = db.GetCollection<Edge>("Edges");

            this.eList = eCollection.Find(allFilter).ToList();

            log.Here().Information("KG data has been parsed from MongoDB.");

            IMongoCollection<VisulizationConfig> vcCollection = db.GetCollection<VisulizationConfig>("VisulizationConfigs");

            this.vcList = vcCollection.Find(allFilter).ToList();

            log.Here().Information("Visulization Config data has been parsed from MongoDB.");
        }        
    }
}
