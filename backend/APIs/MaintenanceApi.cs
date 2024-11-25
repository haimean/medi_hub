using DashboardApi.Application.DashboardMaintenance;
using Microsoft.AspNetCore.Mvc;

namespace DashboardApi.Apis
{
    public class MaintenanceApi : IApis
    {
        public void RegisterApi(WebApplication app)
        {
            #region Summary DLP Page
            app.MapGet("/dashboard/maintenance/dlp/summary-cases-defects",
                async (IDashboardMaintenanceService service, [FromQuery] string request) =>
                    await service.DLPSummaryTotalCasesDefects(request)).WithTags("Dashboard.Maintenance");

            app.MapGet("/dashboard/maintenance/dlp/summary-total-defect-statuses",
                async (IDashboardMaintenanceService service, [FromQuery] string request) =>
                    await service.DLPSummaryTotalDefectsStatues(request)).WithTags("Dashboard.Maintenance");

            app.MapGet("/dashboard/maintenance/dlp/summary-total-defect-overdue-type",
                async (IDashboardMaintenanceService service, [FromQuery] string request) =>
                    await service.DLPSummaryTotalDefectsOverdueType(request)).WithTags("Dashboard.Maintenance");

            app.MapGet("/dashboard/maintenance/dlp/summary-total-water-seepages-chokages-status",
                async (IDashboardMaintenanceService service, [FromQuery] string request) =>
                    await service.DLPSummaryTotalWaterSeepagesChokagesStatus(request)).WithTags("Dashboard.Maintenance");

            app.MapGet("/dashboard/maintenance/dlp/cases-defects-project/water-seepages-chokages-details",
                async (IDashboardMaintenanceService service, [FromQuery] string request) =>
                    await service.DLPSummaryTotalWaterSeepagesChokagesDetails(request)).WithTags("Dashboard.Maintenance");
            #endregion

            #region Detail Total Cases and Defects by Project
            app.MapGet("/dashboard/maintenance/dlp/cases-defects-project/overview",
                async (IDashboardMaintenanceService service, [FromQuery] string request) =>
                    await service.DLPCasesDefectsProjectOverview(request)).WithTags("Dashboard.Maintenance");

            app.MapGet("/dashboard/maintenance/dlp/cases-defects-project/overview-export",
                async (IDashboardMaintenanceService service, [FromQuery] string request) =>
                    await service.DLPCasesDefectsProjectOverviewExport(request)).WithTags("Dashboard.Maintenance");

            app.MapGet("/dashboard/maintenance/dlp/cases-defects-project/units-oif-1st",
                async (IDashboardMaintenanceService service, [FromQuery] string request) =>
                    await service.DLPCasesDefectsProjectUnitsOIF1ST(request)).WithTags("Dashboard.Maintenance");

            app.MapGet("/dashboard/maintenance/dlp/cases-defects-project/units-oif-2nd",
                async (IDashboardMaintenanceService service, [FromQuery] string request) =>
                    await service.DLPCasesDefectsProjectUnitsOIF2ND(request)).WithTags("Dashboard.Maintenance");

            app.MapGet("/dashboard/maintenance/dlp/cases-defects-project/defect-outstanding-items-1st",
                async (IDashboardMaintenanceService service, [FromQuery] string request) =>
                    await service.DLPCasesDefectsProjectOutStandingItem1st(request)).WithTags("Dashboard.Maintenance");

            app.MapGet("/dashboard/maintenance/dlp/cases-defects-project/defect-outstanding-items-2nd",
                async (IDashboardMaintenanceService service, [FromQuery] string request) =>
                    await service.DLPCasesDefectsProjectOutStandingItem2nd(request)).WithTags("Dashboard.Maintenance");

            app.MapGet("/dashboard/maintenance/dlp/cases-defects-project/defect-overdue",
                async (IDashboardMaintenanceService service, [FromQuery] string request) =>
                    await service.DLPCasesDefectsProjectDefectOverdue(request)).WithTags("Dashboard.Maintenance");

            app.MapGet("/dashboard/maintenance/dlp/cases-defects-project/water-seepages-leakages-units",
                async (IDashboardMaintenanceService service, [FromQuery] string request) =>
                    await service.DLPCasesDefectsProjectWaterSeepagesLeakagesUnits(request)).WithTags("Dashboard.Maintenance");

            app.MapGet("/dashboard/maintenance/dlp/cases-defects-project/chokage-units",
                async (IDashboardMaintenanceService service, [FromQuery] string request) =>
                    await service.DLPCasesDefectsProjectChokageUnits(request)).WithTags("Dashboard.Maintenance");

            app.MapGet("/dashboard/maintenance/dlp/cases-defects-project/total-workers-on-site",
                async (IDashboardMaintenanceService service, [FromQuery] string request) =>
                    await service.DLPCasesDefectsProjectTotalWorkersOnSite(request)).WithTags("Dashboard.Maintenance");

            app.MapGet("/dashboard/maintenance/dlp/cases-defects-project/defect-overdue-exceeding-30-days",
                async (IDashboardMaintenanceService service, [FromQuery] string request) =>
                    await service.DLPSummaryTotalDefectsOverdueExceeding30Days(request)).WithTags("Dashboard.Maintenance");

            app.MapGet("/dashboard/maintenance/dlp/cases-defects-project/defect-overdue-exceeding-30-days-detail",
                async (IDashboardMaintenanceService service, [FromQuery] string request) =>
                    await service.DLPSummaryTotalDefectsOverdueExceeding30DaysDetail(request)).WithTags("Dashboard.Maintenance");
            #endregion

            #region Summary Post-DLP Page

            app.MapGet("/dashboard/maintenance/post-dlp/summary-cases-defects-status",
                async (IDashboardMaintenanceService service, [FromQuery] string request) =>
                    await service.PostDLPSummaryCasesDefectsStatus(request)).WithTags("Dashboard.Maintenance");

            app.MapGet("/dashboard/maintenance/post-dlp/summary-cases-defects-incoming-outgoing",
                async (IDashboardMaintenanceService service, [FromQuery] string request) =>
                    await service.PostDLPSummaryIncomingOutgoing(request)).WithTags("Dashboard.Maintenance");

            app.MapGet("/dashboard/maintenance/post-dlp/summary-cases-defects-total-numbers",
                async (IDashboardMaintenanceService service, [FromQuery] string request) =>
                    await service.PostDLPSummaryTotalNumber(request)).WithTags("Dashboard.Maintenance");

            app.MapGet("/dashboard/maintenance/post-dlp/summary-cases-defects-top-10-item",
                async (IDashboardMaintenanceService service, [FromQuery] string request) =>
                    await service.PostDLPSummaryTop10Items(request)).WithTags("Dashboard.Maintenance");

            app.MapGet("/dashboard/maintenance/post-dlp/summary-cases-defects-item-by-type-detail",
                async (IDashboardMaintenanceService service, [FromQuery] string request) =>
                    await service.PostDLPSummaryItemByTypeDetail(request)).WithTags("Dashboard.Maintenance");
            #endregion
        }
    }
}