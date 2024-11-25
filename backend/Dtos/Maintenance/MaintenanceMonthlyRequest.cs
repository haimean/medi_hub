
namespace DashboardApi.Dtos.Maintenance
{
    public class MaintenanceMonthlyRequest
    {
        public DateTime Month { get; set; }
        public string MonthString { get; set; }

        public int? UnitsPurchaserCollectedKeys { get; set; }

        public int? UnitsMovedIn { get; set; }

        public int? UnitsUnsold { get; set; }

        public int? WaterSeepageLeakageUnitsUnitCompleted { get; set; }

        public int? WaterSeepageLeakageUnitsPendingOwnerKeyArrangement { get; set; }

        public int? WaterSeepageLeakageUnitsUnitWorkInProgress { get; set; }

        public int? ChokageUnitCompleted { get; set; }

        public int? ChokagePendingOwnerKeyArrangement { get; set; }

        public int? ChokageUnitWorkInProgress { get; set; }

        public int? TotalWorkersOnSite { get; set; }

        public int? TotalWHWorkersOnSite { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime ConfirmationDate { get; set; }
    }
}
