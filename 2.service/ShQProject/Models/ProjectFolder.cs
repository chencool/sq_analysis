using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Dxc.Shq.WebApi.Models
{
    public class ProjectFolder : DataBase
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Path { get; set; }

        [ForeignKey("Project")]
        public Guid ProjectId { get; set; }
        public Project Project { get; set; }

        public virtual List<ProjectFile> ProjectFiles { get; set; } = new List<ProjectFile>();

        [ForeignKey("FolderTemplate")]
        public Guid FolderTemplateId { get; set; }
        public ProjectFolderTemplate FolderTemplate { get; set; }
    }
}