using Dxc.Shq.WebApi.Core;
using Dxc.Shq.WebApi.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Dxc.Shq.WebApi.ViewModels
{
    public class ProjectRequestViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public string Type { get; set; }

        public ProjectRequestViewModel()
        {

        }

        public Project ToProject()
        {
            Project p = new Project();
            p.Id = Id;
            p.Name = Name;
            p.Description = Description;
            p.Type = Type;
            //p.CreateBy = CreateBy.ToShqUser();
            //p.CreateTime = DateTime.Parse(CreateTime);

            return p;
        }
    }

    public class ProjectViewModel: ProjectRequestViewModel
    {
        public int Privilege { get; set; }

        public ShqUserRequestViewModel CreateBy { get; set; }

        public string CreateTime { get; set; }

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
            Name = project.Name;
            Description = project.Description;
            Type = project.Type;

            CreateBy = new ShqUserRequestViewModel(db.ShqUsers.Where(u => u.IdentityUser.Id == project.CreateById).FirstOrDefault(), db);
            CreateTime = project.CreateTime.ToString();
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