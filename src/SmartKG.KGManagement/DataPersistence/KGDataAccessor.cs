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
using System;
using System.Collections.Generic;

namespace SmartKG.KGManagement.DataPersistance
{
    public class KGDataAccessor
    {
        private static KGDataAccessor uniqueInstance;

        private ILogger log;

        private List<Vertex> vList;
        private List<Edge> eList;
        private List<VisulizationConfig> vcList;

        private KGDataAccessor(IConfiguration config)
        {
            log = Log.Logger.ForContext<KGDataAccessor>();

            string persistanceType = config.GetSection("PersistanceType").Value;

            if (persistanceType == "File")
            {
                FilePathConfig filePaths = config.GetSection("FileDataPath").Get<FilePathConfig>();

                string kgPath = filePaths.KGFilePath;

                log.Here().Information("KGFilePath: " + kgPath);

                KGDataImporter importer = new KGDataImporter(kgPath);

                this.vList = importer.ParseKGVertexes();
                this.eList = importer.ParseKGEdges();

                log.Here().Information("KG data has been parsed from Files.");

                string vcPath = filePaths.VCFilePath;

                log.Here().Information("VCFilePath: " + vcPath);

                VisuliaztionImporter vImporter = new VisuliaztionImporter(vcPath);

                this.vcList = vImporter.GetVisuliaztionConfigs();

            }
            else 
            {
                BsonDocument allFilter = new BsonDocument();

                string connectionString = config.GetConnectionString("MongoDbConnection");
                string dbName = config.GetConnectionString("DatabaseName");

                Console.WriteLine("Database Name: " + dbName);

                MongoClient client = new MongoClient(connectionString);

                IMongoDatabase db = client.GetDatabase(dbName);

                log.Here().Information("connectionString: " + connectionString + ", databaseName: " + dbName);

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

        public static KGDataAccessor GetInstance()
        {
            return uniqueInstance;
        }


        public static KGDataAccessor initInstance(IConfiguration config)
        {
            if (uniqueInstance == null)
            {
                uniqueInstance = new KGDataAccessor(config);
            }

            return uniqueInstance;
        }

        public List<Vertex> GetVertexCollection()
        {           
            return this.vList;
        }

        public List<Edge> GetEdgeCollection()
        {
            
            return this.eList;
        }     
        
        public List<VisulizationConfig> GetVisulizationConfigs()
        {
            return this.vcList;
        }
    }
}
