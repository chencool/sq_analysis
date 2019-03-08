using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Dxc.Shq.WebApi.Models
{
    public class FTANodeEventReport
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("FTANode")]
        public int FTANodeId { get; set; }
        public FTANode FTANode { get; set; }

        [ForeignKey("FTAProject")]
        public Guid FTAProjectId { get; set; }
        public FTAProject FTAProject { get; set; }

        [ForeignKey("FTAFailureType")]
        public int FTAFailureTypeId { get; set; }
        public FTAFailureType FTAFailureType { get; set; }

        [ForeignKey("FTAEventType")]
        public int FTAEventTypeId { get; set; }
        public FTAEventType FTAEventType { get; set; }

        public double Value { get; set; }

        /// <summary>
        /// 0 means user input
        /// 1 mean calc result
        /// </summary>
        public int ValueType { get; set; }

    }
}