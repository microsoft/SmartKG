using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartKG.KGBot.Data.Response;
using SmartKG.KGBot.Data;
using SmartKG.KGBot.StorageAccessor;
using SmartKG.KGBot.NaturalLanguageUnderstanding;
using Serilog;
using SmartKG.KGBot.DataStore;
using SmartKG.KGBot.Data.Request;
using SmartKG.Common.Logger;
using SmartKG.Common.Data.LU;
using SmartKG.Common.Data.KG;

namespace SmartKG.KGBot.Managment
{

    public class DialogManager : KGBotLogHandler
    {
        private string userId;
        private string sessionId;
        private string query;

        private RUNNINGMODE runningMode;      

        ILogger _log;        

        public DialogManager()
        {           
            _log = Log.Logger.ForContext<DialogManager>();
        }

        private string GetOverallLogMsg()
        {
            return "userId: " + this.userId + ", sessionId: " + this.sessionId + ", query: " + query;
        }
        public void LogInformation(ILogger log, string title, string content)
        {
                                    
            log.Information(GetOverallLogMsg() + "\n" + title + ": " + content);
        }

        public void LogError(ILogger log, Exception e)
        {
            log.Error(GetOverallLogMsg() + "\n" + e.Message, e);
        }

        public async Task<QueryResult> Process(string userId, string sessionId, string query, RUNNINGMODE runningMode)
        {
            this.userId = userId;
            this.sessionId = sessionId;
            this.query = query;
            this.runningMode = runningMode;

            LogInformation(_log.Here(), "runningMode", runningMode.ToString());

            ContextManager contextMgmt = new ContextManager(userId, sessionId);
            contextMgmt.GetContext();
            
            NLUResult nlu = new NLUProcessor().Parse(query);

            LogInformation(_log.Here(), "nlu", nlu.ToString());

            NLUResultType type = nlu.GetType();            

            QueryResult result = null;

            if (type == NLUResultType.UNKNOWN && contextMgmt.GetIntent() == null)
            {
                result = GenerateErrorMessage("无法识别意图。", contextMgmt);
            }
            else
            {              
                result = ResponseDialog(nlu, contextMgmt);
            }

            contextMgmt.UpdateContext();

            return result;
        }

        private QueryResult GenerateErrorMessage(string message, ContextManager contextMgmt)
        {
            
            QueryResult result = new QueryResult(false,message, ResponseItemType.Other);
            
            contextMgmt.ExitDialog();
            
            return result;
        }       

        private QueryResult GenerateSlotMessage(DialogSlot slot, ContextManager contextMgmt)
        {
            string question = slot.question;

            if (this.runningMode == RUNNINGMODE.DEVELOPMENT)
            {                
                foreach(OptionItem item in slot.items)
                {                               
                    question += item.seqNo.ToString() + ". " + item.vertex.name + "\n";
                }
            }

            QueryResult result = new QueryResult(true, question, ResponseItemType.Option);
            result.AddResponseItems(slot.items);

            return result;
        }

        public QueryResult GenerateEndVertexMessage(Vertex vertex, ContextManager contextMgmt)
        {
            if (vertex == null)
            {
                return GenerateErrorMessage("要返回的项目为空。", contextMgmt);
            }

            string resultStr = GetInformationOfVertex(vertex);

            QueryResult result = new QueryResult(true, resultStr, ResponseItemType.Other);

            contextMgmt.ExitDialog();

            return result;
        }        

        private string GetInformationOfVertex(Vertex vertex)
        {
            string resultStr = "";
            resultStr += vertex.name + "\n";

            if (vertex.properties != null && vertex.properties.Count > 0)
            { 
                foreach (VertexProperty p in vertex.properties)
                {
                    resultStr += p.name + ":" + p.value + "\n";
                }
            }

            resultStr += vertex.leadSentence + "\n";
            return resultStr;
        }

        private QueryResult GenerateItemsMessage(string headMessage, Dictionary<string, List<Vertex>> vertexDict, ContextManager contextMgmt)
        {
            if (vertexDict == null || vertexDict.Count == 0)
            {
                return GenerateErrorMessage("要返回的项目为空。", contextMgmt);
            }           

            
            string resultStr = headMessage;
            List<Object> items = new List<Object>();
            int index = 1;
            Dictionary<int, Vertex> candidates = new Dictionary<int, Vertex>();

            foreach (string relationType in vertexDict.Keys)
            {
                List<Vertex> vertexs = vertexDict[relationType];

                string resultForAllItems = "";
                
                foreach (Vertex vertex in vertexs)
                {
                    candidates.Add(index, vertex);

                    
                    OptionItem item = new OptionItem();
                    item.seqNo = index;
                    item.vertex = vertex;
                    item.relationType = relationType;
                    items.Add(item);

                    resultForAllItems += "[" + item.seqNo.ToString() + "]: (" + relationType +") " + vertex.name + "\n";


                    index += 1;
                }

                resultStr += resultForAllItems;                
            }

            ResponseItemType itemType = ResponseItemType.Option;
            QueryResult result = new QueryResult(true, resultStr, itemType);
            result.AddResponseItems(items);

            contextMgmt.SetCandidates(candidates);

            return result;
                                
        }

