using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Dxc.Shq.WebApi.Models
{
    public class WorkProjectTemplate : DataBase
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        public string Description { get; set; }

        public string Path { get; set; }

        public virtual List<ProjectFile> ProjectFiles { get; set; } = new List<ProjectFile>();
    }
}