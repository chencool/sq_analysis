using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using Dxc.Shq.WebApi.Core;
using Dxc.Shq.WebApi.Models;
using Dxc.Shq.WebApi.ViewModels;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace Dxc.Shq.WebApi.Controllers
{
    public class FTAProjectsController : ApiController
    {
        private ShqContext db = new ShqContext();

        [HttpGet]
        [Route("api/FTAProjects/GetTree")]
        // POST: api/FTAProjects
        [ResponseType(typeof(FTATreeViewModel))]
        public IHttpActionResult GetFTAProjectTree(Guid projectId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var docs = db.FTAProjects.Include("Project").Where(item => item.ProjectId == projectId).FirstOrDefault();
            if (docs == null)
            {
                return NotFound();
            }

            if (ProjectHelper.HasReadAccess(docs.Project) == false)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Forbidden, "No Access"));
            }

            var tr = docs.FTATrees.OrderByDescending(item => item.CreatedTime).FirstOrDefault();
            if (tr != null)
            {
                return Ok(new FTATreeViewModel(tr, db));
            }
            else
            {
                return Ok(new FTATreeViewModel(new FTATree() { FTAProjectId = docs.Id, FTAProject = docs }, db));
            }
        }

        [HttpPost]
        [Route("api/FTAProjects/AddTree")]
        // POST: api/FTAProjects
        [ResponseType(typeof(FTATreeViewModel))]
        public async Task<IHttpActionResult> AddFTAProjectTree(FTATreeRequestViewModel tree)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var docs = db.FTAProjects.Include("Project").Where(item => item.ProjectId == tree.ProjectId).FirstOrDefault();
            if (docs == null)
            {
                return NotFound();
            }

            if (ProjectHelper.HasUpdateAccess(docs.Project) == false)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Forbidden, "No Access"));
            }

            ShqUser shqUser = await db.ShqUsers.Where(item => item.IdentityUser.UserName == HttpContext.Current.User.Identity.Name).Include("IdentityUser").FirstOrDefaultAsync();
            var tr = docs.FTATrees.Where(item => item.Id == tree.Id).FirstOrDefault();
            if (tr == null)
            {
                FTATree ftaTree = new FTATree() { Id = tree.Id, FTAProjectId = docs.Id, FTAProject = docs, AnalysisStatus = 0, Content = tree.Content, CreatedById = shqUser.IdentityUserId, CreatedTime = DateTime.Now, LastModifiedById = shqUser.IdentityUserId, LastModfiedTime = DateTime.Now };
                docs.FTATrees.Add(ftaTree);
                await db.SaveChangesAsync();

                return Ok(new FTATreeViewModel(ftaTree, db));
            }
            else
            {
                return Conflict();
            }
        }

        [HttpPost]
        [Route("api/FTAProjects/AnalyzeTree")]
        // POST: api/FTAProjects
        [ResponseType(typeof(FTATreeViewModel))]
        public async Task<IHttpActionResult> AnalyzeFTAProjectTree(FTATreeRequestViewModel tree)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var docs = db.FTAProjects.Include("Project").Where(item => item.ProjectId == tree.ProjectId).FirstOrDefault();
            if (docs == null)
            {
                return NotFound();
            }

            if (ProjectHelper.HasUpdateAccess(docs.Project) == false)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Forbidden, "No Access"));
            }

            ShqUser shqUser = await db.ShqUsers.Where(item => item.IdentityUser.UserName == HttpContext.Current.User.Identity.Name).Include("IdentityUser").FirstOrDefaultAsync();
            var tr = docs.FTATrees.Where(item => item.Id == tree.Id).FirstOrDefault();
            if (tr == null)
            {
                FTATree ftaTree = new FTATree() { Id = tree.Id, FTAProjectId = docs.Id, AnalysisStatus = 1, FTAProject = docs, Content = tree.Content, CreatedById = shqUser.IdentityUserId, CreatedTime = DateTime.Now, LastModifiedById = shqUser.IdentityUserId, LastModfiedTime = DateTime.Now };
                docs.FTATrees.Add(ftaTree);
                db.SaveChanges();

                var jsonFTATree = JsonConvert.DeserializeObject<JsonFTATree>(tree.Content);
                Analyze(docs, jsonFTATree);
                var result = new FTATreeViewModel(ftaTree, db);

                string exeString = RunPythonAnalysis(docs.Id);
                if (exeString == null)
                {
                    result.AnalysisStatus = "Ok";

                    foreach (var jsNode in jsonFTATree.FTANodes)
                    {
                        var node = db.FTANodes.FirstOrDefault(item => item.FTAProjectId == docs.Id && item.EventId == jsNode.Id);
                        if (node != null)
                        {
                            jsNode.SmallFailureRateQ = node.SmallFailureRateQ;
                        }
                    }

                    foreach (var jsProperty in jsonFTATree.FTAProperties)
                    {
                        var node = db.FTANodeProperties.FirstOrDefault(item => item.FTAProjectId == docs.Id && item.Name == jsProperty.Name);
                        if (node != null)
                        {
                            jsProperty.InvalidRate = node.InvalidRate;
                        }
                    }
                    result.Content = JsonConvert.SerializeObject(jsonFTATree);

                    //// remove C:\Users\phimath\Source\Repos\sq_analysis\2.service\packages\MySqlConnector.0.47.1\lib\net45\MySqlConnector.dll
                    //using (var con = new MySqlConnection(ConfigurationManager.ConnectionStrings["ShqContext"].ConnectionString))
                    //{
                    //    con.Open();
                    //    var cmd = con.CreateCommand();
                    //    cmd.CommandText = "SELECT ftanodes.eventid as nodeid,ftanodeeventreportsreview.FTAEventTypeId,ftanodeeventreportsreview.FTAFailureTypeId, ftanodeeventreportsreview.EventValue,ftanodeeventreportsreview.EventValueType  FROM ftanodeeventreportsreview inner join ftanodes	on ftanodes.id = ftanodeeventreportsreview.FTANodeId";
                    //    using (var rdr = cmd.ExecuteReader(CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection))
                    //    {
                    //        result.JsonNodeEvents = new List<JsonNodeEvent>();

                    //        while (rdr.Read())
                    //        {
                    //            JsonNodeEvent e = new JsonNodeEvent();
                    //            e.NodeId = rdr.GetString(0);
                    //            e.EventId = rdr.GetInt32(1);
                    //            e.FalureId = rdr.GetInt32(2);
                    //            e.EventValue = rdr.GetDouble(3);
                    //            e.EventValueType = rdr.GetInt32(4);
                    //            result.JsonNodeEvents.Add(e);
                    //        }
                    //    }
                    //}

                    //using (var con = new MySqlConnection(ConfigurationManager.ConnectionStrings["ShqContext"].ConnectionString))
                    //{
                    //    con.Open();
                    //    var cmd = con.CreateCommand();
                    //    cmd.CommandText = string.Format("SELECT FTAEventTypeId,FTAFailureTypeId,FailureRateQ,InvalidRate FROM shqdb.ftaeventreports where ftaprojectid = '{0}'", docs.Id);
                    //    using (var rdr = cmd.ExecuteReader(CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection))
                    //    {
                    //        result.JsonTreeEvents = new List<JsonTreeEvent>();

                    //        while (rdr.Read())
                    //        {
                    //            JsonTreeEvent e = new JsonTreeEvent();
                    //            e.EventId = rdr.GetInt32(0);
                    //            e.FalureId = rdr.GetInt32(1);
                    //            e.FailureRateQ = rdr.GetDouble(2);
                    //            e.InvalidRate = rdr.GetInt32(3);
                    //            result.JsonTreeEvents.Add(e);
                    //        }
                    //    }
                    //}
                }
                else
                {
                    result.AnalysisStatus = "Error:" + exeString;
                }

                return Ok(result);
            }
            else
            {
                return Conflict();
            }
        }

        [HttpGet]
        [Route("api/FTAProjects/GetTreeReport")]
        // POST: api/FTAProjects
        [ResponseType(typeof(FTATreeReportViewModel))]
        public IHttpActionResult GetFTAProjectTreeReport(Guid projectId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var docs = db.FTAProjects.Include("Project").Where(item => item.ProjectId == projectId).FirstOrDefault();
            if (docs == null)
            {
                return NotFound();
            }

            if (ProjectHelper.HasReadAccess(docs.Project) == false)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Forbidden, "No Access"));
            }

            FTATreeReportViewModel result = new FTATreeReportViewModel();

            var resultNodes = db.FTAAnalysisResultByNames.Where(r => r.FTAProjectId == docs.Id).ToList();

            Dictionary<int, List<string>> dicNames = new Dictionary<int, List<string>>();
            Dictionary<int, List<string>> dicIds = new Dictionary<int, List<string>>();
            if (resultNodes != null)
            {
                foreach (var nodeName in resultNodes)
                {
                    if (dicNames.ContainsKey(nodeName.BranchId) == false)
                    {
                        dicNames.Add(nodeName.BranchId, new List<string>());
                    }
                    dicNames[nodeName.BranchId].Add(nodeName.FTANodeName);

                    if (dicIds.ContainsKey(nodeName.BranchId) == false)
                    {
                        dicIds.Add(nodeName.BranchId, new List<string>());
                    }

                    var nds = db.FTANodes.Where(item => item.NodeName == nodeName.FTANodeName).Select(r => r.EventId).ToList();
                    dicIds[nodeName.BranchId].AddRange(nds);
                }
            }

            if (dicNames.Values != null)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var key in dicNames.Keys)
                {
                    sb.Append(JsonConvert.SerializeObject(dicNames[key]));
                }

                result.MinimalCutSetNames = sb.ToString();
            }


            if (dicIds.Values != null)
            {
                StringBuilder sb = new StringBuilder();
                foreach(var key in dicIds.Keys)
                {
                    sb.Append(JsonConvert.SerializeObject(dicIds[key]));
                }

                result.MinimalCutSetIds = sb.ToString();
            }

            using (var con = new MySqlConnection(ConfigurationManager.ConnectionStrings["ShqContext"].ConnectionString))
            {
                con.Open();
                var cmd = con.CreateCommand();
                cmd.CommandText = "SELECT " +
                                    "ftanodes.EventId as '事件ID',  " +
                                    "(CASE shqdb.ftanodeeventreportsreview.FTAEventTypeId " +
                                      "WHEN 1 THEN '单点事件' " +
                                                "WHEN 2 THEN '双点事件' " +
                                                "WHEN 3 THEN '安全事件' " +
                                                "WHEN 4 THEN '节点失效概率' " +
                                                "WHEN 5 THEN '顶事件' " +
                                                "WHEN 6 THEN '底事件' " +
                                                "WHEN 7 THEN '最小割集' " +
                                                "ELSE '未知' " +
                                                "END) as '事件类型', " +
                                    "sum( if (FTAFailureTypeId = 1, EventValue, 0 ) ) AS '单点故障', " +
                                    "sum( if (FTAFailureTypeId = 2, EventValue, 0 ) ) AS '残余故障', " +
                                    "sum( if (FTAFailureTypeId = 3, EventValue, 0 ) ) AS '潜伏故障', " +
                                    "sum( if (FTAFailureTypeId = 4, EventValue, 0 ) ) AS '可探测的双点故障', " +
                                    "sum( if (FTAFailureTypeId = 5, EventValue, 0 ) ) AS '安全故障', " +
                                    "shqdb.ftanodes.NodeName " +
                                "FROM " +
                                    "shqdb.ftanodeeventreportsreview inner join shqdb.ftanodes on shqdb.ftanodeeventreportsreview.FTAProjectId = shqdb.ftanodes.FTAProjectId and shqdb.ftanodeeventreportsreview.FTANodeId = shqdb.ftanodes.id " +
                                    "where shqdb.ftanodeeventreportsreview.FTAProjectId = '" + docs.Id.ToString() + "' " +
                                "GROUP BY " +
                                    "FTANodeId; ";
                using (var rdr = cmd.ExecuteReader(CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection))
                {
                    while (rdr.Read())
                    {
                        FTATreeReportP1RowViewModel row = new FTATreeReportP1RowViewModel();
                        row.NodeId = rdr.GetString(0);
                        row.EventName = rdr.GetString(1);
                        row.SinglePointEvent = rdr.GetDouble(2);
                        row.DualPointEvent = rdr.GetDouble(3);
                        row.SafeEvent = rdr.GetDouble(4);
                        row.NodeFailureProbability = rdr.GetDouble(5);
                        row.TopEvent = rdr.GetDouble(6);
                        row.NodeName = rdr.GetString(7);
                        result.TableP1.Add(row);
                    }
                }
            }

            using (var con = new MySqlConnection(ConfigurationManager.ConnectionStrings["ShqContext"].ConnectionString))
            {
                con.Open();
                var cmd = con.CreateCommand();
                cmd.CommandText = "SELECT " +
                                    "(CASE FTAFailureTypeId " +
                                      "WHEN 1 THEN '单点故障' " +
                                                "WHEN 2 THEN '残余故障' " +
                                                "WHEN 3 THEN '潜伏故障' " +
                                                "WHEN 4 THEN '可探测的双点故障' " +
                                                "WHEN 5 THEN '安全故障' " +
                                                "ELSE '未知' " +
                                                "END) as '故障类型', " +
                                    "shqdb.FTAProjectReports.ProjectValue as '失效率', " +
                                    "ftanodes.EventId as '事件ID', " +
                                    "(CASE FTAEventTypeId " +
                                      "WHEN 1 THEN '单点事件' " +
                                                "WHEN 2 THEN '双点事件' " +
                                                "WHEN 3 THEN '安全事件' " +
                                                "WHEN 4 THEN '节点失效概率' " +
                                                "WHEN 5 THEN '顶事件' " +
                                                "WHEN 6 THEN '底事件' " +
                                                "WHEN 7 THEN '最小割集' " +
                                                "ELSE '未知' " +
                                                "END) as '事件名称', " +
                                    "EventValue as '故障率', " +
                                     "shqdb.ftanodes.NodeName " +
                                "FROM " +
                                    "shqdb.ftanodeeventreportsreview inner join shqdb.ftanodes on shqdb.ftanodeeventreportsreview.FTAProjectId = shqdb.ftanodes.FTAProjectId and shqdb.ftanodeeventreportsreview.FTANodeId = shqdb.ftanodes.id " +
                                    "inner join shqdb.FTAProjectReports on shqdb.FTAProjectReports.FTAProjectId = shqdb.ftanodeeventreportsreview.FTAProjectId and shqdb.FTAProjectReports.ProjectValueType = shqdb.ftanodeeventreportsreview.FTAFailureTypeId " +
                                "where FTAFailureTypeId< 6 and shqdb.ftanodeeventreportsreview.FTAProjectId = '" + docs.Id.ToString() + "' " +
                                "order by FTAFailureTypeId asc; ";
                using (var rdr = cmd.ExecuteReader(CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection))
                {
                    while (rdr.Read())
                    {
                        FTATreeReportP2RowViewModel row = new FTATreeReportP2RowViewModel();
                        row.FailureName = rdr.GetString(0);
                        row.InvalidValue = rdr.GetDouble(1);
                        row.NodeId = rdr.GetString(2);
                        row.EventName = rdr.GetString(3);
                        row.FailureValue = rdr.GetDouble(4);
                        row.NodeName = rdr.GetString(5);
                        result.TableP2.Add(row);
                    }
                }
            }

            using (var con = new MySqlConnection(ConfigurationManager.ConnectionStrings["ShqContext"].ConnectionString))
            {
                con.Open();
                var cmd = con.CreateCommand();
                cmd.CommandText = "select ProjectValue from shqdb.ftaprojectreports where shqdb.ftaprojectreports.FTAProjectId = '" + docs.Id.ToString() + "' " + " and ProjectValueType = 6 limit 1;";
                using (var rdr = cmd.ExecuteReader(CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection))
                {
                    while (rdr.Read())
                    {
                        result.SinglePointFaultMeasure = rdr.GetDouble(0);
                    }
                }
            }

            using (var con = new MySqlConnection(ConfigurationManager.ConnectionStrings["ShqContext"].ConnectionString))
            {
                con.Open();
                var cmd = con.CreateCommand();
                cmd.CommandText = "select ProjectValue from shqdb.ftaprojectreports where shqdb.ftaprojectreports.FTAProjectId = '" + docs.Id.ToString() + "' " + " and ProjectValueType = 7 limit 1;";
                using (var rdr = cmd.ExecuteReader(CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection))
                {
                    while (rdr.Read())
                    {
                        result.LatentFaultMeasure = rdr.GetDouble(0);
                    }
                }
            }

            using (var con = new MySqlConnection(ConfigurationManager.ConnectionStrings["ShqContext"].ConnectionString))
            {
                con.Open();
                var cmd = con.CreateCommand();
                cmd.CommandText = "select ProjectValue from shqdb.ftaprojectreports where shqdb.ftaprojectreports.FTAProjectId = '" + docs.Id.ToString() + "' " + " and ProjectValueType = 8 limit 1;";
                using (var rdr = cmd.ExecuteReader(CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection))
                {
                    while (rdr.Read())
                    {
                        result.RandomFaultMeasure = rdr.GetDouble(0);
                    }
                }
            }

            using (var con = new MySqlConnection(ConfigurationManager.ConnectionStrings["ShqContext"].ConnectionString))
            {
                con.Open();
                var cmd = con.CreateCommand();
                cmd.CommandText = "select shqdb.ftanodes.EventId, shqdb.ftanodes.NodeName from shqdb.ftanodeeventreports " +
                                    "inner join shqdb.ftanodes on shqdb.ftanodeeventreports.FTAProjectId = shqdb.ftanodes.FTAProjectId and shqdb.ftanodeeventreports.FTANodeId = shqdb.ftanodes.id " +
                                    "where FTAEventTypeId = 5 and shqdb.ftanodeeventreports.FTAProjectId = '" + docs.Id.ToString() + "' " +
                                    "limit 1";
                using (var rdr = cmd.ExecuteReader(CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection))
                {
                    while (rdr.Read())
                    {
                        result.TopEventIds = rdr.GetString(0);
                        result.TopEventNames = rdr.GetString(1);
                    }
                }
            }

            using (var con = new MySqlConnection(ConfigurationManager.ConnectionStrings["ShqContext"].ConnectionString))
            {
                con.Open();
                var cmd = con.CreateCommand();
                cmd.CommandText = "select shqdb.ftanodes.EventId, shqdb.ftanodes.NodeName,shqdb.ftanodeeventreports.FTAProjectId from shqdb.ftanodeeventreports " +
                                    "inner join shqdb.ftanodes on shqdb.ftanodeeventreports.FTAProjectId = shqdb.ftanodes.FTAProjectId and shqdb.ftanodeeventreports.FTANodeId = shqdb.ftanodes.id " +
                                    "where FTAEventTypeId = 6 and shqdb.ftanodeeventreports.FTAProjectId = '" + docs.Id.ToString() + "' ";
                using (var rdr = cmd.ExecuteReader(CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection))
                {
                    List<string> ids = new List<string>();
                    List<string> names = new List<string>();
                    while (rdr.Read())
                    {
                        ids.Add(rdr.GetString(0));
                        names.Add(rdr.GetString(1));
                    }

                    result.BaseEventIds = JsonConvert.SerializeObject(ids);
                    result.BaseEventNames = JsonConvert.SerializeObject(names);
                }
            }

            using (var con = new MySqlConnection(ConfigurationManager.ConnectionStrings["ShqContext"].ConnectionString))
            {
                con.Open();
                var cmd = con.CreateCommand();
                cmd.CommandText = "select shqdb.ftanodes.EventId, shqdb.ftanodes.NodeName,shqdb.ftanodeeventreports.FTAProjectId from shqdb.ftanodeeventreports " +
                                    "inner join shqdb.ftanodes on shqdb.ftanodeeventreports.FTAProjectId = shqdb.ftanodes.FTAProjectId and shqdb.ftanodeeventreports.FTANodeId = shqdb.ftanodes.id " +
                                    "where FTAEventTypeId = 1 and shqdb.ftanodeeventreports.FTAProjectId = '" + docs.Id.ToString() + "' ";
                using (var rdr = cmd.ExecuteReader(CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection))
                {
                    List<string> ids = new List<string>();
                    List<string> names = new List<string>();
                    while (rdr.Read())
                    {
                        ids.Add(rdr.GetString(0));
                        names.Add(rdr.GetString(1));
                    }

                    result.SinglePointEventIds = JsonConvert.SerializeObject(ids);
                    result.SinglePointEventNames = JsonConvert.SerializeObject(names);
                }
            }

            using (var con = new MySqlConnection(ConfigurationManager.ConnectionStrings["ShqContext"].ConnectionString))
            {
                con.Open();
                var cmd = con.CreateCommand();
                cmd.CommandText = "select shqdb.ftanodes.EventId, shqdb.ftanodes.NodeName,shqdb.ftanodeeventreports.FTAProjectId from shqdb.ftanodeeventreports " +
                                    "inner join shqdb.ftanodes on shqdb.ftanodeeventreports.FTAProjectId = shqdb.ftanodes.FTAProjectId and shqdb.ftanodeeventreports.FTANodeId = shqdb.ftanodes.id " +
                                    "where FTAEventTypeId = 2 and shqdb.ftanodeeventreports.FTAProjectId = '" + docs.Id.ToString() + "' ";
                using (var rdr = cmd.ExecuteReader(CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection))
                {
                    List<string> ids = new List<string>();
                    List<string> names = new List<string>();
                    while (rdr.Read())
                    {
                        ids.Add(rdr.GetString(0));
                        names.Add(rdr.GetString(1));
                    }

                    result.DualPointEventIds = JsonConvert.SerializeObject(ids);
                    result.DualPointEventNames = JsonConvert.SerializeObject(names);
                }
            }

            using (var con = new MySqlConnection(ConfigurationManager.ConnectionStrings["ShqContext"].ConnectionString))
            {
                con.Open();
                var cmd = con.CreateCommand();
                cmd.CommandText = "select shqdb.ftanodes.EventId, shqdb.ftanodes.NodeName,shqdb.ftanodeeventreports.FTAProjectId from shqdb.ftanodeeventreports " +
                                    "inner join shqdb.ftanodes on shqdb.ftanodeeventreports.FTAProjectId = shqdb.ftanodes.FTAProjectId and shqdb.ftanodeeventreports.FTANodeId = shqdb.ftanodes.id " +
                                    "where FTAEventTypeId = 3 and shqdb.ftanodeeventreports.FTAProjectId = '" + docs.Id.ToString() + "' ";
                using (var rdr = cmd.ExecuteReader(CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection))
                {
                    List<string> ids = new List<string>();
                    List<string> names = new List<string>();
                    while (rdr.Read())
                    {
                        ids.Add(rdr.GetString(0));
                        names.Add(rdr.GetString(1));
                    }

                    result.SafeEventIds = JsonConvert.SerializeObject(ids);
                    result.SafeEventNames = JsonConvert.SerializeObject(names);
                }
            }

            return Ok(result);
        }

        [HttpPost]
        [Route("api/FTAProjects/test")]
        [ResponseType(typeof(JsonFTATree))]
        public async Task<IHttpActionResult> TestFormat(JsonFTATree tree)
        {
            await db.SaveChangesAsync();

            return Ok(tree);
        }

        private JsonFTATree Analyze(FTAProject docs, JsonFTATree tree)
        {
            var temp = ParseTree(docs, tree);

            db.FTANodes.RemoveRange(docs.FTANodes);
            db.FTANodeProperties.RemoveRange(docs.FTANodeProperties);
            db.FTANodeGates.RemoveRange(docs.FTANodeGates);

            docs.FTANodes.AddRange(temp.FTANodes);
            docs.FTANodeProperties.AddRange(temp.FTANodeProperties);
            docs.FTANodeGates.AddRange(temp.FTANodeGates);

            db.SaveChanges();

            return tree;
        }

        private FTAProject ParseTree(FTAProject docs, JsonFTATree tree)
        {
            FTAProject resultDocs = new FTAProject();

            int i = 1;
            int nodeId = 1;
            int gateId = 1;
            try
            {
                Dictionary<string, FTANodeGate> gateList = new Dictionary<string, FTANodeGate>();
                List<JsonFTANode> FTANodesList = tree.FTANodes.FindAll(item => "square,rectangle,round".Contains(item.ItemType) == true);
                while (i <= FTANodesList.Count)
                {
                    var node = FTANodesList[i - 1];
                    i++;

                    FTANode fn = new FTANode();
                    fn.Id = nodeId;
                    nodeId++;
                    fn.FTAProjectId = docs.Id;
                    fn.FTAProject = docs;
                    fn.Index = node.Index;
                    fn.EventId = node.Id;
                    fn.NodeName = node.Name;
                    fn.Shape = node.Shape;
                    fn.Size = node.Size;
                    fn.X = node.X;
                    fn.Y = node.Y;
                    fn.ParentId = -1;
                    fn.FTANodeGateId = -1;
                    fn.SmallFailureRateQ = node.SmallFailureRateQ;
                    fn.QValueIsModifiedByUser = node.SmallFailureRateQValueType;

                    switch (node.ItemType.ToLower())
                    {
                        case "square":
                            fn.FTANodeType = db.FTANodeTypes.FirstOrDefault(item => item.Id == ShqConstants.FTANodeTypeRoot);
                            break;
                        case "rectangle":
                            fn.FTANodeType = db.FTANodeTypes.FirstOrDefault(item => item.Id == ShqConstants.FTANodeTypeBrand);
                            break;
                        case "round":
                            fn.FTANodeType = db.FTANodeTypes.FirstOrDefault(item => item.Id == ShqConstants.FTANodeTypeLeaf);
                            break;
                    }
                    fn.FTANodeTypeId = fn.FTANodeType.Id;

                    resultDocs.FTANodes.Add(fn);

                    // 获取属性
                    var property = tree.FTAProperties.FirstOrDefault(item => item.Name == node.Name);
                    if (property != null)
                    {
                        FTANodeProperties fp = new FTANodeProperties();
                        fp.Id = Guid.NewGuid();
                        fp.FTAProjectId = docs.Id;
                        fp.FTAProject = docs;
                        fp.Name = property.Name;
                        fp.DClf = property.DClf;
                        fp.DCrf = property.DCrf;
                        fp.FailureRateQ = property.FailureRateQ;
                        fp.FailureTime = property.FailureTime;
                        fp.InvalidRate = property.InvalidRate;
                        fp.InvalidRateValueIsModifiedByUser = property.InvalidRateValueIsModifiedByUser;
                        fp.ReferenceFailureRateq = property.ReferenceFailureRateq;

                        fn.FTANodePropertiesId = fp.Id;
                        fn.FTANodeProperties = fp;

                        resultDocs.FTANodeProperties.Add(fp);
                    }

                    // 获取 门 或父亲节点
                    var edge = tree.FTAEdges.FirstOrDefault(item => item.Target == node.Id);
                    if (edge != null)
                    {
                        var parentNode = tree.FTANodes.FirstOrDefault(item => item.Id == edge.Source);
                        if ("orgate,andgate,nongate".Contains(parentNode.ItemType.ToLower()))
                        {
                            FTANodeGate gate = null;
                            if (gateList.ContainsKey(edge.Source) ==false)
                            {
                                gate = new FTANodeGate(); //to do
                                gate.Id = gateId;
                                gate.NodeGateName = parentNode.Name;
                                gate.FTAProjectId = docs.Id;
                                gate.FTAProject = docs;
                                switch (parentNode.ItemType.ToLower())
                                {
                                    case "orgate":
                                        gate.FTANodeGateType = db.FTANodeGateTypes.FirstOrDefault(item => item.Id == ShqConstants.FTANodeGateTypeOr);
                                        break;
                                    case "andgate":
                                        gate.FTANodeGateType = db.FTANodeGateTypes.FirstOrDefault(item => item.Id == ShqConstants.FTANodeGateTypeAnd);
                                        break;
                                    case "nongate":
                                        gate.FTANodeGateType = db.FTANodeGateTypes.FirstOrDefault(item => item.Id == ShqConstants.FTANodeGateTypeXor);
                                        break;
                                }
                                gate.FTANodeGateTypeId = gate.FTANodeGateType.Id;
                                gateId++;
                                gateList.Add(parentNode.Id, gate);

                                resultDocs.FTANodeGates.Add(gate);
                            }
                            else
                            {
                                gate = gateList[edge.Source];
                            }
                            
                            fn.FTANodeGateId = gate.Id;

                            var parentEdge = tree.FTAEdges.FirstOrDefault(item => item.Target == edge.Source);
                            if (parentEdge != null)
                            {
                                var grandParentNode = FTANodesList.FirstOrDefault(item => item.Id == parentEdge.Source);

                                if (grandParentNode != null)
                                {
                                    fn.ParentId = FTANodesList.IndexOf(grandParentNode) + 1;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, ex.Message + ex.StackTrace));
            }

            if (resultDocs.FTANodes.Count > 0)
            {
                var roots = resultDocs.FTANodes.Where(item => item.ParentId == -1 && item.FTANodeTypeId == 1);
                if (roots != null && roots.Count() > 1)
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "根节点有多个"));
                }

                foreach (var node in resultDocs.FTANodes)
                {
                    switch (node.FTANodeTypeId)
                    {
                        case 1:
                            if (resultDocs.FTANodes.FirstOrDefault(item => item.ParentId == node.Id) == null)
                            {
                                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "根节点没有子节点"));
                            }
                            break;
                        case 2:
                            if (resultDocs.FTANodes.FirstOrDefault(item => item.Id == node.ParentId) == null)
                            {
                                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "中间节点没有父节点"));
                            }

                            if (resultDocs.FTANodes.FirstOrDefault(item => item.ParentId == node.Id) == null)
                            {
                                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "中间节点没有子节点"));
                            }
                            break;
                        case 3:
                            if (resultDocs.FTANodes.FirstOrDefault(item => item.ParentId == node.Id) != null)
                            {
                                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "叶子节点不能有子节点"));
                            }

                            if (resultDocs.FTANodes.FirstOrDefault(item => item.Id == node.ParentId) == null)
                            {
                                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "叶子节点没有父节点"));
                            }
                            break;
                    }
                }
            }
            else
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "没有节点"));
            }

            int maxLayer = int.Parse(ConfigurationManager.AppSettings["MaxLayer"]);

            for (int k = 0; k < resultDocs.FTANodes.Count; k++)
            {
                int layer = 0;
                var node = resultDocs.FTANodes[k];
                while (node != null && node.ParentId != -1 && layer <= maxLayer)
                {
                    layer++;
                    node = resultDocs.FTANodes.FirstOrDefault(item => item.Id == node.ParentId);
                }

                if (layer == maxLayer)
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "超出最大允许深度：" + maxLayer));
                }

                resultDocs.FTANodes[k].LayerNumber = layer;
            }

            return resultDocs;
        }

        private string RunPythonAnalysis(Guid ftaProjectId)
        {
            int timeout = 5 * 60 * 1000;
            using (Process process = new Process())
            {
                process.StartInfo.FileName = ConfigurationManager.AppSettings["PythonExe"];
                process.StartInfo.Arguments = string.Format("\"{0}\" --C={1}", ConfigurationManager.AppSettings["PythonScripts"], ftaProjectId.ToString());
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;

                StringBuilder outputMessage = new StringBuilder();
                StringBuilder errorMessage = new StringBuilder();
                using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
                using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                {
                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            outputWaitHandle.Set();
                        }
                        else
                        {
                            outputMessage.AppendLine(e.Data);
                        }
                    };
                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            errorWaitHandle.Set();
                        }
                        else
                        {
                            errorMessage.AppendLine(e.Data);
                        }
                    };

                    process.Start();

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    if (process.WaitForExit(timeout) &&
                        outputWaitHandle.WaitOne(timeout) &&
                        errorWaitHandle.WaitOne(timeout))
                    {
                        string result = outputMessage.ToString() + errorMessage.ToString();
                        if (result.Contains("the program is completed"))
                        {
                            return null;
                        }
                        else
                        {
                            return result;
                        }
                    }
                    else
                    {
                        // Timed out.
                        return "Time out to process the FTA analysis request";
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool FTADocumentExists(Guid id)
        {
            return db.FTAProjects.Count(e => e.Id == id) > 0;
        }
    }
}