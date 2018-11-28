using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Dxc.Shq.WebApi.Models
{
    public class ProjectFile : DataBase
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public string Path { get; set; }

        [ForeignKey("Folder")]
        public Guid FolderId { get; set; }

        public ProjectFolder Folder { get; set; }

        [ForeignKey("FileTemplate")]
        public Guid FileTemplateId { get; set; }

        public ProjectFileTemplate FileTemplate { get; set; }
    }
}