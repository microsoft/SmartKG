// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;
using SmartKG.Common.ContextStore;
using SmartKG.Common.Data;
using SmartKG.Common.Data.Configuration;
using SmartKG.Common.Data.KG;
using SmartKG.Common.Data.Visulization;
using SmartKG.Common.DataStoreMgmt;
using SmartKG.Common.Parser;
using SmartKG.KGManagement.Data.Request;
using SmartKG.KGManagement.Data.Response;
using System.Drawing;
using SmartKG.Common.Data.LU;

namespace SmartKG.KGManagement.Controllers
{
    
    [ApiController]
    public class DataStoreMgmtController : ControllerBase
    {
        private ILogger log;
        DataStoreManager dsManager = DataStoreManager.GetInstance();
        
        public DataStoreMgmtController()
        {
            log = Log.Logger.ForContext<DataStoreMgmtController>();
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
                msg.success = false;
                msg.responseMessage = "Fail to delete Datastore " + datastoreName + ". Make sure the datastore exists and you are the creator of it.\n";

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

        // POST api/datastoremgmt/preprocess/upload
        [HttpPost]
        [Route("api/[controller]/preprocess/upload")]
        [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ResponseResult>> Upload([FromForm] FileForm form)
        {
            string scenario = form.Scenario;
            string datastoreName = form.DatastoreName;
            var file = form.UploadFile;

            
            string excelDir = dsManager.GetUploadedFileSaveDir();

            ResponseResult msg = new ResponseResult();

            try
            { 
                Directory.CreateDirectory(excelDir);
            }   
            catch (Exception e)
            {
                msg.success = false;
                msg.responseMessage = "创建临时目录失败，请确认您有创建 " + excelDir + " 目录的权限";

                return Ok(msg);
            }

            string savedFileName = null;
            if (file != null)
            {
                if (file.Length > 0)
                {
                    try
                    { 
                        string newFileName = GenerateTempFileName(file.FileName);

                        var filePath = excelDir + Path.DirectorySeparatorChar + newFileName;

                        using (var stream = System.IO.File.Create(filePath))
                        {
                            await file.CopyToAsync(stream);
                        }

                        savedFileName = newFileName;
                    }
                    catch (Exception e)
                    {
                        msg.success = false;
                        msg.responseMessage = "在临时目录内保存 Excel 文件失败，请确认您有访问 " + excelDir + " 目录的权限";

                        return Ok(msg);
                    }
                }

            }
            
            try
            { 
                (bool success, string message, string targetDir) = ConvertFiles(savedFileName, datastoreName, scenario);

                if (success)
                { 

                    if (this.dsManager.GetPersistanceType() == PersistanceType.MongoDB)
                    {
                        try
                        { 
                            SmartKG.DataUploader.Executor.DataUploader uploader = new SmartKG.DataUploader.Executor.DataUploader();
                            uploader.UploadDataFile(targetDir, datastoreName);
                        }
                        catch (Exception e)
                        {
                            success = false;
                            message = "上传数据到 MongoDB 失败，请确认 MongoDB 链接";
                        }
                    }
                }

                msg.success = success;

                if (msg.success)
                {
                    msg.responseMessage = "File: " + savedFileName + " has been received.\n";
                }
                else
                { 
                    msg.responseMessage = message;
                }
            }
            catch (Exception e)
            {
                msg.success = false;
                msg.responseMessage = "解析 Excel 文件失败，请确认 Excel 有效。";
            }
                        
            return Ok(msg);
        }

        private string GenerateTempFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return null;
            }

            string[] fileNameSecs = fileName.Split(Path.DirectorySeparatorChar);

            fileName = fileNameSecs[fileNameSecs.Length - 1];

            string[] secs = fileName.Split(".");

            string suffix = secs[secs.Length - 1];

            string tmpStr = DateTime.Now.ToString("yyyyMMddHHmmssfff");

            string newSuffix = "_" + tmpStr + "." + suffix;

            string newFileName = fileName.Replace("." + suffix, newSuffix);

            return newFileName;
        }

        private (bool, string, string) ConvertFiles(string savedFileName, string datastoreName, string scenario)
        {
            bool success = false;
            string message = "";

            string savedFilePath = dsManager.GetSavedExcelFilePath(savedFileName);

            ExcelParser eParser = new ExcelParser();
            (List<Vertex> vertexes, List<Edge> edges) =  eParser.ParserExcel(savedFilePath, scenario);

            string targetDir = null;
            if (vertexes != null && vertexes.Count > 0)
            { 
                targetDir = GenerateKGNLUConfigFiles(vertexes, edges, datastoreName, scenario);
                success = true;
            }
            else
            {
                message = "无法从上传的 Excel 中解析出实体，请确认该 Excel 与模板格式一致。";
            }

            return (success, message, targetDir);
        }

