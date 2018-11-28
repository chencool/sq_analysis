using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Dxc.Shq.WebApi.Models
{
    public class ProjectFileTemplate : DataBase
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public string Path { get; set; }

        [ForeignKey("FolderTemplate")]
        public Guid FolderTemplateId { get; set; }
        public ProjectFolderTemplate FolderTemplate { get; set; }
    }
}