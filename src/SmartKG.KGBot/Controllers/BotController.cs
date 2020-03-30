// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using SmartKG.KGBot.Managment;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using SmartKG.KGBot.Data.Request;
using Newtonsoft.Json;
using Serilog;
using Microsoft.Extensions.Configuration;
using SmartKG.KGBot.Data;
using SmartKG.KGBot.Data.Response;
using SmartKG.Common.Logger;

namespace SmartKG.KGBot.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class BotController : Controller
    {

        private ILogger log;

        private RUNNINGMODE runningMode = RUNNINGMODE.PRODUCTION;

        public BotController(IConfiguration configuration)
        {
            log = Log.Logger.ForContext<BotController>();
            runningMode = configuration.GetSection("RunningMode").Get<RUNNINGMODE>();            
        }

        // POST api/bot
        [HttpPost("")]
        [ProducesResponseType(typeof(ResponseMsg), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ResponseMsg>> Post( [FromBody]BotRequestMessage request)
        {
            
            log.Here().Information("[Request]: " + JsonConvert.SerializeObject(request));

            string userId = request.userId;
            string query = request.query;
            string sessionId = request.sessionId;
            
            QueryResult queryResult;

            if (userId == null)
            {
                queryResult = new QueryResult(false, "userId 不能为空", ResponseItemType.Other);
            }
            else if (query == null)
            {
                queryResult = new QueryResult(false, "查询内容不能为空", ResponseItemType.Other);
            }
            else
            {             
                if (string.IsNullOrWhiteSpace(sessionId))
                {
                    sessionId = this.ControllerContext.HttpContext.Session.Id;
                }

                query = query.ToLower();

                DialogManager dm = new DialogManager();
                queryResult = await dm.Process(userId, sessionId, query, runningMode);            
            }

            ResponseMsg msg = new ResponseMsg();
            msg.result = queryResult;
            msg.sessionId = sessionId;

            log.Here().Information("[Response]: " + JsonConvert.SerializeObject(msg));

            return Ok(msg);
        }
    }
}