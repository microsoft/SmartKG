// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using SmartKG.Common.Data;
using Newtonsoft.Json;
using System;
using System.IO;

namespace SmartKG.Common.ContextStore
{
    public class ContextFileAccessor : IContextAccessor
    {
        private string filePath;
        int MAX_DURATION_INVALID_INPUT = 3;
        public ContextFileAccessor(string filePath)
        {
            this.filePath = filePath;
        }

        private string GetKey(string userId, string sessionId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                userId = "";
            }

            if (string.IsNullOrWhiteSpace(sessionId))
            {
                sessionId = "";
            }

            return userId + "_" + sessionId;
        }

        public DialogContext GetContext(string userId, string sessionId)
        {
            DialogContext context;

            if (File.Exists(filePath))
            {

                string content = File.ReadAllText(filePath);


                string[] lines = content.Split('\n');

                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    string[] tmps = line.Split("\t");

                    if (tmps.Length != 2)
                    {
                        throw new Exception("Context File is invalid.");
                    }

                    string key = tmps[0];
                    string contextJsonStr = tmps[1];

                    if (string.IsNullOrWhiteSpace(key))
                    {
                        throw new Exception("Context Key cannot be empty.");
                    }

                    if (key == GetKey(userId, sessionId))
                    {
                        return JsonConvert.DeserializeObject<DialogContext>(contextJsonStr);
                    }
                }

                context = new DialogContext(userId, sessionId, MAX_DURATION_INVALID_INPUT);

                string newLine = GetKey(userId, sessionId) + "\t" + JsonConvert.SerializeObject(context) + Environment.NewLine;

                File.AppendAllText(this.filePath, newLine);
                return context;
            }
            else
            {
                context = new DialogContext(userId, sessionId, MAX_DURATION_INVALID_INPUT);

                string newLine = GetKey(userId, sessionId) + "\t" + JsonConvert.SerializeObject(context) + Environment.NewLine;

                File.WriteAllText(this.filePath, newLine);
                return context;
            }
        }

        public void UpdateContext(string userId, string sessionId, DialogContext context)
        {
            if (!File.Exists(filePath))
            {
                throw new Exception("Context File doesn't exist. Cannot update context into it.");
            }
            
            string content = File.ReadAllText(filePath);

            string[] lines = content.Split('\n');

            string newContent = "";

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                string[] tmps = line.Split("\t");

                if (tmps.Length != 2)
                {
                        throw new Exception("Context File is invalid.");
                }

                string key = tmps[0];
                string contextJsonStr = tmps[1];

                if (string.IsNullOrWhiteSpace(key))
                {
                        throw new Exception("Context Key cannot be empty.");
                }

                if (key == GetKey(userId, sessionId))
                {
                    newContent += key + "\t" + JsonConvert.SerializeObject(context) + Environment.NewLine;
                }
                else
                {
                    newContent += line + Environment.NewLine;
                }
                
            }

            File.WriteAllText(this.filePath, newContent);
        }

        public void CleanContext()
        {            
            File.WriteAllText(this.filePath, ""); 
        }
    }
}
