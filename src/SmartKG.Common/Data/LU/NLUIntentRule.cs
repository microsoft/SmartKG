// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;

namespace SmartKG.Common.Data.LU
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
