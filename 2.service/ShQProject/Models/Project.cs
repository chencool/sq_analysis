namespace Dxc.Shq.WebApi.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Project: DataBase
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(255)]
        public string ProjectName { get; set; }
        public string Description { get; set; }

        public string Tag { get; set; }

        [Required]
        public string Type { get; set; }

        public virtual List<ProjectShqUsers> ProjectsAccess { get; set; } = new List<ProjectShqUsers>();
    }
}