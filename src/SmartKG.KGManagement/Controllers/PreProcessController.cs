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
using System.Threading.Tasks;
using SmartKG.DataUploader.Executor;
using System.ComponentModel.DataAnnotations;

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
        static PersistanceType persistanceType = (PersistanceType) Enum.Parse(typeof(PersistanceType), config.GetSection("PersistanceType").Value, true);

        // POST api/preprocess/upload
        [HttpPost]
        [Route("api/[controller]/upload")]
        [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ResponseResult>> Upload([FromForm] FileForm form)
        {
            string scenario = form.Scenario;
            string datastoreName = form.DatastoreName;
            var file = form.UploadFile;

            FileUploadConfig uploadConfig = config.GetSection("FileUploadConfig").Get<FileUploadConfig>();
            string excelDir = uploadConfig.ExcelDir;

            Directory.CreateDirectory(excelDir);            

            string savedFileName = null;            
            if (file != null)
            {
                    if (file.Length > 0)
                    {
                        string newFileName = GenerateTempFileName(file.FileName);

                        var filePath = excelDir + Path.DirectorySeparatorChar + newFileName;

                        using (var stream = System.IO.File.Create(filePath))
                        {
                            await file.CopyToAsync(stream);
                        }

                        savedFileName = newFileName;                
                    }
                
            }

            ConvertFiles(savedFileName, scenario, datastoreName);

            ResponseResult msg = new ResponseResult();
            msg.success = true;
            msg.responseMessage = "File: " + savedFileName + " has been received.\n" ;
           
            return Ok(msg);
        }

        private string GenerateTempFileName(string fileName)
        {            
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return null;
            }

            string[] fileNameSecs = fileName.Split(Path.DirectorySeparatorChar);

            fileName = fileNameSecs[fileNameSecs.Length - 1];

            string[] secs = fileName.Split(".");

            string suffix = secs[secs.Length - 1];

            string tmpStr = DateTime.Now.ToString("yyyyMMddHHmmssfff");

            string newSuffix = "_" + tmpStr + "." + suffix;

            string newFileName = fileName.Replace("." + suffix, newSuffix);

            return newFileName;
        }

        private void ConvertFiles(string savedFileName, string scenario, string datastoreName)
        {
            FileUploadConfig uploadConfig = config.GetSection("FileUploadConfig").Get<FileUploadConfig>();
            string excelDir = uploadConfig.ExcelDir;

            string pythonArgs = "--configPath \"" + uploadConfig.ColorConfigPath + "\" ";

            pythonArgs += " --srcPaths ";
                        
            pythonArgs += "\"" + excelDir + Path.DirectorySeparatorChar + savedFileName + "\" ";
            

            pythonArgs += " --scenarios ";

            
            pythonArgs += "\"" + scenario + "\" ";
            
            string targetDir = null;

            if (persistanceType == PersistanceType.File)
            {
                targetDir = uploadConfig.LocalRootPath + Path.DirectorySeparatorChar + datastoreName;
            }
            else
            {
                targetDir = excelDir + Path.DirectorySeparatorChar + DateTime.Now.ToString("MMddyyyyHHmmss");                
            }

            pythonArgs += " --destPath \"" + targetDir + "\" ";

            RunCommand(uploadConfig.PythonEnvPath, uploadConfig.ConvertScriptPath, pythonArgs);

            if (persistanceType == PersistanceType.MongoDB)
            {
                SmartKG.DataUploader.Executor.DataUploader uploader = new SmartKG.DataUploader.Executor.DataUploader();
                uploader.UploadDataFile(targetDir, datastoreName);
            }
            
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
        [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ResponseResult>> ReloadData([FromForm] ReloadForm form)
        {            
            string dsName = form.DatastoreName;

            DataLoader.GetInstance().Load(dsName);

            ContextAccessor.GetInstance().CleanContext(); // Clean all contexts and restart from clean env for a new datastore

            ResponseResult msg = new ResponseResult();
            msg.success = true;
            msg.responseMessage = "Data has been reloaded.\n";

            return Ok(msg);
        }
    }

    public class FileForm
    {
        [Required] public string DatastoreName { get; set; }
        [Required] public string Scenario { get; set; }
        [Required] public IFormFile UploadFile { get; set; }
    }

    public class ReloadForm
    {
        [Required] public string DatastoreName { get; set; }
    }
}
