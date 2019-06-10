using Dxc.Shq.WebApi.Core;
using Dxc.Shq.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Dxc.Shq.WebApi.ViewModels
{
    public class WorkProjectViewModel : ProjectViewModel
    {
        public WorkProjectViewModel() : base()
        {
        }

        public WorkProjectViewModel(WorkProject workProject, ShqContext db)
            : base(workProject.Project, db)
        {
            WorkProjectTemplateId = workProject.WorkProjectTemplateId;
            Level = workProject.Level;
            FilesToCopyNum = workProject.FilesToCopyNum;
            FilesCopiedNum = workProject.FilesCopiedNum;
            var ftas = workProject.FTAProjects.ToList();
            foreach (var f in ftas)
            {
                FTAProjects.Add(new ProjectViewModel(db.Projects.FirstOrDefault(item => item.Id == f.ProjectId), db));
            }

            var fmeas = workProject.FMEAProjects.ToList();
            foreach (var f in fmeas)
            {
                FMEAProjects.Add(new ProjectViewModel(db.Projects.FirstOrDefault(item => item.Id == f.ProjectId), db));
            }
        }

        public Guid WorkProjectTemplateId { get; set; }

        public int Level { get; set; }

        public int FilesToCopyNum { get; set; }
        public int FilesCopiedNum { get; set; }

        public ProjectFolderViewModel ProjectFiles { get; set; }

        public List<ProjectViewModel> FTAProjects { get; set; } = new List<ProjectViewModel>();

        public List<ProjectViewModel> FMEAProjects { get; set; } = new List<ProjectViewModel>();
    }
}