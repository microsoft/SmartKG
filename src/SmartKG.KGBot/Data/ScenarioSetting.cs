using SmartKG.Common.Data.KG;
using System.Collections.Generic;


namespace SmartKG.KGBot.Data
{
    public enum VertexSortOrderType
    {
        ASC=0, DESC=1
    }

    public class SortSetting
    {        
        public string attributeName { get; set; }
        public VertexSortOrderType orderType { get; set; }
    }

    public class ScenarioSetting
    {
        public string scenarioName { get; set; }        
        public SortSetting sortSetting { get; set; }
        public int maxOptions { get; set; }

        public List<DialogSlot> slots { get; set; }

        public Vertex root { get; set; }

        public ScenarioSetting(string scenarioName, Vertex root)
        {
            this.scenarioName = scenarioName;
            this.root = root;

            this.sortSetting = null;
            this.maxOptions = -1;
            this.slots = null;
        }
    }
}
