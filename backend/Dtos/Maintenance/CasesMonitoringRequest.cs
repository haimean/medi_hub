namespace DashboardApi.Dtos.Maintenance
{
    public class CasesMonitoringRequest
    {
        public Guid? MaintenanceId { get; set; }

        public string ProjectID { get; set; }

        public string ProjectName { get; set; }

        public string Block { get; set; }

        public string Level { get; set; }

        public string Unit { get; set; }

        public string CaseNumber { get; set; }

        public string Defects { get; set; }

        public string Status { get; set; }

        public string Archived { get; set; }

        public DateTime? ReopenedDate { get; set; }

        public DateTime? CreationDate { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? SubmittedDate { get; set; }

        public string SubmittedBy { get; set; }

        public DateTime? ConfirmationDate { get; set; }

        public string ConfirmedBy { get; set; }

        public DateTime? TargetCompletionDate { get; set; }

        public string CompletedBy { get; set; }

        public DateTime? ClosingDate { get; set; }

        public string ClosedBy { get; set; }

        public string Tag { get; set; }

        public string DaysSinceLodged { get; set; }

        public string DaysSinceConfirmation { get; set; }

        public string TotalDefects { get; set; }

        public string IsNew { get; set; }

        public string PendingStart { get; set; }

        public string Wip { get; set; }

        public string Rectified { get; set; }

        public string Completed { get; set; }

        public string Closed { get; set; }

        public string Rejected { get; set; }

        public string LatestComment { get; set; }

        public string IsTypeMaintenance { get; set; }

        public string TypeWarranty { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}
