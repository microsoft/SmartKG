// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using SmartKG.Common.Data.Visulization;
using SmartKG.Common.Logger;
using SmartKG.Common.Utils;
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
       
        // GET api/graph/scenarios
        [HttpGet]
        [Route("api/[controller]/scenarios")]
        [ProducesResponseType(typeof(RelationResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IResult>> Get(string datastoreName)
        {
            ScenarioResult result = new ScenarioResult();

            if (string.IsNullOrWhiteSpace(datastoreName))
            {             
                result.success = false;
                result.responseMessage = "datastoreName不能为空。";                
            }
            else
            { 
                GraphExecutor executor = new GraphExecutor(datastoreName);

                (bool isDSExist, List<string> scenarioNames) = executor.GetScenarioNames();
                       
                if (!isDSExist)
                {
                    result.success = false;
                    result.responseMessage = "Datastore " + datastoreName + "不存在，或没有数据导入。";
                }
                else
                {
                    result.success = true;

                    if (scenarioNames == null)
                    {
                        result.responseMessage = "图谱中不包含任何 scenario。" ;
                    }
                    else
                    {
                        result.responseMessage = "一共有 " + scenarioNames.Count + " 个 scenario。";
                        result.scenarioNames = scenarioNames;
                    }
                }
            }
            log.Information("[Response]: " + JsonConvert.SerializeObject(result));

            return Ok(result);
        }

        // GET api/graph/search
        [HttpGet]
        [Route("api/[controller]/search")]
        [ProducesResponseType(typeof(SearchResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IResult>> Search(string datastoreName, string keyword)
        {
            SearchResult searchResult;

            if (string.IsNullOrWhiteSpace(datastoreName) || string.IsNullOrWhiteSpace(keyword))
            {
                searchResult = new SearchResult(false, "datastoreName 和关键字不能为空");                
            }
            else
            {
                keyword = keyword.Trim();

                searchResult = new SearchResult(true, "根据\"" + keyword + "\"为您搜索到以下节点：");
                GraphExecutor executor = new GraphExecutor(datastoreName);
                (bool isDSExist, List < VisulizedVertex> vvs) = executor.SearchVertexesByName(keyword);

                if (!isDSExist)
                {
                    searchResult.responseMessage = "Datastore " + datastoreName + "不存在，或没有数据导入。";
                }
                else
                { 
                    searchResult.nodes = vvs;

                    if (searchResult.nodes == null)
                    {
                        searchResult.responseMessage = "根据\"" + keyword + "\"无法搜索到任何节点。";
                    }
                }
            }

            log.Here().Information("[Response]: " + JsonConvert.SerializeObject(searchResult));

            return Ok(searchResult);
        }

        // GET api/graph/search/property
        [HttpGet]
        [Route("api/[controller]/search/property")]
        [ProducesResponseType(typeof(RelationResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IResult>> Filter(string datastoreName, string name, string value)
        {
            RelationResult filterResult;

            if (string.IsNullOrWhiteSpace(datastoreName) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(value))
            {
                filterResult = new RelationResult(false, "datastoreName, 属性名和属性值都不能为空");                
            }
            else
            {
                name = name.ToLower();
                value = value.ToLower();

                GraphExecutor executor = new GraphExecutor(datastoreName);
                (bool isDSExist, List < VisulizedVertex> vvs) = executor.FilterVertexesByProperty(name, value);

                if(!isDSExist)
                {
                    filterResult = new RelationResult(false, "Datastore " + datastoreName + "不存在，或没有数据导入。");                    
                }
                else
                { 
                    if (vvs == null || vvs.Count == 0)
                    {
                        filterResult = new RelationResult(true, "根据\"" + name + " = " + value + ",\"未能找到任何符合条件的节点。");
                    }
                    else
                    {
                        VisulizedVertex propertyVV = KGUtility.GeneratePropertyVVertex("", name, value);
                        filterResult = new RelationResult(true, "根据\"" + name + " = " + value + ",\"为您搜索到以下节点：");

                        List<VisulizedEdge> ves = new List<VisulizedEdge>();

                        foreach (VisulizedVertex vv in vvs)
                        {
                            VisulizedEdge ve = new VisulizedEdge();

                            ve.value = name;
                            ve.sourceId = propertyVV.id;
                            ve.targetId = vv.id;

                            ves.Add(ve);
                        }

                        vvs.Insert(0, propertyVV);

                        filterResult.nodes = vvs;
                        filterResult.relations = ves;
                    }
                }
            }

            log.Here().Information("[Response]: " + JsonConvert.SerializeObject(filterResult));

            return Ok(filterResult);
        }


        // GET api/graph/search/{id}
        [HttpGet]
        [Route("api/[controller]/relations/{id}")]
        [ProducesResponseType(typeof(RelationResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IResult>> GetRelations(string datastoreName, string id)
        {
            RelationResult relationResult;

            if (string.IsNullOrWhiteSpace(datastoreName) || string.IsNullOrWhiteSpace(id))
            {
                relationResult = new RelationResult(false, "datastoreName 和 id 不能为空");
            }
            else
            {
                GraphExecutor executor = new GraphExecutor(datastoreName);
                (bool isDSExist, VisulizedVertex theVVertex) = executor.GetVertexById(id);

                if (!isDSExist)
                {
                    relationResult = new RelationResult(false, "Datastore " + datastoreName + "不存在，或没有数据导入。");
                }
                else
                { 
                    if (theVVertex == null)
                    {
                        relationResult = new RelationResult(true, "无法找到节点 " + id + "。");
                    }
                    else
                    {
                        (bool dsExist, List<VisulizedVertex> vvs, List<VisulizedEdge> ves) = executor.GetFirstLevelRelationships(id);

                        relationResult = new RelationResult(true, "节点 id =" + id + " 的一阶关系如下：");
                        relationResult.nodes = vvs;
                        if (relationResult.nodes == null)
                        {
                            relationResult.nodes = new List<VisulizedVertex> { theVVertex };
                        }
                        else
                        {
                            relationResult.nodes.Add(theVVertex);
                        }
                        relationResult.relations = ves;
                    }
                }
            }

            log.Here().Information("[Response]: " + JsonConvert.SerializeObject(relationResult));

            return Ok(relationResult);
        }

        [HttpGet]
        [Route("api/[controller]/visulize")]
        [ProducesResponseType(typeof(RelationResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IResult>> Get(string datastoreName, string scenarioName)
        {
            RelationResult relationResult;

            if (string.IsNullOrWhiteSpace(datastoreName) || string.IsNullOrWhiteSpace(scenarioName))
            {
                relationResult = new RelationResult(false, "datastoreName 和 scenario 不能为空");                
            }
            else
            { 
                GraphExecutor executor = new GraphExecutor(datastoreName);
                (bool isDSExist, bool isScenarioExist, List<VisulizedVertex> vvs, List<VisulizedEdge> ves) = executor.GetVertexesAndEdgesByScenarios(new List<string>() { scenarioName });

                if (!isDSExist)
                {
                    relationResult = new RelationResult(false, "Datastore " + datastoreName + "不存在，或没有数据导入。");
                }
                else
                { 
                    if (!isScenarioExist)
                    {
                        relationResult = new RelationResult(false, "Scenario " + scenarioName + "不存在，或没有数据导入。");
                    }
                    else
                    { 
                        string scenarioStr = "scenario: " + scenarioName + " 的图谱如下：";

                        relationResult = new RelationResult(true, scenarioStr + " 的图谱如下：");
                        relationResult.nodes = vvs;

                        relationResult.relations = ves;
                    }
                }
            }

            log.Information("[Response]: " + JsonConvert.SerializeObject(relationResult));

            return Ok(relationResult);
        }
    }
}