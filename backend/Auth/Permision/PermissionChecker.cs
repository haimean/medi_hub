using MediHub.Web.Auth.CurrentUser;
using MediHub.Web.Auth.PermisionChecker.Dtos;
using MediHub.Web.HttpConfig;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Net;

namespace MediHub.Web.Auth.PermisionChecker
{
    public class PermissionChecker : IPermissionChecker
    {
        private const int DefaultCacheExpirationTimeInMinutes = 10;
        private readonly IDistributedCache _cache;
        public List<string> Permissions { get; set; }
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOptions<HttpEndpoint> _options;
        private readonly ICurrentUser _currentUser;
        public PermissionChecker(IDistributedCache cache, IHttpContextAccessor httpContextAccessor, IOptions<HttpEndpoint> options, ICurrentUser currentUser)
        {
            _cache = cache;
            _httpContextAccessor = httpContextAccessor;
            _options = options;
            _currentUser = currentUser;
        }

        public async Task<bool> HasPermission(string permission)
        {
            var token = _currentUser.GetToken();
            var userId = _currentUser.GetId();

            if (userId != Guid.Empty)
            {
                string url = _options.Value.Https + "/permissions/check-user-has-permission?permissionName=" + permission + "&userId=" + userId;
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.ContentType = "application/json";
                request.Headers.Add("Authorization", token);
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.100 Safari/537.36";
                var converter = new ExpandoObjectConverter();

                try
                {
                    using (var response1 = request.GetResponse())
                    {
                        using (var reader = new StreamReader(response1.GetResponseStream()))
                        {
                            var responseString = await reader.ReadToEndAsync();

                            var granted = JsonConvert.DeserializeObject<CheckPermissionResponse>(responseString, converter);

                            return granted.Data;
                        }
                    }
                }
                catch (Exception e)
                {
                    return false;
                }
            }

            return false;
        }

        public async Task<List<string>> GetPermissions()
        {
            return new List<string>();
        }

        public virtual async Task<bool> IsGrantedAsync(string emailUpper, string permission)
        {
            var cachedValue = await _cache.GetStringAsync(emailUpper + permission);
            if (!string.IsNullOrEmpty(cachedValue))
            {
                bool.TryParse(cachedValue, out bool isCachedValueGranted);
                return isCachedValueGranted;
            }

            if (emailUpper.ToUpper() == "admin@wohhup.com".ToUpper())
            {
                return true;
            }

            var isGranted = await HasPermission(permission);

            await _cache.SetStringAsync(emailUpper + permission, isGranted.ToString(), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(DefaultCacheExpirationTimeInMinutes)
            });

            return isGranted;
        }
    }
}
