using SmartKG.Common.Data.LU;
using SmartKG.KGBot.DataStore;

using System.Collections.Generic;

namespace SmartKG.KGBot.NaturalLanguageUnderstanding
{
    public class NLUProcessor
    {
        private NLUStore nluStore = NLUStore.GetInstance();
        public NLUProcessor()
        {

        }

        public NLUResult Parse(string query)
        {
            
            if (int.TryParse(query, out int num))
            {
                NLUResult result = new NLUResult(num);
                return result;
            }
            else
            {
                string intentName = nluStore.DetectIntent(query);

                if (intentName == null)
                {
                    NLUResult result = new NLUResult();
                    return result;
                }
                else
                { 
                    NLUResult result = ParseIntentEntity(intentName, query);
                    return result;
                }
            }
            
        }

        private NLUResult ParseIntentEntity(string intentName, string query)
        {           
            List<NLUEntity> entities = nluStore.DetectEntities(intentName, query);

            List<AttributePair> attributes = nluStore.ParseAttributes(intentName, entities);

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