// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using SmartKG.Common.Data;
using SmartKG.Common.Data.KG;
using SmartKG.Common.Data.LU;
using System;
using System.Collections.Generic;

namespace SmartKG.Common.Data
{
    public enum RUNNINGMODE
    {
        PRODUCTION, DEVELOPMENT
    }

    public enum DialogStatus
    {
        PENDING, SLOTFILLING, INPROCESS
    }

    [BsonIgnoreExtraElements]
    public class DialogContext
    {        
        public DialogStatus status { get; set; }
        public string intent { get; set; }
        public string scenarioName { get; set; }
        public List<Object> entities { get; set; }
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<int, Vertex> vertexCandidates { get; set; }
        public List<DialogSlot> slots { get; set; }
        public int currentSlotSeqNum { get; set; }
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<string, AttributePair> savedAttributes { get; set; }
        public string startVertexName { get; set; }
        public int currentDurationTime { get; set; }
        public string question { get; set; }
        public string sessionId { get; set; }
        public string userId { get; set; }

        public DialogContext(string userId, string sessionId, int maxDurationInvalidInput)
        {
            this.entities = new List<object>();
            this.vertexCandidates = null;
            this.status = DialogStatus.PENDING;
            this.startVertexName = null;
            this.currentDurationTime = maxDurationInvalidInput; 

            this.currentSlotSeqNum = 0;
            this.savedAttributes = new Dictionary<string, AttributePair>();
            this.question = null;

            this.userId = userId;
            this.sessionId = sessionId;
            this.scenarioName = null;
        }        
               
    }    
}