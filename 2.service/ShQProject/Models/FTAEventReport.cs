using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Dxc.Shq.WebApi.Models
{
    public class FTAEventReport
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("FTAProject")]
        public Guid FTAProjectId { get; set; }
        public FTAProject FTAProject { get; set; }

        [ForeignKey("FTAFailureType")]
        public int FTAFailureTypeId { get; set; }
        public FTAFailureType FTAFailureType { get; set; }

        [ForeignKey("FTAEventType")]
        public int FTAEventTypeId { get; set; }
        public FTAEventType FTAEventType { get; set; }

        /// <summary>
        /// 失效率
        /// </summary>
        public double FailureRateQ { get; set; }

        /// <summary>
        /// 故障率
        /// </summary>
        public double InvalidRate { get; set; }

    }
}