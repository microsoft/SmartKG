// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using Serilog;
using SmartKG.Common.Data.KG;
using SmartKG.Common.Data.Visulization;
using SmartKG.Common.DataStoreMgmt;
using SmartKG.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartKG.KGManagement.GraphSearch
{
    public class GraphExecutor
    {
        private KnowledgeGraphDataFrame kgDF = null;
        private ILogger log;

        public GraphExecutor(string datastoreName)
        {
            log = Log.Logger.ForContext<GraphExecutor>();

            DataStoreFrame dsFrame = DataStoreManager.GetInstance().GetDataStore(datastoreName);

            if (dsFrame != null)
                this.kgDF = dsFrame.GetKG();            
        }

        public void LogInformation(ILogger log, string title, string content)
        {
            log.Information(title + " " + content);
        }

        public void LogError(ILogger log, Exception e)
        {
            log.Error(e.Message, e);
        }

        public (bool, VisulizedVertex) GetVertexById(string vId)
        {
            if (this.kgDF == null)
                return (false, null);

            Vertex vertex =  this.kgDF.GetVertexById(vId);

            if (vertex == null)
                return (true, null);
            else
                return (true, ConvertVertex(vertex));
        }
        
        public (bool, List<VisulizedVertex>) SearchVertexesByName(string keyword)
        {

            if (this.kgDF == null)
                return (false, null);

            List<Vertex> searchedVertexes =  this.kgDF.GetVertexByKeyword(keyword);

            if (searchedVertexes == null || searchedVertexes.Count == 0)
            {
                return (true, null);
            }

            List<VisulizedVertex> results = new List<VisulizedVertex>();
            
            foreach(Vertex vertex in searchedVertexes)
            {
                results.Add(ConvertVertex(vertex));
            }

            return (true, results);
        }

        public (bool, List<VisulizedVertex>) FilterVertexesByProperty(string propertyName, string propertyValue)
        {
            if (this.kgDF == null)
                return (false, null);

            if (string.IsNullOrEmpty(propertyName))
            {
                Exception e = new Exception("Invalid input: propertyName is empty.");
                LogError(this.log, e);
                throw (e);
            }

            if (string.IsNullOrEmpty(propertyValue))
            {
                Exception e = new Exception("Invalid input: propertyValue is empty.");
                LogError(this.log, e);
                throw (e);
            }

            List<Vertex> allVertexes = this.kgDF.GetAllVertexes();

            List<VisulizedVertex> results = new List<VisulizedVertex>();

            foreach(Vertex vertex in allVertexes)
            {
                string value = vertex.GetPropertyValue(propertyName);

                if (value == null || string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }
                else
                {
                    if (value == propertyValue)
                    {
                        results.Add(ConvertVertex(vertex));
                    }
                }
            }

            return (true, results);
        }

        public (bool, List<string>) GetScenarioNames()
        {
            if (this.kgDF == null)
                return (false, null);

            HashSet<string> names = this.kgDF.GetScenarioNames();
            if (names == null || names.Count == 0)
            {
                return (true, null);
            }
            else
            { 
                return (true, this.kgDF.GetScenarioNames().ToList());
            }
        }

        public (bool, bool, List<ColorConfig>) GetColorConfigs(string scenarioName)
        {

            if (this.kgDF == null)
                return (false, false, null);

            Dictionary<string, List<ColorConfig>> colorConfigs = this.kgDF.GetVertexLabelColorMap();

            if (colorConfigs == null || colorConfigs.Count == 0)
            {
                return (true, false, null);
            }
            else if (string.IsNullOrWhiteSpace(scenarioName))
            {
                List<ColorConfig> configs = new List<ColorConfig>();
                foreach (string scenario in colorConfigs.Keys)
                {
                    configs.AddRange(colorConfigs[scenario]);
                }

                return (true, true, configs);
            }
            else
            {
                if (!colorConfigs.ContainsKey(scenarioName))
                {
                    return (true, false, null);
                }
                else
                {
                    return (true, true, colorConfigs[scenarioName]);
                }
            }
        }

        public (bool, bool, List<VisulizedVertex>, List<VisulizedEdge>) GetVertexesAndEdgesByScenarios(List<string> scenarios)
        {

            if (this.kgDF == null)
                return (false, false, null, null);

            (bool isSceanrioExist, List<Vertex> catchedVertexes) = this.kgDF.GetVertexesByScenarios(scenarios);

            List<VisulizedVertex> vvs = new List<VisulizedVertex>();

            foreach(Vertex vertex in catchedVertexes)
            {
                vvs.Add(ConvertVertex(vertex));
            }

            List<Edge> catchedEdges = this.kgDF.GetRelationsByScenarios(scenarios);

            List<VisulizedEdge> ves = new List<VisulizedEdge>();

            foreach(Edge edge in catchedEdges)
            {
                ves.Add(ConvertEdge(edge));
            }
            
            return (true, isSceanrioExist, vvs, ves);
        }

        public (bool, List<VisulizedVertex>, List<VisulizedEdge>) GetFirstLevelRelationships(string vId)
        {
            if (this.kgDF == null)
                return (false, null, null);

            Vertex vertex = this.kgDF.GetVertexById(vId);

            if (vertex == null)
            {
                return (true, null, null);
            }

            List<VisulizedVertex> rVVs = new List<VisulizedVertex>();
            List<VisulizedEdge> rVEs = new List<VisulizedEdge>();

            if (vertex.properties != null && vertex.properties.Count > 0)
            {
                foreach(VertexProperty property in vertex.properties)
                {
                    VisulizedVertex vP = KGUtility.GeneratePropertyVVertex(vertex.label, property.name, property.value);
                    rVVs.Add(vP);

                    VisulizedEdge vEdge = new VisulizedEdge();
                    vEdge.value = vP.displayName;
                    vEdge.sourceId = vertex.id;
                    vEdge.targetId = vP.id;

                    rVEs.Add(vEdge);
                }
            }

            List<VisulizedVertex> tmpRVVs;
            List<VisulizedEdge> tmpRVEs;

            Dictionary <RelationLink, List<string>> childrenLinkDict = this.kgDF.GetChildrenLinkDict(vId);

            HashSet<string> addedIDs = new HashSet<string>();

            if (childrenLinkDict != null)
            {
                (tmpRVVs, tmpRVEs) = GetConnectedVertexesAndEdges(addedIDs, vId, childrenLinkDict, true);
                
                rVVs.AddRange(tmpRVVs);
                rVEs.AddRange(tmpRVEs);
            }

            Dictionary<RelationLink, List<string>> parentLinkDict = this.kgDF.GetParentLinkDict(vId);

            if (parentLinkDict != null)
            {
                (tmpRVVs, tmpRVEs) = GetConnectedVertexesAndEdges(addedIDs, vId, parentLinkDict, false);

                rVVs.AddRange(tmpRVVs);
                rVEs.AddRange(tmpRVEs);
            }

            return (true, rVVs, rVEs);
        }

        private (List<VisulizedVertex>, List<VisulizedEdge>) GetConnectedVertexesAndEdges(HashSet<string> addedIDs, string vId, Dictionary<RelationLink, List<string>> relationDict, bool vIsSrc)
        {
            List<VisulizedVertex> rVVs = new List<VisulizedVertex>();
            List<VisulizedEdge> rVEs = new List<VisulizedEdge>();

            HashSet<string> cIdSet = new HashSet<string>();

            if (relationDict != null && relationDict.Count > 0)
            {
                foreach (RelationLink link in relationDict.Keys)
                {
                    string relationType = link.relationType;

                    foreach (string cId in relationDict[link])
                    {
                        if (cIdSet.Contains(cId))
                        {
                            continue;
                        }
                        
                        cIdSet.Add(cId);
                        
                        VisulizedVertex vRV = ConvertVertex(this.kgDF.GetVertexById(cId));

                        VisulizedEdge vRE = new VisulizedEdge();
                        vRE.value = relationType;

                        if (vIsSrc)
                        { 
                            vRE.sourceId = vId;
                            vRE.targetId = vRV.id;
                        }
                        else
                        {
                            vRE.targetId = vId;
                            vRE.sourceId = vRV.id;
                        }

                        if (!addedIDs.Contains(vRV.id))
                        { 
                            rVVs.Add(vRV);
                            addedIDs.Add(vRV.id);
                        }
                        rVEs.Add(vRE);
                    }
                }
            }

            return (rVVs, rVEs);
        }
        

        private VisulizedVertex ConvertVertex(Vertex vertex)
        {
            if (vertex == null)
            {
                return null;
            }

            VisulizedVertex vv = new VisulizedVertex();
            vv.id = vertex.id;
            vv.name = vertex.name;
            vv.displayName = vertex.name;
            vv.label = vertex.label;

            return vv;
        }

        private VisulizedEdge ConvertEdge(Edge edge)
        {
            if (edge == null)
            {
                return null;
            }

            VisulizedEdge ve = new VisulizedEdge();
            ve.sourceId = edge.headVertexId;
            ve.targetId = edge.tailVertexId;
            ve.value = edge.relationType;

            return ve;
        }
    }
}
