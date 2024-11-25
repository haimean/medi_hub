using DashboardApi.Application.AttributeCustom;

namespace DashboardApi.Dtos.Maintenance
{
    public class DefectsMonitoringName
    {
        [ColumnReportName("Block")]
        public string Block { get; set; }

        [ColumnReportName("Level")]
        public string Level { get; set; }

        [ColumnReportName("Unit")]
        public string Unit { get; set; }

        [ColumnReportName("Unit reference")]
        public string UnitReference { get; set; }

        [ColumnReportName("Case Number")]
        public string CaseNumber { get; set; }

        [ColumnReportName("Location")]
        public string Location { get; set; }

        [ColumnReportName("Type")]
        public string Type { get; set; }

        [ColumnReportName("Subtype")]
        public string Subtype { get; set; }

        [ColumnReportName("Description")]
        public string Description { get; set; }

        [ColumnReportName("Priority")]
        public string Priority { get; set; }

        [ColumnReportName("Status")]
        public string Status { get; set; }

        [ColumnReportName("Archived")]
        public string Archived { get; set; }

        [ColumnReportName("Contractor")]
        public string Contractor { get; set; }

        [ColumnReportName("Cc")]
        public string Cc { get; set; }

        [ColumnReportName("Nb. Days Open")]
        public string NbDaysOpen { get; set; }

        [ColumnReportName("Creation Date")]
        public DateTime CreationDate { get; set; }

        [ColumnReportName("Created by")]
        public string CreatedBy { get; set; }

        [ColumnReportName("Confirmation Date")]
        public DateTime? ConfirmationDate { get; set; }

        [ColumnReportName("Confirmed by")]
        public string ConfirmedBy { get; set; }

        [ColumnReportName("Intervention Date")]
        public DateTime? InterventionDate { get; set; }

        [ColumnReportName("Target Completion Date")]
        public DateTime? TargetCompletionDate { get; set; }

        [ColumnReportName("Completion Date")]
        public DateTime? CompletionDate { get; set; }

        [ColumnReportName("Completed by")]
        public string CompletedBy { get; set; }

        [ColumnReportName("Closing Date")]
        public DateTime? ClosingDate { get; set; }

        [ColumnReportName("Closed by")]
        public string ClosedBy { get; set; }

        [ColumnReportName("Tag")]
        public string Tag { get; set; }

        [ColumnReportName("Latest Comment")]
        public string LatestComment { get; set; }

        [ColumnReportName("Nb of Days Overdue")]
        public string NbOfDaysOverdue { get; set; }

        [ColumnReportName("Work Start Date")]
        public DateTime? WorkStartDate { get; set; }

        [ColumnReportName("Link to Form")]
        public string LinkToForm { get; set; }

        [ColumnReportName("Predicted Completion Date")]
        public DateTime? PredictedCompletionDate { get; set; }

        [ColumnReportName("Predicted Completion Days")]
        public string PredictedCompletionDays { get; set; }
    }
}
