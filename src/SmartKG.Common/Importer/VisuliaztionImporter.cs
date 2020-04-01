using Newtonsoft.Json;
using SmartKG.Common.Data.Visulization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace SmartKG.Common.Importer
{
    public class VisuliaztionImporter
    {
        private string path;
        private List<VisulizationConfig> vConfig;
        //private Dictionary<string, List<ColorConfig>> vertexLabelsMap;
        public VisuliaztionImporter(string path)
        {
            this.path = path;
            string content = File.ReadAllText(this.path);
            this.vConfig = JsonConvert.DeserializeObject<List<VisulizationConfig>>(content);
        }

        public List<VisulizationConfig> GetVisuliaztionConfigs()
        {
            return this.vConfig;
        }
    }
}
