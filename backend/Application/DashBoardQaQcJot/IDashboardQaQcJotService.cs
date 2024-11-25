using DashboardApi.Dtos.QaQc.Requests;
using DashboardApi.HttpConfig;

namespace DashboardApi.Application.DashBoardQaQcJot
{
    public interface IDashboardQaQcJotService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<ServiceResponse> QaQcJotGetSummaryData();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<ServiceResponse> QaQcJotGetCommonCheckData();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<ServiceResponse> QaQcJotGetCriticalCheckData();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<ServiceResponse> QMHandoverSchedule(BaseRequest request);
    }
}
