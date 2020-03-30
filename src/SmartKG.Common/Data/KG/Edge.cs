// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

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
