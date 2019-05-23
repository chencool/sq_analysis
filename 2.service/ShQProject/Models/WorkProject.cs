using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Dxc.Shq.WebApi.Models
{
    public class WorkProject
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("Project")]
        public Guid ProjectId { get; set; }

        public Project Project { get; set; }

        //public virtual List<ProjectFolder> ProjectFolders { get; set; } = new List<ProjectFolder>();

        public virtual List<FTAProject> FTAProjects { get; set; } = new List<FTAProject>();
    }
}