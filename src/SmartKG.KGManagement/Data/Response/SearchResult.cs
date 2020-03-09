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
}
