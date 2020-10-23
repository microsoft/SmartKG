// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using SmartKG.Common.Data;
using Microsoft.Extensions.Configuration;
using Serilog;
using SmartKG.Common.Logger;
using SmartKG.Common.Data.Configuration;

namespace SmartKG.Common.ContextStore
{
    public class ContextAccessor
    {
        private static ContextAccessor uniqueInstance;
        private IContextAccessor accessor;

        private ILogger log;

        private ContextAccessor(IConfiguration config)
        {
            log = Log.Logger.ForContext<ContextAccessor>();

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
                string dbName = config.GetConnectionString("ContextDatabaseName");

                this.accessor = new ContextDBAccessor(connectionString, dbName);

                this.accessor.CleanContext();
            }
        }

        public static ContextAccessor GetInstance()
        {
            return uniqueInstance;
        }


        public static ContextAccessor initInstance(IConfiguration config)
        {
            if (uniqueInstance == null)
            {
                uniqueInstance = new ContextAccessor(config);
            }

            return uniqueInstance;
        }

        public (bool, DialogContext) GetContext(string userId, string sessionId)
        {
            return this.accessor.GetContext(userId, sessionId);

        }

        public void UpdateContext(string userId, string sessionId, DialogContext context)
        {
            this.accessor.UpdateContext(userId, sessionId, context);
        }

        public void CleanContext()
        {
            this.accessor.CleanContext();
        }
    }
}
