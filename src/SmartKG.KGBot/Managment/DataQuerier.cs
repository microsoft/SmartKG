// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using SmartKG.Common.ContextStore;
using SmartKG.Common.Data;
using SmartKG.Common.Data.KG;
using SmartKG.Common.Data.LU;
using SmartKG.Common.DataStoreMgmt;
using SmartKG.KGBot.Data.Response;
using System;
using System.Collections.Generic;
using System.Linq;


namespace SmartKG.KGBot.Managment
{
    public class DataQuerier
    {
        private string quitPromptStr = "\n或者输入 q 退出当前对话。\n";
        
        private MessageGenerator msgGenerator;
        private DataStoreFrame dsFrame;

        public DataQuerier(string datastoreName, RUNNINGMODE runningMode)
        {
            this.dsFrame = DataStoreManager.GetInstance().GetDataStore(datastoreName);            
            this.msgGenerator = new MessageGenerator(runningMode);
        }

        public QueryResult SearchVertexes(ContextManager contextMgmt, List<NLUEntity> entities)
        {            
            QueryResult responseContent = new QueryResult(false, "Failed", ResponseItemType.Other); 

            if (entities == null && entities.Count == 0)
            {
                responseContent = this.msgGenerator.GenerateErrorMessage("无法查找到对应节点，请确定输入的限定条件正确");
                contextMgmt.ExitDialog();
            }            
            else
            {
                NLUEntity firstEntity = entities[0];

                List<Vertex> results = new List<Vertex>();

                if (firstEntity.GetEntityType() == "NodeName")
                {
                    
                    Vertex vertex = dsFrame.SearchGraph(firstEntity.GetEntityValue(), contextMgmt.GetSecnarioName(), contextMgmt.GetSavedAttributes());

                    results.Add(vertex);
                }
                else if (firstEntity.GetEntityType() == "Label")
                {
                    results = dsFrame.SearchGraphByLabel(firstEntity.GetEntityValue(), contextMgmt.GetSecnarioName(), contextMgmt.GetSavedAttributes());
                }
                

                if (results == null || results.Count == 0)
                {
                    responseContent = this.msgGenerator.GenerateErrorMessage("无法查找到对应节点，请确定输入的限定条件正确");
                    contextMgmt.ExitDialog();
                }
                else 
                {
                    string headMessage = "";

                    if (entities.Count > 1)
                    {
                        headMessage = firstEntity.GetEntityValue();

                        for (int i = 1; i < entities.Count; i ++)
                        {
                            NLUEntity entity = entities[i];

                            if (entity.GetEntityType() == "RelationType")
                            {
                                HashSet<string> relationTypeSet = new HashSet<string>();
                                relationTypeSet.Add(entity.GetEntityValue());

                                List<Vertex> children = new List<Vertex>();

                                foreach (Vertex vertex in results)
                                {
                                    Dictionary<string, List<Vertex>> childrenDict = dsFrame.GetChildren(vertex, relationTypeSet, contextMgmt.GetSavedAttributes(), contextMgmt.GetSecnarioName());
                                    if (childrenDict != null)
                                    {
                                        List<Vertex> currentChildren = childrenDict[entity.GetEntityValue()];
                                        children.AddRange(currentChildren);
                                    }
                                }

                                if (children != null && children.Count > 0)
                                {
                                    headMessage += "的" + entity.GetEntityValue();

                                    results = children;
                                }
                            }
                            else if (entity.GetEntityType() == "Label")
                            {
                                List<Vertex> newResults = new List<Vertex>();
                                foreach (Vertex vertex in results)
                                {
                                    if (vertex.label == entity.GetEntityValue())
                                    {
                                        newResults.Add(vertex);
                                    }
                                }

                                if (newResults.Count > 0)
                                {
                                    headMessage += "的" + entity.GetEntityValue();
                                    results = newResults;
                                }
                            }

                        }
                    }

                    if (results.Count == 1)
                    {
                        Vertex vertex = results[0];

                        if (vertex.isLeaf())
                        {
                            responseContent = this.msgGenerator.GenerateEndVertexMessage(vertex);
                            contextMgmt.ExitDialog();
                        }
                        else
                        {
                            List<DialogSlot> validSlots = GetValidSlots(contextMgmt);

                            if (validSlots == null || validSlots.Count() == 0)
                            {
                                contextMgmt.StartDialog();
                                responseContent = GetChildren(contextMgmt, vertex, null);
                            }
                            else
                            {
                                contextMgmt.SetSlots(validSlots);
                                contextMgmt.EnterSlotFilling();

                                responseContent = this.msgGenerator.GenerateSlotMessage(validSlots[0]);
                            }
                        }
                    }
                    else
                    {
                        contextMgmt.StartDialog();
                        responseContent = GetMessageForVertexes(contextMgmt, results);
                    } 
                    
                    if (!string.IsNullOrWhiteSpace(headMessage) && responseContent.success)
                    {
                        responseContent.responseMessage = headMessage + ":\n" + responseContent.responseMessage;
                    }
                }

            }

            return responseContent;
        }

