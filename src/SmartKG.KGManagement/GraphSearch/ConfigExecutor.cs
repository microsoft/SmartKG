using Serilog;
using SmartKG.Common.Data.Visulization;
using SmartKG.Common.DataStoreMgmt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartKG.KGManagement.GraphSearch
{
    public class ConfigExecutor
    {
        private ILogger log;
        private DataStoreManager dsMgmt = DataStoreManager.GetInstance();
        private KGConfigFrame kgConfigFrame = null;

        private string user;
        private string dsName;

        public ConfigExecutor(string user, string dsName)
        {
            log = Log.Logger.ForContext<ConfigExecutor>();
            DataStoreFrame dsFrame = dsMgmt.GetDataStore(dsName);
            this.kgConfigFrame = dsFrame.GetKGConfig();

            this.user = user;
            this.dsName = dsName;
        }

        public (bool, string) UpdateColorConfigs(string scenarioName, List<ColorConfig> colorConfigs)
        {
            if (this.kgConfigFrame == null)
                return (false, "KG Color Config doesn't exist.");

            (bool success, string msg) = this.kgConfigFrame.UpdateVertexLabelColorMapForScenario(scenarioName, colorConfigs);

            if (!success)
            {
                return (success, msg);
            }

            
            success = dsMgmt.UpdateColorConfig(this.user, this.dsName, scenarioName, colorConfigs);

            if (!success)
            {
                msg = "Failed to update color config in persistent data store.";
            }
            else
            {
                msg = "Update color config successfully.";
            }

            return (success, msg);
        }

        public (bool, bool, List<ColorConfig>) GetColorConfigs(string scenarioName)
        {

            if (this.kgConfigFrame == null)
                return (false, false, null);


            (bool isScenarioExist, List<ColorConfig> colorConfigs) = this.kgConfigFrame.GetColorConfigForScenario(scenarioName);

            return (true, isScenarioExist, colorConfigs);
            
        }

    }
}
