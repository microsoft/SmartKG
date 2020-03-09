using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartKG.Common.Data.KG
{
    [BsonIgnoreExtraElements]
    public class Edge
    {
        public List<string> scenarios { get; set; }
        public string relationType { get; set; }
        public string headVertexId { get; set; }
        public string tailVertexId { get; set; }
    }
}
