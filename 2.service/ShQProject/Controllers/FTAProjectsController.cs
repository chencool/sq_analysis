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
                return NotFound();
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
                await db.SaveChangesAsync();

                Analyze(docs, JsonConvert.DeserializeObject<JsonFTATree>(tree.Content));
                var result = new FTATreeViewModel(ftaTree, db);

                string exeString = RunPythonAnalysis(docs.Id);
                if (exeString == null)
                {
                    JsonFTATree resultTree = new JsonFTATree();
                    var resultNodes = db.FTAAnalysisResultByNames.Where(r => r.FTAProjectId == docs.Id).Select(item => item.FTANodeName).ToList();
                    var nds = db.FTANodes.Where(item => resultNodes.Contains(item.NodeName) == true).Select(r => r.EventId).ToList();
                    result.AnalysisStatus = "Ok";
                    result.AnalysisNodeIds = nds;

                    // remove C:\Users\phimath\Source\Repos\sq_analysis\2.service\packages\MySqlConnector.0.47.1\lib\net45\MySqlConnector.dll
                    using (var con = new MySqlConnection(ConfigurationManager.ConnectionStrings["ShqContext"].ConnectionString))
                    {
                        con.Open();
                        var cmd = con.CreateCommand();
                        cmd.CommandText = "SELECT ftanodes.eventid as nodeid,ftanodeeventreports.FTAEventTypeId,ftanodeeventreports.FTAFailureTypeId, ftanodeeventreports.EventValue,ftanodeeventreports.EventValueType  FROM ftanodeeventreports inner join ftanodes	on ftanodes.id = ftanodeeventreports.FTANodeId";
                        using (var rdr = cmd.ExecuteReader(CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection))
                        {
                            result.JsonNodeEvents = new List<JsonNodeEvent>();

                            while (rdr.Read())
                            {
                                JsonNodeEvent e = new JsonNodeEvent();
                                e.NodeId = rdr.GetString(0);
                                e.EventId = rdr.GetInt32(1);
                                e.FalureId = rdr.GetInt32(2);
                                e.EventValue = rdr.GetDouble(3);
                                e.EventValueType = rdr.GetInt32(4);
                                result.JsonNodeEvents.Add(e);
                            }
                        }
                    }

                    using (var con = new MySqlConnection(ConfigurationManager.ConnectionStrings["ShqContext"].ConnectionString))
                    {
                        con.Open();
                        var cmd = con.CreateCommand();
                        cmd.CommandText = string.Format("SELECT FTAEventTypeId,FTAFailureTypeId,FailureRateQ,InvalidRate FROM shqdb.ftaeventreports where ftaprojectid = '{0}'", docs.Id);
                        using (var rdr = cmd.ExecuteReader(CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection))
                        {
                            result.JsonTreeEvents = new List<JsonTreeEvent>();

                            while (rdr.Read())
                            {
                                JsonTreeEvent e = new JsonTreeEvent();
                                e.EventId = rdr.GetInt32(0);
                                e.FalureId = rdr.GetInt32(1);
                                e.FailureRateQ = rdr.GetDouble(2);
                                e.InvalidRate = rdr.GetInt32(3);
                                result.JsonTreeEvents.Add(e);
                            }
                        }
                    }
                }
                else
                {
                    result.AnalysisStatus = "Error:" + exeString;
                    result.AnalysisNodeIds = new List<string>();
                }

                return Ok(result);
            }
            else
            {
                return Conflict();
            }
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
                        fp.DClf = property.DClf;
                        fp.DCrf = property.DCrf;
                        fp.FailureRateQ = property.FailureRateQ;
                        fp.FailureTime = property.FailureTime;
                        fp.InvalidRate = property.InvalidRate;
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
                            FTANodeGate gate = new FTANodeGate(); //to do
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

                            resultDocs.FTANodeGates.Add(gate);
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

            return resultDocs;
        }

        private string RunPythonAnalysis(Guid ftaProjectId)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = ConfigurationManager.AppSettings["PythonExe"];
            start.Arguments = string.Format("\"{0}\" --C={1}", ConfigurationManager.AppSettings["PythonScripts"], ftaProjectId.ToString());
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd();
                    if (result.Contains("the program is completed"))
                    {
                        return null;
                    }
                    else
                    {
                        return result;
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