// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartKG.KGBot.Data.Response;
using SmartKG.KGBot.NaturalLanguageUnderstanding;
using Serilog;
using SmartKG.Common.Logger;
using SmartKG.Common.Data.LU;
using SmartKG.Common.Data.KG;
using SmartKG.Common.Data;
using SmartKG.Common.ContextStore;

namespace SmartKG.KGBot.Managment
{

    public class DialogManager : KGBotLogHandler
    {
        private string userId;
        private string sessionId;
        private string query;

        private MessageGenerator msgGenerator;
        private DataQuerier dQuerier;

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
            this.dQuerier = new DataQuerier(runningMode);
            this.msgGenerator = new MessageGenerator(runningMode);


            LogInformation(_log.Here(), "runningMode", runningMode.ToString());

            ContextManager contextMgmt = new ContextManager(userId, sessionId);
            contextMgmt.GetContext();
            
            NLUResult nlu = new NLUProcessor().Parse(query);

            LogInformation(_log.Here(), "nlu", nlu.ToString());

            NLUResultType type = nlu.GetType();            

            QueryResult result = null;

            if (type == NLUResultType.UNKNOWN && contextMgmt.GetIntent() == null)
            {
                result = this.msgGenerator.GenerateErrorMessage("无法识别意图。");
                contextMgmt.ExitDialog();
            }
            else if (type == NLUResultType.QUITDIALOG)
            {
                result = this.msgGenerator.GenerateQuitMessage();
                contextMgmt.ExitDialog();
            }
            else
            {              
                result = ResponseDialog(nlu, contextMgmt);
            }

            contextMgmt.UpdateContext();

            return result;
        }

                
        private QueryResult ResponseDialog(NLUResult nlu, ContextManager contextMgmt)
        {            
            string intent = nlu.GetIntent();            

            if (string.IsNullOrWhiteSpace(intent))
            {
                intent = contextMgmt.GetIntent();                
            }
            else
            {
                contextMgmt.SetIntent(intent);
                contextMgmt.SetScenarioName(intent);
            }

            try
            {                               
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
                        QueryResult responseContent = dQuerier.SearchVertexes(contextMgmt, nlu.GetEntities());

                        if (responseContent.success)
                        {
                            contextMgmt.SetIntent(intent);                            
                            contextMgmt.SaveQuestion(responseContent.responseMessage);
                            contextMgmt.RefreshDurationTime();
                        }
                        

                        return responseContent;
                    }
                    else
                    {
                        QueryResult responseContent =  this.msgGenerator.GenerateErrorMessage("无法识别意图。");
                        contextMgmt.ExitDialog();
                        return responseContent;
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
                            responseContent = dQuerier.HandleSlotFilling(contextMgmt, nlu.GetRelationTypeSet());

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
                            
                            if (vertex.isLeaf())
                            {
                                responseContent = msgGenerator.GenerateEndVertexMessage(vertex);
                                contextMgmt.ExitDialog();
                                
                            }
                            else
                            {
                                responseContent = dQuerier.GetChildren(contextMgmt, vertex, nlu.GetRelationTypeSet());
                            }

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
                QueryResult responseContent =  this.msgGenerator.GenerateErrorMessage("无法识别意图。");
                contextMgmt.ExitDialog();

                return responseContent;
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
                    foreach (OptionItem item in currentSlot.items)
                    {
                        items.Add(item);
                    }
                }
                else
                {
                    foreach (int seqNo in contextMgmt.GetCandidates().Keys)
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
                QueryResult responseContent = this.msgGenerator.GenerateErrorMessage("连续" + (contextMgmt.GetMaxDurationTime()) + "次输入无效选项，退出当前对话。");
                contextMgmt.ExitDialog();
                return responseContent;
            }
        }
    }    
}