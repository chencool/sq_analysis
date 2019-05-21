using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Dxc.Shq.WebApi.ViewModels
{
    public class FTATreeReportP1RowViewModel
    {
        public string NodeId { get; set; }

        public string NodeName { get; set; }

        public string EventName { get; set; }

        public double singlePointFault { get; set; }
        public double residualFaults { get; set; }
        public double latentFault { get; set; }
        public double detectedDualPointFault { get; set; }
        public double safeFault { get; set; }
    }

    public class FTATreeReportP2RowViewModel
    {
        public string FailureName { get; set; }

        public double InvalidValue { get; set; }

        public string NodeId { get; set; }

        public string NodeName { get; set; }

        public string EventName { get; set; }

        public double FailureValue { get; set; }
    }

    public class FTATreeReportViewModel
    {
        public double SinglePointFaultMeasure { get; set; }
        public double LatentFaultMeasure { get; set; }
        public double RandomFaultMeasure { get; set; }

        public string SinglePointEventIds;
        public string SinglePointEventNames;
        public string DualPointEventIds;
        public string DualPointEventNames;
        public string SafeEventIds;
        public string SafeEventNames;
        public string TopEventIds;
        public string TopEventNames;
        public string BaseEventIds;
        public string BaseEventNames;

        public string MinimalCutSetIds;
        public string MinimalCutSetNames;

        public List<FTATreeReportP1RowViewModel> TableP1 { get; set; } = new List<FTATreeReportP1RowViewModel>();
        public List<FTATreeReportP2RowViewModel> TableP2 { get; set; } = new List<FTATreeReportP2RowViewModel>();
    }
}