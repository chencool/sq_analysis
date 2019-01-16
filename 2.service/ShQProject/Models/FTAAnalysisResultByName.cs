using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Dxc.Shq.WebApi.Models
{
    public class FTAAnalysisResultByName
    {
        public int Id { get; set; }

        public int BranchId { get; set; }
        public string FTANodeName{ get; set; }

        [ForeignKey("FTAProject")]
        public Guid FTAProjectId { get; set; }
    }
}