// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using Microsoft.Extensions.Configuration;
using Serilog;
using SmartKG.Common.DataPersistence;
using System.Collections.Generic;

namespace SmartKG.Common.DataStoreMgmt
{
    public class DataStoreManager
    {
        private static DataStoreManager uniqueInstance;

        private Dictionary<string, DataStoreFrame> datastoreDict;

        private ILogger log;

        private DataLoader dataLoader;

        private DataStoreManager(IConfiguration config)
        {
            log = Log.Logger.ForContext<DataStoreManager>();
            this.datastoreDict = new Dictionary<string, DataStoreFrame>();

            this.dataLoader = new DataLoader(config);
        }

        public static DataStoreManager initInstance(IConfiguration config)
        {
            if (uniqueInstance == null)
            {
                uniqueInstance = new DataStoreManager(config);
            }
            return uniqueInstance;
        }

        public static DataStoreManager GetInstance()
        {
            return uniqueInstance;
        }

        public bool CreateDataStore(string user, string dsName)
        {
            return this.dataLoader.CreateDataStore(user, dsName);
        }

        public bool DeleteDataStore(string user, string dsName)
        {
            return this.dataLoader.DeleteDataStore(user, dsName);
        }

        public List<string> GetDataStoreList()
        {
            return this.dataLoader.GetDataStoreList();
        }

        public void LoadDataStores()
        {
            List<DataStoreFrame> datastores = this.dataLoader.Load();

            int count = 0;
            foreach(DataStoreFrame dsFrame in datastores)
            {
                if (this.SaveDataStoreInDict(dsFrame))
                    count += 1;
            }

            log.Information("Totally " + count + " datastores have been loaded.");
        }

        public void LoadDataStore(string dsName)
        {
            DataStoreFrame dsFrame = this.dataLoader.LoadDataStore(dsName);
            this.SaveDataStoreInDict(dsFrame);
        }

        private bool SaveDataStoreInDict(DataStoreFrame dsFrame)
        {
            if (dsFrame == null)
            {
                log.Error("Error: DataStoreFrame doesn't exist.");
                return false;
            }

            string dsName = dsFrame.GetName();

            if (this.datastoreDict.ContainsKey(dsName))
            {
                log.Error("Error: DataStore Name cannot be duplicated. The " + dsName + " has existed.");
                return false;
            }

            this.datastoreDict.Add(dsName, dsFrame);
            log.Information("The " + dsName + " has been loaded.");
            return true;
        }

        public DataStoreFrame GetDataStore(string dsName)
        {
            if (this.datastoreDict.ContainsKey(dsName))
            {
                return this.datastoreDict[dsName];
            }

            log.Error("The " + dsName + " doesn't existed.");
            return null;
        }
    }
}
