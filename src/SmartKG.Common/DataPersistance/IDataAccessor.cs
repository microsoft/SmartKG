using SmartKG.Common.Data.KG;
using SmartKG.Common.Data.LU;
using SmartKG.Common.Data.Visulization;
using System.Collections.Generic;


namespace SmartKG.Common.DataPersistence
{
    public interface IDataAccessor
    {
        (List<Vertex>, List<Edge>) LoadKG(string datastoreName);
        List<VisulizationConfig> LoadConfig(string datastoreName);
        (List<NLUIntentRule>, List<EntityData>, List<EntityAttributeData>) LoadNLU(string datastoreName);

        (List<Vertex>, List<Edge>, List<VisulizationConfig>, List<NLUIntentRule>, List<EntityData>, List<EntityAttributeData>) Load(string location);

        List<string> GetDataStoreList();

        bool AddDataStore(string user, string dsName);

        bool DeleteDataStore(string user, string dsName);

    }
}
