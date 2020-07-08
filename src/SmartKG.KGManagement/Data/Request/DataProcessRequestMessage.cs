using SmartKG.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartKG.KGManagement.Data.Request
{
    public class DataProcessRequestMessage
    {
        public List<string> srcFileNames { get; set; }
        public List<string> scenarios { get; set; }

        public string datastoerName { get; set; }
    }

    public class ReloadRequestMessage
    {
        public string datastoreName { get; set; }

    }
}
