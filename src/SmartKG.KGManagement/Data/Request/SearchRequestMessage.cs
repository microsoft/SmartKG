// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace SmartKG.KGManagement.Data.Request
{
    public class SearchRequestMessage
    {
        public string keyword { get; set; }
    }

    public class FilterRequestMessage
    {
        public string propertyName { get; set; }
        public string propertyValue { get; set; }
    }

    public class RelationRequestMessage
    {
        public string vertexId { get; set; }
    }

    public class DatastoreRequestMessage
    {
        public string datastoreName { get; set; }
    }
}
