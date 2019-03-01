using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
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
                FTATree ftaTree = new FTATree() { Id = tree.Id, FTAProjectId = docs.Id, FTAProject = docs, Content = tree.Content, CreatedById = shqUser.IdentityUserId, CreatedTime = DateTime.Now, LastModifiedById = shqUser.IdentityUserId, LastModfiedTime = DateTime.Now };
                docs.FTATrees.Add(ftaTree);
                await db.SaveChangesAsync();

                if (tree.Analysis == true)
                {
                    Analyze(docs, JsonConvert.DeserializeObject<JsonFTATree>(tree.Content));
                }

                return Ok(new FTATreeViewModel(ftaTree, db));
            }
            else
            {
                return Conflict();
            }
        }

        private JsonFTATree Analyze(FTAProject docs, JsonFTATree tree)
        {
            db.FTANodes.RemoveRange(docs.FTANodes);
            db.FTANodeProperties.RemoveRange(docs.FTANodeProperties);
            db.FTANoteGates.RemoveRange(docs.FTANoteGates);

            db.SaveChanges();

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
                    fn.Index = node.Mode.Index;
                    fn.EventId = node.Id;
                    fn.Name = node.Name;
                    fn.Shape = node.Mode.Shape;
                    fn.Size = node.Mode.Size;
                    fn.X = node.Mode.X;
                    fn.Y = node.Mode.Y;
                    fn.ParentId = -1;
                    fn.FTANoteGateId = -1;

                    switch (node.ItemType.ToLower())
                    {
                        case "square":
                            fn.FTANoteType = db.FTANoteTypes.FirstOrDefault(item => item.Id == ShqConstants.FTANoteTypeRoot);
                            break;
                        case "rectangle":
                            fn.FTANoteType = db.FTANoteTypes.FirstOrDefault(item => item.Id == ShqConstants.FTANoteTypeBrand);
                            break;
                        case "round":
                            fn.FTANoteType = db.FTANoteTypes.FirstOrDefault(item => item.Id == ShqConstants.FTANoteTypeLeaf);
                            break;
                    }
                    fn.FTANoteTypeId = fn.FTANoteType.Id;

                    docs.FTANodes.Add(fn);

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

                        docs.FTANodeProperties.Add(fp);
                    }

                    // 获取 门 或父亲节点
                    var edge = tree.FTAEdges.FirstOrDefault(item => item.Target == node.Id);
                    if (edge != null)
                    {
                        var parentNode = tree.FTANodes.FirstOrDefault(item => item.Id == edge.Source);
                        if ("orgate,andgate,nongate".Contains(parentNode.ItemType.ToLower()))
                        {
                            FTANoteGate gate = new FTANoteGate(); //to do
                            gate.Id = gateId;
                            gate.Name = parentNode.Name;
                            gate.FTAProjectId = docs.Id;
                            gate.FTAProject = docs;
                            switch (parentNode.ItemType.ToLower())
                            {
                                case "orgate":
                                    gate.FTANoteGateType = db.FTANoteGateTypes.FirstOrDefault(item => item.Id == ShqConstants.FTANoteGateTypeOr);
                                    break;
                                case "andgate":
                                    gate.FTANoteGateType = db.FTANoteGateTypes.FirstOrDefault(item => item.Id == ShqConstants.FTANoteGateTypeAnd);
                                    break;
                                case "nongate":
                                    gate.FTANoteGateType = db.FTANoteGateTypes.FirstOrDefault(item => item.Id == ShqConstants.FTANoteGateTypeXor);
                                    break;
                            }
                            gate.FTANoteGateTypeId = gate.FTANoteGateType.Id;
                            gateId++;
                            fn.FTANoteGateId = gate.Id;

                            var parentEdge = tree.FTAEdges.FirstOrDefault(item => item.Target == edge.Source);
                            var grandParentNode = FTANodesList.FirstOrDefault(item => item.Id == parentEdge.Source);

                            fn.ParentId = FTANodesList.IndexOf(grandParentNode) + 1;

                            docs.FTANoteGates.Add(gate);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, ex.Message + ex.StackTrace));
            }

            db.SaveChanges();

            return tree;
        }

        [HttpPost]
        [Route("api/FTAProjects/test")]
        [ResponseType(typeof(JsonFTATree))]
        public async Task<IHttpActionResult> TestFormat(JsonFTATree tree)
        {

            string content = JsonConvert.SerializeObject(tree);
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

            Analyze(docs, tree);

            await db.SaveChangesAsync();

            return Ok(tree);
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