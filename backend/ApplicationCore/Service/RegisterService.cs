using MediHub.Web.ApplicationCore.Auth.CurrentUser;
using MediHub.Web.ApplicationCore.Interfaces;
using MediHub.Web.Aws;
using MediHub.Web.Data.Repository;
using MediHub.Web.DatabaseContext.AppDbcontext;
using MediHub.Web.Models;

namespace MediHub.Web.ApplicationCore.Service
{
    public static class RegisterService
    {
        public static void RegisterAppService(this IServiceCollection services)
        {
            #region Reponsitory
            services.AddScoped<IRepository, Repository<MediHubContext>>();
            #endregion

            #region Services
            services.AddScoped<IAwsService, AwsService>();
            services.AddScoped<ICurrentUser, CurrentUserService>();
            services.AddScoped<IMaintenanceRecordService, MaintenanceRecordService>();
            services.AddScoped<IDevicesService, DevicesService>();
            services.AddScoped<IUsersService, UsersService>();
            services.AddScoped<ICommonService, CommonService>();

            services.AddScoped<IAuthService, AuthService>();
            #endregion
        }
    }
}
