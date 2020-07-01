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

namespace SmartKG.Common.DataPersistance
{
    public class FileDataAccessor : IDataAccessor
    {
        private ILogger log;

        public FileDataAccessor()
        {
            this.log = Log.Logger.ForContext<MongoDataAccessor>();
        }  
        
        public (List<Vertex>, List<Edge>) LoadKG(string location)
        {
            string kgPath = location;

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

        public List<VisulizationConfig> LoadConfig(string location)
        {
            string vcPath = location;

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

        public  ( List<NLUIntentRule>, List<EntityData>, List<EntityAttributeData> ) LoadNLU(string location)
        {
            string nluPath = location;

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

        public  (List<Vertex>, List<Edge>, List<VisulizationConfig>, List<NLUIntentRule>, List<EntityData>, List<EntityAttributeData>) Load(string location)
        {
            string kgPath = location + "\\KG\\";
            string vcPath = location + "\\Visulization\\";
            string nluPath = location + "\\NLU\\";

            (var vList, var eList) = LoadKG(kgPath);

            var vcList = this.LoadConfig(vcPath);

            (var iList, var enList, var eaList) = this.LoadNLU(nluPath);

            return (vList, eList, vcList, iList, enList, eaList);
        }

        
    }
}
