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
    public class FMEAProjectsController : ApiController
    {
        private ShqContext db = new ShqContext();

        [HttpGet]
        [Route("api/FMEAProjects/GetTree")]
        // POST: api/FMEAProjects
        [ResponseType(typeof(FTATreeRequestViewModel))]
        public IHttpActionResult GetFMEAProjectTree(Guid projectId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var docs = db.FMEAProjects.Include("Project").Where(item => item.ProjectId == projectId).FirstOrDefault();
            if (docs == null)
            {
                return NotFound();
            }

            if (ProjectHelper.HasReadAccess(docs.Project) == false)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Forbidden, "No Access"));
            }

            var tr = docs.FMEATrees.OrderByDescending(item => item.CreatedTime).FirstOrDefault();
            if (tr != null)
            {
                return Ok(new FTATreeRequestViewModel() { Id = tr.Id, Content = tr.Content, ProjectId = docs.ProjectId });
            }
            else
            {
                return Ok(new FTATreeRequestViewModel() { ProjectId = docs.ProjectId });
            }
        }

        [HttpPost]
        [Route("api/FMEAProjects/AddTree")]
        // POST: api/FMEAProjects
        [ResponseType(typeof(FTATreeRequestViewModel))]
        public async Task<IHttpActionResult> AddFMEAProjectTree(FTATreeRequestViewModel tree)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var docs = db.FMEAProjects.Include("Project").Where(item => item.ProjectId == tree.ProjectId).FirstOrDefault();
            if (docs == null)
            {
                return NotFound();
            }

            if (ProjectHelper.HasUpdateAccess(docs.Project) == false)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Forbidden, "No Access"));
            }

            ShqUser shqUser = await db.ShqUsers.Where(item => item.IdentityUser.UserName == HttpContext.Current.User.Identity.Name).Include("IdentityUser").FirstOrDefaultAsync();
            var tr = docs.FMEATrees.Where(item => item.Id == tree.Id).FirstOrDefault();
            if (tr == null)
            {
                FMEATree FMEATree = new FMEATree() { Id = tree.Id, FMEAProjectId = docs.Id, FMEAProject = docs, Content = tree.Content, CreatedById = shqUser.IdentityUserId, CreatedTime = DateTime.Now, LastModifiedById = shqUser.IdentityUserId, LastModfiedTime = DateTime.Now };
                docs.FMEATrees.Add(FMEATree);
                await db.SaveChangesAsync();

                return Ok(new FTATreeRequestViewModel() { Id = FMEATree.Id, Content = FMEATree.Content, ProjectId = docs.ProjectId });
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

        private bool FMEADocumentExists(Guid id)
        {
            return db.FMEAProjects.Count(e => e.Id == id) > 0;
        }
    }
}