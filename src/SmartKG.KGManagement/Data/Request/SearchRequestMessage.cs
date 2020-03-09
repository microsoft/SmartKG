using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartKG.KGManagement.Data.Request
{
    public class SearchRequestMessage
    {
        public string keyword { get; set; }
    }

    public class FilterRequestMessage
    {
        public string propertyName { get; set; }
        public string propertyValue { get; set; }
    }

    public class RelationRequestMessage
    {
        public string vertexId { get; set; }
    }
}
