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

        public int Level { get; set; }

        /// <summary>
        /// 1 means deleted
        /// </summary>
        public int Status { get; set; }

        [Required]
        public bool IsFolder { get; set; }

        [Required]
        public string Path { get; set; }
    }
}