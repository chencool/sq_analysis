using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Dxc.Shq.WebApi.ViewModels
{
    public class FTATreeReportP1RowViewModel
    {
        public string NodeId { get; set; }

        public string EventName { get; set; }

        public double SPE { get; set; }
        public double DPE { get; set; }
        public double SE { get; set; }
        public double NFP { get; set; }
        public double TE { get; set; }
    }

    public class FTATreeReportP2RowViewModel
    {
        public string FailureName { get; set; }

        public double InvalidValue { get; set; }

        public string NodeId { get; set; }

        public string EventName { get; set; }

        public double FailureValue { get; set; }
    }

    public class FTATreeReportViewModel
    {
        public double SPFM { get; set; }
        public double LFM { get; set; }
        public double PMHF { get; set; }

        public string AnalysisNodeIds;

        public List<FTATreeReportP1RowViewModel> TableP1 { get; set; } = new List<FTATreeReportP1RowViewModel>();
        public List<FTATreeReportP2RowViewModel> TableP2 { get; set; } = new List<FTATreeReportP2RowViewModel>();
    }
}