using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Dxc.Shq.WebApi.Core;
using Dxc.Shq.WebApi.Models;
using Dxc.Shq.WebApi.ViewModels;

namespace Dxc.Shq.WebApi.Controllers
{
    public class ProjectFilesController : ApiController
    {
        private ShqContext db = new ShqContext();


        // GET: api/ProjectFiles
        [Route("api/ProjectFiles")]
        public ProjectFolderViewModel GetProjectFolder(string path, int folderType)
        {
            if(path == "root")
            {
                return new ProjectFolderViewModel("", folderType);
            }
            else
            {
                return new ProjectFolderViewModel(path, folderType);
            }
        }

        // GET: api/WorkProjects/5
        [ResponseType(typeof(WorkProject))]
        public async Task<IHttpActionResult> GetWorkProject(Guid id)
        {
            WorkProject workProject = await db.WorkProjects.FindAsync(id);
            if (workProject == null)
            {
                return NotFound();
            }

            return Ok(workProject);
        }

        // POST: api/WorkProjects
        [ResponseType(typeof(WorkProject))]
        public async Task<IHttpActionResult> AddFTA(WorkProject workProject)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.WorkProjects.Add(workProject);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (WorkProjectExists(workProject.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = workProject.Id }, workProject);
        }

        [ResponseType(typeof(WorkProject))]
        public async Task<IHttpActionResult> RemoveFTA(WorkProject workProject)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.WorkProjects.Add(workProject);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (WorkProjectExists(workProject.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = workProject.Id }, workProject);
        }

        [ResponseType(typeof(WorkProject))]
        public async Task<IHttpActionResult> AddFMEA(WorkProject workProject)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.WorkProjects.Add(workProject);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (WorkProjectExists(workProject.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = workProject.Id }, workProject);
        }

        [ResponseType(typeof(WorkProject))]
        public async Task<IHttpActionResult> RemoveFMEA(WorkProject workProject)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.WorkProjects.Add(workProject);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (WorkProjectExists(workProject.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = workProject.Id }, workProject);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool WorkProjectExists(Guid id)
        {
            return db.WorkProjects.Count(e => e.Id == id) > 0;
        }
    }
}