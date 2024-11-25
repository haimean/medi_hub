using DashboardApi.Application.DashBoardSafety;
using DashboardApi.Dtos.Safety.Request;

namespace DashboardApi.Apis
{
    public class SafetyApi : IApis
    {
        public void RegisterApi(WebApplication app)
        {
            app.MapPost("/dashboard/safety/get-projects",
               async (IDashboardSafetyService service) => await service.GetProjects());

            app.MapPost("/dashboard/safety/get-statistic-report",
               async (IDashboardSafetyService service, SearchRequest request) => await service.GetStatistic(request));

            app.MapPost("/dashboard/safety/get-total-incident-numbers-by-hazard-type-report",
               async (IDashboardSafetyService service, SearchRequest request) => await service.GetTotalIncidentNumbersByHazardTypeReport(request));

            app.MapPost("/dashboard/safety/get-total-incident-numbers-by-projects-report",
               async (IDashboardSafetyService service, SearchRequest request) => await service.GetTotalIncidentNumbersByProjectsReport(request));

            app.MapPost("/dashboard/safety/get-total-authority-compliance-report",
               async (IDashboardSafetyService service, SearchRequest request) => await service.GetTotalAuthorityComplianceReport(request));

            app.MapPost("/dashboard/safety/get-summary-of-performance-score-report",
              async (IDashboardSafetyService service, SearchRequest request) => await service.GetSummaryOfPerformanceScoreReportV2(request));

            app.MapPost("/dashboard/safety/get-total-incident-numbers-by-month-report",
              async (IDashboardSafetyService service, SearchRequest request) => await service.GetTotalIncidentNumbersByMonthReport(request));

            app.MapPost("/dashboard/safety/get-detail-of-total-incident-numbers-by-hazard-type-report",
              async (IDashboardSafetyService service, SearchRequest request) => await service.GetDetailOfTotalIncidentNumbersByHazardTypeReport(request));

            app.MapPost("/dashboard/safety/get-wir-report",
              async (IDashboardSafetyService service, SearchRequest request) => await service.GetWir(request));

            app.MapPost("/dashboard/safety/get-mom-notice-of-cf",
              async (IDashboardSafetyService service, SearchRequest request) => await service.GetMOMNoticeOfNonCF(request));
        }
    }
}
