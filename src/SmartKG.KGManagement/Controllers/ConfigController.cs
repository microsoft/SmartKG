using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;
using SmartKG.Common.Data.Visulization;
using SmartKG.KGManagement.Data.Response;
using SmartKG.KGManagement.GraphSearch;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SmartKG.KGManagement.Controllers
{
    [Produces("application/json")]
    [ApiController]
    public class ConfigController : Controller
    {
        private ILogger log;

        public ConfigController()
        {
            log = Log.Logger.ForContext<ScenariosController>();
        }

        [HttpGet]
        [Route("api/[controller]")]
        [ProducesResponseType(typeof(ConfigResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IResult>> Get(string scenarioName)
        {
            GraphExecutor executor = new GraphExecutor();

            List<ColorConfig> configs = executor.GetColorConfigs(scenarioName);

            ConfigResult result = new ConfigResult();
            result.success = true;

            if (configs == null)
            {
                result.responseMessage = "There is no color config defined in the KG. ";
            }
            else
            {
                result.responseMessage = "There are " + configs.Count + " color config defined.";
                result.vertexColorConfigs = configs;
            }

            log.Information("[Response]: " + JsonConvert.SerializeObject(result));

            return Ok(result);
        }
    }
}
