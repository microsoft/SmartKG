// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using Serilog;
using SmartKG.Common.Data.Configuration;
using SmartKG.Common.Data.KG;
using SmartKG.Common.Data.Visulization;
using SmartKG.Common.Importer;
using SmartKG.Common.Logger;
using SmartKG.KGManagement.DataPersistence;
using System;
using System.Collections.Generic;
using System.IO;

namespace SmartKG.KGManagement.DataPersistance
{
    public class FileDataAccessor : KGDataAccessor
    {
        private ILogger log;

        public FileDataAccessor()
        {
            this.log = Log.Logger.ForContext<MongoDataAccessor>();
        }  
        
        public override void Load(string kgPath)
        {
           
            log.Here().Information("KGFilePath: " + kgPath);

            KGDataImporter importer = new KGDataImporter(kgPath + "\\KG\\");

            this.vList = importer.ParseKGVertexes();
            this.eList = importer.ParseKGEdges();

            log.Here().Information("KG data has been parsed from Files.");           

            VisuliaztionImporter vImporter = new VisuliaztionImporter(kgPath + "\\Visulization\\");

            this.vcList = vImporter.GetVisuliaztionConfigs();

            log.Here().Information("Visulization Config data has been parsed from Files.");
        }
    }
}
