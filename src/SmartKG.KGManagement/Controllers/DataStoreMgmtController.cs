using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartKG.Common.DataPersistence;
using SmartKG.KGManagement.Data.Request;
using SmartKG.KGManagement.Data.Response;

namespace SmartKG.KGManagement.Controllers
{
    
    [ApiController]
    public class DataStoreMgmtController : ControllerBase
    {
        static DataLoader dataLoader = DataLoader.GetInstance();


        [HttpGet]
        [Route("api/[controller]/current")]
        [ProducesResponseType(typeof(CurrentDatastoreResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CurrentDatastoreResult>> GetCurrent()
        {
            CurrentDatastoreResult msg = new CurrentDatastoreResult();
            msg.success = true;

            string cdsName = dataLoader.GetCurrentDataStoreName();

            msg.currentDatastoreName = cdsName;

            return Ok(msg);
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

            List<string> dsNames =  dataLoader.GetDataStoreList();

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
            string datastoreName = request.datastoreName;

            if (!dataLoader.AddDataStore(datastoreName))
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
            string datastoreName = request.datastoreName;

            if (!dataLoader.DeleteDataStore(datastoreName))
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
