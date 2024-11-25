using DashboardApi.Application.Project.Blocks;
using DashboardApi.HttpConfig;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Dynamic;
using System.Net;

namespace DashboardApi.Application.Project
{
    public class BlockService : Service, IBlockService
    {
        private readonly IOptions<HttpEndpoint> _options;

        public BlockService(IOptions<HttpEndpoint> options)
        {
            _options = options;
        }

        public async Task<List<BlockResponse>> GetBlock()
        {
            string url = _options.Value.Https + "/block-tower";
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "application/json";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.100 Safari/537.36";
            var converter = new ExpandoObjectConverter();
            dynamic message = null;

            var listBlock = new List<BlockResponse>();
            using (var response1 = await request.GetResponseAsync())
            {
                using (var reader = new StreamReader(response1.GetResponseStream()))
                {
                    var responseString = reader.ReadToEnd();
                    message = JsonConvert.DeserializeObject<ExpandoObject>(responseString, converter);
                    var rows = (List<dynamic>)message.data;

                    foreach (var item in rows)
                    {

                    };
                }
            }

            return listBlock;
        }

        public async Task<List<BlockResponse>> GetBlock(string id)
        {
            var listBlock = new List<BlockResponse>();

            try
            {
                string url = _options.Value.Https + $"/block-tower/get-all-block-by-project/{id}";
                // string url = $"http://localhost:8000/block-tower/get-all-block-by-project/{id}";

                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.ContentType = "application/json";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.100 Safari/537.36";
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
                            listBlock.Add(new BlockResponse()
                            {
                                Id = (((IDictionary<string, object>)item)["id"])?.ToString(),
                                Name = ((IDictionary<string, object>)item)["name"]?.ToString()
                            });
                        };
                    }
                }
            }
            catch (Exception ce)
            {
                Console.Write(ce.ToString());
                throw ce;
            }

            return listBlock;
        }

        public async Task<List<UnitResponse>> GetUnit()
        {
            string url = _options.Value.Https + "/units";
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "application/json";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.100 Safari/537.36";
            var converter = new ExpandoObjectConverter();
            dynamic message = null;

            var listUnits = new List<UnitResponse>();
            using (var response1 = await request.GetResponseAsync())
            {
                using (var reader = new StreamReader(response1.GetResponseStream()))
                {
                    var responseString = reader.ReadToEnd();
                    message = JsonConvert.DeserializeObject<ExpandoObject>(responseString, converter);
                    var rows = (List<dynamic>)message.data;

                    foreach (var item in rows)
                    {
                        listUnits.Add(new UnitResponse()
                        {
                            Id = (((IDictionary<string, object>)item)["id"])?.ToString(),
                            Name = ((IDictionary<string, object>)item)["name"]?.ToString(),
                            TypeId = ((IDictionary<string, object>)item)["typeId"]?.ToString(),
                            TypeName = ((IDictionary<string, object>)item)["typeName"]?.ToString(),
                            ProjectId = ((IDictionary<string, object>)item)["projectId"]?.ToString(),
                            ProjectName = ((IDictionary<string, object>)item)["projectName"]?.ToString(),
                            BlockId = ((IDictionary<string, object>)item)["blockId"]?.ToString(),
                            BlockName = ((IDictionary<string, object>)item)["blockName"]?.ToString(),
                            LevelId = ((IDictionary<string, object>)item)["levelId"]?.ToString(),
                            LevelName = ((IDictionary<string, object>)item)["levelName"]?.ToString(),
                        });
                    };
                }
            }

            return listUnits;
        }
    }
}
