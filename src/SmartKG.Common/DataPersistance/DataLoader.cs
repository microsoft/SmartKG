using Microsoft.Extensions.Configuration;
using Serilog;
using SmartKG.Common.Data.Configuration;
using SmartKG.Common.Data.KG;
using SmartKG.Common.Data.LU;
using SmartKG.Common.Data.Visulization;
using SmartKG.Common.DataPersistance;
using System;
using System.Collections.Generic;

namespace SmartKG.Common.DataPersistence
{

    public enum PersistanceType
    {
        File, MongoDB
    }
    public class DataLoader
    {
        private static DataLoader uniqueInstance;
    
        private ILogger log = Log.Logger.ForContext<DataLoader>();

        private static PersistanceType persistanceType;

        private List<Vertex> vList;
        private List<Edge> eList;
        private List<VisulizationConfig> vcList;

        private List<NLUIntentRule> iList;
        private List<EntityData> enList;
        private List<EntityAttributeData> eaList;

        private IDataAccessor dataAccessor;

        public static DataLoader GetInstance()
        {
            return uniqueInstance;
        }


        public static DataLoader initInstance(IConfiguration config)
        {
            if (uniqueInstance == null)
            {
                uniqueInstance = new DataLoader();

                persistanceType = (PersistanceType)Enum.Parse(typeof(PersistanceType), config.GetSection("PersistanceType").Value, true);

                if (persistanceType == PersistanceType.File)
                {
                    uniqueInstance.dataAccessor = new FileDataAccessor();
                }
                else
                {
                    string connectionString = config.GetConnectionString("MongoDbConnection");
                    uniqueInstance.dataAccessor = new MongoDataAccessor(connectionString);
                }                                    
            }

            return uniqueInstance;
        }

        public PersistanceType GetPersistanceType()
        {
            return persistanceType;
        }

        public void Load(IConfiguration config)
        {
            if (persistanceType == PersistanceType.File)
            {
                FilePathConfig filePaths = config.GetSection("FileDataPath").Get<FilePathConfig>();

                uniqueInstance.Load(filePaths.LocalRootPath);
                                
            }
            else
            {
                string dbName = config.GetConnectionString("DatabaseName");
                uniqueInstance.Load(dbName);
                
            }
        }

        public void Load(string location)
        {
           
            (this.vList, this.eList, this.vcList, this.iList , this.enList, this.eaList ) = this.dataAccessor.Load(location);            
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

        public List<NLUIntentRule> GetIntentCollection()
        {
            return iList;
        }

        public List<EntityData> GetEntityCollection()
        {
            return enList;
        }

        public List<EntityAttributeData> GetEntityAttributeCollection()
        {
            return eaList;
        }
    }
}
