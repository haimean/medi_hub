using DashboardApi.Application.Project.Response;
using DashboardApi.Dtos.QaQc.Requests;
using DashboardApi.Dtos.QaQc.Responses;

namespace DashboardApi.Infastructure.QAQC
{
    public interface IDashboardQAQCRepositoryV2
    {
        /// <summary>
        /// query summary worker app pqo critical common
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (08.04.2023)
        Task<List<SummaryCriticalCommon>> WorkerAppGetSummaryPQOCriticalCommon(string query, SummaryRequest request);

        /// <summary>
        /// query summary pqo critical common
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (08.04.2023)
        Task<List<SummaryQAQCTab>> QaQcGetSummaryPQAIQAReworkDefect(string query, SummaryRequest request);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (23.02.2024)
        Task<List<SummaryQAQCTab>> QaQcGetSummaryPQAIQAReworkDefectDigicheck(string query, SummaryRequest request);

        /// <summary>
        /// List project get app setting
        /// </summary>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (23.02.2024)
        Task<List<ProjectResponse>> ProjectApp();

        /// <summary>
        /// query summary worker app pqo critical common
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (08.04.2023)
        Task<List<SummaryCriticalCommon>> GetSummaryCriticalCommonDigicheck(string query, SummaryRequest request);
    }
}
