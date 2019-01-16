using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Dxc.Shq.WebApi.Models
{
    public class FTANoteGateType
    {
        [Key]
        public int Id { get; set; }
        public string Description { get; set; }
    }
}