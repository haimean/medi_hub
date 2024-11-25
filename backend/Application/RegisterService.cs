using DashboardApi.Auth.CurrentUser;
using QAQCApi.Aws;
using QAQCApi.Data.Repository;
using QAQCApi.DatabaseContext.AppDbcontext;

namespace DashboardApi.Application
{
    public static class RegisterService
    {
        public static void RegisterAppService(this IServiceCollection services)
        {
            services.AddScoped<IAwsService, AwsService>();
            services.AddScoped<IRepository, Repository<MediHubContext>>();
            services.AddScoped<ICurrentUser, CurrentUser>();
        }
    }
}
