// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using SmartKG.DataUploader.Executor;

namespace MongoDBUploader.DataProcessor
{
    public class Program
    {
        

        public static void Main(string[] args)
        {
            /* default to import all arguments */
            string usage = "Usage: ./DataProcessor  --rootPath=<rootPath> --dbName=<dbName>  [--init]";
            
            string rootPath = null;
            string dbName = null;

            bool importMgmtInfo = false;

            if (args.Length == 0)
            {
                Console.WriteLine(usage);
                Environment.Exit(1);
            }
            else
            {                
                foreach(string arg in args)                
                {
                    
                    if (arg.StartsWith("--dbName="))
                    {
                        dbName = arg.Split("=")[1];
                    }
                    else if (arg.StartsWith("--rootPath="))
                    {
                        rootPath = arg.Split("=")[1];
                    }
                    else if (arg == "--help")
                    {
                        Console.WriteLine();
                        Environment.Exit(0);
                    }else if (arg == "--init")
                    {
                        importMgmtInfo = true;
                    }
                }
            }

            DataUploader uploader = new DataUploader();
            uploader.UploadDataFile(rootPath, dbName);

            if (importMgmtInfo)
            {
                uploader.ImportMgmtInfo(dbName);
            }

            Console.WriteLine("Finished!");
        }       
    }
}