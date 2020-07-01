// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using SmartKG.Common.Data.KG;

namespace SmartKG.Common.Data
{
    public class OptionItem
    {
        public int seqNo { get; set; }
        public Vertex vertex { get; set; }
        public string relationType { get; set; }
    }
}
