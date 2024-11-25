using DashboardApi.Application.Project.Blocks;
using DashboardApi.Auth.CurrentUser;
using DashboardApi.HttpConfig;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Dynamic;
using System.Net;

namespace DashboardApi.Application.Project
{
    public class UnitsService : Service, IUnitsService
    {
        private readonly IOptions<HttpEndpoint> _options;
        public readonly ICurrentUser _currentUser;

        public UnitsService(IOptions<HttpEndpoint> options, ICurrentUser currentUser)
        {
            _options = options;
            _currentUser = currentUser;
        }

        public async Task<List<UnitskResponse>> GetUnits(string id)
        {
            var listBlock = new List<UnitskResponse>();

            try
            {
                // get current user token
                var accessToken = _currentUser.GetToken();

                string url = _options.Value.Https + $"/units?projectId={id}";
                // string url = $"http://localhost:8000/units?projectId={id}";

                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.ContentType = "application/json";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.100 Safari/537.36";
                request.Headers.Add("Authorization", accessToken);
                var converter = new ExpandoObjectConverter();
                dynamic message = null;

                using (var response1 = await request.GetResponseAsync())
                {
                    using (var reader = new StreamReader(response1.GetResponseStream()))
                    {
                        var responseString = reader.ReadToEnd();
                        message = JsonConvert.DeserializeObject<ExpandoObject>(responseString, converter);
                        var rows = (List<dynamic>)message.data;

                        foreach (var item in rows)
                        {
                            listBlock.Add(new UnitskResponse()
                            {
                                Id = (((IDictionary<string, object>)item)["id"])?.ToString(),
                                Name = (((IDictionary<string, object>)item)["name"])?.ToString(),
                                FullName = (((IDictionary<string, object>)item)["fullname"])?.ToString(),
                                BlockId = (((IDictionary<string, object>)item)["blockId"])?.ToString(),
                                TypeId = (((IDictionary<string, object>)item)["typeId"])?.ToString(),
                                LevelId = (((IDictionary<string, object>)item)["levelId"])?.ToString(),
                                BlockName = (((IDictionary<string, object>)item)["blockName"])?.ToString(),
                                LevelName = (((IDictionary<string, object>)item)["levelName"])?.ToString(),
                                TypeName = (((IDictionary<string, object>)item)["typeName"])?.ToString(),
                                NoOfRoom = (((IDictionary<string, object>)item)["noOfRoom"])?.ToString(),
                                NoOfSample = (((IDictionary<string, object>)item)["noOfSample"])?.ToString(),
                                TypeImage = (((IDictionary<string, object>)item)["typeImage"])?.ToString(),
                                Area = (((IDictionary<string, object>)item)["area"])?.ToString(),
                                Description = (((IDictionary<string, object>)item)["description"])?.ToString(),
                                ProjectId = (((IDictionary<string, object>)item)["projectId"])?.ToString(),
                                ProjectName = (((IDictionary<string, object>)item)["projectName"])?.ToString(),
                            });
                        };
                    }
                }
            }
            catch (Exception ce)
            {
                Console.Write(ce.Message);
                throw ce;
            }

            return listBlock;
        }
    }
}
