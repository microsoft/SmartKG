// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using Serilog;
using SmartKG.Common.Data.KG;
using SmartKG.Common.Data.LU;
using SmartKG.Common.Data.Visulization;
using SmartKG.Common.DataPersistence;
using SmartKG.Common.Importer;
using SmartKG.Common.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SmartKG.Common.DataPersistance
{
    public class FileDataAccessor : IDataAccessor
    {
        private ILogger log;

        private string rootPath;
        

        public FileDataAccessor(string rootPath)
        {
            this.rootPath = rootPath;
            this.log = Log.Logger.ForContext<MongoDataAccessor>();
        }  
        
        public (List<Vertex>, List<Edge>) LoadKG(string dsName)
        {
            string kgPath = this.rootPath + Path.DirectorySeparatorChar + dsName + Path.DirectorySeparatorChar + "KG" + Path.DirectorySeparatorChar;

            if (string.IsNullOrWhiteSpace(kgPath) || !Directory.Exists(kgPath))
            {
                log.Here().Warning("The path of KGFilePath: " + kgPath + " doesn't exist.");
                return (null, null);
            }
           
            log.Here().Information("KGFilePath: " + kgPath);

            KGDataImporter importer = new KGDataImporter(kgPath);

            List<Vertex> vList = importer.ParseKGVertexes();
            List<Edge> eList = importer.ParseKGEdges();
        

            log.Here().Information("KG data has been parsed from Files.");   
            
            return (vList, eList);
            
        }

        public List<VisulizationConfig> LoadConfig(string dsName)
        {
            string vcPath = this.rootPath + Path.DirectorySeparatorChar + dsName + Path.DirectorySeparatorChar + "Visulization" + Path.DirectorySeparatorChar;

            if (string.IsNullOrWhiteSpace(vcPath) || !Directory.Exists(vcPath))
            {
                log.Here().Warning("The path of VCFilePath: " + vcPath + " doesn't exist.");
                return null;
            }

            VisuliaztionImporter vImporter = new VisuliaztionImporter(vcPath);

            List<VisulizationConfig>  vcList = vImporter.GetVisuliaztionConfigs();

            log.Here().Information("Visulization Config data has been parsed from Files.");

            return vcList;
        }

        public  ( List<NLUIntentRule>, List<EntityData>, List<EntityAttributeData> ) LoadNLU(string dsName)
        {
            string nluPath = this.rootPath + Path.DirectorySeparatorChar + dsName + Path.DirectorySeparatorChar + "NLU" + Path.DirectorySeparatorChar;

            if (string.IsNullOrWhiteSpace(nluPath) || !Directory.Exists(nluPath))
            {
                log.Here().Warning("The path of NLUFilePath: " + nluPath + " doesn't exist.");
                return (null, null, null);
            }

            log.Here().Information("NLUFilePath: " + nluPath);

            NLUDataImporter importer = new NLUDataImporter(nluPath);

            List<NLUIntentRule> iList = importer.ParseIntentRules();
            List<EntityData> eList = importer.ParseEntityData();
            List<EntityAttributeData> eaList = importer.ParseEntityAttributeData();

            return (iList, eList, eaList);
        }

        public  (List<Vertex>, List<Edge>, List<VisulizationConfig>, List<NLUIntentRule>, List<EntityData>, List<EntityAttributeData>) Load(string dsName)
        {
            (var vList, var eList) = LoadKG(dsName);

            var vcList = this.LoadConfig(dsName);

            (var iList, var enList, var eaList) = this.LoadNLU(dsName);

            return (vList, eList, vcList, iList, enList, eaList);
        }

        public List<string> GetDataStoreList()
        {
            List<string> list = new List<string>();
            string[] directories = Directory.GetDirectories(rootPath);
            
            foreach(string dir in directories)
            {
                string subDir = dir.Replace(rootPath, "");
                subDir = subDir.Replace(Path.DirectorySeparatorChar.ToString(), "");

                list.Add(subDir);
            }
            
            return list;
        }

        public bool AddDataStore(string user, string datastoreName)
        {            
            string targetDir = this.rootPath + Path.DirectorySeparatorChar + datastoreName;

            if (Directory.Exists(targetDir))
            {
                return false;
            }
            else
            {
                Directory.CreateDirectory(targetDir);

                string real_targetDir = this.rootPath + Path.DirectorySeparatorChar + user + Path.DirectorySeparatorChar + datastoreName;
                Directory.CreateDirectory(real_targetDir);

                return true;
            }
        }

        public bool DeleteDataStore(string user, string datastoreName)
        {
            string targetDir = this.rootPath + Path.DirectorySeparatorChar + datastoreName;
            if (!Directory.Exists(targetDir))
            {
                return false;
            }
            else
            {
                string real_targetDir = this.rootPath + Path.DirectorySeparatorChar + user + Path.DirectorySeparatorChar + datastoreName;
                if (!Directory.Exists(real_targetDir))
                    return false;
                else
                    return DeleteDir(targetDir);                
            }
        }

        private bool DeleteDir(string targetDir)
        {
            System.IO.DirectoryInfo di = new DirectoryInfo(targetDir);

            try
            { 
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }
            }
            catch(Exception e)
            {
                log.Error(e.Message);
                return false;
            }

            Directory.Delete(targetDir);

            return true;
        }
    }
}
