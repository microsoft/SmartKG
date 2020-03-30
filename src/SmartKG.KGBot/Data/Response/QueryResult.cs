// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using SmartKG.KGManagement.Data.Response;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SmartKG.KGBot.Data.Response
{    
    public enum ResponseItemType
    {
        Option, Other
    }
    public class QueryResult: IResult
    {
        public bool success { get; set; }
        public string responseMessage { get; set; }

        public ResponseItemType itemType { get; set; }

        public IList items { get; set; }

        public QueryResult(bool success, string msg, ResponseItemType itemType)
        {
            this.success = success;
            this.responseMessage = msg;
            this.itemType = itemType;
            this.items = null;
        }        

        public void AddResponseItems(IList items)
        {
            this.items = items;
        }        
    }
}