        private QueryResult ResponseDialog(NLUResult nlu, ContextManager contextMgmt)
        {            
            string intent = nlu.GetIntent();
            string scenarioName = intent;


            if (string.IsNullOrWhiteSpace(intent))
            {
                intent = contextMgmt.GetIntent();
                scenarioName = contextMgmt.GetSecnarioName();
            }
            else
            {
                contextMgmt.SetIntent(intent);
                contextMgmt.SetScenarioName(scenarioName);
            }

            try
            {
                DataManager kgMgmt = new DataManager();
               
                if (nlu.GetAttributes() != null && nlu.GetAttributes().Count() > 0)
                {
                    foreach(AttributePair attribute in nlu.GetAttributes())
                    {
                        if (attribute != null)
                        {
                            contextMgmt.AddAttributeFilterCondition(attribute);
                        }
                    }
                }

                LogInformation(_log.Here(), "DialogStatus", contextMgmt.GetStatus().ToString());

                if (contextMgmt.GetStatus() == DialogStatus.PENDING)
                {                    
                    if (nlu.GetType() == NLUResultType.NORMAL)
                    {
                        string startVertexName = null;

                        foreach (NLUEntity entity in nlu.GetEntities())
                        {
                            if (entity.GetEntityType() == "NodeName")
                            {
                                startVertexName = entity.GetEntityValue();
                                break;
                            }
                        }

                        contextMgmt.SetStartVertexName(startVertexName);

                        Vertex vertex = kgMgmt.SearchGraph(contextMgmt.GetStartVertexName(), contextMgmt.GetSecnarioName(), contextMgmt.GetSavedAttributes());

                        QueryResult responseContent;
                        
                        if (vertex == null)
                        {
                            responseContent = GenerateErrorMessage("无法查找到对应节点，请确定输入的限定条件正确", contextMgmt);
                        }
                        if (vertex.isLeaf())
                        {
                            responseContent = GenerateEndVertexMessage(vertex, contextMgmt);
                        }
                        else
                        { 
                            List<DialogSlot> validSlots = new List<DialogSlot>();                            
                            List<DialogSlot> slots = kgMgmt.GetConfiguredSlots(scenarioName);

                            if (slots != null && slots.Count() > 0)
                            { 

                                List<AttributePair> attributes = contextMgmt.GetSavedAttributes();
                                if (attributes != null && attributes.Count() > 0)
                                {

                                    foreach(DialogSlot slot in slots)
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

                            if (validSlots.Count() == 0)
                            {
                                contextMgmt.StartDialog();
                                responseContent = GetChildren(contextMgmt, kgMgmt, vertex, nlu.GetRelationTypeSet(), scenarioName);                                
                            }
                            else
                            {
                                contextMgmt.SetSlots(validSlots);
                                contextMgmt.EnterSlotFilling();
                                
                                responseContent = GenerateSlotMessage(validSlots[0], contextMgmt);
                            }
                        
                        }

                        if (responseContent.success)
                        {
                            contextMgmt.SetIntent(intent);
                            contextMgmt.SetScenarioName(scenarioName);
                            contextMgmt.SaveQuestion(responseContent.responseMessage);
                            contextMgmt.RefreshDurationTime();
                        }

                        return responseContent;
                    }
                    else
                    {
                        return GenerateErrorMessage("无法识别意图。", contextMgmt);
                    }
                }
                else if (contextMgmt.GetStatus() == DialogStatus.SLOTFILLING)
                {
                    QueryResult responseContent;
                    if (nlu.GetType() == NLUResultType.NUMBER)
                    {
                        int lastStep = contextMgmt.GetCurrentSlotSeqNum();
                        DialogSlot lastSlot = contextMgmt.GetSlot(lastStep);
                        int option = nlu.GetOption();

                        try
                        {
                            string attributeName = lastSlot.correspondingAttribute;
                            string attributeValue = lastSlot.answerValues[option - 1];

                            contextMgmt.AddAttributeFilterCondition(new AttributePair(attributeName, attributeValue));
                            responseContent = HandleSlotFilling(contextMgmt, kgMgmt, nlu.GetRelationTypeSet(), scenarioName);

                            contextMgmt.SaveQuestion(responseContent.responseMessage);
                            contextMgmt.RefreshDurationTime();
                        }
                        catch(Exception e) 
                        {
                            LogError(_log.Here(), e);
                            responseContent = ResolveInvalidOptionInput(contextMgmt);
                        }

                    }
                    else
                    {
                        responseContent = ResolveInvalidOptionInput(contextMgmt);
                    }

                    return responseContent;
                }
                else //if (context.GetStatus() == DialogStatus.INPROCESS)
                {
                    QueryResult responseContent ;

                    if (nlu.GetType() == NLUResultType.NUMBER)
                    {

                        var candidates = contextMgmt.GetCandidates();
                        try
                        {
                            Vertex vertex = candidates[nlu.GetOption()];
                            responseContent = GoForward(contextMgmt, kgMgmt, vertex, nlu.GetRelationTypeSet(), scenarioName);

                            contextMgmt.SaveQuestion(responseContent.responseMessage);
                            contextMgmt.RefreshDurationTime();
                        }
                        catch(Exception e)
                        {
                            LogError(_log.Here(), e);
                            responseContent = ResolveInvalidOptionInput(contextMgmt);
                        }
                    }
                    else
                    {
                        responseContent = ResolveInvalidOptionInput(contextMgmt);
                    }

                    return responseContent;
                }
            }
            catch(Exception e)
            {
                LogError(_log.Here(), e);
                return GenerateErrorMessage("无法识别意图。", contextMgmt);
            } 
        }        

        private QueryResult ResolveInvalidOptionInput(ContextManager contextMgmt)
        {
            if (contextMgmt.DureInvalidInput())
            {
                //Repeat the previous question
                QueryResult result = new QueryResult(true, "请选择正确的选项：\n" + contextMgmt.GetQuestion(), ResponseItemType.Option);
                List<OptionItem> items = new List<OptionItem>();

                if (contextMgmt.GetStatus() == DialogStatus.SLOTFILLING)
                {
                    DialogSlot currentSlot = contextMgmt.GetSlot(contextMgmt.GetCurrentSlotSeqNum());
                    foreach(OptionItem item in currentSlot.items)
                    {
                        items.Add(item);
                    }
                }
                else
                { 
                    foreach( int seqNo in contextMgmt.GetCandidates().Keys)
                    {
                        OptionItem item = new OptionItem();
                        item.seqNo = seqNo;
                        item.vertex = contextMgmt.GetCandidates()[seqNo];

                        items.Add(item);
                    }
                }                

                result.AddResponseItems(items);

                return result;
            }
            else
            {
                contextMgmt.ExitDialog();
                return GenerateErrorMessage("连续" + (contextMgmt.GetMaxDurationTime()) + "次输入无效选项，退出当前对话。", contextMgmt);
            }
        }        
        
        private QueryResult HandleSlotFilling(ContextManager contextMgmt, DataManager kgMgmt, HashSet<string> relationTypeSet, string scenarioName)
        {
            Dictionary<string, List<Vertex>> vertexDict = kgMgmt.FilterGraph(contextMgmt.GetStartVertexName(), contextMgmt.GetSecnarioName(), relationTypeSet, contextMgmt.GetSavedAttributes());

            if (vertexDict == null || vertexDict.Count() == 0)
            {
                return GenerateErrorMessage("找不到符合要求的保险产品", contextMgmt);
            }
            else
            {                
                if (contextMgmt.IsAllSlotsFilled())
                {
                    contextMgmt.StartDialog();

                    Vertex startVertex = kgMgmt.SearchGraph(contextMgmt.GetStartVertexName(), contextMgmt.GetSecnarioName(), contextMgmt.GetSavedAttributes());

                    return GetChildren(contextMgmt, kgMgmt, startVertex, relationTypeSet, scenarioName);
                }
                else
                {
                    contextMgmt.ForwardSlotFilling();                    
                    return GenerateSlotMessage(contextMgmt.GetSlot(contextMgmt.GetCurrentSlotSeqNum()), contextMgmt);
                }

            }
        }

        private QueryResult GoForward(ContextManager contextMgmt, DataManager kgMgmt, Vertex vertex, HashSet<string> relationTypeSet, string scenarioName)
        {
            if (vertex.isLeaf())
            {
                QueryResult respResult;
                string result = vertex.GetContent();

                if (string.IsNullOrWhiteSpace(result))
                {
                    respResult = GenerateErrorMessage("内容为空。", contextMgmt);
                }
                else
                {
                    respResult = GenerateEndVertexMessage(vertex, contextMgmt);
                }
                contextMgmt.ExitDialog();
                return respResult;
            }
            else
            {
                return GetChildren(contextMgmt, kgMgmt, vertex, relationTypeSet, scenarioName);
            }
        }

        private QueryResult GetChildren(ContextManager contextMgmt, DataManager kgMgmt, Vertex vertex, HashSet<string> relationTypeSet, string scenarioName)
        {
            Dictionary<string,List<Vertex>> childrenDict = kgMgmt.GetChildren(vertex, relationTypeSet, contextMgmt.GetSavedAttributes(), scenarioName);

            string headMessage = GetInformationOfVertex(vertex);

            if ((childrenDict == null || childrenDict.Count == 0) && (relationTypeSet != null && relationTypeSet.Count > 0))
            {
                childrenDict = kgMgmt.GetChildren(vertex, null, contextMgmt.GetSavedAttributes(), scenarioName);
            }

            if (childrenDict == null || childrenDict.Count == 0)
            {
                return GenerateErrorMessage(vertex.id + "没有子节点", contextMgmt);
            }

            QueryResult result = GenerateItemsMessage(headMessage, childrenDict, contextMgmt);            

            return result;
        }
    }    
}