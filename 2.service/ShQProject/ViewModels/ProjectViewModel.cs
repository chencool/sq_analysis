using Dxc.Shq.WebApi.Core;
using Dxc.Shq.WebApi.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Dxc.Shq.WebApi.ViewModels
{
    public class UsersPrivileges
    {
        public string RealName { get; set; }

        [Required]
        public string EmailAddress { get; set; }

        [Required]
        public int Privilege { get; set; }
    }

    public class ProjectRequestViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public string Tag { get; set; }

        [Required]
        public string Type { get; set; }

        public List<UsersPrivileges> UsersPrivileges = new List<UsersPrivileges>();

        public ProjectRequestViewModel()
        {

        }

        public Project ToProject()
        {
            Project p = new Project();
            p.Id = Id;
            p.ProjectName = Name;
            p.Description = Description;
            p.Tag = Tag;
            p.Type = Type;
            //p.CreatedBy = CreatedBy.ToShqUser();
            //p.CreatedTime = DateTime.Parse(CreatedTime);

            return p;
        }
    }

    public class ProjectViewModel: ProjectRequestViewModel
    {
        public int Privilege { get; set; }

        public ShqUserRequestViewModel CreatedBy { get; set; }

        public string CreatedTime { get; set; }

        public ShqUserRequestViewModel LastModifiedBy { get; set; }
        public string LastModfiedTime { get; set; }

        public ProjectViewModel():base()
        {
        }

        public ProjectViewModel(Project project, ShqContext db)
        {
            if (project == null)
            {
                return;
            }

            Id = project.Id;
            Name = project.ProjectName;
            Description = project.Description;
            Type = project.Type;
            Tag = project.Tag;

            var ps = db.ProjectShqUsers.Include("ShqUser").Where(item => item.ProjectId == project.Id).ToList();
            foreach(var item in ps)
            {
                this.UsersPrivileges.Add(new ViewModels.UsersPrivileges() { RealName = item.ShqUser.RealName, EmailAddress = item.ShqUser.EmailAddress, Privilege = item.Privilege });
            }

            CreatedBy = new ShqUserRequestViewModel(db.ShqUsers.Where(u => u.IdentityUser.Id == project.CreatedById).FirstOrDefault(), db);
            CreatedTime = project.CreatedTime.ToString();

            LastModifiedBy = new ShqUserRequestViewModel(db.ShqUsers.Where(u => u.IdentityUser.Id == project.LastModifiedById).FirstOrDefault(), db);
            LastModfiedTime = project.LastModfiedTime.ToString();
        }

        public static List<ProjectViewModel> ToProjectViewModelList(List<Project> projects, ShqContext db)
        {
            List<ProjectViewModel> result = new List<ProjectViewModel>();
            foreach (var item in projects)
            {
                result.Add(new ProjectViewModel(item, db));
            }

            return result;
        }
    }
}