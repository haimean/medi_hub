using DashboardApi.HttpConfig;

namespace DashboardApi.Application.DashboardWorker
{
    public interface IDashboardWorkerService
    {
        Task<ServiceResponse> GetCostReport(string projectId, string request);
        Task<ServiceResponse> QaQcFilterQmFromWorkerApp();
        Task<ServiceResponse> QaQcGetQmFromWorkerApp(string siteId, string blockName);
        Task<ServiceResponse> QaQcGetQmFromJotFormData(string project_code);
    }
}
