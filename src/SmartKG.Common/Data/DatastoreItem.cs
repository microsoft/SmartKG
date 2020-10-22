using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartKG.Common.Data
{
    [BsonIgnoreExtraElements]
    public class DatastoreItem
    {
        public string name { get; set; }
        public string creator { get; set; }        
    }
}
