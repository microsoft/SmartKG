// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using SmartKG.Common.DataStore;
using System.Collections.Generic;
using SmartKG.Common.Data;
using System;
using Serilog;
using SmartKG.Common.Data.LU;
using SmartKG.Common.Data.KG;

namespace SmartKG.Common.DataPersistance
{
    public class DataPersistanceNLUParser
    {
        private List<NLUIntentRule> iList;
        private List<EntityData> eList;
        private List<EntityAttributeData> eaList;

        private NLUStore nluStore = NLUStore.GetInstance();
        public DataPersistanceNLUParser(List<NLUIntentRule> iList, List<EntityData> eList, List<EntityAttributeData> eaList)
        {
            this.iList = iList;
            this.eList = eList;
            this.eaList = eaList;
        }

        public void Parse()
        {
            if (iList == null || iList.Count == 0)
            {
                throw new Exception("No Intent is defined.");
            }
            else
            { 
                foreach(NLUIntentRule intentRule in iList)
                {
                    nluStore.AddIntentRule(intentRule);
                }
            }

            if (eList == null || eList.Count == 0)
            {
                throw new Exception("No Entity is defined.");
            }
            else
            { 
                foreach (EntityData entity in eList)
                {
                    NLUEntity nluEntity = nluStore.AddEntity(entity.intentName, entity.entityValue, entity.entityType);
                    nluStore.AddSimilarWord(entity.intentName, entity.similarWord, nluEntity);
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
                    nluStore.AddAttribute(ea.intentName, new NLUEntity(ea.entityValue, ea.entityType), new AttributePair(ea.attributeName, ea.attributeValue));
                }
            }
        }

        public void ParseScenarioSettings(List<ScenarioSetting> kgSettings, List<Vertex> rootVertexes)
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

            NLUStore.GetInstance().SetSceanrioCache(scenarioCache);
        }
    }
}
