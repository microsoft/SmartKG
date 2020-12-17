// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using Serilog;
using SmartKG.Common.Data.KG;
using SmartKG.Common.Data.Visulization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartKG.Common.DataStoreMgmt
{
    public class KnowledgeGraphDataFrame
    {
        private Dictionary<string, List<Vertex>> vertexNameCache { get; set; }
        private Dictionary<string, Vertex> vertexIdCache { get; set; }
        private Dictionary<string, Dictionary<RelationLink, List<string>>> outRelationDict { get; set; }
        private Dictionary<string, Dictionary<RelationLink, List<string>>> inRelationDict { get; set; }        

        private Dictionary<string, List<Edge>> scenarioEdgesDict { get; set; }

        //private Dictionary<string, List<ColorConfig>> vertexLabelColorMap { get; set; }

        private List<Vertex> rootVertexes { get; set; }

        private HashSet<string> scenarioNames { get; set; }

        private ILogger log;

        public KnowledgeGraphDataFrame()
        {
            log = Log.Logger.ForContext<KnowledgeGraphDataFrame>();
            this.Clean();
        }

        public void Clean()
        {
            this.vertexNameCache = new Dictionary<string, List<Vertex>>();

            this.vertexIdCache = new Dictionary<string, Vertex>();

            this.outRelationDict = new Dictionary<string, Dictionary<RelationLink, List<string>>>();

            this.inRelationDict = new Dictionary<string, Dictionary<RelationLink, List<string>>>();            

            this.scenarioEdgesDict = new Dictionary<string, List<Edge>>();

            //this.vertexLabelColorMap = new Dictionary<string, List<ColorConfig>>();

            this.rootVertexes = new List<Vertex>();

            this.scenarioNames = new HashSet<string>();
        }

        public void SetRootVertexes(List<Vertex> roots)
        {
            this.rootVertexes = roots;
        }

        public List<Vertex> GetRootVertexes()
        {
            return this.rootVertexes;
        }

        

        public HashSet<string> GetScenarioNames()
        {
            return this.scenarioNames;
        }

        public void SetScenarioNames(HashSet<string> scenarios)
        {
            this.scenarioNames = scenarios;
        }

        public void SetVertexNameCache(Dictionary<string, List<Vertex>> vertexNameCache)
        {
            this.vertexNameCache = vertexNameCache;
        }

        public void SetVertexIdCache(Dictionary<string, Vertex> vertexIdCache)
        {
            this.vertexIdCache = vertexIdCache;
        }

        public void SetOutRelationDict(Dictionary<string, Dictionary<RelationLink, List<string>>> outRelationDict)
        {
            this.outRelationDict = outRelationDict;
        }

        public void SetInRelationDict(Dictionary<string, Dictionary<RelationLink, List<string>>> inRelationDict)
        {
            this.inRelationDict = inRelationDict;
        }       

        public List<Vertex> GetVertexByLabel(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                return null;
            }
            List<Vertex> results = new List<Vertex>();

            foreach (List<Vertex> vertexes in this.vertexNameCache.Values)
            {
                foreach (Vertex vertex in vertexes)
                {
                    if (vertex.label.Contains(label))
                    {
                        results.Add(vertex);
                    }
                }
            }

            return results;
        }

        public List<Vertex> GetVertexByName(string vertexName)
        {
            if (string.IsNullOrWhiteSpace(vertexName))
            {
                return null;
            }

            if (!this.vertexNameCache.Keys.Contains(vertexName))
            {
                log.Information(vertexName, "doesn't exist");
                return null;
            }
            else
            {
                return this.vertexNameCache[vertexName];
            }
        }

        public List<Vertex> GetVertexByKeyword(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return null;
            }

            List<string> vNames = this.vertexNameCache.Keys.ToList();

            HashSet<string> matchedNames = new HashSet<string>();

            foreach (string vName in vNames)
            {
                if (vName.Contains(keyword))
                {
                    matchedNames.Add(vName);
                }
            }

            List<Vertex> results = new List<Vertex>();

            foreach (string mV in matchedNames)
            {
                List<Vertex> mR = GetVertexByName(mV);
                if (mR != null && mR.Count > 0)
                {
                    results.AddRange(mR);
                }
            }

            if (results.Count == 0)
            {
                return null;
            }
            else
            {
                return results;
            }
        }

        public (bool, List<Vertex>) GetVertexesByScenarios(List<string> scenarios)
        {
            List<Vertex> allVertexes = this.vertexIdCache.Values.ToList();

            if (scenarios == null || scenarios.Count == 0)
            {
                return (true, allVertexes);
            }

            bool isScenarioExist = false;

            List<Vertex> catchedVertexes = new List<Vertex>();

            foreach (Vertex vertex in allVertexes)
            {
                if (vertex.scenarios == null || vertex.scenarios.Count == 0)
                {
                    continue;
                }

                foreach (string scenario in scenarios)
                {
                    if (vertex.scenarios.Contains(scenario))
                    {
                        isScenarioExist = true;
                        catchedVertexes.Add(vertex);

                        break;
                    }
                }
            }

            return (isScenarioExist, catchedVertexes);
        }

        public List<Edge> GetRelationsByScenarios(List<string> scenarios)
        {
            List<Edge> results = new List<Edge>();

            if (scenarios == null || scenarios.Count == 0)
            {
                foreach (string key in this.scenarioEdgesDict.Keys)
                {
                    results.AddRange(this.scenarioEdgesDict[key]);
                }
            }
            else
            {
                foreach (string scenario in scenarios)
                {
                    if (this.scenarioEdgesDict.ContainsKey(scenario))
                    {
                        results.AddRange(this.scenarioEdgesDict[scenario]);
                    }
                }
            }

            return results;
        }

        public void SetScenarioEdgesDict(Dictionary<string, List<Edge>> scenarioEdgesDict)
        {
            this.scenarioEdgesDict = scenarioEdgesDict;
        }

        public List<Vertex> GetAllVertexes()
        {
            return this.vertexIdCache.Values.ToList();
        }

        public Vertex GetVertexById(string vertexId)
        {
            if (!this.vertexIdCache.Keys.Contains(vertexId))
            {
                log.Information(vertexId, "doesn't exist");
                return null;
            }
            else
            {
                return this.vertexIdCache[vertexId];
            }
        }

        public Dictionary<RelationLink, List<string>> GetChildrenLinkDict(string vertexId)
        {
            if (vertexId != null && this.outRelationDict.ContainsKey(vertexId))
            {
                return this.outRelationDict[vertexId];
            }

            return null;
        }

        public Dictionary<RelationLink, List<string>> GetParentLinkDict(string vertexId)
        {
            if (vertexId != null && this.inRelationDict.ContainsKey(vertexId))
            {
                return this.inRelationDict[vertexId];
            }

            return null;
        }

        public Dictionary<string, HashSet<string>> GetChildrenIds(string vertexId, string relationType, string scenarioName)
        {
            if (string.IsNullOrWhiteSpace(vertexId))
            {
                return null;
            }

            RelationLink targetLink = new RelationLink(relationType, scenarioName);

            Dictionary<string, HashSet<string>> childrenIds = new Dictionary<string, HashSet<string>>();

            if (outRelationDict.ContainsKey(vertexId))
            {
                foreach (RelationLink link in outRelationDict[vertexId].Keys)
                {
                    if (targetLink.IsCompatible(link))
                    {
                        string linkRelationType = link.relationType;

                        if (!childrenIds.ContainsKey(linkRelationType))
                        {
                            HashSet<string> cIds = new HashSet<string>();
                            childrenIds.Add(linkRelationType, cIds);
                        }

                        List<string> conncectedIds = outRelationDict[vertexId][link];
                        foreach (string cid in conncectedIds)
                        {
                            childrenIds[linkRelationType].Add(cid);
                        }
                    }
                }
            }

            return childrenIds;
        }
    }
}
