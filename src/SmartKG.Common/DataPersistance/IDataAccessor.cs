using SmartKG.Common.Data.KG;
using SmartKG.Common.Data.LU;
using SmartKG.Common.Data.Visulization;
using System.Collections.Generic;


namespace SmartKG.Common.DataPersistence
{
    public interface IDataAccessor
    {
        (List<Vertex>, List<Edge>) LoadKG(string location);
        List<VisulizationConfig> LoadConfig(string location);
        (List<NLUIntentRule>, List<EntityData>, List<EntityAttributeData>) LoadNLU(string location);

        (List<Vertex>, List<Edge>, List<VisulizationConfig>, List<NLUIntentRule>, List<EntityData>, List<EntityAttributeData>) Load(string location);
    }
}
