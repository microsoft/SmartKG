using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Serilog;
using SmartKG.Common.Data.Configuration;
using SmartKG.Common.Data.KG;
using SmartKG.Common.Data.Visulization;
using SmartKG.KGManagement.DataPersistance;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SmartKG.KGManagement.DataPersistence
{

    public enum PersistanceType
    {
        File, MongoDB
    }
    public abstract class KGDataAccessor
    {
        private static KGDataAccessor uniqueInstance;

        //private static IConfigurationBuilder builder = new ConfigurationBuilder()
        //  .SetBasePath(Directory.GetCurrentDirectory())
        //  .AddJsonFile("appsettings.json");
        //private static IConfiguration config = builder.Build();

       
        private ILogger log = Log.Logger.ForContext<KGDataAccessor>();

        private static PersistanceType persistanceType;

        protected List<Vertex> vList;
        protected List<Edge> eList;
        protected List<VisulizationConfig> vcList;

        public static KGDataAccessor GetInstance()
        {
            return uniqueInstance;
        }


        public static KGDataAccessor initInstance(IConfiguration config)
        {
            if (uniqueInstance == null)
            {
                persistanceType = (PersistanceType)Enum.Parse(typeof(PersistanceType), config.GetSection("PersistanceType").Value, true);

                if (persistanceType == PersistanceType.File)
                {                    
                    uniqueInstance = new FileDataAccessor();                                        
                }
                else
                {
                    string connectionString = config.GetConnectionString("MongoDbConnection");                                        
                    uniqueInstance = new MongoDataAccessor(connectionString);                    
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
                uniqueInstance.Load(filePaths.KGFilePath);
            }
            else
            {
                string dbName = config.GetConnectionString("DatabaseName");
                uniqueInstance.Load(dbName);
            }
        }

        public abstract void Load(string location);
        

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
