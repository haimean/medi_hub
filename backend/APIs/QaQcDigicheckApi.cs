using DashboardApi.Application.DashBoardQaQcDigicheck;

namespace DashboardApi.Apis
{
    public class QaQcDigicheckApi : IApis
    {
        public void RegisterApi(WebApplication app)
        {
            app.MapGet("/dashboard/qaqcdigicheck/bcainspection/{siteId}",
                async (string siteId, string[] strataBlocks, IDashboardQaQcDigicheckService service) =>
                    await service.QaQcGetBcaInspection(siteId, strataBlocks));

            app.MapGet("/dashboard/qaqcdigicheck/handover/{siteId}",
                async (string siteId, string[] strataBlocks, IDashboardQaQcDigicheckService service) =>
                    await service.QaQcGetHandedOver(siteId, strataBlocks));

            app.MapGet("/dashboard/qaqcdigicheck/handover-block/{siteId}",
                async (string siteId, IDashboardQaQcDigicheckService service) =>
                    await service.QaQcGetHandedOverBlock(siteId));
        }
    }
}