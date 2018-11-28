using Dxc.Shq.WebApi.Models;
using Dxc.Shq.WebApi.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace Dxc.Shq.WebApi.Core
{
    public class ProjectHelper
    {
        public static List<Project> GetProjects()
        {

            using (ShqContext db = new ShqContext())
            {
                if (HttpContext.Current.User.IsInRole(ShqConstants.AdministratorRole) == true)
                {
                    return db.Projects.ToList();
                }

                var shqUser = db.ShqUsers.Where(u => u.IdentityUser.UserName == HttpContext.Current.User.Identity.Name).FirstOrDefault();
                List<Project> projects = new List<Project>(db.Projects.Where(item => item.CreateById == shqUser.IdentityUserId).ToList());
                foreach (var item in shqUser.ProjectsAccess)
                {
                    if (item.Privilege == ShqConstants.AllowProjectRead || item.Privilege == ShqConstants.AllowProjectUpdate)
                    {
                        projects.Add(db.Projects.FirstOrDefault(p => p.Id == item.ProjectId));
                    }
                }

                return projects;
            }
        }

        public static List<ProjectViewModel> GetProjectViewModels()
        {
            List<ProjectViewModel> pvs = new List<ProjectViewModel>();

            using (ShqContext db = new ShqContext())
            {
                if (HttpContext.Current.User.IsInRole(ShqConstants.AdministratorRole) == true)
                {
                    var list = db.Projects.ToList();
                    foreach (var item in list)
                    {
                        pvs.Add(new ProjectViewModel(item, db) { Privilege = ShqConstants.AllowProjectUpdate });
                    }
                }
                else
                {
                    var shqUser = db.ShqUsers.Where(u => u.IdentityUser.UserName == HttpContext.Current.User.Identity.Name).FirstOrDefault();
                    List<Project> projects = new List<Project>(db.Projects.Where(item => item.CreateById == shqUser.IdentityUserId).ToList());
                    foreach (var item in projects)
                    {
                        ProjectViewModel pv = new ProjectViewModel(item, db);
                        pv.Privilege = ShqConstants.AllowProjectUpdate;
                        pvs.Add(pv);
                    }
                    foreach (var item in shqUser.ProjectsAccess)
                    {
                        if (item.Privilege == ShqConstants.AllowProjectRead || item.Privilege == ShqConstants.AllowProjectUpdate)
                        {
                            pvs.Add(new ProjectViewModel(db.Projects.FirstOrDefault(p => p.Id == item.ProjectId), db) { Privilege = item.Privilege });
                        }
                    }
                }

                return pvs;
            }
        }

        public static bool HasReadAccess(Project project)
        {
            if (HttpContext.Current.User.IsInRole(ShqConstants.AdministratorRole) == true)
            {
                return true;
            }



            using (ShqContext db = new ShqContext())
            {
                var shqUser = db.ShqUsers.Where(u => u.IdentityUser.UserName == HttpContext.Current.User.Identity.Name).FirstOrDefault();
                if (project.CreateById == shqUser.IdentityUserId)
                {
                    return true;
                }

                var ps = project.ProjectsAccess.Where(item => item.ProjectId == project.Id && item.ShqUserId == shqUser.IdentityUserId).FirstOrDefault();

                if (ps == null)
                {
                    return false;
                }
                else
                {
                    return ps.Privilege == ShqConstants.AllowProjectRead || ps.Privilege == ShqConstants.AllowProjectUpdate;
                }
            }
        }

        public static bool HasUpdateAccess(Project project)
        {
            if (HttpContext.Current.User.IsInRole(ShqConstants.AdministratorRole) == true)
            {
                return true;
            }

            using (ShqContext db = new ShqContext())
            {
                var shqUser = db.ShqUsers.Where(u => u.IdentityUser.UserName == HttpContext.Current.User.Identity.Name).FirstOrDefault();
                if (project.CreateById == shqUser.IdentityUserId)
                {
                    return true;
                }

                var ps = project.ProjectsAccess.Where(item => item.ProjectId == project.Id && item.ShqUserId == shqUser.IdentityUserId).FirstOrDefault();

                if (ps == null)
                {
                    return false;
                }
                else
                {
                    {
                        return ps.Privilege == ShqConstants.AllowProjectUpdate;
                    }
                }
            }
        }
    }
}