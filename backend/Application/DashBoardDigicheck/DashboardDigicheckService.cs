using AutoMapper;
using Dapper;
using DashboardApi.Application.BaseCommon;
using DashboardApi.Application.Project;
using DashboardApi.Auth.CurrentUser;
using DashboardApi.Auth.PermisionChecker;
using DashboardApi.DatabaseContext.DapperDbContext;
using DashboardApi.Dtos.Maintenance;
using DashboardApi.Dtos.QaQc.Requests;
using DashboardApi.HttpConfig;
using Newtonsoft.Json;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace DashboardApi.Application.DashboardDigicheck
{
    public class DashboardDigicheckService : Service, IDashboardDigicheckService
    {
        #region Declare
        private readonly DigiCheckDapperContext _appDigicheckDbContext;
        private readonly AppMainDapperContext _appMainDapper;
        private readonly IPermissionChecker _permissionChecker;
        public readonly IMapper _mapper;
        public readonly IBaseCommonService _baseCommonService;
        public readonly ICurrentUser _currentUser;
        public readonly IUnitsService _unitsService;
        #endregion

        #region Constructor
        public DashboardDigicheckService(
            IPermissionChecker permissionChecker,
            IMapper mapper,
            AppMainDapperContext appMainDapper,
            ICurrentUser currentUser,
            IBaseCommonService baseCommonService,
            DigiCheckDapperContext appDigicheckDbContext,
            IUnitsService unitsService)
        {
            _permissionChecker = permissionChecker;
            _mapper = mapper;
            _appMainDapper = appMainDapper;
            _currentUser = currentUser;
            _baseCommonService = baseCommonService;
            _appDigicheckDbContext = appDigicheckDbContext;
            _unitsService = unitsService;
        }
        #endregion

        #region Report progress table

        /// <summary>
        /// Func get data for report ProgressTableBlock
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (12.09.2023)
        public async Task<ServiceResponse> ProgressTableBlock(string request)
        {
            using var connectionDigicheck = _appDigicheckDbContext.CreateConnection();
            ResourceManager rm = new ResourceManager("DashboardApi.Resource.QueryDigicheck", Assembly.GetExecutingAssembly());
            List<DigicheckProgressTableRequest> result = new List<DigicheckProgressTableRequest>();
            SummaryRequest requestConvert = new SummaryRequest();
            string projectName = "";

            try
            {
                // init data
                string whereSQL = $"1=1",
                       querySQL = rm.GetString("DigicheckPPVCProgressTable", CultureInfo.CurrentCulture);

                // build where query
                if (!string.IsNullOrEmpty(querySQL))
                {
                    if (!string.IsNullOrWhiteSpace(request))
                    {
                        if ((request.StartsWith("{") && request.EndsWith("}")) || (request.StartsWith("[") && request.EndsWith("]")))
                        {
                            requestConvert = JsonConvert.DeserializeObject<SummaryRequest>(request);

                            if (requestConvert.PrecastType != "PPVC")
                            {
                                querySQL = rm.GetString("DigicheckProgressTable", CultureInfo.CurrentCulture);
                            }

                            // filter by project
                            if (requestConvert.listProject != null && requestConvert.listProject.Count() > 0)
                            {
                                projectName = requestConvert.listProject.ToArray().FirstOrDefault();
                                whereSQL = $"{whereSQL} AND MP.SITE_ID IN ('{string.Join("','", requestConvert.listProject.ToArray())}')";
                            }
                            // list id
                            if (!string.IsNullOrEmpty(requestConvert.IdFlowCasting)
                                && !string.IsNullOrEmpty(requestConvert.IdFlowFitOut)
                                && !string.IsNullOrEmpty(requestConvert.IdFlowPrestorage)
                                && !string.IsNullOrEmpty(requestConvert.IdFlowOnSite))
                            {
                                querySQL = querySQL.Replace("@IDFLOWCASTING", $"'{requestConvert.IdFlowCasting}'");
                                querySQL = querySQL.Replace("@IDFLOWFITOUT", $"'{requestConvert.IdFlowFitOut}'");
                                querySQL = querySQL.Replace("@IDFLOWPRESTORAGE", $"'{requestConvert.IdFlowPrestorage}'");
                                querySQL = querySQL.Replace("@IDFLOWONSITE", $"'{requestConvert.IdFlowOnSite}'");
                            }
                            else
                            {
                                return Ok(result, message: "Do not have enough id flow");
                            }

                            if (!string.IsNullOrEmpty(requestConvert.PrecastType))
                            {
                                querySQL = querySQL.Replace("@PRECAST_TYPE", $"'{requestConvert.PrecastType}'").Replace("@PRECASTTYPE_HANDLE", $"'{requestConvert.PrecastType.Replace(" ", "").Replace("_", "")}'");
                            }

                            // filter by date
                            if (!string.IsNullOrWhiteSpace(requestConvert.gteDate))
                            {
                                whereSQL = $"{whereSQL} AND MP.UPDATED_AT <= '{requestConvert.gteDate}'";
                            }
                            if (!string.IsNullOrWhiteSpace(requestConvert.lteDate))
                            {
                                whereSQL = $"{whereSQL} AND MP.UPDATED_AT >= '{requestConvert.lteDate}'";
                            }
                        }
                    }
                }

                // get all data unit 
                // var lstUnit = await _unitsService.GetUnits(projectName);

                // query
                using (var resultQuery = await connectionDigicheck.QueryMultipleAsync(querySQL.Replace("@WHERESQL", whereSQL), new { }))
                {
                    result = (await resultQuery.ReadAsync<DigicheckProgressTableRequest>()).ToList();
                }

                connectionDigicheck.Close();
                connectionDigicheck.Dispose();
            }
            catch (Exception ce)
            {
                return BadRequest(ce.Message);
            }

            return Ok(result);
        }

        /// <summary>
        /// Func get data for report ProgressTableBlock
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (12.09.2023)
        public async Task<ServiceResponse> ProgressTableUnit(string request)
        {
            using var connectionDigicheck = _appDigicheckDbContext.CreateConnection();
            ResourceManager rm = new ResourceManager("DashboardApi.Resource.QueryDigicheck", Assembly.GetExecutingAssembly());
            List<DigicheckProgressTableUnitRequest> result = new List<DigicheckProgressTableUnitRequest>();

            try
            {
                // init data
                string whereSQL = $"1=1",
                       querySQL = rm.GetString("DigicheckPPVCProgressTableUnit", CultureInfo.CurrentCulture);

                // build where query
                if (!string.IsNullOrEmpty(querySQL) && !string.IsNullOrWhiteSpace(request))
                {
                    if ((request.StartsWith("{") && request.EndsWith("}")) || (request.StartsWith("[") && request.EndsWith("]")))
                    {
                        SummaryRequest requestConvert = JsonConvert.DeserializeObject<SummaryRequest>(request);

                        if (requestConvert.PrecastType != "PPVC")
                        {
                            querySQL = rm.GetString("DigicheckProgressTableUnit", CultureInfo.CurrentCulture);
                        }

                        // filter by project
                        if (requestConvert.listProject != null && requestConvert.listProject.Count() > 0)
                        {
                            whereSQL = $"{whereSQL} AND MP.SITE_ID IN ('{string.Join("','", requestConvert.listProject.ToArray())}')";
                        }
                        if (requestConvert.FlowID != null && requestConvert.FlowID.Count() > 0)
                        {
                            whereSQL = $"{whereSQL} AND S.FLOW_ID IN ('{string.Join("','", requestConvert.FlowID.ToArray())}')";
                        }
                        if (!string.IsNullOrEmpty(requestConvert.PrecastType))
                        {
                            querySQL = querySQL.Replace("@PRECAST_TYPE", $"'{requestConvert.PrecastType}'").Replace("@PRECASTTYPE_HANDLE", $"'{requestConvert.PrecastType.Replace(" ", "").Replace("_", "")}'");
                        }

                        // list id
                        if (!string.IsNullOrEmpty(requestConvert.IdFlowCasting)
                            && !string.IsNullOrEmpty(requestConvert.IdFlowFitOut)
                            && !string.IsNullOrEmpty(requestConvert.IdFlowPrestorage)
                            && !string.IsNullOrEmpty(requestConvert.IdFlowOnSite))
                        {
                            querySQL = querySQL.Replace("@IDFLOWCASTING", $"'{requestConvert.IdFlowCasting}'");
                            querySQL = querySQL.Replace("@IDFLOWFITOUT", $"'{requestConvert.IdFlowFitOut}'");
                            querySQL = querySQL.Replace("@IDFLOWPRESTORAGE", $"'{requestConvert.IdFlowPrestorage}'");
                            querySQL = querySQL.Replace("@IDFLOWONSITE", $"'{requestConvert.IdFlowOnSite}'");
                        }
                        else
                        {
                            return Ok(result, message: "Do not have enough id flow");
                        }

                        // filter by date
                        if (!string.IsNullOrWhiteSpace(requestConvert.gteDate))
                        {
                            whereSQL = $"{whereSQL} AND MP.UPDATED_AT <= '{requestConvert.gteDate}'";
                        }
                        if (!string.IsNullOrWhiteSpace(requestConvert.lteDate))
                        {
                            whereSQL = $"{whereSQL} AND MP.UPDATED_AT >= '{requestConvert.lteDate}'";
                        }
                    }
                }

                // query
                using (var resultQuery = await connectionDigicheck.QueryMultipleAsync(querySQL.Replace("@WHERESQL", whereSQL), new { }))
                {
                    result = (await resultQuery.ReadAsync<DigicheckProgressTableUnitRequest>()).ToList();
                }

                connectionDigicheck.Close();
                connectionDigicheck.Dispose();
            }
            catch (Exception ce)
            {
                return BadRequest(ce.Message);
            }

            return Ok(result);
        }

        /// <summary>
        /// Func get casting completion
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (05.01.2024)
        public async Task<ServiceResponse> CastingCompletion(string request)
        {
            using var connectionDigicheck = _appDigicheckDbContext.CreateConnection();
            ResourceManager rm = new ResourceManager("DashboardApi.Resource.QueryDigicheck", Assembly.GetExecutingAssembly());
            Dictionary<string, dynamic> result = new Dictionary<string, dynamic>();

            try
            {
                // init data
                string whereSQL = $"1=1",
                       whereUpdate = "",
                       querySQL1 = rm.GetString("DigicheckPPVCTotalModulesV2", CultureInfo.CurrentCulture),
                       querySQL2 = rm.GetString("DigicheckPPVCTotalM3RebarV2", CultureInfo.CurrentCulture);

                // build where query
                if (!string.IsNullOrEmpty(querySQL1) && !string.IsNullOrEmpty(querySQL2) && !string.IsNullOrWhiteSpace(request))
                {
                    if ((request.StartsWith("{") && request.EndsWith("}")) || (request.StartsWith("[") && request.EndsWith("]")))
                    {
                        SummaryRequest requestConvert = JsonConvert.DeserializeObject<SummaryRequest>(request);

                        // filter by project
                        if (!string.IsNullOrEmpty(requestConvert.PrecastType))
                        {
                            querySQL1 = querySQL1.Replace("@PRECAST_TYPE", $"'{requestConvert.PrecastType}'").Replace("@PRECASTTYPE_HANDLE", $"'{requestConvert.PrecastType.Replace(" ", "").Replace("_", "")}'");
                            querySQL2 = querySQL2.Replace("@PRECAST_TYPE", $"'{requestConvert.PrecastType}'").Replace("@PRECASTTYPE_HANDLE", $"'{requestConvert.PrecastType.Replace(" ", "").Replace("_", "")}'");
                        }

                        // filter by project
                        if (requestConvert.listProject != null && requestConvert.listProject.Count() > 0)
                        {
                            whereSQL = $"{whereSQL} AND MP.SITE_ID IN ('{string.Join("','", requestConvert.listProject.ToArray())}')";
                        }

                        // filter by date
                        if (!string.IsNullOrWhiteSpace(requestConvert.gteDate))
                        {
                            whereUpdate = $"{whereUpdate} AND MP.UPDATED_AT <= '{requestConvert.gteDate}'";
                        }
                        if (!string.IsNullOrWhiteSpace(requestConvert.lteDate))
                        {
                            whereUpdate = $"{whereUpdate} AND MP.UPDATED_AT >= '{requestConvert.lteDate}'";
                        }

                        querySQL1 = querySQL1.Replace("@IDFLOWCASTING", $"'{requestConvert.IdFlowCasting}'");
                        querySQL2 = querySQL2.Replace("@IDFLOWCASTING", $"'{requestConvert.IdFlowCasting}'");
                    }
                }

                // query
                using (var resultQuery = await connectionDigicheck.QueryMultipleAsync($"{querySQL1.Replace("@WHERECLAUSE", whereSQL).Replace("@WHEREDATE", whereUpdate)} {querySQL2.Replace("@WHERECLAUSE", whereSQL)}", new { }))
                {
                    var tempModules = (await resultQuery.ReadAsync<dynamic>()).ToList();
                    var tempM3Rebars = (await resultQuery.ReadAsync<dynamic>()).ToList();
                    result["modules"] = tempModules;
                    result["m3Rebars"] = tempM3Rebars;
                }

                connectionDigicheck.Close();
                connectionDigicheck.Dispose();
            }
            catch (Exception ce)
            {
                return BadRequest(ce.Message);
            }

            return Ok(result);
        }

        /// <summary>
        /// Func get start date + end date of each month by year
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (08.01.2024)
        static List<(string MonthName, DateTime StartDate, DateTime EndDate)> GetStartEndDatesForYear(int year)
        {
            // lst month declare
            List<string> months = new List<string>() { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

            List<(string MonthName, DateTime StartDate, DateTime EndDate)> result = new List<(string MonthName, DateTime, DateTime)>();

            for (int month = 1; month <= 12; month++)
            {
                DateTime startDate = new DateTime(year, month, 1);
                DateTime endDate = startDate.AddMonths(1).AddDays(-1);

                result.Add((months[month - 1], startDate, endDate));
            }

            return result;
        }

        /// <summary>
        /// Get monthly report for digiechk dashboard
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (08.10.2024)
        public async Task<ServiceResponse> DigicheckDashboardMonthly(string request)
        {
            using var connectionDigicheck = _appDigicheckDbContext.CreateConnection();
            ResourceManager rm = new ResourceManager("DashboardApi.Resource.QueryDigicheck", Assembly.GetExecutingAssembly());
            List<dynamic> result = new List<dynamic>();

            try
            {
                // init data
                string whereSQL = $"1=1",
                       querySQL = rm.GetString("DigicheckPPVCInProgressModulesV2", CultureInfo.CurrentCulture),
                       queryTotal = string.Empty;

                // build where query
                if (!string.IsNullOrEmpty(querySQL) && !string.IsNullOrWhiteSpace(request))
                {
                    if ((request.StartsWith("{") && request.EndsWith("}")) || (request.StartsWith("[") && request.EndsWith("]")))
                    {
                        SummaryRequest requestConvert = JsonConvert.DeserializeObject<SummaryRequest>(request);

                        // filter by project
                        if (requestConvert.listProject != null && requestConvert.listProject.Count() > 0)
                        {
                            whereSQL = $"{whereSQL} AND MP.SITE_ID IN ('{string.Join("','", requestConvert.listProject.ToArray())}')";
                        }

                        if (!string.IsNullOrEmpty(requestConvert.PrecastType))
                        {
                            querySQL = querySQL.Replace("@PRECAST_TYPE", $"'{requestConvert.PrecastType}'").Replace("@PRECASTTYPE_HANDLE", $"'{requestConvert.PrecastType.Replace(" ", "").Replace("_", "")}'");
                        }

                        querySQL = querySQL.Replace("@IDFLOWCASTING", $"'{requestConvert.IdFlowCasting}'");
                        querySQL = querySQL.Replace("@IDFLOWPRESTORAGE", $"'{requestConvert.IdFlowPrestorage}'");
                        querySQL = querySQL.Replace("@IDFLOWONSITE", $"'{requestConvert.IdFlowOnSite}'");
                        querySQL = querySQL.Replace("@IDFLOWMEP", $"'{requestConvert.IdFlowMEP}'");
                    }
                }


                using (var resultQuery = await connectionDigicheck.QueryMultipleAsync(querySQL.Replace("@WHERECLAUSE", whereSQL), new { }))
                {
                    result = (await resultQuery.ReadAsync<dynamic>()).ToList();
                }

                connectionDigicheck.Close();
                connectionDigicheck.Dispose();

            }
            catch (Exception ce)
            {
                return BadRequest(ce.Message);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get monthly increase report for digiechk dashboard
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (08.10.2024)
        public async Task<ServiceResponse> DigicheckDashboardMonthlyIncrease(string request)
        {
            using var connectionDigicheck = _appDigicheckDbContext.CreateConnection();
            ResourceManager rm = new ResourceManager("DashboardApi.Resource.QueryDigicheck", Assembly.GetExecutingAssembly());
            List<dynamic> result = new List<dynamic>();
            List<(string MonthName, DateTime StartDate, DateTime EndDate)> months = new List<(string MonthName, DateTime StartDate, DateTime EndDate)>();

            try
            {
                // init data
                string whereSQL = $"1=1",
                       querySQL = rm.GetString("DigicheckPPVCInProgressModulesIncreaseV2", CultureInfo.CurrentCulture),
                       queryTotal = string.Empty;

                // get current date
                var currentDate = DateTime.Today;
                var lastCurrentMonth = new DateTime(currentDate.Year, currentDate.Month, DateTime.DaysInMonth(currentDate.Year, currentDate.Month));

                // build where query
                if (!string.IsNullOrEmpty(querySQL) && !string.IsNullOrWhiteSpace(request))
                {
                    if ((request.StartsWith("{") && request.EndsWith("}")) || (request.StartsWith("[") && request.EndsWith("]")))
                    {
                        SummaryRequest requestConvert = JsonConvert.DeserializeObject<SummaryRequest>(request);

                        if (!string.IsNullOrEmpty(requestConvert.PrecastType))
                        {
                            querySQL = querySQL.Replace("@PRECAST_TYPE", $"'{requestConvert.PrecastType}'").Replace("@PRECASTTYPE_HANDLE", $"'{requestConvert.PrecastType.Replace(" ", "").Replace("_", "")}'");
                        }

                        // filter by project
                        if (requestConvert.listProject != null && requestConvert.listProject.Count() > 0)
                        {
                            whereSQL = $"{whereSQL} AND MP.SITE_ID IN ('{string.Join("','", requestConvert.listProject.ToArray())}')";
                        }

                        querySQL = querySQL.Replace("@IDFLOWCASTING", $"'{requestConvert.IdFlowCasting}'");
                        querySQL = querySQL.Replace("@IDFLOWPRESTORAGE", $"'{requestConvert.IdFlowPrestorage}'");
                        querySQL = querySQL.Replace("@IDFLOWONSITE", $"'{requestConvert.IdFlowOnSite}'");
                        querySQL = querySQL.Replace("@IDFLOWMEP", $"'{requestConvert.IdFlowMEP}'");
                    }
                }

                using (var resultQuery = await connectionDigicheck.QueryMultipleAsync(querySQL.Replace("@WHERECLAUSE", whereSQL), new { }))
                {
                    result = (await resultQuery.ReadAsync<dynamic>()).ToList();
                }

                connectionDigicheck.Close();
                connectionDigicheck.Dispose();
            }
            catch (Exception ce)
            {
                return BadRequest(ce.Message);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get in progress modules
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (07.10.2024)
        public async Task<ServiceResponse> InProgressModules(string request)
        {
            using var connectionDigicheck = _appDigicheckDbContext.CreateConnection();
            ResourceManager rm = new ResourceManager("DashboardApi.Resource.QueryDigicheck", Assembly.GetExecutingAssembly());
            List<dynamic> result = new List<dynamic>();

            try
            {
                // init data
                string whereSQL = $"1=1",
                       querySQL = rm.GetString("DigicheckPPVCInProgressModulesV2", CultureInfo.CurrentCulture);

                // build where query
                if (!string.IsNullOrEmpty(querySQL) && !string.IsNullOrWhiteSpace(request))
                {
                    if ((request.StartsWith("{") && request.EndsWith("}")) || (request.StartsWith("[") && request.EndsWith("]")))
                    {
                        SummaryRequest requestConvert = JsonConvert.DeserializeObject<SummaryRequest>(request);

                        if (!string.IsNullOrEmpty(requestConvert.PrecastType))
                        {
                            querySQL = querySQL.Replace("@PRECAST_TYPE", $"'{requestConvert.PrecastType}'").Replace("@PRECASTTYPE_HANDLE", $"'{requestConvert.PrecastType.Replace(" ", "").Replace("_", "")}'");
                        }

                        // filter by project
                        if (requestConvert.listProject != null && requestConvert.listProject.Count() > 0)
                        {
                            whereSQL = $"{whereSQL} AND MP.SITE_ID IN ('{string.Join("','", requestConvert.listProject.ToArray())}')";
                        }

                        // filter by date
                        if (!string.IsNullOrWhiteSpace(requestConvert.gteDate))
                        {
                            whereSQL = $"{whereSQL} AND MP.UPDATED_AT <= '{requestConvert.gteDate}'";
                        }
                        if (!string.IsNullOrWhiteSpace(requestConvert.lteDate))
                        {
                            whereSQL = $"{whereSQL} AND MP.UPDATED_AT >= '{requestConvert.lteDate}'";
                        }

                        querySQL = querySQL.Replace("@IDFLOWCASTING", $"'{requestConvert.IdFlowCasting}'");
                        querySQL = querySQL.Replace("@IDFLOWPRESTORAGE", $"'{requestConvert.IdFlowPrestorage}'");
                        querySQL = querySQL.Replace("@IDFLOWONSITE", $"'{requestConvert.IdFlowOnSite}'");
                        querySQL = querySQL.Replace("@IDFLOWMEP", $"'{requestConvert.IdFlowMEP}'");
                    }
                }

                // query
                using (var resultQuery = await connectionDigicheck.QueryMultipleAsync(querySQL.Replace("@WHERECLAUSE", whereSQL), new { }))
                {
                    result = (await resultQuery.ReadAsync<dynamic>()).ToList();
                }

                connectionDigicheck.Close();
                connectionDigicheck.Dispose();
            }
            catch (Exception ce)
            {
                return BadRequest(ce.Message);
            }

            return Ok(result);
        }
        #endregion
    }
}