namespace DashboardApi.Dtos.QMWeekly
{
    public class TargetQMWeekly
    {
        public string? ProjectId { get; set; }
        public DateTime? WeekDateForm { get; set; }
        public DateTime? WeekDateTo { get; set; }
        public double? Unit { get; set; }
        public double? CumUnit { get; set; }
    }
}
