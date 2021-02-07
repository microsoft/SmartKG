// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Collections.Generic;
using SmartKG.Common.Data;
using System;
using Serilog;
using SmartKG.Common.Data.LU;
using SmartKG.Common.Data.KG;
using SmartKG.Common.DataStoreMgmt;

namespace SmartKG.Common.DataPersistance
{
    public class DataPersistanceNLUParser
    {
        private ILogger log;

        public DataPersistanceNLUParser()
        {
            log = Log.Logger.ForContext<DataPersistanceNLUParser>();
        }

        public NLUDataFrame Parse(List<NLUIntentRule> iList, List<EntityData> eList, List<EntityAttributeData> eaList)
        {
            NLUDataFrame nluDF = new NLUDataFrame();

            if (iList == null || iList.Count == 0)
            {
                //throw new Exception("No Intent is defined.");
                log.Warning("No Intent is defined.");
            }
            else
            {
                foreach (NLUIntentRule intentRule in iList)
                {
                    nluDF.AddIntentRule(intentRule);
                }
            }

            if (eList == null || eList.Count == 0)
            {
                //throw new Exception("No Entity is defined.");
                log.Warning("No Entity is defined.");
            }
            else
            {
                foreach (EntityData entity in eList)
                {
                    NLUEntity nluEntity = nluDF.AddEntity(entity.intentName, entity.entityValue, entity.entityType);
                    nluDF.AddSimilarWord(entity.intentName, entity.similarWord, nluEntity);
                }
            }

            if (eaList == null || eaList.Count == 0)
            {
                Log.Information("There is not any entity attribute defined.");
            }
            else
            {
                foreach (EntityAttributeData ea in eaList)
                {
                    nluDF.AddAttribute(ea.intentName, new NLUEntity(ea.entityValue, ea.entityType), new AttributePair(ea.attributeName, ea.attributeValue));
                }
            }

            return nluDF;
        }

        public void ParseScenarioSettings(NLUDataFrame nluDF, List<ScenarioSetting> kgSettings, List<Vertex> rootVertexes)
        {

            Dictionary<string, ScenarioSetting> scenarioCache = new Dictionary<string, ScenarioSetting>();
            if (kgSettings != null && kgSettings.Count > 0)
            {
                foreach (ScenarioSetting setting in kgSettings)
                {
                    scenarioCache.Add(setting.scenarioName, setting);
                }
            }

            foreach (Vertex vertex in rootVertexes)
            {

                if (vertex.nodeType == "ROOT" && vertex.scenarios != null)
                {
                    foreach(string scenarioName in vertex.scenarios)
                    { 

                    if (scenarioCache.ContainsKey(scenarioName))
                    {
                        scenarioCache[scenarioName].root = vertex;
                    }
                    else
                    {
                        ScenarioSetting defaultSetting = new ScenarioSetting(scenarioName, vertex);
                        scenarioCache.Add(scenarioName, defaultSetting);
                    }
                    }
                }
            }

            nluDF.SetSceanrioCache(scenarioCache);
        }
    }
}
