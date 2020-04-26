// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;

namespace SmartKG.Common.Data.LU
{

    public enum NLUResultType
    {
        [Description("Not Completed")]
        NORMAL, NUMBER, QUITDIALOG, UNKNOWN
    }

    public class NLUResult
    {
        private NLUResultType type;
        private string intent = null;
        private List<NLUEntity> entities = null;
        private List<AttributePair> attributes = null;
        private HashSet<string> relationTypeSet = null;
        private int option = -1;

        public NLUResult(int option)
        {
            this.type = NLUResultType.NUMBER;
            this.option = option;
        }

        public NLUResult(NLUResultType type)
        {
            this.type = type;

            if (type == NLUResultType.QUITDIALOG)
            {
                this.option = 0;
            }
        }

        public NLUResult(string intent)
        {
            this.type = NLUResultType.NORMAL;
            this.intent = intent;
            this.entities = new List<NLUEntity>();
            this.attributes = new List<AttributePair>();
        }

        public NLUResult(string intent, List<NLUEntity> entities, List<AttributePair> attributes, HashSet<string> relationTypeSet)
        {
            this.type = NLUResultType.NORMAL;
            this.intent = intent;
            this.entities = entities;

            if (attributes != null)
            { 
                this.attributes = attributes;
            }

            if (relationTypeSet.Count > 0)
            {
                this.relationTypeSet = relationTypeSet;
            }
        }        

        public string GetIntent()
        {
            return this.intent;
        }

        public List<NLUEntity> GetEntities()
        {
            return this.entities;
        }

        public List<AttributePair> GetAttributes()
        {
            return this.attributes;
        }

        public HashSet<string> GetRelationTypeSet()
        {
            return relationTypeSet;
        }

        public int GetOption()
        {
            return this.option;
        }

        public new NLUResultType GetType()
        {
            return this.type;
        }

        override
        public string ToString()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("type", this.type.ToString());
            if (!string.IsNullOrWhiteSpace(this.intent))
            { 
                dict.Add("intent", this.intent);
            }
            
            if (entities != null && entities.Count > 0)
            {
                string entitiestr = "";
                foreach (NLUEntity e in entities)
                {
                    entitiestr += e.GetEntityType() + ":" + e.GetEntityValue();
                    entitiestr += ", ";
                }

                dict.Add("entities", entitiestr);
            }

            if (attributes != null && attributes.Count > 0)
            {
                string attrStr = "";

                foreach(AttributePair attr in attributes)
                {
                    attrStr += attr.attributeName + ": " + attr.attributeValue;
                    attrStr += ", ";
                }
                dict.Add("attributes", attrStr);
            }

            string result = JsonConvert.SerializeObject(dict);
            return result;
        }
    }

    
}