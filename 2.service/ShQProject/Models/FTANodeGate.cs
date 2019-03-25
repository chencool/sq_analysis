using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Dxc.Shq.WebApi.Models
{
    public class FTANodeGate
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 0)]
        public int Id { get; set; }
        public string NodeGateName { get; set; }

        [ForeignKey("FTANodeGateType")]
        public int FTANodeGateTypeId { get; set; }
        public FTANodeGateType FTANodeGateType { get; set; }

        [Key, ForeignKey("FTAProject")]
        [Column(Order = 1)]
        public Guid FTAProjectId { get; set; }

        public FTAProject FTAProject { get; set; }
    }
}