using DashboardApi.HttpConfig;

namespace DashboardApi.Application.DashboardDigicheck
{
    public interface IDashboardDigicheckService
    {
        /// <summary>
        /// Func get data for report ProgressTableBlock
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (12.09.2023)
        Task<ServiceResponse> ProgressTableBlock(string request);

        /// <summary>
        /// Func get data for report ProgressTableBlock
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (20.09.2023)
        Task<ServiceResponse> ProgressTableUnit(string request);

        /// <summary>
        /// Func get casting completion
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (05.01.2024)
        Task<ServiceResponse> CastingCompletion(string request);

        /// <summary>
        /// Get in progress modules
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (07.10.2024)
        Task<ServiceResponse> InProgressModules(string request);

        /// <summary>
        /// Get monthly report for digiechk dashboard
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (08.10.2024)
        Task<ServiceResponse> DigicheckDashboardMonthly(string request);

        /// <summary>
        /// Get monthly increase report for digiechk dashboard
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (08.10.2024)
        Task<ServiceResponse> DigicheckDashboardMonthlyIncrease(string request);
    }
}
