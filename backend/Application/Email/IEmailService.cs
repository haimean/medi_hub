using DashboardApi.Application.Email.Request;
using DashboardApi.HttpConfig;

namespace DashboardApi.Application.Email
{
    public interface IEmailService
    {
        Task<ServiceResponse> SendEmailTemplate(SendEmailRequest body);

        Task<ServiceResponse> SendMailAsync(SendMailCommandRequest body);
    }
}
