namespace Dxc.Shq.WebApi.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class FMEAProject : DataBase
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("Project")]
        public Guid ProjectId { get; set; }
        public Project Project { get; set; }

        public virtual List<FMEATree> FMEATrees { get; set; } = new List<FMEATree>();

        public virtual List<WorkProject> WorkProjects { get; set; } = new List<WorkProject>();
    }
}