using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Dxc.Shq.WebApi.Models
{
    public class ProjectFolderTemplate : DataBase
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public string Path { get; set; }

        public virtual List<ProjectFileTemplate> ProjectFilesTemplates { get; set; } = new List<ProjectFileTemplate>();
    }
}