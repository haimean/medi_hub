using DashboardApi.Application.DashboardWorker;
using Microsoft.AspNetCore.Mvc;

namespace DashboardApi.Apis
{
    public class WorkerApi : IApis
    {
        public void RegisterApi(WebApplication app)
        {
            app.MapGet("/dashboard/worker/cost-chart/{siteId}",
               async (string siteId, [FromQuery] string request, IDashboardWorkerService service) => await service.GetCostReport(siteId, request));

            app.MapGet("/dashboard/worker/filterqmworker",
               async (string siteId, [FromQuery] string request, IDashboardWorkerService service) => await service.QaQcFilterQmFromWorkerApp());

            app.MapGet("/dashboard/worker/qmworker/{siteId}",
               async (string siteId, [FromQuery] string request, IDashboardWorkerService service) => await service.QaQcGetQmFromWorkerApp(siteId, request));

            app.MapGet("/dashboard/worker/qmjotdata/{project_code}",
                async (string project_code, IDashboardWorkerService service) => await service.QaQcGetQmFromJotFormData(project_code));
        }
    }
}
