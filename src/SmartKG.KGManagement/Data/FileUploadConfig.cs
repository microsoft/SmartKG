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
        public string ExcelDir { get; set; }   
        
        public string LocalRootPath { get; set; }
    }
}
