// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using SmartKG.Common.Data.KG;
using SmartKG.Common.Data.LU;
using SmartKG.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;


namespace SmartKG.Common.DataStore
{
    public class NLUStore
    {
        private static NLUStore uniqueInstance;

        private Dictionary<string, Dictionary<string, List<NLUEntity>>> standValueMap;
        
        private Dictionary<string, Dictionary<string, Dictionary<string, NLUEntity>>> entityMap;
        private Dictionary<string, Dictionary<string, List<AttributePair>>> attributeMap;

        private List<NLUIntentRule> intentPositiveDetermineRules;
        private Dictionary<string, List<NLUIntentRule>> intentNegativeDeterminRules;

        private Dictionary<string, ScenarioSetting> sceanrioCache;

        private NLUStore()
        {
            this.standValueMap = new Dictionary<string, Dictionary<string, List<NLUEntity>>>();
            this.attributeMap = new Dictionary<string, Dictionary<string, List<AttributePair>>>();

            this.entityMap = new Dictionary<string, Dictionary<string, Dictionary<string, NLUEntity>>>();            
            this.intentPositiveDetermineRules = new List<NLUIntentRule>();
            this.intentNegativeDeterminRules = new Dictionary<string, List<NLUIntentRule>>();
            this.sceanrioCache = new Dictionary<string, ScenarioSetting>();
        }

        public static NLUStore GetInstance()
        {
            if (uniqueInstance == null)
            {
                uniqueInstance = new NLUStore();
            }
            return uniqueInstance;
        }

        public void Clean()
        {
            this.standValueMap = new Dictionary<string, Dictionary<string, List<NLUEntity>>>();
            this.attributeMap = new Dictionary<string, Dictionary<string, List<AttributePair>>>();

            this.entityMap = new Dictionary<string, Dictionary<string, Dictionary<string, NLUEntity>>>();
            this.intentPositiveDetermineRules = new List<NLUIntentRule>();
            this.intentNegativeDeterminRules = new Dictionary<string, List<NLUIntentRule>>();
            this.sceanrioCache = new Dictionary<string, ScenarioSetting>();
        }

        public NLUEntity AddEntity(string intentName, string entityValue, string entityType)
        {
            Dictionary<string, Dictionary<string, NLUEntity>> typeEntities;
            if (!this.entityMap.Keys.Contains(intentName))
            {
                typeEntities = new Dictionary<string, Dictionary<string, NLUEntity>>();
                this.entityMap.Add(intentName, typeEntities);
            }

            typeEntities = this.entityMap[intentName];


            Dictionary<string, NLUEntity> entities;


            if (!typeEntities.Keys.Contains(entityType))
            {
                entities = new Dictionary<string, NLUEntity>();
                typeEntities.Add(entityType, entities);

            }

            entities = typeEntities[entityType];
            

            if (!entities.Keys.Contains(entityValue))
            {
                NLUEntity entity = new NLUEntity(entityValue, entityType);
                
                entities.Add(entityValue, entity);
                return entity;
            }
            else
            {                
                return entities[entityValue];
            }
        }

        public void AddSimilarWord(string intentName, string similarWord, NLUEntity entity)
        {
            if (!this.standValueMap.Keys.Contains(intentName))
            {
                Dictionary<string, List<NLUEntity>> wordToEntity = new Dictionary<string, List<NLUEntity>>();
                this.standValueMap.Add(intentName, wordToEntity);
            }

            if (this.standValueMap[intentName].Keys.Contains(similarWord))
            {
                this.standValueMap[intentName][similarWord].Add(entity);
            }
            else
            { 
                this.standValueMap[intentName].Add(similarWord, new List<NLUEntity>() { entity });
            }
        }

        public void AddAttribute(string intentName, NLUEntity entity, AttributePair attribute)
        {
            if (!this.attributeMap.Keys.Contains(intentName))
            {
                Dictionary<string, List<AttributePair>> wordToAttribute = new Dictionary<string, List<AttributePair>>();
                this.attributeMap.Add(intentName, wordToAttribute);
            }

            string key = entity.GetIdentity();

            if (!this.attributeMap[intentName].Keys.Contains(key))
            {
                this.attributeMap[intentName].Add(key, new List<AttributePair>());
            }

            this.attributeMap[intentName][key].Add(attribute);
        }

