// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace SmartKG.Common.Data.LU
{
    public class NLUEntity
    {
        private string type { get; set; }
        private string value { get; set; }

        public NLUEntity(string value, string type)
        {
            this.type = type;
            this.value = value;
        }

        public string GetEntityValue()
        {
            return this.value;
        }

        public string GetEntityType()
        {
            return this.type;
        }

        public static string GenerateIdentity(string value, string type)
        {
            string identity = "";

            if (type != null)
            {
                identity += type + "_";
            }
            
            if (value != null)
            {
                identity += value;
            }

            if (identity == "")
            {
                return null;
            }
            else
            {
                return identity;
            }
        }

        public string GetIdentity()
        {
            string identity = "";

            if (this.type != null)
            {
                identity += this.type + "_";
            }

            if (this.value != null)
            {
                identity += this.value;
            }

            if (identity == "")
            {
                return null;
            }
            else
            {
                return identity;
            }
        }

        public NLUEntity Clone()
        {
            NLUEntity newOne = new NLUEntity( this.value, this.type);
            return newOne;
        }
    }
}