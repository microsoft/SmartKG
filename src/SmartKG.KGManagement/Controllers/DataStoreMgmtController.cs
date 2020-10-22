// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartKG.Common.DataStoreMgmt;
using SmartKG.KGManagement.Data.Request;
using SmartKG.KGManagement.Data.Response;

namespace SmartKG.KGManagement.Controllers
{
    
    [ApiController]
    public class DataStoreMgmtController : ControllerBase
    {
        DataStoreManager dsManager = DataStoreManager.GetInstance();
        
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
    }
}
