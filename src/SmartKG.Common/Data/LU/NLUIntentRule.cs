using System;
using System.Collections.Generic;

namespace CommonSmartKG.Common.Data.LU
{
    public enum IntentRuleType
    {
        NEGATIVE, POSITIVE
    }
    public class NLUIntentRule
    {
        public string id { get; set; }
        public string intentName { get; set; }
        public IntentRuleType type { get; set; }
        public List<String> ruleSecs { get; set; }
    }
}
