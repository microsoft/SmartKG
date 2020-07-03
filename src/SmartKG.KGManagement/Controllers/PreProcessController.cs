using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SmartKG.Common.ContextStore;
using SmartKG.Common.Data;
using SmartKG.Common.DataPersistence;
using SmartKG.KGManagement.Data;
using SmartKG.KGManagement.Data.Request;
using SmartKG.KGManagement.Data.Response;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SmartKG.KGManagement.Controllers
{

    [Produces("application/json")]
    [ApiController]
    public class PreProcessController: Controller
    {

        static IConfigurationBuilder builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) // requires Microsoft.Extensions.Configuration.Json
            .AddJsonFile("appsettings.json"); // requires Microsoft.Extensions.Configuration.Json                    
        static IConfiguration config = builder.Build();

        // POST api/preprocess/upload
        [HttpPost]
        [Route("api/[controller]/upload")]
        [ProducesResponseType(typeof(UploadResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UploadResult>> Upload(List<IFormFile> file, [FromForm]List<string> scenario, [FromForm] string datastoreName)
        {
            FileUploadConfig uploadConfig = config.GetSection("FileUploadConfig").Get<FileUploadConfig>();
            string excelDir = uploadConfig.ExcelDir;
      
            //var requestForm = HttpContext.Request.Form;
            int count = 0;

            List<string> savedFilePaths = new List<string>();

            if (file != null && file.Count > 0)
            {
                foreach(var formFile in file)
                {
                    if (formFile.Length > 0)
                    {
                        var filePath = excelDir + Path.DirectorySeparatorChar + formFile.FileName;//Path.GetTempFileName();

                        using (var stream = System.IO.File.Create(filePath))
                        {
                            await formFile.CopyToAsync(stream);
                        }

                        savedFilePaths.Add(formFile.FileName);

                        count += 1;
                    }
                }
            }

            ConvertFiles(savedFilePaths, scenario, datastoreName);

            UploadResult msg = new UploadResult();
            msg.success = true;
            msg.responseMessage = count + " file(s) have been received.\n" + string.Join(",", savedFilePaths.ToArray());
           
            return Ok(msg);
        }

        private void ConvertFiles(List<string> savedFileNames, List<string> scenarios, string datastoreName)
        {
            FileUploadConfig uploadConfig = config.GetSection("FileUploadConfig").Get<FileUploadConfig>();
            string excelDir = uploadConfig.ExcelDir;
            string targetDir = uploadConfig.LocalRootPath + Path.DirectorySeparatorChar + datastoreName;
           
            if (!Directory.Exists(targetDir))
                Directory.CreateDirectory(targetDir);

            string pythonArgs = "--srcPaths ";

            foreach (var srcFileName in savedFileNames)
            {
                pythonArgs += "\"" + excelDir + Path.DirectorySeparatorChar + srcFileName + "\" ";
            }

            pythonArgs += " --scenarios ";

            foreach (string scenario in scenarios)
            {
                pythonArgs += "\"" + scenario + "\" ";
            }

            pythonArgs += " --destPath \"" + targetDir + "\" ";

            RunCommand(uploadConfig.PythonEnvPath, uploadConfig.ConvertScriptPath, pythonArgs);

            return;
        }    

        private void RunCommand(string envPath, string cmd, string args)
        {
           
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = envPath;

            start.Arguments = string.Format("{0} {1}", cmd, args);
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd();
                    Console.Write(result);
                }
            }
        }

        // POST api/reload
        [HttpPost]
        [Route("api/[controller]/reload")]
        [ProducesResponseType(typeof(UploadResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UploadResult>> ReloadData([FromBody] ReloadRequestMessage request)
        {
            PersistanceType type = request.persistenceType;
            string location = request.datastoreName;

            DataLoader.GetInstance().Load(location);

            ContextAccessor.GetInstance().CleanContext(); // Clean all contexts and restart from clean env for a new datastore

            ReloadResult msg = new ReloadResult();
            msg.success = true;
            msg.responseMessage = "Data has been reloaded.\n";

            return Ok(msg);
        }
    }
}
