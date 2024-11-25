namespace DashboardApi.Dtos.Maintenance
{
    public class TempSummaryCasesDefectsRequest
    {
        public string ProjectID { get; set; }
        public string ProjectName { get; set; }
        public int TotalMaintenance { get; set; }
        public int TotalStatus { get; set; }
        public float Percent { get; set; }
        public int Total { get; set; }
        public string TypeMaintenance { get; set; }
        public string Status { get; set; }
        public string MinDate { get; set; }
        public string MaxDate { get; set; }
        public string Description { get; set; }
        public string Tag { get; set; }

        public string OverdueMore30Days { get; set; }
        public string Overdue14To30Days { get; set; }
        public string OverdueOnTrack { get; set; }

        public string ProjectTotalUnits { get; set; }
        public string TotalUnitsPurchaserCollectedKeys { get; set; }
        public string UnitsMovedIn { get; set; }
        public string DefectsCasesNotSubmitted { get; set; }


        public string TotalSeepagesLeakagesUnits { get; set; }
        public string TotalChokageUnits { get; set; }

        public int? WaterSeepageLeakageUnitsUnitCompleted { get; set; }

        public int? WaterSeepageLeakageUnitsPendingOwnerKeyArrangement { get; set; }

        public int? WaterSeepageLeakageUnitsUnitWorkInProgress { get; set; }

        public int? ChokageUnitCompleted { get; set; }

        public int? ChokagePendingOwnerKeyArrangement { get; set; }

        public int? ChokageUnitWorkInProgress { get; set; }

        public DateTime ConfirmationDate { get; set; }
        public string DateTemp { get; set; }
        public string CreationDate { get; set; }
        public string DateConcat { get; set; }
        public int? TotalCases { get; set; }
        public int? TotalDefects { get; set; }
    }
}
