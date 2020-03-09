using CommonSmartKG.Common.Data.LU;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using Serilog;
using SmartKG.Common.Data.Configuration;
using SmartKG.Common.Data.LU;
using SmartKG.Common.Importer;
using SmartKG.Common.Logger;
using System;
using System.Collections.Generic;

namespace SmartKG.KGBot.DataPersistance
{
    public class NLUDataAccessor
    {
        private static NLUDataAccessor uniqueInstance;

        private List<NLUIntentRule> iList;
        private List<EntityData> eList;
        private List<EntityAttributeData> eaList;

        private ILogger log;

        private NLUDataAccessor(IConfiguration config)
        {
            log = Log.Logger.ForContext<NLUDataAccessor>();

            string persistanceType = config.GetSection("PersistanceType").Value;

            if (persistanceType == "File")
            {
                FilePathConfig filePaths = config.GetSection("FileDataPath").Get<FilePathConfig>();
                string nluPath = filePaths.NLUFilePath;

                log.Here().Information("NLUFilePath: " + nluPath);

                NLUDataImporter importer = new NLUDataImporter(nluPath);

                this.iList = importer.ParseIntentRules();
                this.eList = importer.ParseEntityData();
                this.eaList = importer.ParseEntityAttributeData();
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

                IMongoCollection<NLUIntentRule> iCollection = db.GetCollection<NLUIntentRule>("IntentRules");
                this.iList = iCollection.Find(allFilter).ToList();

                IMongoCollection<EntityData> eCollection = db.GetCollection<EntityData>("Entities");
                this.eList = eCollection.Find(allFilter).ToList();

                IMongoCollection<EntityAttributeData> eaCollection = db.GetCollection<EntityAttributeData>("EntityAttributes");
                this.eaList = eaCollection.Find(allFilter).ToList();

                log.Here().Information("NLU Data has been parsed from MongoDB.");
            }
        }

        public static NLUDataAccessor GetInstance()
        {
            return uniqueInstance;
        }

        public static NLUDataAccessor initInstance(IConfiguration config)
        {
            if (uniqueInstance == null)
            {
                uniqueInstance = new NLUDataAccessor(config);
            }

            return uniqueInstance;
        }

        public List<NLUIntentRule> GetIntentCollection()
        {
            return iList;
        }

        public List<EntityData> GetEntityCollection()
        {
            return eList;
        }

        public List<EntityAttributeData> GetEntityAttributeCollection()
        {
            return eaList;
        }
    }
}
