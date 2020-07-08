using Microsoft.Extensions.Configuration;
using MongoDBUploader.DataProcessor.Accessor;
using SmartKG.Common.Importer;
using System;
using System.IO;


namespace SmartKG.DataUploader.Executor
{
    public class DataUploader
    {
        static IConfigurationBuilder builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory()) // requires Microsoft.Extensions.Configuration.Json
                    .AddJsonFile("appsettings.json"); // requires Microsoft.Extensions.Configuration.Json                    
        static IConfiguration config = builder.Build();
        static MDBWriter writer = new MDBWriter(config);

        private string defaultDBName = null;

        public DataUploader()
        {
            defaultDBName = config.GetConnectionString("DefaultDataStore");
        }

        public void UploadDataFile(string rootPath, string dbName)
        {
            string kgPath = rootPath +  Path.DirectorySeparatorChar + "KG" + Path.DirectorySeparatorChar;
            string nluPath = rootPath + Path.DirectorySeparatorChar + "NLU" + Path.DirectorySeparatorChar;
            string vcPath = rootPath + Path.DirectorySeparatorChar + "Visulization" + Path.DirectorySeparatorChar;
           
            if (dbName == null)
            {
                dbName = defaultDBName;
            }

            ImportKG(kgPath, dbName);            
            ImportNLU(nluPath, dbName);            
            ImportVC(vcPath, dbName);            
        }

        private void ImportNLU(string nluPath, string dbName)
        {
            NLUDataImporter importer = new NLUDataImporter(nluPath);

            writer.CreateNLUCollections(dbName, importer.ParseIntentRules(), importer.ParseEntityData(), importer.ParseEntityAttributeData());
            Console.WriteLine("Imported NLU materials to MongoDB!");
        }

        private void ImportKG(string kgPath, string dbName)
        {
            KGDataImporter importer = new KGDataImporter(kgPath);
            writer.CreateKGCollections(dbName, importer.ParseKGVertexes(), importer.ParseKGEdges());

            Console.WriteLine("Imported KG materials to MongoDB!");
        }

        private void ImportVC(string vcPath, string dbName)
        {
            VisuliaztionImporter importer = new VisuliaztionImporter(vcPath);
            writer.CreateVisuliaztionConfigCollections(dbName, importer.GetVisuliaztionConfigs());

            Console.WriteLine("Imported Visulization Config materials to MongoDB!");
        }
    }
}
