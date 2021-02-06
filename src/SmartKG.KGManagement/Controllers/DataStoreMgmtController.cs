// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using SmartKG.Common.ContextStore;
using SmartKG.Common.Data;
using SmartKG.Common.Data.Configuration;
using SmartKG.Common.Data.KG;
using SmartKG.Common.DataStoreMgmt;
using SmartKG.Common.Parser;
using SmartKG.KGManagement.Data.Request;
using SmartKG.KGManagement.Data.Response;

namespace SmartKG.KGManagement.Controllers
{
    
    [ApiController]
    public class DataStoreMgmtController : ControllerBase
    {
        private ILogger log;
        DataStoreManager dsManager = DataStoreManager.GetInstance();
        
        public DataStoreMgmtController()
        {
            log = Log.Logger.ForContext<DataStoreMgmtController>();
        }

        // Get api/datastoremgmt
        [HttpGet]
        [Route("api/[controller]")]
        [ProducesResponseType(typeof(DatastoreResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DatastoreResult>> Get()
        {
            
            DatastoreResult msg = new DatastoreResult();
            msg.success = true;

            List<string> dsNames = dsManager.GetDataStoreList();

            if (dsNames.Count == 0)
            { 
                msg.responseMessage = "There is no datastore so far.\n";
            }
            else
            {
                msg.responseMessage = "There are " + dsNames.Count + " datastore(s).\n";
            }

            msg.datastoreNames = dsNames;

            return Ok(msg);
            
        }

        // POST api/datastoremgmt
        [HttpPost]
        [Route("api/[controller]")]
        [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ResponseResult>> Post([FromBody] DatastoreRequestMessage request)
        {
            string user = request.user;
            string datastoreName = request.datastoreName;            

            if (!dsManager.CreateDataStore(user, datastoreName))
            {
                ResponseResult msg = new ResponseResult();
                msg.success = false;
                msg.responseMessage = "Datastore " + datastoreName + " have existed, please use another name.\n";

                return Ok(msg);
            }
            else
            {
                ResponseResult msg = new ResponseResult();
                msg.success = true;
                msg.responseMessage = "Datastore " + datastoreName + " have been created.\n";

                return Ok(msg);
            }            
        }

        // Delete api/datastoremgmt
        [HttpDelete]
        [Route("api/[controller]")]
        [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ResponseResult>> Delete([FromBody] DatastoreRequestMessage request)
        {
            string user = request.user;
            string datastoreName = request.datastoreName;            

            if (!dsManager.DeleteDataStore(user, datastoreName))
            {
                ResponseResult msg = new ResponseResult();
                msg.success = false;
                msg.responseMessage = "Fail to delete Datastore " + datastoreName + ". Make sure the datastore exists and you are the creator of it.\n";

                return Ok(msg);
            }
            else
            {
                ResponseResult msg = new ResponseResult();
                msg.success = true;
                msg.responseMessage = "Datastore " + datastoreName + " have been deleted.\n";

                return Ok(msg);
            }
        }


        // POST api/datastoremgmt/preprocess/upload
        [HttpPost]
        [Route("api/[controller]/preprocess/upload")]
        [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ResponseResult>> Upload([FromForm] FileForm form)
        {
            string scenario = form.Scenario;
            string datastoreName = form.DatastoreName;
            var file = form.UploadFile;

            string excelDir = dsManager.GetUploadedFileSaveDir();

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
            msg.responseMessage = "File: " + savedFileName + " has been received.\n";

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
            string savedFilePath = dsManager.GetSavedExcelFilePath(savedFileName);

            ExcelParser eParser = new ExcelParser();
            (List<ExcelSheetVertexesRow> vRows, List<ExcelSheetEdgesRow> eRows) =  eParser.ParserExcel(savedFilePath);

            (string configPathh, string targetDir) = dsManager.GetTargetDirPath(datastoreName);


            /*
            (FileUploadConfig uploadConfig, string pythonArgs, string targetDir) = dsManager.GenerateConvertDirs(datastoreName, savedFileName, scenario);

            RunCommand(uploadConfig.PythonEnvPath, uploadConfig.ConvertScriptPath, pythonArgs);

            if (this.dsManager.GetPersistanceType() == PersistanceType.MongoDB)
            {
                SmartKG.DataUploader.Executor.DataUploader uploader = new SmartKG.DataUploader.Executor.DataUploader();
                uploader.UploadDataFile(targetDir, datastoreName);
            }
            */
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

        // POST api/datastoremgmt/preprocess/reload
        [HttpPost]
        [Route("api/[controller]/preprocess/reload")]
        [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ResponseResult>> ReloadData([FromForm] ReloadForm form)
        {
            string dsName = form.DatastoreName;

            (bool success, string message) = DataStoreManager.GetInstance().LoadDataStore(dsName);
            ResponseResult msg = new ResponseResult();

            if (success)
            {
                ContextAccessor.GetInstance().CleanContext(); // Clean all contexts and restart from clean env for a new datastore
            }
            msg.success = success;
            msg.responseMessage = message;
            
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
