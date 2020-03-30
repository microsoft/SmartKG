// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using SmartKG.KGManagement.DataStore;
using System;
using System.Collections.Generic;
using SmartKG.Common.Data.KG;

namespace SmartKG.KGManagement.DataPersistance
{
    public class DataPersistanceKGParser
    {
        public static string defaultRelationType = "contains";
          

        private List<Vertex> vCollection;
        private List<Edge> eColletion;

        public DataPersistanceKGParser(List<Vertex> vList, List<Edge> eList)
        {
            
            this.vCollection = vList; 
            this.eColletion = eList;           
        }       

        private (Dictionary<string, HashSet<string>>, Dictionary<string, Dictionary<RelationLink, List<string>>>, Dictionary<string, Dictionary<RelationLink, List<string>>>, Dictionary<string, List<Edge>>) GenerateRelationship(List<Vertex> vertexes, List<Edge> edges)
        {            
            Dictionary<string, string> vertexIdNameMap = new Dictionary<string, string>();
            Dictionary<string, HashSet<string>> vertexNameIdsMap = new Dictionary<string, HashSet<string>>();
            foreach (Vertex vertex in vertexes)
            {
                vertexIdNameMap.Add(vertex.id, vertex.name);

                if (vertexNameIdsMap.ContainsKey(vertex.name))
                {
                    vertexNameIdsMap[vertex.name].Add(vertex.id);
                }
                else
                {
                    HashSet<string> idSet = new HashSet<string>();
                    idSet.Add(vertex.id);

                    vertexNameIdsMap.Add(vertex.name, idSet);
                }
            }

            Dictionary<string, Dictionary<RelationLink, List<string>>> outRelationMap = new Dictionary<string, Dictionary<RelationLink, List<string>>>();
            Dictionary<string, Dictionary<RelationLink, List<string>>> inRelationMap = new Dictionary<string, Dictionary<RelationLink, List<string>>>();


            Dictionary<string, List<Edge>> scenarioEdgesMap = new Dictionary<string, List<Edge>>();

            foreach (Edge edge in edges)
            {
                if (edge.scenarios != null && edge.scenarios.Count > 0)
                {
                    foreach(string scenario in edge.scenarios)
                    {
                        if (!scenarioEdgesMap.ContainsKey(scenario))
                        {                         
                            List<Edge> edgesForScenario = new List<Edge>();
                            scenarioEdgesMap.Add(scenario, edgesForScenario);
                        }
                        scenarioEdgesMap[scenario].Add(edge);
                    }
                }

                string headVertexId = edge.headVertexId;
                string tailVertexId = edge.tailVertexId;

                if (!vertexIdNameMap.ContainsKey(headVertexId) || !vertexIdNameMap.ContainsKey(tailVertexId))
                {
                    throw new Exception("VertexId is invalid: " + headVertexId + "; " + tailVertexId);
                }

                string relationType = edge.relationType;
                
                
                if (relationType == null)
                {
                    throw new Exception("relationType is empty in Edge from " + headVertexId + " to " + tailVertexId + ".\nIf you have no dedicated scenarios, please set the default value of relationType as \"contains\"");
                }

                if (edge.scenarios == null || edge.scenarios.Count == 0)
                {
                    throw new Exception("scenarios is empty in Edge from " + headVertexId + " to " + tailVertexId + ".\nIf you have no dedicated scenarios, please set the default value of secanrios as [\"Default\"]");                   
                }

                List<RelationLink> links = new List<RelationLink>();

                foreach (string scenarioName in edge.scenarios)
                {
                    links.Add(new RelationLink(relationType, scenarioName));
                }
                               

                foreach (RelationLink link in links)
                {
                    // --- Create outRelationDict

                    string vertexId = headVertexId;
                    string childrenId = tailVertexId;

                    Dictionary<RelationLink, List<string>> outLinkDict;

                    if (!outRelationMap.ContainsKey(vertexId))
                    {
                        outLinkDict = new Dictionary<RelationLink, List<string>>();
                        outLinkDict.Add(link, new List<string> { childrenId });

                        outRelationMap.Add(vertexId, outLinkDict);
                    }
                    else
                    {
                        outLinkDict = outRelationMap[vertexId];
                        bool inserted = false;
                        foreach(RelationLink savedLink in outLinkDict.Keys)
                        {
                            if (savedLink.Equals(link))
                            {
                                outLinkDict[savedLink].Add(childrenId);
                                inserted = true;
                                break;
                            }
                        }

                        if (!inserted)
                        {
                            outLinkDict.Add(link, new List<string> { childrenId });
                        }
                    }

                    // --- Create inRelationDict

                    vertexId = tailVertexId;
                    string parentId = headVertexId;

                    Dictionary<RelationLink, List<string>> inLinkDict;

                    if (!inRelationMap.ContainsKey(vertexId))
                    {
                        inLinkDict = new Dictionary<RelationLink, List<string>>();
                        inLinkDict.Add(link, new List<string> { parentId });

                        inRelationMap.Add(vertexId, inLinkDict);
                    }
                    else
                    {
                        inLinkDict = inRelationMap[vertexId];
                        bool inserted = false;
                        foreach (RelationLink savedLink in inLinkDict.Keys)
                        {
                            if (savedLink.Equals(link))
                            {
                                inLinkDict[savedLink].Add(parentId);
                                inserted = true;
                                break;
                            }
                        }

                        if (!inserted)
                        {
                            inLinkDict.Add(link, new List<string> { parentId });
                        }
                    }
                    
                }
            }

            return (vertexNameIdsMap, outRelationMap, inRelationMap, scenarioEdgesMap);
        }        

        public void ParseKG()
        {
            List<Vertex> vertexes = this.vCollection; // this.GetVertexes(null);
            List<Edge> edges = this.eColletion; // this.GetEdges(null);

            (Dictionary<string, HashSet<string>> nameIdMap, Dictionary<string, Dictionary<RelationLink, List<string>>> outRelationDict,  Dictionary<string, Dictionary<RelationLink, List<string>>> inRelationDict, Dictionary<string, List<Edge>> scenarioEdgesMap)  = this.GenerateRelationship(vertexes, edges);           

            Dictionary<string, List<Vertex>> vNameCache = new Dictionary<string, List<Vertex>>();
            Dictionary<string, Vertex> vIdCache = new Dictionary<string, Vertex>();
            List<Vertex> roots = new List<Vertex>();

            HashSet<string> scenarioNames = new HashSet<string>();
            
            foreach (Vertex vertex in vertexes)
            {
                scenarioNames.UnionWith(vertex.scenarios);

                if (vertex.nodeType == "ROOT")
                {
                    roots.Add(vertex);
                }                

                if (vNameCache.ContainsKey(vertex.name))
                {
                    vNameCache[vertex.name].Add(vertex);
                }
                else
                {
                    List<Vertex> vertexesGroupedByName = new List<Vertex>();
                    vertexesGroupedByName.Add(vertex);
                    vNameCache.Add(vertex.name, vertexesGroupedByName);
                }
                vIdCache.Add(vertex.id, vertex);
            }

            KnowledgeGraphStore store = KnowledgeGraphStore.GetInstance();
            
            store.SetRootVertexes(roots);
            store.SetVertexIdCache(vIdCache);
            store.SetVertexNameCache(vNameCache);
            store.SetOutRelationDict(outRelationDict);
            store.SetInRelationDict(inRelationDict);
            store.SetNameIdCache(nameIdMap);
            store.SetScenarioEdgesDict(scenarioEdgesMap);
            store.SetScenarioNames(scenarioNames);
        }
    }
}

