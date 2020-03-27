using MongoDBUploader.DataProcessor.Accessor;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using SmartKG.Common.Data.Configuration;
using SmartKG.Common.Importer;

namespace MongoDBUploader.DataProcessor
{
    public class Program
    {
        static IConfigurationBuilder builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory()) // requires Microsoft.Extensions.Configuration.Json
                    .AddJsonFile("appsettings.json"); // requires Microsoft.Extensions.Configuration.Json                    
        static IConfiguration config = builder.Build();        
        static MDBWriter writer = new MDBWriter(config);

        public static void Main(string[] args)
        {
            /* default to import all arguments */
            string usage = "Usage: ./DataProcessor [options] --rootPath=<rootPath>\nThere are three arguments to trigger different data importing jobs.\nThey are '--kg' and '--nlu'. Or you can use '--all' to trigger all data importing jobs.";
            

            FilePathConfig filePaths = config.GetSection("FileDataPath").Get<FilePathConfig>();
            string kgPath = filePaths.KGFilePath;
            string nluPath = filePaths.NLUFilePath;

            //log.Here().Information("NLUFilePath: " + nluPath);


            bool kg = false;
            bool nlu = false;            

            //bool cleanContext = false;

            if (args.Length == 0)
            {
                Console.WriteLine(usage);
                Environment.Exit(1);

            }
            else
            {
                
                foreach(string arg in args)                
                {
                    
                    if (arg == "--all")
                    {
                        kg = true;
                        nlu = true;                        

                        //cleanContext = true;
                        
                    }else if (arg =="--kg")
                    {
                        kg = true;
                        //cleanContext = true;
                    }
                    else if (arg == "--nlu")
                    {
                        nlu = true;
                        //cleanContext = true;
                    }                    
                    else if (arg == "--help")
                    {
                        Console.WriteLine();
                        Environment.Exit(0);
                    }
                    
                                      
                }
            }

            if (kg)
            {                
                ImportKG(kgPath);
            }

            if (nlu)
            { 
                ImportNLU(nluPath);
            }

            Console.WriteLine("Finished!");
        }

        private static void ImportNLU(string rootPath)
        {
            NLUDataImporter importer = new NLUDataImporter(rootPath);

            writer.CreateNLUCollections(importer.ParseIntentRules(), importer.ParseEntityData(), importer.ParseEntityAttributeData());
            Console.WriteLine("Imported NLU materials to MongoDB!");
        }

        public static void ImportKG(string rootPath)
        {                                
            KGDataImporter importer = new KGDataImporter(rootPath);
            writer.CreateKGCollections(importer.ParseKGVertexes(), importer.ParseKGEdges());
            
            Console.WriteLine("Imported KG materials to MongoDB!");
        }      
    }
}