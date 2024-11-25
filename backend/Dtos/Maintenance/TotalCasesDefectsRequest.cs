namespace DashboardApi.Dtos.Maintenance
{
    public class SummaryCasesDefectsRequest
    {
        public string ProjectID { get; set; }
        public string ProjectName { get; set; }
        public int TotalMaintenance { get; set; }
        public string TypeMaintenance { get; set; }
        public string Status { get; set; }
        public string MinDate { get; set; }
        public string MaxDate { get; set; }
        public string ExpectedTopDate { get; set; }
        public string TargetCompletionDate { get; set; }
        public string DateConcat { get; set; }
    }
}
