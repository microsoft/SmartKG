// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace SmartKG.Common.Data.LU
{
    public class AttributePair
    {
        public string attributeName { get; set; }
        public string attributeValue { get; set; }

        public AttributePair(string name, string value)
        {
            this.attributeName = name;
            this.attributeValue = value;
        }       
    }
}
