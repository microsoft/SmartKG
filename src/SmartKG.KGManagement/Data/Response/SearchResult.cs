// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using SmartKG.Common.Data.Visulization;
using System.Collections.Generic;

namespace SmartKG.KGManagement.Data.Response
{
    public class SearchResult : IResult
    {
        public bool success { get; set; }
        public string responseMessage { get; set; }
        public List<VisulizedVertex> nodes { get; set; }

        public SearchResult(bool success, string message)
        {
            this.success = success;
            this.responseMessage = message;
        }
    }

    public class RelationResult : IResult
    {
        public bool success { get; set; }
        public string responseMessage { get; set; }
        public List<VisulizedVertex> nodes { get; set; }
        public List<VisulizedEdge> relations { get; set; }


        public RelationResult(bool success, string message) 
        {
            this.success = success;
            this.responseMessage = message;
        }
    }

    public class ScenarioResult: IResult
    {
        public bool success { get; set; }
        public string responseMessage { get; set; }

        public List<string> scenarioNames { get; set; }
    }

    public class DatastoreResult : IResult
    {
        public bool success { get; set; }
        public string responseMessage { get; set; }

        public List<string> datastoreNames { get; set; }
    }

    public class CurrentDatastoreResult: IResult
    {
        public bool success { get; set; }
        public string responseMessage { get; set; }

        public string currentDatastoreName { get; set; }
    }

    public class ConfigResult: IResult
    {
        public bool success { get; set; }
        public string responseMessage { get; set; }

        public Dictionary<string, string> entityColorConfig { get; set; }
    }

    public class ColorResult: IResult
    {
        public bool success { get; set; }
        public string responseMessage { get; set; }

        public List<string> colors;
    }

    public class ResponseResult : IResult
    {
        public bool success { get; set; }
        public string responseMessage { get; set; }
    }
}
