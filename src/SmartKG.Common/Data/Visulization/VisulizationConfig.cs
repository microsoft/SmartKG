using System;
using System.Collections.Generic;
using System.Text;

namespace SmartKG.Common.Data.Visulization
{
    public class VisulizationConfig
    {
        public string scenario { get; set; }
        public List<ColorConfig> labelsOfVertexes { get; set; }
        public List<ColorConfig> relationTypesOfEdges { get; set; }
    }

    public class ColorConfig
    {
        public string itemLabel { get; set; }
        public string color { get; set; }
    }
}