        public List<NLUEntity> DetectEntities(string intentName, string query)
        {
            List<NLUEntity> resultList = new List<NLUEntity>();
            
            if (!this.standValueMap.Keys.Contains(intentName))
            {
                return resultList;
            }

            try
            {
                SortedDictionary<int, List<NLUEntity>> results = new SortedDictionary<int, List<NLUEntity>>();

                Dictionary<string, List<NLUEntity>> wordToEntity = this.standValueMap[intentName];

                List<string> similarWords = wordToEntity.Keys.ToList<string>();

                foreach(string word in similarWords)
                {
                    string lowerWord = null;
                    if (word != null)
                        lowerWord = word.ToLower();
                    
                    if (query.Contains(lowerWord))
                    {
                        List<NLUEntity> entities = wordToEntity[word];

                        int key = word.Length;

                        if (results.ContainsKey(key))
                        {
                            results[key].AddRange(entities.Select(item => item.Clone()).ToList());
                        }
                        else
                        {
                            results.Add(key, entities.Select(item => item.Clone()).ToList());
                        }
                        
                    }
                }

                foreach (var pair in results.Reverse())
                {
                    resultList.AddRange(pair.Value);
                }
            }
            catch(Exception e)
            {
                throw new Exception("Parse entities error.", e);
            }

            return resultList;
        }

        public List<AttributePair> ParseAttributes(string intentName, List<NLUEntity> entities)
        {
            if (entities == null || entities.Count() == 0)
                return null;
            
            try
            {
                List<AttributePair> pairs = new List<AttributePair>();

                Dictionary<string, List<AttributePair>> map = this.attributeMap[intentName];

                foreach (NLUEntity entity in entities)
                {
                    string identity = entity.GetIdentity();
                    if (map.Keys.Contains(identity))
                    {
                        pairs.AddRange(map[identity]);
                    }
                }

                if (pairs.Count() > 0)
                {
                    return pairs;
                }
                else
                {
                    return null;
                }
            }
            catch { return null; }
            
        }

        public void AddIntentRule(NLUIntentRule intentRule)
        {
            if (intentRule.type == IntentRuleType.POSITIVE)
                this.intentPositiveDetermineRules.Add(intentRule);
            else if (intentRule.type == IntentRuleType.NEGATIVE)
            {
                string intentName = intentRule.intentName;

                if (!this.intentNegativeDeterminRules.Keys.Contains(intentName))
                {
                    this.intentNegativeDeterminRules.Add(intentName, new List<NLUIntentRule>());
                }

                this.intentNegativeDeterminRules[intentName].Add(intentRule);
            }
        }

        private bool IsMatched(List<string> ruleSecs, string query)
        {
            bool matched = true;
            foreach (string ruleSec in ruleSecs)
            {
                Regex regex = new Regex(ruleSec.ToLower());
                Match match = regex.Match(query);
                if (!match.Success)
                {
                    matched = false;
                    break;
                }
            }

            return matched;
        }

        private TempData MatchPositiveRules(string query, int startIndex)
        {
            TempData result = new TempData();

            int index = startIndex;

            while(index < this.intentPositiveDetermineRules.Count())
            {
                NLUIntentRule rule = this.intentPositiveDetermineRules[index];

                bool matched = IsMatched(rule.ruleSecs, query);
                
                if (matched)
                {
                    result.intentName = rule.intentName;
                    result.positionInPosRules = index;

                    return result;
                }
                else
                {
                    index += 1;
                }
            }

            return null;
        }

        private bool IsNegative(string query, string intentName)
        {
            if (!this.intentNegativeDeterminRules.Keys.Contains(intentName))
            {
                return false;
            }
            else
            {
                foreach(NLUIntentRule rule in this.intentNegativeDeterminRules[intentName])
                {
                    bool matched = IsMatched(rule.ruleSecs, query);

                    if (matched)
                    {
                        return true;
                    }
                }
                return false;
            }            
        }

        public string DetectIntent(string query)
        {           
            int index = 0;

            while (index < this.intentPositiveDetermineRules.Count())
            {
                TempData temp = MatchPositiveRules(query, index);

                if (temp == null)
                { 
                    index += 1;
                }
                else
                {
                    if (IsNegative(query, temp.intentName))
                    {
                        index = temp.positionInPosRules + 1;
                    }
                    else
                    {
                        return temp.intentName;
                    }
                }

            }

            return null;
        }

        public void SetSceanrioCache(Dictionary<string, ScenarioSetting> sceanrioCache)
        {
            this.sceanrioCache = sceanrioCache;
        }

        public ScenarioSetting GetSetting(string scenarioName)
        {
            if (!this.sceanrioCache.Keys.Contains(scenarioName))
            {
                /*this.sceanrioCache.Add(scenarioName, new ScenarioSetting());*/
                throw new Exception("The scenario " + scenarioName + " doesn't exist.");
            }
            return this.sceanrioCache[scenarioName];
        }

        public Vertex GetRoot(string scenarioName)
        {
            if (this.sceanrioCache.ContainsKey(scenarioName))
            {
                return this.sceanrioCache[scenarioName].root;
            }
            else
            {
                return null;
            }
        }
    }

    class TempData
    {
        public string intentName { get; set; }
        public int positionInPosRules { get; set; }
    }
}