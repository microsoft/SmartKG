// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using SmartKG.Common.Data;
using SmartKG.Common.Data.KG;
using SmartKG.KGBot.Data.Response;

namespace SmartKG.KGBot.Managment
{
    public class MessageGenerator
    {        
        private RUNNINGMODE runningMode;
        public MessageGenerator(RUNNINGMODE runningMode)
        {
            this.runningMode = runningMode;
        }

        public QueryResult GenerateErrorMessage(string message)
        {
            QueryResult result = new QueryResult(false, message, ResponseItemType.Other);           
            return result;
        }

        public QueryResult GenerateQuitMessage()
        {
            QueryResult result = new QueryResult(true, "已退出上轮对话，请提出您的问题。\n", ResponseItemType.Other);            

            return result;
        }

        public QueryResult GenerateSlotMessage(DialogSlot slot)
        {
            string question = slot.question;

            if (this.runningMode == RUNNINGMODE.DEVELOPMENT)
            {
                foreach (OptionItem item in slot.items)
                {
                    question += item.seqNo.ToString() + ". " + item.vertex.name + "\n";
                }
            }

            QueryResult result = new QueryResult(true, question, ResponseItemType.Option);
            result.AddResponseItems(slot.items);

            return result;
        }

        public string GetInformationOfVertex(Vertex vertex)
        {
            string resultStr = "";
            resultStr += vertex.name + "\n";

            if (vertex.properties != null && vertex.properties.Count > 0)
            {
                foreach (VertexProperty p in vertex.properties)
                {
                    resultStr += p.name + ":" + p.value + "\n";
                }
            }

            resultStr += vertex.leadSentence + "\n";
            return resultStr;
        }

        public QueryResult GenerateEndVertexMessage(Vertex vertex)
        {
            if (vertex == null)
            {
                return GenerateErrorMessage("要返回的项目为空。");
            }

            string content = vertex.GetContent();

            if (string.IsNullOrWhiteSpace(content))
            {
                return GenerateErrorMessage("节点内容为空。");
            }

            string resultStr = GetInformationOfVertex(vertex);

            QueryResult result = new QueryResult(true, resultStr, ResponseItemType.Other);
            
            return result;
        }        
    }
}
