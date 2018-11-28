﻿using System;
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

namespace Dxc.Shq.WebApi.Controllers
{
    public class ProjectsController : ApiController
    {
        private ShqContext db = new ShqContext();

        [HttpGet]
        [Route("api/Projects/All")]
        public IQueryable<ProjectViewModel> GetProjects()
        {
            return ProjectHelper.GetProjectViewModels().AsQueryable();
        }

        [HttpGet]
        [Route("api/Projects")]
        public IQueryable<ProjectViewModel> GetProjects(string type)
        {
            var projects = ProjectHelper.GetProjectViewModels();
            return projects.Where(item => item.Type == type).AsQueryable();
        }

        // query by type

        [HttpGet]
        [Route("api/Projects")]
        [ResponseType(typeof(ProjectViewModel))]
        public async Task<IHttpActionResult> GetProject(Guid id)
        {
            Project project = await db.Projects.FirstOrDefaultAsync(item => item.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            ProjectViewModel pv = new ProjectViewModel(project, db);

            if (ProjectHelper.HasReadAccess(project) == false)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Forbidden, "No Access"));
            }

            pv.Privilege = ShqConstants.AllowProjectRead;
            if (ProjectHelper.HasUpdateAccess(project) == true)
            {
                pv.Privilege = ShqConstants.AllowProjectUpdate;
            }

