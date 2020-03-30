// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using SmartKG.KGBot.Data;
using Microsoft.Extensions.Configuration;
using Serilog;
using SmartKG.Common.Logger;
using SmartKG.Common.Data.Configuration;

namespace SmartKG.KGBot.StorageAccessor
{
    public class ContextAccessController
    {
        private static ContextAccessController uniqueInstance;
        private IContextAccessor accessor;

        private ILogger log;

        private ContextAccessController(IConfiguration config)
        {
            log = Log.Logger.ForContext<ContextAccessController>();

            string persistanceType = config.GetSection("PersistanceType").Value;

            log.Here().Information("PersistanceType is " + persistanceType);

            if (persistanceType == "File")
            {
                FilePathConfig filePaths = config.GetSection("FileDataPath").Get<FilePathConfig>();
                string contextPath = filePaths.ContextFilePath;

                this.accessor = new ContextFileAccessor(contextPath);

                this.accessor.CleanContext();
            }
            else
            {
                string connectionString = config.GetConnectionString("MongoDbConnection");
                string dbName = config.GetConnectionString("DatabaseName");

                this.accessor = new ContextDBAccessor(connectionString, dbName);

                this.accessor.CleanContext();
            }
        }

        public static ContextAccessController GetInstance()
        {
            return uniqueInstance;
        }


        public static ContextAccessController initInstance(IConfiguration config)
        {
            if (uniqueInstance == null)
            {
                uniqueInstance = new ContextAccessController(config);
            }

            return uniqueInstance;
        }

        public DialogContext GetContext(string userId, string sessionId)
        {
            return this.accessor.GetContext(userId, sessionId);

        }

        public void UpdateContext(string userId, string sessionId, DialogContext context)
        {
            this.accessor.UpdateContext(userId, sessionId, context);
        }
    }
}
