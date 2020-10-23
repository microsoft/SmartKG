// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using SmartKG.Common.Data.LU;
using SmartKG.Common.DataStoreMgmt;
using System.Collections.Generic;

namespace SmartKG.KGBot.NaturalLanguageUnderstanding
{
    public class NLUProcessor
    {
        private NLUDataFrame nluDF = null;
        public NLUProcessor(string datastoreName)
        {
            DataStoreFrame dsFrame = DataStoreManager.GetInstance().GetDataStore(datastoreName);

            if (dsFrame != null)
                this.nluDF = DataStoreManager.GetInstance().GetDataStore(datastoreName).GetNLU();
        }

        public NLUResult Parse(string query)
        {
            if (nluDF == null)
            {
                NLUResult result = new NLUResult(NLUResultType.NOTEXIST);
                return result;
            }

            if (int.TryParse(query, out int num))
            {
                NLUResult result = new NLUResult(num);
                return result;
            }
            else
            {
                if (query.Trim().ToLower() == "q")
                {
                    NLUResult result = new NLUResult(NLUResultType.QUITDIALOG);
                    return result;

                }
                else
                { 
                    string intentName = nluDF.DetectIntent(query);

                    if (intentName == null)
                    {
                        NLUResult result = new NLUResult(NLUResultType.UNKNOWN);
                        return result;
                    }
                    else
                    { 
                        NLUResult result = ParseIntentEntity(intentName, query);
                        return result;
                    }
                }
            }
            
        }

        private NLUResult ParseIntentEntity(string intentName, string query)
        {           
            List<NLUEntity> entities = nluDF.DetectEntities(intentName, query);

            List<AttributePair> attributes = nluDF.ParseAttributes(intentName, entities);

            HashSet<string> relationTypeSet = new HashSet<string>();

            if (entities != null && entities.Count > 0)
            {
                foreach(NLUEntity entity in entities)
                {
                    if (entity.GetEntityType() == "RelationType")
                    {
                        relationTypeSet.Add(entity.GetEntityValue());
                    }
                }
            }

            NLUResult result = new NLUResult(intentName, entities, attributes, relationTypeSet);

            return result;
        }        
    }
}