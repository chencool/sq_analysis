using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Dxc.Shq.WebApi.Models
{
    public class WorkProject : DataBase
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("Project")]
        public Guid ProjectId { get; set; }

        public Project Project { get; set; }

        [ForeignKey("WorkProjectTemplate")]
        public Guid? WorkProjectTemplateId { get; set; }

        public WorkProjectTemplate WorkProjectTemplate { get; set; }

        public int Level { get; set; }

        public int FilesToCopyNum { get; set; }
        public int FilesCopiedNum { get; set; }

        public virtual List<FTAProject> FTAProjects { get; set; } = new List<FTAProject>();

        public virtual List<FMEAProject> FMEAProjects { get; set; } = new List<FMEAProject>();
    }
}