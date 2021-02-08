// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace SmartKG.Common.Data.Visulization
{
    [BsonIgnoreExtraElements]
    public class VisulizationConfig
    {        
        public string scenario { get; set; }
        public List<ColorConfig> labelsOfVertexes { get; set; }
        public List<ColorConfig> relationTypesOfEdges { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class ColorConfig
    {
        public string itemLabel { get; set; }
        public string color { get; set; }
    }
}
