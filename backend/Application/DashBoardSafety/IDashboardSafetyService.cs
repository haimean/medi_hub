using DashboardApi.Dtos.Safety.Request;
using DashboardApi.Dtos.Safety.Response;
using DashboardApi.HttpConfig;

namespace DashboardApi.Application.DashBoardSafety
{
    public interface IDashboardSafetyService
    {
         Task<ServiceResponse> GetTotalIncidentNumbersByHazardTypeReport(SearchRequest request);
         Task<ServiceResponse> GetStatistic(SearchRequest request);
         Task<ServiceResponse> GetTotalIncidentNumbersByProjectsReport(SearchRequest request);
         Task<ServiceResponse> GetTotalAuthorityComplianceReport(SearchRequest request);
         Task<ServiceResponse> GetSummaryOfPerformanceScoreReportV2(SearchRequest request);
         Task<List<ProjectResponse>> GetProjects();
         Task<ServiceResponse> GetTotalIncidentNumbersByMonthReport(SearchRequest request);
         Task<ServiceResponse> GetDetailOfTotalIncidentNumbersByHazardTypeReport(SearchRequest request);
         Task<ServiceResponse> GetWir(SearchRequest request);
         Task<ServiceResponse> GetMOMNoticeOfNonCF(SearchRequest request);

    }
}
