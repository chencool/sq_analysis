using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Dxc.Shq.WebApi.Models
{
    public class FTANodeProperties
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("FTAProject")]
        public Guid FTAProjectId { get; set; }
        public FTAProject FTAProject { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// 失效概率Q
        /// </summary>
        public double FailureRateQ { get; set; }

        /// <summary>
        /// 失效率 lambda
        /// </summary>
        public double InvalidRate { get; set; }
        public bool InvalidRateValueIsModifiedByUser { get; set; }

        /// <summary>
        /// 故障时间
        /// </summary>
        public double FailureTime { get; set; }

        /// <summary>
        /// 单点故障诊断覆盖率
        /// </summary>
        public double DCrf { get; set; }

        /// <summary>
        /// 潜伏故障诊断覆盖率
        /// </summary>
        public double DClf { get; set; }

        /// <summary>
        /// 参考失效率 q
        /// </summary>
        public double ReferenceFailureRateq { get; set; }
    }
}