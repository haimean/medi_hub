using DashboardApi.Models;

namespace DashboardApi.Dtos.QMWeekly
{
    public class CombineQMWeekly
    {
        public QMWeeklyReport? QMWeeklyReport { get; set; } = new QMWeeklyReport();
        public string? Remark { get; set; }

        public string? ProjectId { get; set; }
        public DateTime? WeekDateForm { get; set; }
        public DateTime? WeekDateTo { get; set; }

        public double? UnitHandover { get; set; }
        public double? CumUnitHandover { get; set; }

        public double? BCAInspection { get; set; }
        public double? CumBCAInspection { get; set; }

        public double? BCAInspected { get; set; }
        public double? CumBCAInspected { get; set; }

        public double? BCAAssessmentScore { get; set; }
        public double? AvgBCAAssessmentScore { get; set; }
        public double? CumBCAAssessmentScore { get; set; }
    }
}
