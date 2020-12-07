// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
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

        static string[] ColourValues = new string[] {
        "#FF0000", "#000000", "#808080", "#008000",
        "#0000FF", "#000080", "#FF00FF", "#800080",
        "#800000", "#00FF00", "#FFBF00", "#FF7F50",
        "#DE3163", "#6495ED", "#40E0D0", "#2E86C1"
        };

        public ConfigController()
        {
            log = Log.Logger.ForContext<ConfigController>();
        }

        [HttpGet]
        [Route("api/[controller]/colors")]
        [ProducesResponseType(typeof(ConfigResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ColorResult>> Get()
        {
            ColorResult result = new ColorResult();

            result.success = true;
            result.responseMessage = "These colors you could used as your entity colors.";

            result.colors = ColourValues.ToList<string>();

            return result;
        }

        [HttpGet]
        [Route("api/[controller]/entitycolor")]
        [ProducesResponseType(typeof(ConfigResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IResult>> Get(string datastoreName, string scenarioName)
        {
            ConfigResult result = new ConfigResult();

            if (string.IsNullOrWhiteSpace(datastoreName))
            {
                result.success = false;
                result.responseMessage = "datastoreName不能为空。";
            }
            else if (string.IsNullOrWhiteSpace(scenarioName))
            {
                result.success = false;
                result.responseMessage = "scenarioName不能为空。";
            }
            else
            { 
                ConfigExecutor executor = new ConfigExecutor(null, datastoreName);

                (bool isDSExist, bool isScenarioExist, List<ColorConfig> configs) = executor.GetColorConfigs(scenarioName);

                if (!isDSExist)
                {
                    result.success = false;
                    result.responseMessage = "Datastore " + datastoreName + "不存在，或没有数据导入。";
                }
                else
                { 
                    if (!isScenarioExist)
                    {
                        result.success = false;
                        result.responseMessage = "scenario " + scenarioName + " 不存在。";
                    }
                    else
                    { 
                        result.success = true;

                        if (configs == null)
                        {
                            result.responseMessage = "scenario " + scenarioName + " 没有 color config 的定义。";
                        }
                        else
                        {
                            result.responseMessage = "一共有 " + configs.Count + " color config 的定义。";
                            result.entityColorConfig = new Dictionary<string,string>();

                            foreach(ColorConfig config in configs)
                            {
                                if (!result.entityColorConfig.ContainsKey(config.itemLabel))
                                { 
                                    result.entityColorConfig.Add(config.itemLabel, config.color);
                                }
                            }

                        }
                    }
                }
            }
            log.Information("[Response]: " + JsonConvert.SerializeObject(result));

            return Ok(result);
        }

         [HttpPost]
         [Route("api/[controller]/entitycolor")]
         [ProducesResponseType(typeof(ConfigResult), StatusCodes.Status200OK)]
         [ProducesResponseType(StatusCodes.Status400BadRequest)]
         public async Task<ActionResult<IResult>> Post(string user, string datastoreName, string scenarioName, Dictionary<string, string> entityColorConfig)
         {
             ConfigResult result = new ConfigResult();

             if (string.IsNullOrWhiteSpace(datastoreName))
             {
                 result.success = false;
                 result.responseMessage = "datastoreName不能为空。";
             }
             else if (string.IsNullOrWhiteSpace(scenarioName))
             {
                 result.success = false;
                 result.responseMessage = "scenarioName不能为空。";
             }
             else
             {
                ConfigExecutor executor = new ConfigExecutor(user, datastoreName);

                if (entityColorConfig == null || entityColorConfig.Count == 0)
                {
                    result.success = false;
                    result.responseMessage = "color config 不能为空。";
                }

                List<ColorConfig> colorConfigs = new List<ColorConfig>();

                foreach(string eName in entityColorConfig.Keys)
                {
                    ColorConfig config = new ColorConfig();
                    config.itemLabel = eName;
                    config.color = entityColorConfig[eName];

                    colorConfigs.Add(config);
                }

                (bool success, string msg) = executor.UpdateColorConfigs(scenarioName, colorConfigs);
                result.success = success;
                result.responseMessage = msg;

              }

            return Ok(result);
         }
    }
}
