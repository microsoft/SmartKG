
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace SmartKG.Common.Data.KG
{   

    public class RelationLink
    {
        public string relationType { get; set; }
        public string scenarioName { get; set; }

        public RelationLink(string relationType, string scenarioName)
        {
            if (string.IsNullOrWhiteSpace(relationType))
            {
                this.relationType = "ALL";
            }
            else
            {
                this.relationType = relationType;
            }

            if (string.IsNullOrWhiteSpace(scenarioName))
            {
                this.scenarioName = "ALL";
            }
            else
            {
                this.scenarioName = scenarioName;
            }
        }

        public bool Equals(RelationLink other)
        {
            if (this.relationType == other.relationType && this.scenarioName == other.scenarioName)
            {
                return true;
            }

            return false;
        }

        public bool IsCompatible(RelationLink other)
        {
            if (this.relationType != "ALL" && other.relationType != "ALL" && this.relationType != other.relationType)
            {
                return false;
            }

            if (this.scenarioName != "ALL" && other.scenarioName != "ALL" && this.scenarioName != other.scenarioName)
            {
                return false;
            }

            return true;
        }
    }
}