using Serilog;
using SmartKG.Common.Data.KG;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartKG.KGManagement.DataStore
{
    public sealed class KnowledgeGraphStore
    {
        private static KnowledgeGraphStore uniqueInstance;        
       

        private Dictionary<string, List<Vertex>> vertexNameCache { get; set; }
        private Dictionary<string, Vertex> vertexIdCache { get; set; }
        private Dictionary<string, Dictionary<RelationLink, List<string>>> outRelationDict { get; set; }        
        private Dictionary<string, Dictionary<RelationLink, List<string>>> inRelationDict { get; set; }
        private Dictionary<string, HashSet<string>> nameIdCache { get; set; }

        private List<Vertex> rootVertexes { get; set; }
       
        private ILogger log;

        private KnowledgeGraphStore()
        {
            log = Log.Logger.ForContext<KnowledgeGraphStore>();
        }

        public static KnowledgeGraphStore GetInstance()
        {

            if (uniqueInstance == null)
            {
                uniqueInstance = new KnowledgeGraphStore();
            }
            return uniqueInstance;
        }

        public void SetRootVertexes(List<Vertex> roots)
        {
            this.rootVertexes = roots;
        }
        
        public List<Vertex> GetRootVertexes()
        {
            return this.rootVertexes;
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

        public void SetNameIdCache(Dictionary<string, HashSet<string>> nameIdMap)
        {
            this.nameIdCache = nameIdMap;
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
            
            foreach(string vName in vNames)
            {
                if (vName.Contains(keyword))
                {
                    matchedNames.Add(vName);
                }
            }

            List<Vertex> results = new List<Vertex>();

            foreach(string mV in matchedNames)
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

            //relationType = targetLink.relationType;

            //Dictionary<string, List<string>> allTypeChildrenIds = this.relationIdCache[vertexId].childrenIds;

            Dictionary<string, HashSet<string>> childrenIds = new Dictionary<string, HashSet<string>>();

            if (outRelationDict.ContainsKey(vertexId))
            {
                foreach(RelationLink link in outRelationDict[vertexId].Keys)
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