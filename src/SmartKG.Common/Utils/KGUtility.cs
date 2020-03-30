// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using SmartKG.Common.Data.Visulization;

namespace SmartKG.Common.Utils
{
    public class KGUtility
    {
        private static string GenerateIdForProperty(string name, string value)
        {
            string propertyId = name + ":" + value;
            return propertyId;
        }

        public static VisulizedVertex GeneratePropertyVVertex(string vertexLabel, string pName, string pValue)
        {
            if (string.IsNullOrWhiteSpace(vertexLabel))
                vertexLabel = "";
            else
                vertexLabel = "_" + vertexLabel;

            VisulizedVertex propertyVV = new VisulizedVertex();
            propertyVV.name = pValue;
            propertyVV.displayName = pName;
            propertyVV.label = vertexLabel + "属性";
            propertyVV.id = GenerateIdForProperty(pName, pValue);

            return propertyVV;
        }
    }
}
