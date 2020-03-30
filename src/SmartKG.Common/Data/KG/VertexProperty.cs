// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace SmartKG.Common.Data.KG
{
    public class VertexProperty
    {
        public string name { get; set; }
        public string value { get; set; }


        public string GetValue(string name)
        {
            if (!string.IsNullOrWhiteSpace(this.name) && !string.IsNullOrWhiteSpace(name)  && this.name == name)
            {
                return this.value;
            }

            return null;
        }
    }
}
