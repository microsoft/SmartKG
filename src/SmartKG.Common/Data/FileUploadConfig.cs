// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace SmartKG.Common.Data
{
    public class FileUploadConfig
    {
        public string PythonEnvPath { get; set; }
        public string ConvertScriptPath { get; set; }

        public string ColorConfigPath { get; set; }
        public string ExcelDir { get; set; }   
        
        public string LocalRootPath { get; set; }
    }
}
