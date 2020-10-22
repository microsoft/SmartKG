// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
    public class GraphController : Controller
    {
        private ILogger log;

        public GraphController(IConfiguration configuration)
        {
            log = Log.Logger.ForContext<GraphController>();
        }

        [HttpGet]
        [Route("api/[controller]")]
        [ProducesResponseType(typeof(RelationResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IResult>> Get(string datastoreName, string[] scenarios)
        {
            RelationResult relationResult;

            GraphExecutor executor = new GraphExecutor(datastoreName);
            (List<VisulizedVertex> vvs, List<VisulizedEdge> ves) = executor.GetVertexesAndEdgesByScenarios(scenarios.ToList());

            string scenarioStr = "所有 scenarios";
            if (scenarios != null && scenarios.Length > 0)
            {
                scenarioStr = "scnearios: " + string.Join(", ", scenarios);
            }

            relationResult = new RelationResult(true,  scenarioStr + " 的图谱如下：");
            relationResult.nodes = vvs;
                
            relationResult.relations = ves;
           
            log.Information("[Response]: " + JsonConvert.SerializeObject(relationResult));

            return Ok(relationResult);
        }

    }
}