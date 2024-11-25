using DashboardApi.HttpConfig;

namespace DashboardApi.Application.DashboardMaintenance
{
    public interface IDashboardMaintenanceService
    {
        #region DLP

        /// <summary>
        /// Get project KPR follow month and project
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (28.05.2023)
        Task<ServiceResponse> DLPSummaryTotalCasesDefects(string request);

        /// <summary>
        /// Func get summary for Total Number of Defect Lists By Statuses
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (30.05.2023)
        Task<ServiceResponse> DLPSummaryTotalDefectsStatues(string request);

        /// <summary>
        /// Func get summary for Total Number of Defect Lists Overdue Type
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (02.06.2023)
        Task<ServiceResponse> DLPSummaryTotalDefectsOverdueType(string request);

        /// <summary>
        /// Func get summary for Total Number Of Water Seepages/ Chokages by Status
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (02.06.2023)
        Task<ServiceResponse> DLPSummaryTotalWaterSeepagesChokagesStatus(string request);

        /// <summary>
        /// Func get summary for Total Number Of Water Seepages/ Chokages by Status detail
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (02.06.2023)
        Task<ServiceResponse> DLPSummaryTotalWaterSeepagesChokagesDetails(string request);

        /// <summary>
        /// Func get detail Total Cases and Defects by Project
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (02.06.2023)
        Task<ServiceResponse> DLPCasesDefectsProjectOverview(string request);

        /// <summary>
        /// Func get export data project detail
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (06.08.2023)
        Task<IResult> DLPCasesDefectsProjectOverviewExport(string request);

        /// <summary>
        /// Func get detail Total Cases and Defects by Project
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (02.06.2023)
        Task<ServiceResponse> DLPCasesDefectsProjectUnitsOIF1ST(string request);

        /// <summary>
        /// Func get detail Total Cases and Defects by Project
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (02.06.2023)
        Task<ServiceResponse> DLPCasesDefectsProjectUnitsOIF2ND(string request);

        /// <summary>
        /// Func get detail Total Cases and Defects by Project
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (02.06.2023)
        Task<ServiceResponse> DLPCasesDefectsProjectDefectOverdue(string request);

        /// <summary>
        /// Func get detail Total Cases and Defects by Project
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (02.06.2023)
        Task<ServiceResponse> DLPCasesDefectsProjectWaterSeepagesLeakagesUnits(string request);

        /// <summary>
        /// Func get detail Total Cases and Defects by Project
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (02.06.2023)
        Task<ServiceResponse> DLPCasesDefectsProjectChokageUnits(string request);

        /// <summary>
        /// Func get detail Total Cases and Defects by Project
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (02.06.2023)
        Task<ServiceResponse> DLPCasesDefectsProjectTotalWorkersOnSite(string request);


        /// <summary>
        /// Func get detail Total Cases and Defects, Defect List Outstanding Items 1st
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (02.06.2023)
        Task<ServiceResponse> DLPCasesDefectsProjectOutStandingItem1st(string request);

        /// <summary>
        /// Func get detail Total Cases and Defects, Defect List Outstanding Items 2nd
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (02.06.2023)
        Task<ServiceResponse> DLPCasesDefectsProjectOutStandingItem2nd(string request);

        /// <summary>
        /// Func get defects overdue exceeding 30 days
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (18.06.2023)
        Task<ServiceResponse> DLPSummaryTotalDefectsOverdueExceeding30Days(string request);

        /// <summary>
        /// Func get defects overdue exceeding 30 days
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (18.06.2023)
        Task<ServiceResponse> DLPSummaryTotalDefectsOverdueExceeding30DaysDetail(string request);

        #endregion

        #region Post DLP
        /// <summary>
        /// Func get Total Number of Cases by Statuses
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (19.06.2023)
        Task<ServiceResponse> PostDLPSummaryCasesDefectsStatus(string request);

        /// <summary>
        /// Func get Total Numbers of Incoming and Outgoing Defects and Cases
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (19.06.2023)
        Task<ServiceResponse> PostDLPSummaryIncomingOutgoing(string request);

        /// <summary>
        /// Func get Total Numbers of Defects and Cases
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (19.06.2023)
        Task<ServiceResponse> PostDLPSummaryTotalNumber(string request);

        /// <summary>
        /// Func get Top 10 Defects by Number of Items
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (19.06.2023)
        Task<ServiceResponse> PostDLPSummaryTop10Items(string request);

        /// <summary>
        /// Func get Total Number of Defects by Type and Statuses
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (19.06.2023)
        Task<ServiceResponse> PostDLPSummaryItemByTypeDetail(string request);
        #endregion

    }
}
