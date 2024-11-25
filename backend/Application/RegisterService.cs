using DashboardApi.Application.BaseCommon;
using DashboardApi.Application.DashboardDigicheck;
using DashboardApi.Application.DashboardMaintenance;
using DashboardApi.Application.DashBoardQaQc.V1;
using DashboardApi.Application.DashBoardQaQc.V2;
using DashboardApi.Application.DashBoardQaQcDigicheck;
using DashboardApi.Application.DashBoardQaQcJot;
using DashboardApi.Application.DashBoardSafety;
using DashboardApi.Application.DashboardWorker;
using DashboardApi.Application.Email;
using DashboardApi.Application.Maintenance;
using DashboardApi.Application.Project;
using DashboardApi.Auth.CurrentUser;
using DashboardApi.Infastructure.QAQC;

namespace DashboardApi.Application
{
    public static class RegisterService
    {
        public static void RegisterAppService(this IServiceCollection services)
        {
            // service
            services.AddScoped<IDashboardQaQcServiceV1, DashboardQaQcServiceV1>();
            services.AddScoped<IDashboardQaQcServiceV2, DashboardQaQcServiceV2>();
            services.AddScoped<IDashboardQaQcJotService, DashboardQaQcJotService>();
            services.AddScoped<IDashboardQaQcDigicheckService, DashboardQaQcDigiService>();
            services.AddScoped<IDashboardWorkerService, DashboardWorkerService>();
            services.AddScoped<IDashboardSafetyService, DashboardSafetyService>();
            services.AddScoped<IDashboardMaintenanceService, DashboardMaintenanceService>();
            services.AddScoped<IBaseCommonService, BaseCommonService>();
            services.AddScoped<IBlockService, BlockService>();
            services.AddScoped<IUnitsService, UnitsService>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IDashboardDigicheckService, DashboardDigicheckService>();
            services.AddScoped<IEmailService, EmailService>();

            // repository
            services.AddScoped<IDashboardQAQCRepositoryV2, DashboardQAQCRepositoryV2>();

            services.AddScoped<ICurrentUser, CurrentUser>();
        }
    }
}
