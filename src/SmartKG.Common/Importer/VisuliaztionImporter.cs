using Newtonsoft.Json;
using Serilog;
using SmartKG.Common.Data.Visulization;
using SmartKG.Common.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SmartKG.Common.Importer
{
    public class VisuliaztionImporter
    {
        private string rootPath;
        private List<VisulizationConfig> vConfig;
        private ILogger log;

        public VisuliaztionImporter(string rootPath)
        {
            log = Log.Logger.ForContext<VisuliaztionImporter>();

            if (string.IsNullOrWhiteSpace(rootPath))
            {
                throw new Exception("Rootpath of VisulizationConfig files are invalid.");
            }

            this.rootPath = PathUtility.CompletePath(rootPath);

            string[] fileNamess = Directory.GetFiles(this.rootPath, "VisulizationConfig*.json").Select(Path.GetFileName).ToArray();

            this.vConfig = new List<VisulizationConfig>();

            foreach (string fileName in fileNamess)
            {
                string content = File.ReadAllText(this.rootPath + fileName);
                this.vConfig.AddRange(JsonConvert.DeserializeObject<List<VisulizationConfig>>(content));
            }            
        }

        public List<VisulizationConfig> GetVisuliaztionConfigs()
        {
            return this.vConfig;
        }
    }
}
