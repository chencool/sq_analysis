using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Dxc.Shq.WebApi.Models
{
    public class FMEATree : DataBase
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("FMEAProject")]
        public Guid FMEAProjectId { get; set; }
        public FMEAProject FMEAProject { get; set; }

        public string Content { get; set; }
    }
}