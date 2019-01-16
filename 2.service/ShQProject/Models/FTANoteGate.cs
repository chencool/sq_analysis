using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Dxc.Shq.WebApi.Models
{
    public class FTANoteGate
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [ForeignKey("FTANoteGateType")]
        public int FTANoteGateTypeId { get; set; }
        public FTANoteGateType FTANoteGateType { get; set; }

        [ForeignKey("FTAProject")]
        public Guid FTAProjectId { get; set; }

        public FTAProject FTAProject { get; set; }
    }
}