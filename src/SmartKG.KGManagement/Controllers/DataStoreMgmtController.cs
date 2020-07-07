using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SmartKG.KGManagement.Data;
using SmartKG.KGManagement.Data.Request;
using SmartKG.KGManagement.Data.Response;

namespace SmartKG.KGManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataStoreMgmtController : ControllerBase
    {
        static IConfigurationBuilder builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) // requires Microsoft.Extensions.Configuration.Json
            .AddJsonFile("appsettings.json"); // requires Microsoft.Extensions.Configuration.Json                    
        static IConfiguration config = builder.Build();


        // Get api/datastoremgmt
        [HttpGet]
        //[Route("api/[controller]")]
        [ProducesResponseType(typeof(DatastoreResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DatastoreResult>> Get()
        {
            
            DatastoreResult msg = new DatastoreResult();
            msg.success = true;

            List<string> dsNames = GetDatastoreNames();

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

        private List<string> GetDatastoreNames()
        {
            FileUploadConfig uploadConfig = config.GetSection("FileUploadConfig").Get<FileUploadConfig>();

            string baseDir = uploadConfig.LocalRootPath;

            DirectoryInfo directory = new DirectoryInfo(baseDir);
            DirectoryInfo[] directories = directory.GetDirectories();

            List<string> dbNames = new List<string>();

            foreach (DirectoryInfo folder in directories)
                dbNames.Add(folder.Name);

            return dbNames;
        }

        // POST api/datastoremgmt
        [HttpPost]
        //[Route("api/[controller]")]
        [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ResponseResult>> Post([FromBody] DatastoreRequestMessage request)
        {
            string datastoreName = request.datastoreName;

            if (!IsDataStoreNew(datastoreName))
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

        private bool IsDataStoreNew(string datastoreName)
        {
            FileUploadConfig uploadConfig = config.GetSection("FileUploadConfig").Get<FileUploadConfig>();

            string targetDir = uploadConfig.LocalRootPath + Path.DirectorySeparatorChar + datastoreName;

            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
                return true;
            }
            else
            {
                return false;
            }
        }

        // Delete api/datastoremgmt
        [HttpDelete]
        //[Route("api/[controller]")]
        [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ResponseResult>> Delete([FromBody] DatastoreRequestMessage request)
        {
            string datastoreName = request.datastoreName;

            if (!IsDataStoreExist(datastoreName))
            {
                ResponseResult msg = new ResponseResult();
                msg.success = true;
                msg.responseMessage = "Datastore " + datastoreName + " doesn't exist. You don't need to delete it.\n";

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

        private bool IsDataStoreExist(string datastoreName)
        {
            FileUploadConfig uploadConfig = config.GetSection("FileUploadConfig").Get<FileUploadConfig>();
            string targetDir = uploadConfig.LocalRootPath + Path.DirectorySeparatorChar + datastoreName;
            if (Directory.Exists(targetDir))
            {
                Directory.Delete(targetDir);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
