using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Results;
using Dxc.Shq.WebApi.Core;
using Dxc.Shq.WebApi.Models;
using Dxc.Shq.WebApi.ViewModels;

namespace Dxc.Shq.WebApi.Controllers
{
    public class WorkProjectsController : ApiController
    {
        private ShqContext db = new ShqContext();

        [HttpGet]
        [Route("api/WorkProjects/GetWorkProject")]
        [ResponseType(typeof(WorkProjectViewModel))]
        public async Task<IHttpActionResult> GetWorkProject(Guid projectId)
        {
            WorkProject workProject = await db.WorkProjects.Include("Project").FirstOrDefaultAsync(item => item.ProjectId == projectId);
            if (workProject == null)
            {
                return NotFound();
            }

            if (ProjectHelper.HasReadAccess(workProject.Project) == false)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Forbidden, "No Access"));
            }
            var model = new WorkProjectViewModel(workProject, db);
            ProjectFilesController pfc = new ProjectFilesController();
            var obj =(OkNegotiatedContentResult<ProjectFolderViewModel>)pfc.GetProjectFiles(projectId,"Root", System.IO.SearchOption.AllDirectories).Result;
            model.ProjectFiles = obj.Content;
            return Ok(model);
        }

        /// <summary>
        /// link a fta or fmea project to a work project
        /// </summary>
        /// <param name="linkedProjectId">the fta or fmea project id</param>
        /// <param name="workProjectId">the work project id</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/WorkProjects/AddLinkedProject")]
        [ResponseType(typeof(WorkProject))]
        public async Task<IHttpActionResult> AddLinkedProject(Guid linkedProjectId, Guid workProjectId)
        {
            WorkProject workProject = await db.WorkProjects.Include("Project").FirstOrDefaultAsync(item => item.ProjectId == workProjectId);
            if (workProject == null)
            {
                return NotFound();
            }

            if (ProjectHelper.HasUpdateAccess(workProject.Project) == false)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Forbidden, "No Access"));
            }

            Project project = await db.Projects.FirstOrDefaultAsync(item => item.Id == linkedProjectId);
            if (project == null)
            {
                return NotFound();
            }
            if (ProjectHelper.HasUpdateAccess(project) == false)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Forbidden, "No Access"));
            }

            var fta = db.FTAProjects.FirstOrDefault(item => item.ProjectId == linkedProjectId);
            if(fta != null)
            {
                if(workProject.FTAProjects.FirstOrDefault(item => item.ProjectId == linkedProjectId)==null)
                {
                    workProject.FTAProjects.Add(fta);
                }
            }
            else
            {
                var fmea = db.FMEAProjects.FirstOrDefault(item => item.ProjectId == linkedProjectId);
                if(fmea != null)
                {
                    if (workProject.FMEAProjects.FirstOrDefault(item => item.ProjectId == linkedProjectId) == null)
                    {
                        workProject.FMEAProjects.Add(fmea);
                    }
                }
            }

            db.SaveChanges();

            var model = new WorkProjectViewModel(workProject, db);
            ProjectFilesController pfc = new ProjectFilesController();
            var obj = (OkNegotiatedContentResult<ProjectFolderViewModel>)pfc.GetProjectFiles(workProjectId, "Root", System.IO.SearchOption.AllDirectories).Result;
            model.ProjectFiles = obj.Content;
            return Ok(model);
        }

        /// <summary>
        /// unlink a fta or fmea project from a work project
        /// </summary>
        /// <param name="linkedProjectId">the fta or fmea project id</param>
        /// <param name="workProjectId">the work project id</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/WorkProjects/RemoveLinkedProject")]
        [ResponseType(typeof(WorkProject))]
        public async Task<IHttpActionResult> RemoveLinkedProject(Guid linkedProjectId, Guid workProjectId)
        {
            WorkProject workProject = await db.WorkProjects.Include("Project").FirstOrDefaultAsync(item => item.ProjectId == workProjectId);
            if (workProject == null)
            {
                return NotFound();
            }

            if (ProjectHelper.HasUpdateAccess(workProject.Project) == false)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Forbidden, "No Access"));
            }

            Project project = await db.Projects.FirstOrDefaultAsync(item => item.Id == linkedProjectId);
            if (project == null)
            {
                return NotFound();
            }
            if (ProjectHelper.HasUpdateAccess(project) == false)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Forbidden, "No Access"));
            }

            var fta = workProject.FTAProjects.FirstOrDefault(item => item.ProjectId == linkedProjectId);
            if (fta != null)
            {
                workProject.FTAProjects.Remove(fta);
            }
            else
            {
                var fmea = workProject.FMEAProjects.FirstOrDefault(item => item.ProjectId == linkedProjectId);
                if (fmea != null)
                {
                    workProject.FMEAProjects.Remove(fmea);
                }
            }

            db.SaveChanges();

            var model = new WorkProjectViewModel(workProject, db);
            ProjectFilesController pfc = new ProjectFilesController();
            var obj = (OkNegotiatedContentResult<ProjectFolderViewModel>)pfc.GetProjectFiles(workProjectId, "Root", System.IO.SearchOption.AllDirectories).Result;
            model.ProjectFiles = obj.Content;
            return Ok(model);
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