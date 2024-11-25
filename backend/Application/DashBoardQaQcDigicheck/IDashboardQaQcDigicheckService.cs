using DashboardApi.HttpConfig;

namespace DashboardApi.Application.DashBoardQaQcDigicheck
{
    public interface IDashboardQaQcDigicheckService
    {
        Task<ServiceResponse> QaQcGetHandedOver(string SiteId, string[] strataBlocks);
        Task<ServiceResponse> QaQcGetHandedOverBlock(string SiteId);
        Task<ServiceResponse> QaQcGetBcaInspection(string SiteId, string[] straraBlocks);
    }
}
