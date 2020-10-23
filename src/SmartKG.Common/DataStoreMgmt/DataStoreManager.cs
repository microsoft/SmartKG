// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using Microsoft.Extensions.Configuration;
using Serilog;
using SmartKG.Common.Data;
using SmartKG.Common.DataPersistence;
using System;
using System.Collections.Generic;
using System.IO;

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

        public string GetUploadedFileSaveDir()
        {
            FileUploadConfig uploadConfig = this.dataLoader.GetUploadConfig();
            string excelDir = uploadConfig.ExcelDir;

            return excelDir;
        }

        public PersistanceType GetPersistanceType()
        {
            return this.dataLoader.GetPersistanceType();
        }

        public (FileUploadConfig, string, string) GenerateConvertDirs(string datastoreName, string savedFileName, string scenario)
        {
            FileUploadConfig uploadConfig = this.dataLoader.GetUploadConfig();
            PersistanceType persistanceType = this.dataLoader.GetPersistanceType();

            string excelDir = uploadConfig.ExcelDir;

            string pythonArgs = "--configPath \"" + uploadConfig.ColorConfigPath + "\" ";

            pythonArgs += " --srcPaths ";

            pythonArgs += "\"" + excelDir + Path.DirectorySeparatorChar + savedFileName + "\" ";


            pythonArgs += " --scenarios ";


            pythonArgs += "\"" + scenario + "\" ";

            string targetDir = null;

            if (persistanceType == PersistanceType.File)
            {
                targetDir = uploadConfig.LocalRootPath + Path.DirectorySeparatorChar + datastoreName;
            }
            else
            {
                targetDir = excelDir + Path.DirectorySeparatorChar + DateTime.Now.ToString("MMddyyyyHHmmss");
            }

            pythonArgs += " --destPath \"" + targetDir + "\" ";

            return (uploadConfig, pythonArgs, targetDir);
        }
    }
}
