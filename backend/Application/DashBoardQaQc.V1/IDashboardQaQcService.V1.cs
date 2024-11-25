using DashboardApi.Dtos.QaQc.Requests;
using DashboardApi.HttpConfig;

namespace DashboardApi.Application.DashBoardQaQc.V1
{
    public interface IDashboardQaQcServiceV1
    {
        //qaqc summary
        Task<ServiceResponse> QaQcGetSummaryData(SummaryRequest request);
        Task<ServiceResponse> QaQcCriticalCommonTrades();

        Task<ServiceResponse> QaQcGetSummaryDataDashboardQualityKPR(SummaryRequest request);

        Task<ServiceResponse> QaQcGetDetailDataDashboardQualityKPR(SummaryRequest request);

        Task<ServiceResponse> QaQcGetDetailDataDashboardQualityKPRMonth(SummaryRequest request);


        // qm report
        Task<ServiceResponse> QaQcGetSummaryDataDashboardCritical(SummaryRequest request);

        Task<ServiceResponse> QaQcGetSummaryDataDashboardCommon(SummaryRequest request);

        Task<ServiceResponse> QaQcGetSummaryDataDashboardCombine(SummaryRequest request);

        Task<ServiceResponse> QaQcGetCommonData(CommonCheckRequest request);
        Task<ServiceResponse> QaQcGetCommonDataV2(CommonCheckRequest request);
        Task<ServiceResponse> QaQcGetCommonDataChecks(CommonCheckRequest request);

        Task<ServiceResponse> QaQcGetCriticalData(CriticalRequest request);
        Task<ServiceResponse> QaQcGetCriticalDataV2(CriticalRequest request);

        Task<ServiceResponse> QaQcGetCriticalDataChecks(CriticalRequest request);

        Task<ServiceResponse> QaQcGetReworkData(ReworkRequest request);

        Task<ServiceResponse> QaQcGetObservationData(ObservationRequest request);

        Task<ServiceResponse> QaQcGetObservationStaticReport();

        Task<ServiceResponse> QaQcGetViolationData(ViolationRequest request);

        Task<ServiceResponse> QaQcGetUserProject();

        // qm weekly report
        Task<ServiceResponse> QMWeeklyReport(SummaryRequest request);

        Task<ServiceResponse> GetComment();

        Task<ServiceResponse> GetScore(ScorereworkAndDefece score);
    }
}
