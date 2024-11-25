namespace DashboardApi.Dtos.Maintenance
{
    public class CombineCasesDefectsMonitoringRequest
    {
        public Guid? MaintenanceId { get; set; }

        public string ProjectID { get; set; }

        public string ProjectName { get; set; }

        public string Block { get; set; }

        public string Level { get; set; }

        public string Unit { get; set; }

        public string CaseNumber { get; set; }

        public string Status { get; set; }

        public string Archived { get; set; }

        public DateTime? ReopenedDate { get; set; }

        public string CreationDate { get; set; }

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

        public string Total { get; set; }

        public string Count { get; set; }

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

        public string UnitReference { get; set; }

        public string IDDefect { get; set; }

        public string Location { get; set; }

        public string Type { get; set; }

        public string Subtype { get; set; }

        public string Description { get; set; }

        public string Priority { get; set; }

        public string Contractor { get; set; }

        public string Cc { get; set; }

        public string NbDaysOpen { get; set; }

        public DateTime InterventionDate { get; set; }

        public DateTime CompletionDate { get; set; }

        public string NbOfDaysOverdue { get; set; }

        public DateTime WorkStartDate { get; set; }

        public string LinkToForm { get; set; }

        public DateTime PredictedCompletionDate { get; set; }

        public string DateTemp { get; set; }

        public string PredictedCompletionDays { get; set; }

        #region incoming outgoing
        public string DateConcat { get; set; }
        public string TotalIncoming { get; set; }
        public string TotalOutgoing { get; set; }
        public string TimelineIncomingCases { get; set; }
        public string TimelineOutgoingCases { get; set; }
        public string TimelineIncomingDefects { get; set; }
        public string TimelineOutgoingDefects { get; set; }
        #endregion
    }
}
