namespace Dxc.Shq.WebApi.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class FTAProject: DataBase
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("Project")]
        public Guid ProjectId { get; set; }
        public Project Project { get; set; }
       
        public virtual List<FTANode> FTANodes { get; set; } = new List<FTANode>();

        public virtual List<FTANodeProperties> FTANodeProperties { get; set; } = new List<FTANodeProperties>();

        public virtual List<FTANoteGate> FTANoteGates { get; set; } = new List<FTANoteGate>();

        public virtual List<FTATree> FTATrees { get; set; } = new List<FTATree>();

        public virtual List<WorkProject> WorkProjects { get; set; } = new List<WorkProject>();
    }
}