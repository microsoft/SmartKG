using Serilog;
using SmartKG.Common.Data.Visulization;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartKG.Common.DataStoreMgmt
{
    public class KGConfigFrame
    {
        private Dictionary<string, List<ColorConfig>> vertexLabelColorMap { get; set; }
        private ILogger log;
        public KGConfigFrame()
        {
            log = Log.Logger.ForContext<KGConfigFrame>();
            this.Clean();
        }

        public void Clean()
        {            
            this.vertexLabelColorMap = new Dictionary<string, List<ColorConfig>>();            
        }

        public void SetVertexLabelColorMap(Dictionary<string, List<ColorConfig>> map)
        {
            this.vertexLabelColorMap = map;
        }

        /*public Dictionary<string, List<ColorConfig>> GetVertexLabelColorMap()
        {
            return this.vertexLabelColorMap;
        }*/

        public (bool, List<ColorConfig>) GetColorConfigForScenario(string scenarioName)
        {
            if (this.vertexLabelColorMap == null || this.vertexLabelColorMap.Count == 0)
            {
                return (false, null);
            }

            if (string.IsNullOrWhiteSpace(scenarioName))
            {
                List<ColorConfig> configs = new List<ColorConfig>();
                foreach (string scenario in this.vertexLabelColorMap.Keys)
                {
                    configs.AddRange(this.vertexLabelColorMap[scenario]);
                }

                return (true, configs);
            }
            else
            {
                if (!this.vertexLabelColorMap.ContainsKey(scenarioName))
                {
                    return (false, null);
                }
                else
                {
                    return (true, this.vertexLabelColorMap[scenarioName]);
                }
            }
        }

        public (bool, string) UpdateVertexLabelColorMapForScenario(string scenarioName, List<ColorConfig> colorConfigs)
        {            
            if (this.vertexLabelColorMap == null || this.vertexLabelColorMap.Count == 0)
            {
                return (false, "The scenario " + scenarioName + " doesn't exist.");
            }

            if (colorConfigs == null || colorConfigs.Count == 0)
            {
                return (false, "The color config of cannot be null.");
            }

            if (this.vertexLabelColorMap == null)
            {
                this.vertexLabelColorMap = new Dictionary<string, List<ColorConfig>>();
            }

            if (!this.vertexLabelColorMap.ContainsKey(scenarioName))
            {
                this.vertexLabelColorMap.Add(scenarioName, colorConfigs);
            }
            else
            {
                this.vertexLabelColorMap[scenarioName] = colorConfigs;
            }

            return (true, "The scenario " + scenarioName + " color configs are updated successfully.");
        }
    }
}
