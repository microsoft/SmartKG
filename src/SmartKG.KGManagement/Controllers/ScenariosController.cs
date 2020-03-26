using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using Serilog;
using SmartKG.KGManagement.Data.Response;
using SmartKG.KGManagement.GraphSearch;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SmartKG.KGManagement.Controllers
{
    [Produces("application/json")]
    [ApiController]
    public class ScenariosController : Controller
    {
        private ILogger log;

        public ScenariosController(IConfiguration configuration)
        {
            log = Log.Logger.ForContext<ScenariosController>();
        }

        [HttpGet]
        [Route("api/[controller]")]
        [ProducesResponseType(typeof(RelationResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IResult>> Get()
        {
            GraphExecutor executor = new GraphExecutor();

            List<string> scenarioNames = executor.GetScenarioNames();

            ScenarioResult result = new ScenarioResult();
            result.success = true;

            if (scenarioNames == null)
            {
                result.responseMessage = "There is no scenario defined in the KG. ";
            }
            else
            {
                result.responseMessage = "There are " + scenarioNames.Count + " scenarios defined.";
                result.scenarioNames = scenarioNames;
            }

            log.Information("[Response]: " + JsonConvert.SerializeObject(result));

            return Ok(result);
        }
    }
}
