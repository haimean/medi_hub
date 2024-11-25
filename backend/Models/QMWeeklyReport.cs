namespace DashboardApi.Models
{
    public class QMWeeklyReport
    {
        public Guid? ID { get; set; }

        // common
        public DateTime WeekDateForm { get; set; }
        public DateTime WeekDateTo { get; set; }
        public string ProjectID { get; set; }
        public string Project { get; set; }

        //Handover unit
        public double? TargetUnitHandover { get; set; }
        public double? CumTargetUnitHandover { get; set; }
        public double? ActualUnitHandover { get; set; }
        public double? CumActualUnitHandover { get; set; }
        public double? CumBCAComplete { get; set; }
        public double? CumBCAWaterTestCompleted { get; set; }

        //BCA Inspection
        public double? TargetPreparedRealy { get; set; }
        public double? CumTargetPreparedRealy { get; set; }
        public double? ActualPreparedRealy { get; set; }
        public double? CumActualPreparedRealy { get; set; }

        //BCA Assessment
        public double? TargetBCAInspected { get; set; }
        public double? CumTargetBCAInspected { get; set; }
        public double? ActualBCAInspected { get; set; }
        public double? CumActualBCAInspected { get; set; }

        public double? BCAInspected { get; set; }
        public double? SpareUnitsBCAInspected { get; set; }

        public double? BCAAssmentScore { get; set; }
        public double? CumBCAAssmentScore { get; set; }

        public double? AvgBCAAssessmentScore { get; set; }
        public double? AvgCumBCAAssessmentScore { get; set; }

        // CSO (CLD)
        public double? TargetCSO { get; set; }
        public double? CumTargetCSO { get; set; }
        public double? ActualCSO { get; set; }
        public double? CumActualCSO { get; set; }

        //URC(CDL)
        public double? TargetURC { get; set; }
        public double? CumTargetURC { get; set; }
        public double? ActualURC { get; set; }
        public double? CumActualURC { get; set; }

        public string NameOf { get; set; }
        public double? total { get; set; }

        public string? Remark { get; set; }
    }
}
