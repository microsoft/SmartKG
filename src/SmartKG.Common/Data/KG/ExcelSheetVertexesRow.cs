using System;
using System.Collections.Generic;
using System.Text;

namespace SmartKG.Common.Data.KG
{
    public class ExcelSheetVertexesRow
    {
        public string entityId { get; set; }
        public string entityName { get; set; }

        public string entityType { get; set; }
        
        public string leadingSentence { get; set; }
        public List<VertexProperty> properties { get; set; }
    }

    public class ExcelSheetEdgesRow
    {
        public string relationType { get; set; }
        public string sourceEntityId { get; set; }
        public string targetEntityId { get; set; }
    }
}
