// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog;
using Microsoft.Extensions.Configuration;
using SmartKG.KGManagement.Data.Response;
using SmartKG.KGManagement.GraphSearch;
using System.Collections.Generic;
using SmartKG.Common.Data.Visulization;
using SmartKG.Common.Logger;
using SmartKG.Common.Utils;

namespace SmartKG.KGManagement.Controllers
{
    [Produces("application/json")]
    [ApiController]
    public class SearchController : Controller
    {
        private ILogger log;

        public SearchController(IConfiguration configuration)
        {
            log = Log.Logger.ForContext<SearchController>();
        }

        // GET api/search
        [HttpGet]
        [Route("api/[controller]")]
        [ProducesResponseType(typeof(SearchResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IResult>> Search(string keyword)
        {
            SearchResult searchResult;

            if (string.IsNullOrWhiteSpace(keyword))
            {
                searchResult = new SearchResult(false, "关键字不能为空");
            }
            else
            {
                keyword = keyword.ToLower();

                searchResult = new SearchResult(true, "根据\"" + keyword + "\"为您搜索到以下节点：");
                GraphExecutor executor = new GraphExecutor();
                List<VisulizedVertex> vvs = executor.SearchVertexesByName(keyword);

                searchResult.nodes = vvs; 

                if (searchResult.nodes == null)
                {
                    searchResult.responseMessage = "根据\"" + keyword + "\"无法搜索到任何节点。";
                }
            }

            log.Here().Information("[Response]: " + JsonConvert.SerializeObject(searchResult));

            return Ok(searchResult);
        }

       

        
        // GET api/search/property
        [HttpGet]
        [Route("api/[controller]/property")]
        [ProducesResponseType(typeof(RelationResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IResult>> Filter(string name, string value)
        {
            RelationResult filterResult;

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(value))
            {
                filterResult = new RelationResult(false, "属性名和属性值都不能为空");
            }
            else
            {
                name = name.ToLower();
                value = value.ToLower();

                GraphExecutor executor = new GraphExecutor();
                List<VisulizedVertex> vvs = executor.FilterVertexesByProperty(name, value);

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

            log.Here().Information("[Response]: " + JsonConvert.SerializeObject(filterResult));

            return Ok(filterResult);
        }


        // GET api/search/{id}
        [HttpGet]
        [Route("api/[controller]/{id}")]
        [ProducesResponseType(typeof(RelationResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IResult>> GetRelations(string id)
        {
            RelationResult relationResult;

            if (string.IsNullOrWhiteSpace(id))
            {
                relationResult = new RelationResult(false, "id不能为空");
            }
            else
            {               
                GraphExecutor executor = new GraphExecutor();               
                VisulizedVertex theVVertex = executor.GetVertexById(id);
              
                if (theVVertex == null)
                {
                    relationResult = new RelationResult(true, "无法找到节点 " + id + "。");
                }
                else
                {
                    (List<VisulizedVertex>  vvs, List<VisulizedEdge> ves) = executor.GetFirstLevelRelationships(id);

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

            log.Here().Information("[Response]: " + JsonConvert.SerializeObject(relationResult));

            return Ok(relationResult);
        }
    }

}    
