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

                return Ok(new FTATreeViewModel(ftaTree, db));
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

            docs.FTANodeProperties.Clear();
            docs.FTANodes.Clear();
            docs.FTANoteGates.Clear();

            int i = 1;
            int gateId = 1;
            while (tree.FTANodes != null  && i <= tree.FTANodes.Count)
            {
                var node = tree.FTANodes[i-1];
                i++;
                if ("square,rectangle,round".Contains(node.ItemType.ToLower()))
                {// 获取节点
                    FTANode fn = new FTANode();
                    fn.Id = i;
                    fn.FTAProjectId = docs.Id;
                    fn.FTAProject = docs;
                    fn.Index = node.Mode.Index;
                    fn.EventId = node.Id;
                    fn.Name = node.Name;
                    fn.Shape = node.Mode.Shape;
                    fn.Size = node.Mode.Size;
                    fn.X = node.Mode.X;
                    fn.Y = node.Mode.Y;

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
                            gate.Name = parentNode.ItemType;
                            gate.FTAProjectId = docs.Id;
                            gate.FTAProject = docs;
                            gate.FTANoteGateType = db.FTANoteGateTypes.FirstOrDefault(item => item.Description.Contains(parentNode.ItemType));
                            gate.FTANoteGateTypeId = gate.FTANoteGateType.Id;
                            gateId++;
                            fn.FTANoteGateId = gate.Id;

                            var parentEdge = tree.FTAEdges.FirstOrDefault(item => item.Target == edge.Source);
                            var grandParentNode = tree.FTANodes.FirstOrDefault(item => item.Id == parentEdge.Source);

                            fn.ParentId = tree.FTANodes.IndexOf(grandParentNode) + 1;

                            docs.FTANoteGates.Add(gate);
                        }
                    }

                }
            }

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