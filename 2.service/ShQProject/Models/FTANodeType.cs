using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Dxc.Shq.WebApi.Models
{
    public class FTANodeType
    {
        [Key]
        public int Id { get; set; }
        public string Description { get; set; }
    }
}