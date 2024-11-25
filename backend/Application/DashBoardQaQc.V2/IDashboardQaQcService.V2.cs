using DashboardApi.HttpConfig;

namespace DashboardApi.Application.DashBoardQaQc.V2
{
    public interface IDashboardQaQcServiceV2
    {
        /// <summary>
        /// Get data summary Projects Quality Performance against KPI
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (07.04.2023)
        Task<ServiceResponse> SummaryProjectsQualityPerformancerequest(string request);

        /// <summary>
        /// Get project KPR follow month and project
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (14.04.2023)
        Task<ServiceResponse> ProjectsKPRMonth(string request);

        /// <summary>
        /// func get reason comment
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (17.04.2023)
        Task<ServiceResponse> ReasonComment(string request);
    }
}
