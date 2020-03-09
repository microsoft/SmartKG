using SmartKG.KGBot.Data;
using MongoDB.Driver;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SmartKG.Common.Data.KG;
using SmartKG.Common.Logger;
using SmartKG.Common.Data.LU;

namespace SmartKG.KGBot.StorageAccessor
{
    public class ContextManager : KGBotLogHandler
    {
        public const int MAX_DURATION_INVALID_INPUT = 3;

        private DialogContext context;

        private string userId;
        private string sessionId;

        private ContextAccessController accController = ContextAccessController.GetInstance(); 

        private ILogger log;
        public ContextManager(string userId, string sessionId)
        {           
            this.userId = userId;
            this.sessionId = sessionId;

            log = Log.Logger.ForContext<ContextManager>();
        }

        public void LogInformation(ILogger log, string title, string content)
        {            
            log.Information("userId: " + this.userId + ", sessionId:" + this.sessionId + "\n" + title + " " + content);
        }

        public void LogError(ILogger log, Exception e)
        {
            log.Error("userId: " + this.userId + ", sessionId:" + this.sessionId + "\n" + e.Message, e);
        }

        public void GetContext()
        {
            this.context = accController.GetContext(userId, sessionId);
        }

        public void UpdateContext()
        {           
            accController.UpdateContext(this.userId, this.sessionId, this.context);

            return;
        }
       
        public string GetStartVertexName()
        {
            return this.context.startVertexName;
        }

        public void SetStartVertexName(string name)
        {
            this.context.startVertexName = name;
        }

        public int GetMaxDurationTime()
        {
            return MAX_DURATION_INVALID_INPUT;
        }

        public int GetCurrentSlotSeqNum()
        {
            return this.context.currentSlotSeqNum;
        }

        public bool DureInvalidInput()
        {
            this.context.currentDurationTime -= 1;
            if (this.context.currentDurationTime >= 0)
                return true;
            else
                return false;
        }

        public void RefreshDurationTime()
        {
            this.context.currentDurationTime = MAX_DURATION_INVALID_INPUT;
        }

        public void SaveQuestion(string question)
        {
            LogInformation(log.Here(), "saved question:", question);
            this.context.question = question;
        }

        public string GetQuestion()
        {
            return this.context.question;
        }
        public bool IsAllSlotsFilled()
        {
            if (this.context.slots == null)
                return true;

            if (this.context.currentSlotSeqNum < this.context.slots.Count())
                return false;
            else
                return true;
        }

        public void AddAttributeFilterCondition(AttributePair attribute)
        {
            LogInformation(log.Here(), "get attribute:", JsonConvert.SerializeObject(attribute));

            string key = attribute.attributeName;

            if (this.context.savedAttributes.Keys.Contains(key))
            {
                this.context.savedAttributes[key] = attribute;
            }
            else
            {
                this.context.savedAttributes.Add(key, attribute);
            }
        }

        public List<AttributePair> GetSavedAttributes()
        {
            return this.context.savedAttributes.Values.ToList();
        }

        public void SetSlots(List<DialogSlot> slots)
        {
            this.context.slots = slots;
        }

        public DialogSlot GetSlot(int stepNum)
        {
            if (stepNum > this.context.slots.Count())
            {
                return null;
            }
            else
            {
                return this.context.slots[stepNum - 1];
            }
        }

        public void SetIntent(string intent)
        {
            this.context.intent = intent;
        }

        public void AddEntity(Object entity)
        {
            this.context.entities.Add(entity);
        }

        public void SetCandidates(Dictionary<int, Vertex> candidates)
        {
            LogInformation(log.Here(), "set candidates:", JsonConvert.SerializeObject(candidates));
            this.context.vertexCandidates = candidates;
        }

        public string GetIntent()
        {
            return this.context.intent;
        }

        public void SetScenarioName(string scenarioName)
        {
            this.context.scenarioName = scenarioName;
        }

        public string GetSecnarioName()
        {
            return this.context.scenarioName;
        }

        public List<Object> GetEntities()
        {
            return this.context.entities;
        }

        public Dictionary<int, Vertex> GetCandidates()
        {
            return this.context.vertexCandidates;
        }

        public void StartDialog()
        {
            LogInformation(log.Here(), "start a dialog", "");
            this.context.status = DialogStatus.INPROCESS;
        }

        public void EnterSlotFilling()
        {
            LogInformation(log.Here(), "begine to fill slots", "");
            this.context.status = DialogStatus.SLOTFILLING;
            this.context.currentSlotSeqNum = 1;
        }

        public void ForwardSlotFilling()
        {
            LogInformation(log.Here(), "slot [" + this.context.currentSlotSeqNum.ToString() + "]", "is processed");
            this.context.currentSlotSeqNum += 1;
        }

        public DialogStatus GetStatus()
        {
            return this.context.status;
        }

        public void ExitDialog()
        {
            LogInformation(log.Here(), "exit dialog", "");
            this.context.vertexCandidates = null;
            this.context.status = DialogStatus.PENDING;
            this.context.slots = null;
            this.context.currentSlotSeqNum = 0;
            this.context.savedAttributes = new Dictionary<string, AttributePair>();

            this.context.currentDurationTime = MAX_DURATION_INVALID_INPUT;
            this.context.question = null;
            this.context.intent = null;
            this.context.startVertexName = null;
            this.context.scenarioName = null;
        }

        
    }
}