            return Ok(pv);
        }

        [Route("api/Projects/Update")]
        [HttpPut]
        [ResponseType(typeof(ProjectViewModel))]
        public async Task<IHttpActionResult> UpdateProject(ProjectRequestViewModel project)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Project pro = await db.Projects.FindAsync(project.Id);
            if (pro == null)
            {
                return NotFound();
            }

            if (ProjectHelper.HasUpdateAccess(pro) == false)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Forbidden, "No Access"));
            }

            db.Entry(pro).State = EntityState.Modified;
            pro.Name = project.Name;
            pro.Description = project.Description;
            pro.LastModifiedById = db.ShqUsers.Where(u => u.IdentityUser.UserName == HttpContext.Current.User.Identity.Name).FirstOrDefault().IdentityUserId;
            pro.LastModfiedDate = DateTime.Now;

            await db.SaveChangesAsync();

            ProjectViewModel result = new ProjectViewModel(pro, db);
            result.Privilege = ShqConstants.AllowProjectUpdate;
            return Ok(result);
        }

        [Route("api/Projects/Add")]
        [HttpPost]
        [ResponseType(typeof(ProjectViewModel))]
        public async Task<IHttpActionResult> AddProject(ProjectRequestViewModel projectView)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Project pro = await db.Projects.FindAsync(projectView.Id);
            if (pro != null)
            {
                return Conflict();
            }

            Project project = projectView.ToProject();

            project.CreateById = db.ShqUsers.Where(u => u.IdentityUser.UserName == HttpContext.Current.User.Identity.Name).FirstOrDefault().IdentityUserId;
            project.LastModifiedById = project.CreateById;

            db.Projects.Add(project);

            if (project.Type == "FTAProject")
            {
                db.FTAProjects.Add(new FTAProject() { Id = Guid.NewGuid(), ProjectId = projectView.Id });
            }

            await db.SaveChangesAsync();

            ProjectViewModel result = new ProjectViewModel(project, db);
            result.Privilege = ShqConstants.AllowProjectUpdate;

            return Ok(result);
        }

        [HttpPost]
        [Route("api/Projects/AddOrUpdateAccess")]
        [ResponseType(typeof(ProjectShqUsersViewModel))]
        public async Task<IHttpActionResult> AddOrUpdateProjectAccess(ProjectShqUsersViewModel projectShqUsersViewModel)
        {
            if (!ModelState.IsValid)
                return BadRequest("Not a valid model");

            Project project = await db.Projects.Where(item => item.Id == projectShqUsersViewModel.ProjectId).FirstOrDefaultAsync();
            if (project == null)
            {
                return NotFound();
            }

            var createBy = await db.ShqUsers.Include("IdentityUser").FirstOrDefaultAsync(item => item.IdentityUserId == project.CreateById);
            if (HttpContext.Current.User.Identity.Name != createBy.IdentityUser.UserName
               && HttpContext.Current.User.IsInRole(ShqConstants.AdministratorRole) == false)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Forbidden, "No Access"));
            }

            ShqUser shqUser = await db.ShqUsers.Where(item => item.EmailAddress == projectShqUsersViewModel.EmailAddress).FirstOrDefaultAsync();
            if (shqUser == null)
            {
                return NotFound();
            }

            var ps = shqUser.ProjectsAccess.Where(item => item.ProjectId == projectShqUsersViewModel.ProjectId && item.ShqUserId == shqUser.IdentityUserId).FirstOrDefault();
            if (ps != null)
            {
                ps.Privilege = projectShqUsersViewModel.Privilege;
                ps.LastModifiedById = db.ShqUsers.Where(u => u.IdentityUser.UserName == HttpContext.Current.User.Identity.Name).FirstOrDefault().IdentityUserId;
                ps.LastModfiedDate = DateTime.Now;
            }

            else
            {
                var newAccess = new ProjectShqUsers()
                {
                    ProjectId = projectShqUsersViewModel.ProjectId,
                    ShqUserId = shqUser.IdentityUserId,
                    Privilege = projectShqUsersViewModel.Privilege,
                    CreateById = db.ShqUsers.Where(u => u.IdentityUser.UserName == HttpContext.Current.User.Identity.Name).FirstOrDefault().IdentityUserId
                };
                newAccess.LastModifiedById = newAccess.CreateById;
                shqUser.ProjectsAccess.Add(newAccess);
            }

            await db.SaveChangesAsync();

            return Ok(projectShqUsersViewModel);
        }

        [HttpPost]
        [Route("api/Projects/RemoveAccess")]
        [ResponseType(typeof(ProjectShqUsersViewModel))]
        public async Task<IHttpActionResult> RemoveProjectAccess(ProjectShqUsersViewModel projectShqUsersViewModel)
        {
            if (!ModelState.IsValid)
                return BadRequest("Not a valid model");

            Project project = await db.Projects.Where(item => item.Id == projectShqUsersViewModel.ProjectId).FirstOrDefaultAsync();
            if (project == null)
            {
                return NotFound();
            }

            var createBy = await db.ShqUsers.Include("IdentityUser").FirstOrDefaultAsync(item => item.IdentityUserId == project.CreateById);
            if (HttpContext.Current.User.Identity.Name != createBy.IdentityUser.UserName
               && HttpContext.Current.User.IsInRole(ShqConstants.AdministratorRole) == false)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Forbidden, "No Access"));
            }

            ShqUser shqUser = await db.ShqUsers.Include("ProjectsAccess").Where(item => item.EmailAddress== projectShqUsersViewModel.EmailAddress).FirstOrDefaultAsync();
            if (shqUser == null)
            {
                return NotFound();
            }

            var ps = shqUser.ProjectsAccess.Where(item => item.ProjectId == projectShqUsersViewModel.ProjectId && item.ShqUserId == shqUser.IdentityUserId).FirstOrDefault();
            if (ps != null)
            {
                shqUser.ProjectsAccess.Remove(ps);
                project.LastModifiedById = db.ShqUsers.Where(u => u.IdentityUser.UserName == HttpContext.Current.User.Identity.Name).FirstOrDefault().IdentityUserId;
                project.LastModfiedDate = DateTime.Now;
            }

            else
            {
                return NotFound();
            }

            await db.SaveChangesAsync();

            return Ok(projectShqUsersViewModel);
        }


        [Authorize(Roles = ShqConstants.AdministratorRole)]
        [Route("api/Projects/Delete")]
        [ResponseType(typeof(ProjectViewModel))]
        public async Task<IHttpActionResult> DeleteProject(Guid id)
        {
            Project project = await db.Projects.Include("CreateBy").FirstOrDefaultAsync(item => item.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            var ps = db.ProjectShqUsers.Where(item => item.ProjectId == id);
            db.ProjectShqUsers.RemoveRange(ps);

            var ftas = db.FTAProjects.Where(item => item.ProjectId == id);
            db.FTAProjects.RemoveRange(ftas);

            db.Projects.Remove(project);
            await db.SaveChangesAsync();

            return Ok(new ProjectViewModel(project, db));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ProjectExists(Guid id)
        {
            return db.Projects.Count(e => e.Id == id) > 0;
        }
    }
}