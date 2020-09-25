// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Collections.Generic;

namespace SmartKG.Common.Data.KG
{
    public class Vertex
    {
        
        public string id { get; set; }
        public string name { get; set; }
        public string label { get; set; }
        public List<string> scenarios { get; set; }
        public string nodeType { get; set; }

        public string leadSentence { get; set; }

        public List<VertexProperty> properties { get; set; }

        public bool isRoot()
        {
            if (nodeType == "ROOT")
                return true;
            else
                return false;
        }

        public bool isLeaf()
        {
            if (nodeType == "LEAF")
                return true;
            else
                return false;
        }

        public string GetContent()
        {
            string result = this.name + "\n";
            
            foreach(VertexProperty p in this.properties)
            {
                result += p.name + ": " + p.value + "\n";
            }

            return result;
        }



        public string GetName()
        {
            return this.name;
        }

        public string GetPropertyValue(string name)
        {
            if (this.properties == null || this.properties.Count == 0)
                return null;

            foreach (VertexProperty p in this.properties)
            {
                string value = p.GetValue(name);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return null;
        }
    }
}
