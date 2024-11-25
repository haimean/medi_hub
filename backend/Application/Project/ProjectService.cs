using DashboardApi.Application.Project.Response;
using DashboardApi.Auth;
using DashboardApi.Auth.CurrentUser;
using DashboardApi.HttpConfig;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Dynamic;
using System.Net;
using System.Net.Http.Headers;

namespace DashboardApi.Application.Project
{
    public class ProjectService : Service, IProjectService
    {
        private readonly IOptions<HttpEndpoint> _options;
        public readonly ICurrentUser _currentUser;

        public ProjectService(IOptions<HttpEndpoint> options, ICurrentUser currentUser)
        {
            _options = options;
            _currentUser = currentUser;
        }

        /// <summary>
        /// List project
        /// </summary>
        /// <returns></returns>
        public async Task<List<ProjectResponse>> GetAllProjects()
        {
            // var accessToken = _currentUser.GetToken();
            var accessToken = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VySWQiOiIwNzc2MGVkYS0zMjE4LTQ3NjgtOTkyZi01N2Q1MGJkOTQxY2YiLCJpc0FkbWluIjpmYWxzZSwiZW1haWwiOiJwaGFtX3F1YW5naHV5QHdvaGh1cC5jb20udm4iLCJjb25maXJtZWQiOmZhbHNlLCJuYW1lIjoiUGhhbSBRdWFuZyBIdXkiLCJzaG9ydE5hbWUiOiJIdXkiLCJpc0FjdGl2ZSI6dHJ1ZSwiaWF0IjoxNzMyMTU3NDI5LCJleHAiOjE3MzIyNDM4Mjl9.QNwxO7yh6GJQmV3kK72KgsrkuthtNsxFgdLMLZbccOg";
            string url = _options.Value.Https + "/projects";

            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Authorization", accessToken);
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.100 Safari/537.36");

            var res = await client.GetAsync(url);
            var dt = await res.Content.ReadFromJsonAsync<HttpRespon<List<ProjectResponse>>>();

            return dt.Data;
        }


        public async Task<List<ProjectResponse>> GetProjectsByUser()
        {
            var converter = new ExpandoObjectConverter();
            var accessToken = _currentUser.GetToken();
            dynamic message = null;
            var listProject = new List<ProjectResponse>();
            string url = _options.Value.Https + "/projects/get-project-for-user";
            // string url = $"http://localhost:8000/projects/get-project-for-user";

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Headers.Add("Authorization", accessToken);
            request.ContentType = "application/json";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.100 Safari/537.36";
            using (var response1 = await request.GetResponseAsync())
            {
                using (var reader = new StreamReader(response1.GetResponseStream()))
                {
                    var responseString = reader.ReadToEnd();
                    message = JsonConvert.DeserializeObject<ExpandoObject>(responseString, converter);
                    var rows = (List<dynamic>)message.data;
                    foreach (var item in rows)
                    {
                        var name = ((IDictionary<string, object>)item)["name"].ToString();
                        var obj = new ProjectResponse()
                        {
                            Id = (((IDictionary<string, object>)item)["id"])?.ToString(),
                            ProjectId = ((IDictionary<string, object>)item)["id"]?.ToString(),
                            ProjectName = name,
                        };
                        listProject.Add(obj);
                    };
                }
            }
            return listProject;
        }
    }
}
