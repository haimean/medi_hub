using MediHub.Web.Auth.CurrentUser;
using MediHub.Web.Aws;
using MediHub.Web.Data.Repository;
using MediHub.Web.DatabaseContext.AppDbcontext;

namespace MediHub.Web.Application
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
