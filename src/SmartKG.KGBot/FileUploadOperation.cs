using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartKG.KGBot
{
    public class FileUploadOperation : IOperationFilter
    {
        private ILogger log;

        public FileUploadOperation()
        {
            log = Log.Logger.ForContext<FileUploadOperation>();
        }

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.OperationId.ToLower() == "apivaluesuploadpost")
            {
                operation.Parameters.Clear();
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "uploadedFile",
                    //In = "formData",
                    Description = "Upload File",
                    Required = true,
                    //Type = "file"
                });
                //operation.Consumes.Add("multipart/form-data");
                
            }
        }
    }
}
