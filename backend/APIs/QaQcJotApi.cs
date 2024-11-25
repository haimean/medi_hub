using DashboardApi.Application.DashBoardQaQcJot;
using DashboardApi.Dtos.QaQc.Requests;
using Microsoft.AspNetCore.Mvc;

namespace DashboardApi.Apis
{
    public class QaQcJotApi : IApis
    {
        public void RegisterApi(WebApplication app)
        {
            app.MapGet("/dashboard/qaqcjot/summary",
               async (IDashboardQaQcJotService service) => await service.QaQcJotGetSummaryData()).WithTags("Dashboard.QM");

            app.MapGet("/dashboard/qaqcjot/commoncheck",
               async (IDashboardQaQcJotService service) => await service.QaQcJotGetCommonCheckData()).WithTags("Dashboard.QM");

            app.MapGet("/dashboard/qaqcjot/criticalcheck",
               async (IDashboardQaQcJotService service) => await service.QaQcJotGetCriticalCheckData()).WithTags("Dashboard.QM");

            app.MapGet("/dashboard/qm-dashboard/qm-handover-schedule",
               async (IDashboardQaQcJotService service, [FromQuery] string request) => await service.QMHandoverSchedule(Newtonsoft.Json.JsonConvert.DeserializeObject<BaseRequest>(request)))
                .WithTags("Dashboard.QM");
        }
    }
}
