using AutoMapper;
using Dapper;
using DashboardApi.Application.Project.Response;
using DashboardApi.Auth;
using DashboardApi.Auth.CurrentUser;
using DashboardApi.Auth.PermisionChecker;
using DashboardApi.DatabaseContext.DapperDbContext;
using DashboardApi.Dtos.QaQc.Requests;
using DashboardApi.Dtos.QaQc.Responses;
using DashboardApi.HttpConfig;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace DashboardApi.Infastructure.QAQC
{
    public class DashboardQAQCRepositoryV2 : Service, IDashboardQAQCRepositoryV2
    {
        private readonly QaQcDapperContext _qaqcDapper;
        public readonly IMapper _mapper;
        public readonly ICurrentUser _currentUser;
        private readonly WorkerDapperContext _workerDapper;
        private readonly DigiCheckDapperContext _digiCheckDapper;
        private readonly IOptions<HttpEndpoint> _options;

        public DashboardQAQCRepositoryV2(
            QaQcDapperContext qaqcDapper,
            IPermissionChecker permissionChecker,
            IMapper mapper,
            AppMainDapperContext appMainDapper,
            ICurrentUser currentUser,
            WorkerDapperContext workerDapper,
            DigiCheckDapperContext digiCheckDapper,
            IOptions<HttpEndpoint> options
        )
        {
            _qaqcDapper = qaqcDapper;
            _mapper = mapper;
            _currentUser = currentUser;
            _workerDapper = workerDapper;
            _digiCheckDapper = digiCheckDapper;
            _options = options;
        }

        /// <summary>
        /// function query sql summary critical common
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (08.04.2023)
        public async Task<List<SummaryCriticalCommon>> WorkerAppGetSummaryPQOCriticalCommon(string query, SummaryRequest request)
        {
            List<SummaryCriticalCommon> result = new List<SummaryCriticalCommon>();

            try
            {
                using var connection = _workerDapper.CreateConnection();
                string listProject = request.listProject.Length > 0 ? $"'{string.Join("','", request.listProject.ToArray())}'" : "";
                var resultQuery = (await connection.QueryAsync<SummaryCriticalCommon>(query.Replace("@ListProject", listProject), new
                {
                    LteDate = !string.IsNullOrEmpty(request.lteDate) ? DateTime.ParseExact(request.lteDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) : DateTime.Now,
                    GteDate = !string.IsNullOrEmpty(request.gteDate) ? DateTime.ParseExact(request.gteDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) : DateTime.Now
                })).ToList();
                result = resultQuery;
            }
            catch (Exception ce)
            {
                Console.WriteLine($"WorkerAppGetSummaryPQOCriticalCommon: {ce.Message}");
            }

            return result;
        }

        /// <summary>
        /// function query sql summary pqa, iqa, rework, defects
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (08.04.2023)
        public async Task<List<SummaryQAQCTab>> QaQcGetSummaryPQAIQAReworkDefect(string query, SummaryRequest request)
        {
            List<SummaryQAQCTab> result = new List<SummaryQAQCTab>();

            try
            {
                using var connection = _qaqcDapper.CreateConnection();
                string listProject = request.listProject.Length > 0 ? $"'{string.Join("','", request.listProject.ToArray())}'" : "";
                var resultQuery = (await connection.QueryAsync<SummaryQAQCTab>(query.Replace("@ListProject", listProject), new
                {
                    LteDate = !string.IsNullOrEmpty(request.lteDate) ? DateTime.ParseExact(request.lteDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) : DateTime.Now,
                    GteDate = !string.IsNullOrEmpty(request.gteDate) ? DateTime.ParseExact(request.gteDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) : DateTime.Now
                })).ToList();
                result = resultQuery;
            }
            catch (Exception ce)
            {
                Console.WriteLine($"QaQcGetSummaryPQAIQAReworkDefect: {ce.Message}");
            }

            return result;
        }

        /// <summary>
        /// function query sql summary pqa, iqa, rework, defects
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (08.04.2023)
        public async Task<List<SummaryQAQCTab>> QaQcGetSummaryPQAIQAReworkDefectDigicheck(string query, SummaryRequest request)
        {
            List<SummaryQAQCTab> result = new List<SummaryQAQCTab>();

            try
            {
                using var connection = _digiCheckDapper.CreateConnection();
                string listProjectId = request.listProjectId.Length > 0 ? $"'{string.Join("','", request.listProjectId.ToArray())}'" : "";

                var resultQuery = (await connection.QueryAsync<SummaryQAQCTab>(query.Replace("@ListProject", listProjectId), new
                {
                    LteDate = !string.IsNullOrEmpty(request.lteDate) ? DateTime.ParseExact(request.lteDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) : DateTime.Now,
                    GteDate = !string.IsNullOrEmpty(request.gteDate) ? DateTime.ParseExact(request.gteDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) : DateTime.Now
                })).ToList();
                result = resultQuery;
            }
            catch (Exception ce)
            {
                Console.WriteLine($"QaQcGetSummaryPQAIQAReworkDefect: {ce.Message}");
            }

            return result;
        }

        /// <summary>
        /// List project get app setting
        /// </summary>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (23.02.2024)
        public async Task<List<ProjectResponse>> ProjectApp()
        {
            string url = _options.Value.Https + "/projects";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.100 Safari/537.36");

            List<ProjectResponse> result = new List<ProjectResponse>();

            try
            {
                var res = await client.GetAsync(url);
                var a = await res.Content.ReadAsStringAsync();
                var dt = await res.Content.ReadFromJsonAsync<HttpRespon<List<ProjectResponse>>>();
                result = dt.Data;
            }
            catch (Exception ce)
            {
                Console.WriteLine($"ProjectAppSetting-Dashboard: {ce.Message}");
            }

            return result;
        }

        /// <summary>
        /// query summary critical common digicheck
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (23.02.2024)
        public async Task<List<SummaryCriticalCommon>> GetSummaryCriticalCommonDigicheck(string query, SummaryRequest request)
        {
            var result = new List<SummaryCriticalCommon>();

            try
            {
                using var connection = _digiCheckDapper.CreateConnection();
                string listProjectId = request.listProjectId.Length > 0 ? $"'{string.Join("','", request.listProjectId.ToArray())}'" : "";

                string whereCritical = string.Empty,
                       whereCommon = string.Empty;

                // if multiple project id => get all
                if (!string.IsNullOrEmpty(listProjectId) && request.listProjectId.Length <= 1)
                {
                    whereCritical = $"CRC.PROJECT_ID IN ({listProjectId})";
                    whereCommon = $"CC.PROJECT_ID IN ({listProjectId})";
                }
                else
                {
                    if (request.listProjectId.Length > 0)
                    {
                        whereCritical = $"CRC.PROJECT_ID IN ({string.Join(", ", listProjectId.Split(',').ToList().Select(x => $"{x}"))})";
                        whereCommon = $"CC.PROJECT_ID IN ({string.Join(", ", listProjectId.Split(',').ToList().Select(x => $"{x}"))})";
                    }
                }


                if (!string.IsNullOrEmpty(request.gteDate))
                {
                    whereCritical = $"{whereCritical} AND CRC.FORM_DATE <= @GteDate ";
                    whereCommon = $"{whereCommon} AND CC.CREATED_AT <= @GteDate ";
                }

                if (!string.IsNullOrEmpty(request.lteDate))
                {
                    whereCritical = $"{whereCritical} AND CRC.FORM_DATE >= @LteDate ";
                    whereCommon = $"{whereCommon} AND CC.CREATED_AT >= @LteDate ";
                }

                var resultQuery = (await connection.QueryAsync<SummaryCriticalCommon>(query
                    .Replace("@WHERECLAUSECRITICAL", string.IsNullOrEmpty(whereCritical) ? "1=1" : whereCritical)
                    .Replace("@WHERECLAUSECOMMON", string.IsNullOrEmpty(whereCommon) ? "1=1" : whereCommon), new
                    {
                        LteDate = !string.IsNullOrEmpty(request.lteDate) ? DateTime.ParseExact(request.lteDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) : DateTime.Now,
                        GteDate = !string.IsNullOrEmpty(request.gteDate) ? DateTime.ParseExact(request.gteDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) : DateTime.Now
                    })).ToList();
                result = resultQuery;

            }
            catch (Exception ce)
            {
                throw;
            }

            return result;
        }
    }
}
