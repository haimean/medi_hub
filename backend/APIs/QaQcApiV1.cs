using DashboardApi.Application.DashBoardQaQc.V1;
using DashboardApi.Dtos.QaQc.Requests;

namespace DashboardApi.Apis
{
    public class QaQcApiV1 : IApis
    {
        public void RegisterApi(WebApplication app)
        {
            // qaqc report v1
            app.MapPost("/dashboard/qaqc/criticalcheck",
                async (IDashboardQaQcServiceV1 service, CriticalRequest request) =>
                    await service.QaQcGetCriticalData(request)).WithTags("Dashboard.QAQCQM.V1");

            app.MapPost("/dashboard/qaqc/criticalcheck-v2",
                async (IDashboardQaQcServiceV1 service, CriticalRequest request) =>
                    await service.QaQcGetCriticalDataV2(request)).WithTags("Dashboard.QAQCQM.V1");

            app.MapPost("/dashboard/qaqc/criticalcheck/checks",
                async (IDashboardQaQcServiceV1 service, CriticalRequest request) =>
                    await service.QaQcGetCriticalDataChecks(request)).WithTags("Dashboard.QAQCQM.V1");

            app.MapPost("/dashboard/qaqc/commoncheck",
                async (IDashboardQaQcServiceV1 service, CommonCheckRequest request) =>
                    await service.QaQcGetCommonData(request)).WithTags("Dashboard.QAQCQM.V1");
            app.MapPost("/dashboard/qaqc/commoncheck-v2",
                async (IDashboardQaQcServiceV1 service, CommonCheckRequest request) =>
                    await service.QaQcGetCommonDataV2(request)).WithTags("Dashboard.QAQCQM.V1");
            app.MapPost("/dashboard/qaqc/commoncheck/checks",
                async (IDashboardQaQcServiceV1 service, CommonCheckRequest request) =>
                    await service.QaQcGetCommonDataChecks(request)).WithTags("Dashboard.QAQCQM.V1");

            app.MapPost("/dashboard/qaqc/summary",
                async (IDashboardQaQcServiceV1 service, SummaryRequest request) =>
                    await service.QaQcGetSummaryData(request)).WithTags("Dashboard.QAQCQM.V1");
            app.MapGet("/dashboard/qaqc/critical-common-trades",
                async (IDashboardQaQcServiceV1 service) =>
                    await service.QaQcCriticalCommonTrades()).WithTags("Dashboard.QAQCQM.V1");
            app.MapPost("/dashboard/qaqc/rework",
                async (IDashboardQaQcServiceV1 service, ReworkRequest request) =>
                    await service.QaQcGetReworkData(request)).WithTags("Dashboard.QAQCQM.V1");
            app.MapPost("/dashboard/qaqc/observation",
                async (IDashboardQaQcServiceV1 service, ObservationRequest request) =>
                    await service.QaQcGetObservationData(request)).WithTags("Dashboard.QAQCQM.V1");
            app.MapGet("/dashboard/qaqc/static-report",
                async (IDashboardQaQcServiceV1 service) =>
                    await service.QaQcGetObservationStaticReport()).WithTags("Dashboard.QAQCQM.V1");
            app.MapPost("/dashboard/qaqc/violation",
                async (IDashboardQaQcServiceV1 service, ViolationRequest request) =>
                    await service.QaQcGetViolationData(request)).WithTags("Dashboard.QAQCQM.V1");

            // qaqc dashboard report v1
            app.MapPost("/dashboard/qaqc/summary-dashboard-critical",
                async (IDashboardQaQcServiceV1 service, SummaryRequest request) =>
                    await service.QaQcGetSummaryDataDashboardCritical(request)).WithTags("Dashboard.QAQCQM.V1");
            app.MapPost("/dashboard/qaqc/summary-dashboard-common",
                async (IDashboardQaQcServiceV1 service, SummaryRequest request) =>
                    await service.QaQcGetSummaryDataDashboardCommon(request)).WithTags("Dashboard.QAQCQM.V1");
            app.MapPost("/dashboard/qaqc/summary-dashboard-combine",
                async (IDashboardQaQcServiceV1 service, SummaryRequest request) =>
                    await service.QaQcGetSummaryDataDashboardCombine(request)).WithTags("Dashboard.QAQCQM.V1");

            // qm summary report
            app.MapPost("/dashboard/qaqc/summary-dashboard-quanlity-kpr",
               async (IDashboardQaQcServiceV1 service, SummaryRequest request) =>
                   await service.QaQcGetSummaryDataDashboardQualityKPR(request)).WithTags("Dashboard.QAQCQM.V1");

            app.MapPost("/dashboard/qaqc/quanlity-kpr-detail",
               async (IDashboardQaQcServiceV1 service, SummaryRequest request) =>
                   await service.QaQcGetDetailDataDashboardQualityKPR(request)).WithTags("Dashboard.QAQCQM.V1");

            app.MapPost("/dashboard/qaqc/quanlity-kpr-detail-month",
               async (IDashboardQaQcServiceV1 service, SummaryRequest request) =>
                   await service.QaQcGetDetailDataDashboardQualityKPRMonth(request)).WithTags("Dashboard.QAQCQM.V1");

            // sub api
            app.MapGet("/dashboard/qaqc/user-project",
                async (IDashboardQaQcServiceV1 service) =>
                    await service.QaQcGetUserProject()).WithTags("Dashboard.QAQCQM.V1");

            // qm report
            app.MapPost("/dashboard/qaqc/qm-weekly-report",
               async (IDashboardQaQcServiceV1 service, SummaryRequest request) =>
                   await service.QMWeeklyReport(request)).WithTags("Dashboard.QAQCQM.V1");

            app.MapPost("/dashboard/qaqc/get-Score",
                async (IDashboardQaQcServiceV1 service, ScorereworkAndDefece score) =>
                    await service.GetScore(score)).WithTags("Dashboard.QAQCQM.V1");

            app.MapGet("/dashboard/qaqc/get-comment-qaqc",
               async (IDashboardQaQcServiceV1 service) => await service.GetComment()).WithTags("Dashboard.QM");
        }
    }
}