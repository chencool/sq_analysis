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

            var tr = docs.FTATrees.OrderByDescending(item => item.CreateTime).FirstOrDefault();
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
                FTATree ftaTree = new FTATree() { Id = tree.Id, FTAProjectId = docs.Id, FTAProject = docs, Content = tree.Content, CreateById = shqUser.IdentityUserId, CreateTime = DateTime.Now, LastModifiedById = shqUser.IdentityUserId, LastModfiedDate = DateTime.Now };
                docs.FTATrees.Add(ftaTree);
                await db.SaveChangesAsync();

                return Ok(new FTATreeViewModel(ftaTree, db));
            }
            else
            {
                return Conflict();
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