        private string GenerateKGNLUConfigFiles(List<Vertex> vertexes, List<Edge> edges, string datastoreName, string scenario)
        {
            (string configPath, string targetDir) = dsManager.GetTargetDirPath(datastoreName);

            string kgPath = targetDir + Path.DirectorySeparatorChar + "KG" + Path.DirectorySeparatorChar;
            string nluPath = targetDir + Path.DirectorySeparatorChar + "NLU" + Path.DirectorySeparatorChar;
            string vcPath = targetDir + Path.DirectorySeparatorChar + "Visulization" + Path.DirectorySeparatorChar;

            System.IO.Directory.CreateDirectory(kgPath);
            System.IO.Directory.CreateDirectory(nluPath);
            System.IO.Directory.CreateDirectory(vcPath);


            string vJsonPath = kgPath + "Vertexes_" + scenario + ".json";            
            string vJsonContent = JsonConvert.SerializeObject(vertexes, Formatting.Indented);
            System.IO.File.WriteAllText(vJsonPath, vJsonContent, Encoding.UTF8);
            
            string eJsonPath = kgPath + "Edges_" + scenario + ".json";
            if (edges != null && edges.Count > 0)
            { 
                string eJsonContent = JsonConvert.SerializeObject(edges, Formatting.Indented);
                System.IO.File.WriteAllText(eJsonPath, eJsonContent, Encoding.UTF8);
            }
            else
            {
                System.IO.File.WriteAllText(eJsonPath, "[]", Encoding.UTF8);
            }

            string intentPath = nluPath + "intentrules_" + scenario + ".json";
            string entityMapPath = nluPath + "entitymap_" + scenario + ".json";
            string colorJsonPath = vcPath + "VisulizationConfig_" + scenario + ".json";


            (List<NLUIntentRule> intentRules, List<EntityData> entityDatas, VisulizationConfig vConfig) = GenerateNLUAndConfig(vertexes, edges, scenario, configPath);

            System.IO.File.WriteAllText(intentPath, JsonConvert.SerializeObject(intentRules , Formatting.Indented), Encoding.UTF8);
            System.IO.File.WriteAllText(entityMapPath, JsonConvert.SerializeObject(entityDatas, Formatting.Indented), Encoding.UTF8);            
            System.IO.File.WriteAllText(colorJsonPath, JsonConvert.SerializeObject(new List<VisulizationConfig>() { vConfig }, Formatting.Indented), Encoding.UTF8);

            return targetDir;
        }


        private ColorConfig GetColor(Dictionary<string, string> hexDict, Dictionary<string, string> predefinedDict, string entityType)
        {
            ColorConfig colorConfig = new ColorConfig();

            colorConfig.itemLabel = entityType;

            if (predefinedDict.ContainsKey(entityType))
            {
                string color = predefinedDict[entityType];

                if (hexDict.ContainsKey(color))
                {
                    colorConfig.color = hexDict[color];
                    return colorConfig;
                }
            }

            Random rnd = new Random();
            Color randomColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));

