// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using Microsoft.Extensions.Configuration;
using Serilog;
using SmartKG.Common.Data;
using SmartKG.Common.Data.Configuration;
using SmartKG.Common.Data.KG;
using SmartKG.Common.Data.LU;
using SmartKG.Common.Data.Visulization;
using SmartKG.Common.DataPersistance;
using SmartKG.Common.DataStoreMgmt;
using SmartKG.Common.Logger;
using SmartKG.Common.Parser.DataPersistance;
using System;
using System.Collections.Generic;

namespace SmartKG.Common.DataPersistence
{    
    public class DataLoader
    {
        private ILogger log = Log.Logger.ForContext<DataLoader>();

        private IDataAccessor dataAccessor;
        private PersistanceType persistanceType;
        private FileUploadConfig uploadConfig;


        public DataLoader(IConfiguration config)
        {
            uploadConfig = config.GetSection("FileUploadConfig").Get<FileUploadConfig>();
            persistanceType = (PersistanceType)Enum.Parse(typeof(PersistanceType), config.GetSection("PersistanceType").Value, true);

            if (persistanceType == PersistanceType.File)
            {
               FilePathConfig filePathConfig = config.GetSection("FileDataPath").Get<FilePathConfig>();
               this.dataAccessor = new FileDataAccessor(filePathConfig.RootPath);
            }
            else
            {
                string connectionString = config.GetConnectionString("MongoDbConnection");
                string mgmtDBName = config.GetConnectionString("DataStoreMgmtDatabaseName");

                this.dataAccessor = new MongoDataAccessor(connectionString, mgmtDBName);
            }
        }        

        public List<DataStoreFrame> Load()
        {
            List<string> datastoreNames = dataAccessor.GetDataStoreList();

            List<DataStoreFrame> datastores = new List<DataStoreFrame>();

            foreach (string dsName in datastoreNames)
            {
                DataStoreFrame dsFrame = this.LoadDataStore(dsName);

                if (dsFrame != null)
                { 
                    datastores.Add(dsFrame);
                }
            }

            return datastores;
        }


        public DataStoreFrame LoadDataStore(string dsName)
        {
            List<Vertex> vList = null;
            List<Edge> eList = null;
            List<VisulizationConfig> vcList = null;

            List<NLUIntentRule> iList = null;
            List<EntityData> enList = null;
            List<EntityAttributeData> eaList = null;

            (vList, eList, vcList, iList, enList, eaList) = this.dataAccessor.Load(dsName);

            if (vcList == null || vList == null || eList == null)
            {
                log.Here().Warning("No KG Data loaded from persistence");
                return null;
            }

            DataPersistanceKGParser kgParser = new DataPersistanceKGParser();
            KnowledgeGraphDataFrame kgDF = kgParser.ParseKG(vList, eList, vcList);

            log.Information("Knowledge Graph is parsed.");
            Console.WriteLine("Knowledge Graph is parsed.");

            NLUDataFrame nluDF = null;

            if (iList == null || enList == null)
            {
                log.Here().Warning("No NLU Data loaded from persistence");
            }
            else
            {
                DataPersistanceNLUParser nluParser = new DataPersistanceNLUParser();
                nluDF = nluParser.Parse(iList, enList, eaList);                

                log.Information("NLU materials is parsed.");
                Console.WriteLine("NLU materials is parsed.");
            }

            DataStoreFrame dsFrame = new DataStoreFrame(dsName, kgDF, nluDF);

            return dsFrame;
        }        

        public List<string> GetDataStoreList()
        {
           return this.dataAccessor.GetDataStoreList();
        }
        
        public bool CreateDataStore(string user, string datastoreName)
        {
            return this.dataAccessor.AddDataStore(user, datastoreName);
        }

        public bool DeleteDataStore(string user, string datastoreName)
        {
            return this.dataAccessor.DeleteDataStore(user, datastoreName);
        }
        
        public PersistanceType GetPersistanceType()
        {
            return this.persistanceType;
        }

        public FileUploadConfig GetUploadConfig()
        {
            return this.uploadConfig;
        }
    }
}
