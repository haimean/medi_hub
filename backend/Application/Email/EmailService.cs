using DashboardApi.Application.Email.Request;
using DashboardApi.Application.Project;
using DashboardApi.Auth.CurrentUser;
using DashboardApi.Auth.PermisionChecker;
using DashboardApi.HttpConfig;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace DashboardApi.Application.Email
{
    public class EmailService : Service, IEmailService
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IProjectService _projectService;
        private readonly IPermissionChecker _permissionChecker;
        private readonly ICurrentUser _currentUser;
        private readonly IOptions<HttpEndpoint> _options;
        private static readonly HttpClient client = new HttpClient();

        public EmailService(IWebHostEnvironment hostingEnvironment,
            IProjectService projectService,
            IPermissionChecker permissionChecker,
            ICurrentUser currentUser,
            IOptions<HttpEndpoint> options)
        {
            _hostingEnvironment = hostingEnvironment;
            _projectService = projectService;
            _permissionChecker = permissionChecker;
            _currentUser = currentUser;
            _options = options;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (17.09.2024)
        public async Task<ServiceResponse> SendEmailTemplate(SendEmailRequest body)
        {
            try
            {
                string url = $"{_options.Value.Https}/mail/sendMail";

                var stringContent = new StringContent(JsonConvert.SerializeObject(new SendMailCommandRequest()
                {
                    to = body.To,
                    cc = body.CC,
                    subject = body.Subject,
                    emailContent = body.EmailContent,
                    attachments = body.Attachments,
                }), Encoding.UTF8, "application/json");

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/111.0.0.0 Safari/537.36");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _currentUser.GetToken());

                var response = await client.PostAsync(url, stringContent);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (17.09.2024)
        public async Task<ServiceResponse> SendMailAsync(SendMailCommandRequest request)
        {
            string url = $"{_options.Value.Https}/mail/sendMail";

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/111.0.0.0 Safari/537.36");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _currentUser.GetToken());

            var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, stringContent);

            return Ok(response);
        }
    }
}
