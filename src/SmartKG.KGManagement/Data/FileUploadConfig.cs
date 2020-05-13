using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartKG.KGManagement.Data
{
    public class FileUploadConfig
    {
        public string PythonEnvPath { get; set; }
        public string ConvertScriptPath { get; set; }
        public string JsonDir { get; set; }
        public string TargetDir { get; set; }
    }
}
