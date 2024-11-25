using DashboardApi.Application.DashBoardQaQc.V2;
using Microsoft.AspNetCore.Mvc;

namespace DashboardApi.Apis
{
    public class QaQcApiV2 : IApis
    {
        public async void RegisterApi(WebApplication app)
        {
            // qaqc report v2
            app.MapGet("/dashboard/qaqc/summary-projects-quality-performance",
                async (IDashboardQaQcServiceV2 service, [FromQuery] string request) =>
                    await service.SummaryProjectsQualityPerformancerequest(request)).WithTags("Dashboard.QAQCQM.V2");

            // qaqc report v2
            app.MapGet("/dashboard/qm/projects-kpr-month",
                async (IDashboardQaQcServiceV2 service, [FromQuery] string request) =>
                    await service.ProjectsKPRMonth(request)).WithTags("Dashboard.QAQCQM.V2");

            // qaqc report v2 get reason score comment
            app.MapGet("/dashboard/qaqc/reason-comment",
                async (IDashboardQaQcServiceV2 service, [FromQuery] string request) =>
                    await service.ReasonComment(request)).WithTags("Dashboard.QAQCQM.V2");
        }
    }
}
