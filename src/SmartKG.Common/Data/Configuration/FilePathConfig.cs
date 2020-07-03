// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartKG.Common.Data.Configuration
{
    public class FilePathConfig
    {
        public string RootPath { get; set; }
        public string DefaultDataStore { get; set; }
        public string ContextFilePath { get; set; }
    }
}
