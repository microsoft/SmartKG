// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Collections.Generic;

namespace SmartKG.Common.Data
{
    public class DialogSlot
    {
        public string id { get; set; }
        public string kgKey { get; set; }
        public string question { get; set; }

        public List<OptionItem> items { get; set; }

        public List<string> answerValues { get; set; }
        public string correspondingAttribute { get; set; }
    }      
}