            colorConfig.color = "#" + randomColor.R.ToString("X2") + randomColor.G.ToString("X2") + randomColor.B.ToString("X2");
            return colorConfig;
        }

        private EntityData CreateEntityData(string scenario, string entityType, string entityValue)
        {
            EntityData eData = new EntityData();
            eData.intentName = scenario;
            eData.entityType = entityType;
            eData.entityValue = entityValue;
            eData.similarWord = entityValue;

            eData.id = Guid.NewGuid().ToString();

            return eData;
        }

        private (List<NLUIntentRule>, List<EntityData>, VisulizationConfig) GenerateNLUAndConfig(List<Vertex> vertexes, List<Edge> edges, string scenario, string configPath)
        {
            VisulizationConfig vConfig = new VisulizationConfig();
            vConfig.scenario = scenario;
            vConfig.labelsOfVertexes = new List<ColorConfig>();
            vConfig.relationTypesOfEdges = new List<ColorConfig>();

            string hexFilePath = configPath + Path.DirectorySeparatorChar + "HexColorCodeDict.tsv";
            string pdFilePath = configPath + Path.DirectorySeparatorChar + "PreDefinedVertexColor.tsv";

            Dictionary<string, string> hexDict = new Dictionary<string, string>();

            foreach (var line in System.IO.File.ReadLines(hexFilePath))
            {
                string[] tmps = line.Split('\t');

                if (hexDict.ContainsKey(tmps[0]))
                {
                    hexDict[tmps[0]] = tmps[1];
                }
                else
                { 
                    hexDict.Add(tmps[0], tmps[1]);
                }
            }

            Dictionary<string, string> predefinedDict = new Dictionary<string, string>();

            foreach (var line in System.IO.File.ReadLines(pdFilePath))
            {
                string[] tmps = line.Split('\t');
                if (predefinedDict.ContainsKey(tmps[0]))
                {
                    predefinedDict[tmps[0]] = tmps[1];
                }
                else
                { 
                    predefinedDict.Add(tmps[0], tmps[1]);
                }
            }
            // ---
            
            HashSet<string> entityNameSet = new HashSet<string>();
            HashSet<string> entityTypeSet = new HashSet<string>();
            HashSet<string> propertyNameSet = new HashSet<string>();

            foreach (Vertex vertex in vertexes)
            {
                entityTypeSet.Add(vertex.label);
                entityNameSet.Add(vertex.name);
                if (vertex.properties != null && vertex.properties.Count > 0)
                {
                    foreach(VertexProperty p in vertex.properties)
                    {
                        propertyNameSet.Add(p.name);                            
                    }
                }
            }

            HashSet<string> relationTypeSet = new HashSet<string>();
            foreach (Edge edge in edges)
            {
                relationTypeSet.Add(edge.relationType);                
            }


            List<EntityData> entityDatas = new List<EntityData>();

            List<NLUIntentRule> intentRuleList = new List<NLUIntentRule>();
            NLUIntentRule intentRule = new NLUIntentRule();
            intentRule.id = Guid.NewGuid().ToString();
            intentRule.intentName = scenario;
            intentRule.type = IntentRuleType.POSITIVE;
            intentRule.ruleSecs = new List<string>();

            string entityRule = "";                        
            int count = 0;

            foreach (string entityName in entityNameSet)
            {
                if (count % 20 == 0  && count > 0)
                {
                    entityRule = entityRule.Substring(0, entityRule.Length - 1);
                    intentRule.ruleSecs.Add(entityRule);

                    intentRuleList.Add(intentRule);

                    intentRule = new NLUIntentRule();
                    intentRule.id = Guid.NewGuid().ToString();
                    intentRule.intentName = scenario;
                    intentRule.type = IntentRuleType.POSITIVE;
                    intentRule.ruleSecs = new List<string>();

                    entityRule = "";                    
                }

                entityRule += entityName + "|";                     
                entityDatas.Add(CreateEntityData(scenario, "NodeName", entityName));
                count++;
            }

            entityRule = entityRule.Substring(0, entityRule.Length - 1);
            intentRule.ruleSecs.Add(entityRule);

            intentRuleList.Add(intentRule);

            foreach (string entityType in entityTypeSet)
            {
                vConfig.labelsOfVertexes.Add(GetColor(hexDict, predefinedDict, entityType));
            }

            foreach (string pName in propertyNameSet)
            {                                
                entityDatas.Add(CreateEntityData(scenario, "PropertyName", pName));
            }

            foreach (string rType in relationTypeSet)
            {                
                entityDatas.Add(CreateEntityData(scenario, "RelationType", rType));
                vConfig.relationTypesOfEdges.Add(GetColor(hexDict, predefinedDict, rType));
            }

            return (intentRuleList, entityDatas, vConfig);
        }       

        // POST api/datastoremgmt/preprocess/reload
        [HttpPost]
        [Route("api/[controller]/preprocess/reload")]
        [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ResponseResult>> ReloadData([FromForm] ReloadForm form)
        {
            string dsName = form.DatastoreName;

            (bool success, string message) = DataStoreManager.GetInstance().LoadDataStore(dsName);
            ResponseResult msg = new ResponseResult();

            if (success)
            {
                ContextAccessor.GetInstance().CleanContext(); // Clean all contexts and restart from clean env for a new datastore
            }
            msg.success = success;
            msg.responseMessage = message;
            
            return Ok(msg);
        }
    }

    public class FileForm
    {
        [Required] public string DatastoreName { get; set; }
        [Required] public string Scenario { get; set; }
        [Required] public IFormFile UploadFile { get; set; }
    }

    public class ReloadForm
    {
        [Required] public string DatastoreName { get; set; }
    }

}
