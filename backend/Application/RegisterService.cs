using DashboardApi.Auth.CurrentUser;

namespace DashboardApi.Application
{
    public static class RegisterService
    {
        public static void RegisterAppService(this IServiceCollection services)
        {
            // service
            //services.AddScoped<IEmailService, EmailService>();

            // repository

            services.AddScoped<ICurrentUser, CurrentUser>();
        }
    }
}