        public QueryResult HandleSlotFilling(ContextManager contextMgmt, HashSet<string> relationTypeSet)
        {            
            Dictionary<string, List<Vertex>> vertexDict = dsFrame.FilterGraph(contextMgmt.GetStartVertexName(), contextMgmt.GetSecnarioName(), relationTypeSet, contextMgmt.GetSavedAttributes());

            if (vertexDict == null || vertexDict.Count() == 0)
            {
                contextMgmt.ExitDialog();
                return msgGenerator.GenerateErrorMessage("找不到符合要求的保险产品" );
            }
            else
            {
                if (contextMgmt.IsAllSlotsFilled())
                {
                    contextMgmt.StartDialog();

                    Vertex startVertex = dsFrame.SearchGraph(contextMgmt.GetStartVertexName(), contextMgmt.GetSecnarioName(), contextMgmt.GetSavedAttributes());

                    return GetChildren(contextMgmt, startVertex, relationTypeSet);
                }
                else
                {
                    contextMgmt.ForwardSlotFilling();
                    return msgGenerator.GenerateSlotMessage(contextMgmt.GetSlot(contextMgmt.GetCurrentSlotSeqNum()));
                }

            }
        }

        public List<DialogSlot> GetValidSlots(ContextManager contextMgmt)
        {
            List<DialogSlot> validSlots = new List<DialogSlot>();
            List<DialogSlot> slots = dsFrame.GetConfiguredSlots(contextMgmt.GetSecnarioName());

            if (slots != null && slots.Count() > 0)
            {

                List<AttributePair> attributes = contextMgmt.GetSavedAttributes();
                if (attributes != null && attributes.Count() > 0)
                {

                    foreach (DialogSlot slot in slots)
                    {
                        bool isFilled = false;
                        foreach (AttributePair attribute in attributes)
                        {
                            string attributeName = attribute.attributeName;
                            if (slot.correspondingAttribute == attributeName)
                            {
                                isFilled = true;
                                break;
                            }
                        }

                        if (!isFilled)
                        {
                            validSlots.Add(slot);
                        }
                    }
                }
                else
                {
                    validSlots = slots;
                }
            }

            return validSlots;
        }

        public QueryResult GetMessageForVertexes(ContextManager contextMgmt, List<Vertex> vertexes)
        {
            string resultForAllItems = "";

            Dictionary<int, Vertex> candidates = new Dictionary<int, Vertex>();
            List<Object> items = new List<Object>();
            int index = 1;

            foreach (Vertex aVertex in vertexes)
            {
                candidates.Add(index, aVertex);

                OptionItem item = new OptionItem();
                item.seqNo = index;
                item.vertex = aVertex;                
                items.Add(item);

                resultForAllItems += "[" + item.seqNo.ToString() + "]: " + aVertex.name + "\n";

                index += 1;
            }
            
            ResponseItemType itemType = ResponseItemType.Option;
            QueryResult result = new QueryResult(true, resultForAllItems + quitPromptStr, itemType);
            result.AddResponseItems(items);

            contextMgmt.SetCandidates(candidates);

            return result;
        }

        public QueryResult GetChildren(ContextManager contextMgmt, Vertex parentVertex, HashSet<string> relationTypeSet)
        {
            Dictionary<string, List<Vertex>> childrenDict = dsFrame.GetChildren(parentVertex, relationTypeSet, contextMgmt.GetSavedAttributes(), contextMgmt.GetSecnarioName());

            string headMessage = msgGenerator.GetInformationOfVertex(parentVertex);

            if ((childrenDict == null || childrenDict.Count == 0) && (relationTypeSet != null && relationTypeSet.Count > 0))
            {
                childrenDict = dsFrame.GetChildren(parentVertex, null, contextMgmt.GetSavedAttributes(), contextMgmt.GetSecnarioName());
            }

            if (childrenDict == null || childrenDict.Count == 0)
            {
                contextMgmt.ExitDialog();
                return msgGenerator.GenerateErrorMessage(parentVertex.id + "没有子节点");
            }
            else
            {
                string resultStr = headMessage;
                List<Object> items = new List<Object>();
                int index = 1;
                Dictionary<int, Vertex> candidates = new Dictionary<int, Vertex>();

                foreach (string relationType in childrenDict.Keys)
                {
                    List<Vertex> vertexs = childrenDict[relationType];

                    string resultForAllItems = "";

                    foreach (Vertex aVertex in vertexs)
                    {
                        candidates.Add(index, aVertex);

                        OptionItem item = new OptionItem();
                        item.seqNo = index;
                        item.vertex = aVertex;
                        item.relationType = relationType;
                        items.Add(item);

                        resultForAllItems += "[" + item.seqNo.ToString() + "]: (" + relationType + ") " + aVertex.name + "\n";

                        index += 1;
                    }

                    resultStr += resultForAllItems;
                }

                ResponseItemType itemType = ResponseItemType.Option;
                QueryResult result = new QueryResult(true, resultStr + quitPromptStr, itemType);
                result.AddResponseItems(items);

                contextMgmt.SetCandidates(candidates);

                return result;
            }                                    
        }        

    }
}
