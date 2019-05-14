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

        [ForeignKey("FTAFailureType")]
        public int? FTAFailureTypeId { get; set; }
        public FTAFailureType FTAFailureType { get; set; }

        [ForeignKey("FTAEventType")]
        public int? FTAEventTypeId { get; set; }
        public FTAEventType FTAEventType { get; set; }

        public int FTANodeId { get; set; }
        public Guid FTAProjectId { get; set; }

        public double? EventValue { get; set; }

        /// <summary>
        /// 0 means user input
        /// 1 mean calc result
        /// </summary>
        public int? EventValueType { get; set; }

    }
}