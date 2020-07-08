// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using MongoDBUploader.DataProcessor.Accessor;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using SmartKG.Common.Data.Configuration;
using SmartKG.Common.Importer;
using System.Data.Common;
using SmartKG.DataUploader.Executor;

namespace MongoDBUploader.DataProcessor
{
    public class Program
    {
        

        public static void Main(string[] args)
        {
            /* default to import all arguments */
            string usage = "Usage: ./DataProcessor  --rootPath=<rootPath> --dbName=<dbName>";
            
            string rootPath = null;
            string dbName = null;           

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
                    }                                     
                }
            }

            DataUploader uploader = new DataUploader();
            uploader.UploadDataFile(rootPath, dbName);

            Console.WriteLine("Finished!");
        }       
    }
}