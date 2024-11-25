using AutoMapper;
using Dapper;
using DashboardApi.Application.AttributeCustom;
using DashboardApi.Application.BaseCommon;
using DashboardApi.Application.DashboardMaintenance;
using DashboardApi.Auth.CurrentUser;
using DashboardApi.Auth.PermisionChecker;
using DashboardApi.DatabaseContext.DapperDbContext;
using DashboardApi.Dtos.Maintenance;
using DashboardApi.Dtos.QaQc.Requests;
using DashboardApi.HttpConfig;
using Newtonsoft.Json;
using OfficeOpenXml;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace DashboardApi.Application.Maintenance
{
    public class DashboardMaintenanceService : Service, IDashboardMaintenanceService
    {
        #region Declare
        private readonly MaintenanceDbContext _appMaintenanceDbContext;
        private readonly AppMainDapperContext _appMainDapper;
        private readonly IPermissionChecker _permissionChecker;
        public readonly IMapper _mapper;
        public readonly IBaseCommonService _baseCommonService;
        public readonly ICurrentUser _currentUser;
        #endregion

        #region Constructor
        public DashboardMaintenanceService(
            IPermissionChecker permissionChecker,
            IMapper mapper,
            AppMainDapperContext appMainDapper,
            ICurrentUser currentUser,
            IBaseCommonService baseCommonService,
            MaintenanceDbContext appMaintenanceDbContext)
        {
            _permissionChecker = permissionChecker;
            _mapper = mapper;
            _appMainDapper = appMainDapper;
            _currentUser = currentUser;
            _baseCommonService = baseCommonService;
            _appMaintenanceDbContext = appMaintenanceDbContext;
        }
        #endregion

        #region Method Summary DLP Page

        /// <summary>
        /// get report summary total case defects
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ServiceResponse> DLPSummaryTotalCasesDefects(string request)
        {
            using var connectionMainteance = _appMaintenanceDbContext.CreateConnection();
            ResourceManager rm = new ResourceManager("DashboardApi.Resource.QueryMaintenance", Assembly.GetExecutingAssembly());
            Dictionary<string, List<SummaryCasesDefectsRequest>> result = new Dictionary<string, List<SummaryCasesDefectsRequest>>();

            try
            {
                // init data
                string whereSQLDefects = $"1=1",
                       whereSQLCases = $"1=1",
                       querySQL = rm.GetString("SummaryDLPTotalCasesDefectsProject", CultureInfo.CurrentCulture);

                // build where query
                if (!string.IsNullOrEmpty(querySQL))
                {
                    if (!string.IsNullOrWhiteSpace(request))
                    {
                        if ((request.StartsWith("{") && request.EndsWith("}")) || (request.StartsWith("[") && request.EndsWith("]")))
                        {
                            SummaryRequest requestConvert = JsonConvert.DeserializeObject<SummaryRequest>(request);

                            // filter by project
                            if (requestConvert.listProject != null && requestConvert.listProject.Count() > 0)
                            {
                                whereSQLCases = whereSQLDefects = $"{whereSQLDefects} AND M.PROJECT_ID IN ('{string.Join("','", requestConvert.listProject.ToArray())}')";
                            }

                            // filter by date
                            if (!string.IsNullOrWhiteSpace(requestConvert.gteDate))
                            {
                                whereSQLCases = $"{whereSQLCases} AND CM.CREATION_DATE <= '{requestConvert.gteDate}'";
                                whereSQLDefects = $"{whereSQLDefects} AND DM.CONFIRMATION_DATE <= '{requestConvert.gteDate}'";
                            }
                            if (!string.IsNullOrWhiteSpace(requestConvert.lteDate))
                            {
                                whereSQLCases = $"{whereSQLCases} AND CM.CREATION_DATE >= '{requestConvert.lteDate}'";
                                whereSQLDefects = $"{whereSQLDefects} AND DM.CONFIRMATION_DATE >= '{requestConvert.lteDate}'";
                            }
                        }
                    }

                    // query
                    using (var resultQuery = await connectionMainteance.QueryMultipleAsync(querySQL.Replace("@WHERESQLCASES", whereSQLCases).Replace("@WHERESQLDEFECTS", whereSQLDefects), new { }))
                    {
                        var cases = (await resultQuery.ReadAsync<SummaryCasesDefectsRequest>()).ToList();
                        var defects = (await resultQuery.ReadAsync<SummaryCasesDefectsRequest>()).ToList();

                        result.Add("SummaryTotalCases", cases);
                        result.Add("SummaryTotalDefects", defects);
                    }
                }
            }
            catch (Exception ce)
            {
                return Ok(result, ce.Message);
            }

            return Ok(result);
        }

        /// <summary>
        /// Func get summary for Total Number of Defect Lists By Statuses
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (30.05.2023)
        public async Task<ServiceResponse> DLPSummaryTotalDefectsStatues(string request)
        {
            using var connectionMainteance = _appMaintenanceDbContext.CreateConnection();
            ResourceManager rm = new ResourceManager("DashboardApi.Resource.QueryMaintenance", Assembly.GetExecutingAssembly());
            Dictionary<string, object> result = new Dictionary<string, object>();

            try
            {
                // init data
                string whereSQLDefects = $"1=1",
                       querySQL = rm.GetString("SummaryDLPTotalDefectsStatues", CultureInfo.CurrentCulture);
                bool isTimeLine = false;
                bool isOverview = true;

                // build where query
                if (!string.IsNullOrEmpty(querySQL))
                {
                    if (!string.IsNullOrWhiteSpace(request))
                    {
                        if ((request.StartsWith("{") && request.EndsWith("}")) || (request.StartsWith("[") && request.EndsWith("]")))
                        {
                            SummaryRequest requestConvert = JsonConvert.DeserializeObject<SummaryRequest>(request);

                            // filter by project
                            if (requestConvert.listProject != null && requestConvert.listProject.Count() > 0)
                            {
                                whereSQLDefects = $"{whereSQLDefects} AND M.PROJECT_ID IN ('{string.Join("','", requestConvert.listProject.ToArray())}')";
                            }

                            // filter by date
                            if (!string.IsNullOrWhiteSpace(requestConvert.gteDate))
                            {
                                whereSQLDefects = $"{whereSQLDefects} AND DM.CONFIRMATION_DATE <= '{requestConvert.gteDate}'";
                            }
                            if (!string.IsNullOrWhiteSpace(requestConvert.lteDate))
                            {
                                whereSQLDefects = $"{whereSQLDefects} AND DM.CONFIRMATION_DATE >= '{requestConvert.lteDate}'";
                            }

                            // filter by project
                            if (requestConvert.TotalNumberDefectStatuses != null && !string.IsNullOrWhiteSpace(requestConvert.TotalNumberDefectStatuses))
                            {
                                switch (requestConvert.TotalNumberDefectStatuses)
                                {
                                    case "Project":
                                        isOverview = false;
                                        querySQL = rm.GetString("SummaryDLPTotalDefectsStatuesProject", CultureInfo.CurrentCulture);
                                        break;
                                    case "Timeline":
                                        isTimeLine = true;
                                        isOverview = false;
                                        querySQL = rm.GetString("SummaryDLPTotalDefectsStatuestTimeline", CultureInfo.CurrentCulture);
                                        break;
                                }
                            }
                        }
                    }

                    querySQL += rm.GetString("SummaryDLPTotalDefectsStatuesProjectDateTime", CultureInfo.CurrentCulture);

                    // query
                    using (var resultQuery = await connectionMainteance.QueryMultipleAsync(querySQL.Replace("@WHERESQLDEFECTS", whereSQLDefects), new { }))
                    {
                        var defects = (await resultQuery.ReadAsync<SummaryCasesDefectsRequest>()).ToList();

                        foreach (var defect in defects)
                        {
                            switch (defect.Status.ToLower())
                            {
                                case "closed":
                                    defect.Status = "Defect List Signed Off";
                                    break;
                                case "completed":
                                    defect.Status = "Defect List Pending Joint Inspection (Work Completed)";
                                    break;
                                case "wip":
                                    defect.Status = "Defect List Outstanding";
                                    break;
                                case "ps":
                                    defect.Status = "Defect List Outstanding";
                                    break;
                                case "new":
                                    defect.Status = "Defect List New (Pending Joint Inspection)";
                                    break;
                                default:
                                    defect.Status = "Other";
                                    break;
                            }
                        }

                        if (isTimeLine)
                        {
                            var handleResult = defects.GroupBy(x => x.DateConcat).ToList();
                            result.Add("result", handleResult);
                        }
                        else
                        {
                            if (isOverview)
                            {
                                defects = defects.GroupBy(l => l.Status)
                                                .Select(cl => new SummaryCasesDefectsRequest
                                                {
                                                    Status = cl.First().Status,
                                                    ProjectID = cl.First().ProjectID,
                                                    ProjectName = cl.First().ProjectName,
                                                    DateConcat = cl.First().DateConcat,
                                                    TotalMaintenance = cl.Sum(c => c.TotalMaintenance),
                                                }).ToList();
                            }

                            result.Add("result", defects);
                        }

                        var minMaxDate = (await resultQuery.ReadAsync<SummaryCasesDefectsRequest>()).ToList();
                        result.Add("minMaxDate", minMaxDate);
                    }
                }
            }
            catch (Exception ce)
            {
                return Ok(message: ce.Message);
            }

            return Ok(result);
        }

        /// <summary>
        /// Func get summary for Total Number of Defect Lists Overdue Type
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (02.06.2023)
        public async Task<ServiceResponse> DLPSummaryTotalDefectsOverdueType(string request)
        {
            using var connectionMainteance = _appMaintenanceDbContext.CreateConnection();
            ResourceManager rm = new ResourceManager("DashboardApi.Resource.QueryMaintenance", Assembly.GetExecutingAssembly());
            Dictionary<string, object> result = new Dictionary<string, object>();

            try
            {
                // init data
                string whereSQLDefects = $"1=1",
                       querySQL = rm.GetString("SummaryDLPTotalDefectOverdueType", CultureInfo.CurrentCulture);

                // build where query
                if (!string.IsNullOrEmpty(querySQL))
                {
                    if (!string.IsNullOrWhiteSpace(request))
                    {
                        if ((request.StartsWith("{") && request.EndsWith("}")) || (request.StartsWith("[") && request.EndsWith("]")))
                        {
                            SummaryRequest requestConvert = JsonConvert.DeserializeObject<SummaryRequest>(request);

                            // filter by project
                            if (requestConvert.listProject != null && requestConvert.listProject.Count() > 0)
                            {
                                whereSQLDefects = $"{whereSQLDefects} AND M.PROJECT_ID IN ('{string.Join("','", requestConvert.listProject.ToArray())}')";
                            }

                            // filter by date
                            if (!string.IsNullOrWhiteSpace(requestConvert.gteDate))
                            {
                                whereSQLDefects = $"{whereSQLDefects} AND DM.CONFIRMATION_DATE <= '{requestConvert.gteDate}'";
                            }
                            if (!string.IsNullOrWhiteSpace(requestConvert.lteDate))
                            {
                                whereSQLDefects = $"{whereSQLDefects} AND DM.CONFIRMATION_DATE >= '{requestConvert.lteDate}'";
                            }
                        }
                    }

                    // query
                    using (var resultQuery = await connectionMainteance.QueryMultipleAsync(querySQL.Replace("@WHERESQLDEFECTS", whereSQLDefects), new { }))
                    {
                        var readResult = (await resultQuery.ReadAsync<TempSummaryCasesDefectsRequest>()).ToList().FirstOrDefault();
                        result.Add("Overdue More 30 Days", readResult.OverdueMore30Days);
                        result.Add("Overdue 14 To 30 Days", readResult.Overdue14To30Days);
                        result.Add("Over due On Track", readResult.OverdueOnTrack);
                    }
                }
            }
            catch (Exception ce)
            {
                return Ok(message: ce.Message);
            }

            return Ok(result);
        }

        /// <summary>
        /// Func get summary for Total Number Of Water Seepages/ Chokages by Status
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (02.06.2023)
        public async Task<ServiceResponse> DLPSummaryTotalWaterSeepagesChokagesStatus(string request)
        {
            using var connectionMainteance = _appMaintenanceDbContext.CreateConnection();
            ResourceManager rm = new ResourceManager("DashboardApi.Resource.QueryMaintenance", Assembly.GetExecutingAssembly());
            Dictionary<string, object> result = new Dictionary<string, object>();

            try
            {
                // init data
                string whereSQLDefects = $"1=1",
                       querySQL = rm.GetString("SummaryDLPTotalWaterSeepagesChokagesStatus", CultureInfo.CurrentCulture);

                // build where query
                if (!string.IsNullOrEmpty(querySQL))
                {
                    if (!string.IsNullOrWhiteSpace(request))
                    {
                        if ((request.StartsWith("{") && request.EndsWith("}")) || (request.StartsWith("[") && request.EndsWith("]")))
                        {
                            SummaryRequest requestConvert = JsonConvert.DeserializeObject<SummaryRequest>(request);

                            // filter by project
                            if (requestConvert.listProject != null && requestConvert.listProject.Count() > 0)
                            {
                                whereSQLDefects = $"{whereSQLDefects} AND M.PROJECT_ID IN ('{string.Join("','", requestConvert.listProject.ToArray())}')";
                            }

                            // filter by date
                            if (!string.IsNullOrWhiteSpace(requestConvert.gteDate))
                            {
                                whereSQLDefects = $"{whereSQLDefects} AND CONFIRMATION_DATE <= '{requestConvert.gteDate}'";
                            }
                            if (!string.IsNullOrWhiteSpace(requestConvert.lteDate))
                            {
                                whereSQLDefects = $"{whereSQLDefects} AND CONFIRMATION_DATE >= '{requestConvert.lteDate}'";
                            }
                        }
                    }

                    // query
                    using (var resultQuery = await connectionMainteance.QueryMultipleAsync(querySQL.Replace("@WHERESQLDEFECTS", whereSQLDefects), new { }))
                    {
                        var waters = ConvertStatus((await resultQuery.ReadAsync<TempSummaryCasesDefectsRequest>()).ToList());
                        var chokages = ConvertStatus((await resultQuery.ReadAsync<TempSummaryCasesDefectsRequest>()).ToList());

                        var waterResult = new Dictionary<string, object>();
                        var chokageResult = new Dictionary<string, object>();

                        foreach (var water in waters)
                        {
                            waterResult.Add(water.Status.Replace(" ", ""), water.TotalMaintenance);
                        }

                        foreach (var chokage in chokages)
                        {
                            chokageResult.Add(chokage.Status.Replace(" ", ""), chokage.TotalMaintenance);
                        }

                        result.Add("water", waterResult);
                        result.Add("chokage", chokageResult);
                    }
                }
            }
            catch (Exception ce)
            {
                return Ok(message: ce.Message);
            }

            return Ok(result);
        }

        /// <summary>
        /// Func get summary for Total Number Of Water Seepages/ Chokages by Status detail
        /// Water Seepages and Chokages Details
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (02.06.2023)
        public async Task<ServiceResponse> DLPSummaryTotalWaterSeepagesChokagesDetails(string request)
        {
            using var connectionMainteance = _appMaintenanceDbContext.CreateConnection();
            ResourceManager rm = new ResourceManager("DashboardApi.Resource.QueryMaintenance", Assembly.GetExecutingAssembly());
            Dictionary<string, object> result = new Dictionary<string, object>();
            bool isDefects = false;

            try
            {
                // init data
                string whereSQLDefects = $"1=1",
                       querySQL = rm.GetString("SummaryDLPTotalWaterSeepagesChokagesStatusDetailCases", CultureInfo.CurrentCulture);

                // build where query
                if (!string.IsNullOrEmpty(querySQL))
                {
                    if (!string.IsNullOrWhiteSpace(request))
                    {
                        if ((request.StartsWith("{") && request.EndsWith("}")) || (request.StartsWith("[") && request.EndsWith("]")))
                        {
                            SummaryRequest requestConvert = JsonConvert.DeserializeObject<SummaryRequest>(request);

                            // filter detail
                            if (!string.IsNullOrWhiteSpace(requestConvert.typeProject))
                            {
                                if (requestConvert.typeProject.ToLower() == "2")
                                {
                                    isDefects = true;
                                    querySQL = rm.GetString("SummaryDLPTotalWaterSeepagesChokagesStatusDetailDefects", CultureInfo.CurrentCulture);
                                }
                            }

                            // filter by project
                            if (requestConvert.listProject != null && requestConvert.listProject.Count() > 0)
                            {
                                whereSQLDefects = $"{whereSQLDefects} AND M.PROJECT_ID IN ('{string.Join("','", requestConvert.listProject.ToArray())}')";
                            }

                            // filter by date
                            if (!string.IsNullOrWhiteSpace(requestConvert.gteDate))
                            {
                                whereSQLDefects = $"{whereSQLDefects} AND DM.CONFIRMATION_DATE <= '{requestConvert.gteDate}'";
                            }
                            if (!string.IsNullOrWhiteSpace(requestConvert.lteDate))
                            {
                                whereSQLDefects = $"{whereSQLDefects} AND DM.CONFIRMATION_DATE >= '{requestConvert.lteDate}'";
                            }

                            // filter by project
                            if (requestConvert.TotalNumberDefectStatuses != null && !string.IsNullOrWhiteSpace(requestConvert.TotalNumberDefectStatuses))
                            {
                                if (requestConvert.TotalNumberDefectStatuses.ToLower() == "timeline")
                                {
                                    querySQL = rm.GetString("SummaryDLPTotalWaterSeepagesChokagesStatusDetailTimeLineCases", CultureInfo.CurrentCulture);

                                    if (isDefects)
                                    {
                                        querySQL = rm.GetString("SummaryDLPTotalWaterSeepagesChokagesStatusDetailTimeLineDefects", CultureInfo.CurrentCulture);
                                    }
                                }
                            }
                        }
                    }

                    // query
                    using (var resultQuery = await connectionMainteance.QueryMultipleAsync(querySQL.Replace("@WHERESQLDEFECTS", whereSQLDefects), new { }))
                    {
                        var readResults = (await resultQuery.ReadAsync<CombineCasesDefectsMonitoringRequest>()).ToList();
                        result.Add("result", readResults);
                    }
                }
            }
            catch (Exception ce)
            {
                return Ok(message: ce.Message);
            }

            return Ok(result);
        }

        #endregion

        #region Detail Total Cases and Defects by Project
        /// <summary>
        /// Func get detail Total Cases and Defects by Project
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (02.06.2023)
        public async Task<ServiceResponse> DLPCasesDefectsProjectOverview(string request)
        {
            using var connectionMainteance = _appMaintenanceDbContext.CreateConnection();
            ResourceManager rm = new ResourceManager("DashboardApi.Resource.QueryMaintenance", Assembly.GetExecutingAssembly());
            Dictionary<string, object> result = new Dictionary<string, object>();

            try
            {
                // init data
                string whereSQLDefects = $"1=1",
                       whereSQLCases = $"1=1",
                       whereSQLMaintenance = $"1=1",
                       querySQL = rm.GetString("DLPDetailProjectOverview", CultureInfo.CurrentCulture);
                bool isUnitView = false,
                     isCases = false;

                // build where query
                if (!string.IsNullOrEmpty(querySQL))
                {
                    if (!string.IsNullOrWhiteSpace(request))
                    {
                        if ((request.StartsWith("{") && request.EndsWith("}")) || (request.StartsWith("[") && request.EndsWith("]")))
                        {
                            SummaryRequest requestConvert = JsonConvert.DeserializeObject<SummaryRequest>(request);

                            // filter by project
                            if (requestConvert.listProject != null && requestConvert.listProject.Count() > 0)
                            {
                                whereSQLMaintenance = whereSQLCases = whereSQLDefects = $"{whereSQLDefects} AND M.PROJECT_ID IN ('{string.Join("','", requestConvert.listProject.ToArray())}')";
                            }

                            // filter by date
                            if (!string.IsNullOrWhiteSpace(requestConvert.gteDate))
                            {
                                whereSQLCases = $"{whereSQLCases} CM.CREATION_DATE <= '{requestConvert.gteDate}')";
                                whereSQLDefects = $"{whereSQLDefects} DM.CONFIRMATION_DATE <= '{requestConvert.gteDate}')";
                            }
                            if (!string.IsNullOrWhiteSpace(requestConvert.lteDate))
                            {
                                whereSQLCases = $"{whereSQLCases} CM.CREATION_DATE >= '{requestConvert.lteDate}')";
                                whereSQLDefects = $"{whereSQLDefects} DM.CONFIRMATION_DATE >= '{requestConvert.lteDate}')";
                            }

                            // filter by project
                            if (requestConvert.TotalNumberDefectStatuses != null && !string.IsNullOrWhiteSpace(requestConvert.TotalNumberDefectStatuses))
                            {
                                switch (requestConvert.TotalNumberDefectStatuses)
                                {
                                    case "Overview":
                                        querySQL = rm.GetString("DLPDetailProjectOverview", CultureInfo.CurrentCulture);
                                        break;
                                    case "UnitView":
                                        isUnitView = true;
                                        querySQL = rm.GetString("DLPDetailProjectUnitView", CultureInfo.CurrentCulture);
                                        break;
                                }
                            }

                            // filter by project
                            if (!isUnitView && requestConvert.typeProject != null && !string.IsNullOrWhiteSpace(requestConvert.typeProject))
                            {
                                switch (requestConvert.typeProject)
                                {
                                    case "1":
                                        isCases = true;
                                        querySQL = rm.GetString("DLPDetailProjectOverviewCases", CultureInfo.CurrentCulture);
                                        break;
                                    case "2":
                                        isUnitView = true;
                                        querySQL = rm.GetString("DLPDetailProjectOverviewDefects", CultureInfo.CurrentCulture);
                                        break;
                                }
                            }
                        }
                    }

                    // query
                    using (var resultQuery = await connectionMainteance.QueryMultipleAsync(
                        querySQL.Replace("@WHERESQLDEFECTS", whereSQLDefects).Replace("@WHERESQLCASES", whereSQLCases).Replace("@WHERESQL", whereSQLMaintenance)
                        , new { }))
                    {

                        var projectTotalUnits = (await resultQuery.ReadAsync<TempSummaryCasesDefectsRequest>()).ToList();
                        var defectCasesNotSubmitted = (await resultQuery.ReadAsync<TempSummaryCasesDefectsRequest>()).ToList();
                        var casesMonitoring = (!isUnitView && isCases) ? (await resultQuery.ReadAsync<CasesMonitoringRequest>()).ToList() : new List<CasesMonitoringRequest>();
                        var defectsMonitoring = !isCases ? (await resultQuery.ReadAsync<DefectsMonitoringRequest>()).ToList() : new List<DefectsMonitoringRequest>();
                        var defectCasesAll = (await resultQuery.ReadAsync<TempSummaryCasesDefectsRequest>()).ToList();

                        projectTotalUnits.FirstOrDefault().DefectsCasesNotSubmitted = defectCasesNotSubmitted?.FirstOrDefault()?.DefectsCasesNotSubmitted;
                        projectTotalUnits.FirstOrDefault().Total = defectCasesAll.FirstOrDefault().Total;

                        result.Add("ProjectTotalUnits", projectTotalUnits);
                        result.Add("DefectCasesNotSubmitted", defectCasesNotSubmitted);
                        result.Add("CasesMonitoring", casesMonitoring);
                        result.Add("DefectsMonitoring", defectsMonitoring);

                        var groupBlocks = defectsMonitoring.GroupBy(x => x.Block).ToList();
                        var listBlockHandleData = new List<Dictionary<string, dynamic>>();

                        foreach (var groupBlock in groupBlocks)
                        {
                            listBlockHandleData.Add(new Dictionary<string, dynamic>()
                            {
                                {groupBlock.Key, groupBlock.DistinctBy(x => x.Unit).GroupBy(x => x.Level).ToList() }
                            });
                        }

                        // handle data for block view by unit
                        result.Add("DefectsMonitoringBlock", new
                        {
                            ListBlock = defectsMonitoring.Select(x => x.Block).Distinct().ToList(),
                            GroupByBlock = listBlockHandleData
                        });
                    }
                }
            }
            catch (Exception ce)
            {
                return Ok(message: ce.Message);
            }

            return Ok(result);
        }

        /// <summary>
        /// Func get export data project detail
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (06.08.2023)
        public async Task<IResult> DLPCasesDefectsProjectOverviewExport(string request)
        {
            using var connectionMainteance = _appMaintenanceDbContext.CreateConnection();
            var excelBytes = new byte[1024];

            try
            {
                // init data
                string whereSQLDefects = $"1=1",
                       querySQL = "SELECT DM.* FROM DEFECTS_MONITORING DM LEFT JOIN MAINTENANCE M ON DM.MAINTENANCE_ID = M.ID WHERE @WHERESQLDEFECTS ORDER BY BLOCK, LEVEL, UNIT";

                // build where query
                if (!string.IsNullOrWhiteSpace(request))
                {
                    if ((request.StartsWith("{") && request.EndsWith("}")) || (request.StartsWith("[") && request.EndsWith("]")))
                    {
                        SummaryRequest requestConvert = JsonConvert.DeserializeObject<SummaryRequest>(request);

                        // filter by project
                        if (requestConvert.listProject != null && requestConvert.listProject.Count() > 0)
                        {
                            whereSQLDefects = $"{whereSQLDefects} AND M.PROJECT_ID IN ('{string.Join("','", requestConvert.listProject.ToArray())}')";
                        }

                        // filter by date
                        if (!string.IsNullOrWhiteSpace(requestConvert.gteDate))
                        {
                            whereSQLDefects = $"{whereSQLDefects} DM.CONFIRMATION_DATE <= '{requestConvert.gteDate}')";
                        }
                        if (!string.IsNullOrWhiteSpace(requestConvert.lteDate))
                        {
                            whereSQLDefects = $"{whereSQLDefects} DM.CONFIRMATION_DATE >= '{requestConvert.lteDate}')";
                        }

                        if (!string.IsNullOrWhiteSpace(requestConvert.type))
                        {
                            if (requestConvert.type == "current" && !string.IsNullOrWhiteSpace(requestConvert.block))
                            {
                                whereSQLDefects = $"{whereSQLDefects} AND DM.BLOCK LIKE '{requestConvert.block}'";
                            }
                        }
                    }
                }

                // query
                using (var resultQuery = await connectionMainteance.QueryMultipleAsync(querySQL.Replace("@WHERESQLDEFECTS", whereSQLDefects), new { }))
                {
                    // get data
                    var data = (await resultQuery.ReadAsync<DefectsMonitoringName>()).ToList();

                    // init value excel
                    using (var package = new ExcelPackage())
                    {
                        var worksheet = package.Workbook.Worksheets.Add("Sheet1");

                        // init cloumn default

                        Dictionary<string, string> _dict = new Dictionary<string, string>();

                        PropertyInfo[] props = typeof(DefectsMonitoringName).GetProperties();
                        foreach (var propTemp in props.Select((value, i) => new { i, value }))
                        {
                            var prop = propTemp.value;
                            var index = propTemp.i;

                            object[] attrs = prop.GetCustomAttributes(true);
                            foreach (object attr in attrs)
                            {
                                ColumnReportName authAttr = attr as ColumnReportName;
                                if (authAttr != null)
                                {
                                    string propName = prop.Name;
                                    string auth = authAttr.Name;

                                    _dict.Add(propName, auth);

                                    worksheet.Cells[1, index + 1].Value = propName;
                                }
                            }
                        }

                        // assign data to excel
                        if (data?.Count > 0)
                        {
                            int row = 2;
                            foreach (var item in data.Select((value, i) => new { i, value }))
                            {
                                var value = item.value;
                                var index = item.i;

                                // assign to cell

                                worksheet.Cells[row, 1].Value = value.Block;
                                worksheet.Cells[row, 2].Value = value.Level;
                                worksheet.Cells[row, 3].Value = value.Unit;
                                worksheet.Cells[row, 4].Value = value.UnitReference;
                                worksheet.Cells[row, 5].Value = value.CaseNumber;
                                worksheet.Cells[row, 6].Value = value.Location;
                                worksheet.Cells[row, 7].Value = value.Type;
                                worksheet.Cells[row, 8].Value = value.Subtype;
                                worksheet.Cells[row, 9].Value = value.Description;
                                worksheet.Cells[row, 10].Value = value.Priority;
                                worksheet.Cells[row, 11].Value = value.Status;
                                worksheet.Cells[row, 12].Value = value.Archived;
                                worksheet.Cells[row, 13].Value = value.Contractor;
                                worksheet.Cells[row, 14].Value = value.Cc;
                                worksheet.Cells[row, 15].Value = value.NbDaysOpen;

                                worksheet.Cells[row, 16].Value = value.CreationDate;
                                worksheet.Cells[row, 16].Style.Numberformat.Format = "dddd, MMMM dd, yyyy h:mm:ss tt";

                                worksheet.Cells[row, 17].Value = value.CreatedBy;

                                worksheet.Cells[row, 18].Value = value.ConfirmationDate;
                                worksheet.Cells[row, 18].Style.Numberformat.Format = "dddd, MMMM dd, yyyy h:mm:ss tt";

                                worksheet.Cells[row, 19].Value = value.ConfirmedBy;

                                worksheet.Cells[row, 20].Value = value.InterventionDate;
                                worksheet.Cells[row, 20].Style.Numberformat.Format = "dddd, MMMM dd, yyyy h:mm:ss tt";
                                worksheet.Cells[row, 21].Value = value.TargetCompletionDate;
                                worksheet.Cells[row, 21].Style.Numberformat.Format = "dddd, MMMM dd, yyyy h:mm:ss tt";

                                worksheet.Cells[row, 22].Value = value.CompletionDate;
                                worksheet.Cells[row, 22].Style.Numberformat.Format = "dddd, MMMM dd, yyyy h:mm:ss tt";

                                worksheet.Cells[row, 23].Value = value.CompletedBy;

                                worksheet.Cells[row, 24].Value = value.ClosingDate;
                                worksheet.Cells[row, 24].Style.Numberformat.Format = "dddd, MMMM dd, yyyy h:mm:ss tt";

                                worksheet.Cells[row, 25].Value = value.ClosedBy;
                                worksheet.Cells[row, 26].Value = value.Tag;
                                worksheet.Cells[row, 27].Value = value.LatestComment;
                                worksheet.Cells[row, 28].Value = value.NbOfDaysOverdue;

                                worksheet.Cells[row, 29].Value = value.WorkStartDate;
                                worksheet.Cells[row, 29].Style.Numberformat.Format = "dddd, MMMM dd, yyyy h:mm:ss tt";

                                worksheet.Cells[row, 30].Value = value.PredictedCompletionDate;
                                worksheet.Cells[row, 31].Value = value.PredictedCompletionDays;

                                row++;
                            }

                            excelBytes = package.GetAsByteArray();
                        }
                    }
                }
            }
            catch (Exception ce)
            {
                return Results.File(new MemoryStream(excelBytes), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Sample.xlsx");
            }

            return Results.File(new MemoryStream(excelBytes), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Sample.xlsx");
        }
        /// <summary>
        /// Func get detail Total Cases and Defects by Project
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (02.06.2023)
        public async Task<ServiceResponse> DLPCasesDefectsProjectUnitsOIF1ST(string request)
        {
            // init value
            using var connectionMainteance = _appMaintenanceDbContext.CreateConnection();
            ResourceManager rm = new ResourceManager("DashboardApi.Resource.QueryMaintenance", Assembly.GetExecutingAssembly());
            Dictionary<string, object> result = new Dictionary<string, object>();

            // get current week and previous week
            var lstWeek = _baseCommonService.GetListWeekNeed(1);

            try
            {
                // init data
                string whereSQLDefects = $"1=1",
                       whereSQLCases = $"1=1",
                       whereSQLMaintenance = $"1=1",
                       querySQL = rm.GetString("DLPDetailUnitsOIF1stOverview", CultureInfo.CurrentCulture),
                       lastQuery = string.Empty;

                bool isTimeline = false;

                // build where query
                if (!string.IsNullOrEmpty(querySQL))
                {
                    if (!string.IsNullOrWhiteSpace(request))
                    {
                        if ((request.StartsWith("{") && request.EndsWith("}")) || (request.StartsWith("[") && request.EndsWith("]")))
                        {
                            SummaryRequest requestConvert = JsonConvert.DeserializeObject<SummaryRequest>(request);

                            // filter by project
                            if (requestConvert.listProject != null && requestConvert.listProject.Count() > 0)
                            {
                                whereSQLMaintenance = whereSQLCases = whereSQLDefects = $"{whereSQLDefects} AND M.PROJECT_ID IN ('{string.Join("','", requestConvert.listProject.ToArray())}')";
                            }

                            // filter by project
                            if (requestConvert.TotalNumberDefectStatuses != null && !string.IsNullOrWhiteSpace(requestConvert.TotalNumberDefectStatuses))
                            {
                                switch (requestConvert.TotalNumberDefectStatuses)
                                {
                                    case "Overview":
                                        querySQL = rm.GetString("DLPDetailUnitsOIF1stOverview", CultureInfo.CurrentCulture);
                                        break;
                                    case "Timeline":
                                        isTimeline = true;
                                        querySQL = rm.GetString("DLPDetailUnitsOIF1stTimeLine", CultureInfo.CurrentCulture);
                                        break;
                                }
                            }

                            // filter by date
                            if (!string.IsNullOrWhiteSpace(requestConvert.gteDate))
                            {
                                whereSQLDefects = $"{whereSQLDefects} DM.CREATION_DATE <= '{requestConvert.gteDate}')";
                            }
                            if (!string.IsNullOrWhiteSpace(requestConvert.lteDate))
                            {
                                whereSQLDefects = $"{whereSQLDefects} DM.CREATION_DATE >= '{requestConvert.lteDate}')";

                            }
                        }
                    }
                }

                if (!isTimeline)
                {
                    foreach (var week in lstWeek)
                    {
                        string queryDefects = $" AND DM.CREATION_DATE >= '{week.Value[0]}' AND DM.CREATION_DATE < '{week.Value[1]}'",
                               queryCases = $" AND CM.CREATION_DATE >= '{week.Value[0]}' AND CM.CREATION_DATE < '{week.Value[1]}'";

                        lastQuery += querySQL
                            .Replace("@WHERESQLDEFECTS", $"{whereSQLDefects}{queryDefects}")
                            .Replace("@WHERESQLCASES", $"{whereSQLCases}{queryCases}")
                            .Replace("@WHERESQL", whereSQLMaintenance)
                            .Replace("@STARTWEEK", $"'{week.Value[0].ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}'")
                            .Replace("@ENDWEEK", $"'{week.Value[1].ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}'");
                    }
                }

                // query
                using (var resultQuery = await connectionMainteance.QueryMultipleAsync(!isTimeline ? lastQuery : querySQL.Replace("@WHERESQLDEFECTS", $"{whereSQLDefects}")
                            .Replace("@WHERESQLCASES", $"{whereSQLCases}"), new { }))
                {
                    if (resultQuery != null)
                    {
                        if (!isTimeline)
                        {
                            int index = 1;
                            foreach (var week in lstWeek)
                            {
                                var caseQuery = ConvertStatus((await resultQuery.ReadAsync<TempSummaryCasesDefectsRequest>()).ToList());
                                var caseDefects = ConvertStatus((await resultQuery.ReadAsync<TempSummaryCasesDefectsRequest>()).ToList());

                                result.Add($"{week.Value[0].ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)} - {week.Value[1].ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}", new
                                {
                                    cases = caseQuery,
                                    defects = caseDefects,
                                    weekType = index >= lstWeek.Count ? "PreviousWeek" : "ThisWeek"
                                });

                                index++;
                            }
                        }
                        else
                        {
                            var casesQuery = ConvertStatus((await resultQuery.ReadAsync<TempSummaryCasesDefectsRequest>()).ToList());
                            var defectsQuery = ConvertStatus((await resultQuery.ReadAsync<TempSummaryCasesDefectsRequest>()).ToList());


                            // step 1: get all datetime have in each timline case defects incoming/outgoing
                            List<string> times = new List<string>();

                            foreach (var caseData in casesQuery)
                            {
                                times.Add(caseData.DateConcat);
                            }

                            foreach (var defectData in defectsQuery)
                            {
                                times.Add(defectData.DateConcat);
                            }

                            // distinct and order by date
                            times = times.Distinct().ToList();

                            // step 2: for loop list time and get total
                            var handleData = new Dictionary<string, Dictionary<string, int>>();
                            foreach (var time in times)
                            {
                                var findCases = casesQuery.FindAll(x => x.DateConcat == time);
                                if (findCases.Count > 0)
                                {
                                    foreach (var caseData in findCases)
                                    {
                                        if (!handleData.ContainsKey(time))
                                        {
                                            handleData.Add(time, new Dictionary<string, int>());
                                        }

                                        if (!handleData[time].ContainsKey(caseData.Status))
                                        {
                                            handleData[time].Add(caseData.Status, 0);
                                        }

                                        handleData[time][caseData.Status] += caseData.TotalStatus;
                                    }
                                }

                                var findDefects = defectsQuery.FindAll(x => x.DateConcat == time);
                                if (findDefects.Count > 0)
                                {
                                    foreach (var defectData in findDefects)
                                    {
                                        if (!handleData.ContainsKey(time))
                                        {
                                            handleData.Add(time, new Dictionary<string, int>());
                                        }

                                        if (!handleData[time].ContainsKey(defectData.Status))
                                        {
                                            handleData[time].Add(defectData.Status, 0);
                                        }

                                        handleData[time][defectData.Status] += defectData.TotalStatus;
                                    }
                                }
                            }
                            return Ok(handleData);
                        }
                    }
                }
            }
            catch (Exception ce)
            {
                return Ok(message: ce.Message);
            }

            return Ok(result);
        }

        /// <summary>
        /// Func get detail Total Cases and Defects by Project
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (02.06.2023)
        public async Task<ServiceResponse> DLPCasesDefectsProjectUnitsOIF2ND(string request)
        {
            // init value
            using var connectionMainteance = _appMaintenanceDbContext.CreateConnection();
            ResourceManager rm = new ResourceManager("DashboardApi.Resource.QueryMaintenance", Assembly.GetExecutingAssembly());
            Dictionary<string, object> result = new Dictionary<string, object>();

            // get current week and previous week
            var lstWeek = _baseCommonService.GetListWeekNeed(1);

            try
            {
                // init data
                string whereSQLDefects = $"1=1",
                       whereSQLCases = $"1=1",
                       whereSQLMaintenance = $"1=1",
                       querySQL = rm.GetString("DLPDetailUnitsOIF2ndOverview", CultureInfo.CurrentCulture),
                       lastQuery = string.Empty;

                bool isTimeline = false;

                // build where query
                if (!string.IsNullOrEmpty(querySQL))
                {
                    if (!string.IsNullOrWhiteSpace(request))
                    {
                        if ((request.StartsWith("{") && request.EndsWith("}")) || (request.StartsWith("[") && request.EndsWith("]")))
                        {
                            SummaryRequest requestConvert = JsonConvert.DeserializeObject<SummaryRequest>(request);

                            // filter by project
                            if (requestConvert.listProject != null && requestConvert.listProject.Count() > 0)
                            {
                                whereSQLMaintenance = whereSQLCases = whereSQLDefects = $"{whereSQLDefects} AND M.PROJECT_ID IN ('{string.Join("','", requestConvert.listProject.ToArray())}')";
                            }

                            // filter by project
                            if (requestConvert.TotalNumberDefectStatuses != null && !string.IsNullOrWhiteSpace(requestConvert.TotalNumberDefectStatuses))
                            {
                                switch (requestConvert.TotalNumberDefectStatuses)
                                {
                                    case "Overview":
                                        querySQL = rm.GetString("DLPDetailUnitsOIF1stOverview", CultureInfo.CurrentCulture);
                                        break;
                                    case "Timeline":
                                        isTimeline = true;
                                        querySQL = rm.GetString("DLPDetailUnitsOIF1stTimeLine", CultureInfo.CurrentCulture);
                                        break;
                                }
                            }

                            // filter by date
                            if (!string.IsNullOrWhiteSpace(requestConvert.gteDate))
                            {
                                whereSQLDefects = $"{whereSQLDefects} DM.CREATION_DATE <= '{requestConvert.gteDate}')";
                            }
                            if (!string.IsNullOrWhiteSpace(requestConvert.lteDate))
                            {
                                whereSQLDefects = $"{whereSQLDefects} DM.CREATION_DATE >= '{requestConvert.lteDate}')";

                            }
                        }
                    }
                }

                if (!isTimeline)
                {
                    foreach (var week in lstWeek)
                    {
                        string queryDefects = $" AND DM.CREATION_DATE >= '{week.Value[0]}' AND DM.CREATION_DATE < '{week.Value[1]}'",
                               queryCases = $" AND CM.CREATION_DATE >= '{week.Value[0]}' AND CM.CREATION_DATE < '{week.Value[1]}'";

                        lastQuery += querySQL
                            .Replace("@WHERESQLDEFECTS", $"{whereSQLDefects}{queryDefects}")
                            .Replace("@WHERESQLCASES", $"{whereSQLCases}{queryCases}")
                            .Replace("@WHERESQL", whereSQLMaintenance)
                            .Replace("@STARTWEEK", $"'{week.Value[0].ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}'")
                            .Replace("@ENDWEEK", $"'{week.Value[1].ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}'");
                    }
                }

                // query
                using (var resultQuery = await connectionMainteance.QueryMultipleAsync(!isTimeline ? lastQuery : querySQL.Replace("@WHERESQLDEFECTS", $"{whereSQLDefects}")
                            .Replace("@WHERESQLCASES", $"{whereSQLCases}"), new { }))
                {
                    if (resultQuery != null)
                    {
                        if (!isTimeline)
                        {
                            int index = 1;
                            foreach (var week in lstWeek)
                            {
                                var caseQuery = ConvertStatus((await resultQuery.ReadAsync<TempSummaryCasesDefectsRequest>()).ToList());
                                var caseDefects = ConvertStatus((await resultQuery.ReadAsync<TempSummaryCasesDefectsRequest>()).ToList());

                                result.Add($"{week.Value[0].ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)} - {week.Value[1].ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}", new
                                {
                                    cases = caseQuery,
                                    defects = caseDefects,
                                    weekType = index >= lstWeek.Count ? "PreviousWeek" : "ThisWeek"
                                });

                                index++;
                            }
                        }
                        else
                        {
                            var casesQuery = ConvertStatus((await resultQuery.ReadAsync<TempSummaryCasesDefectsRequest>()).ToList());
                            var defectsQuery = ConvertStatus((await resultQuery.ReadAsync<TempSummaryCasesDefectsRequest>()).ToList());


                            // step 1: get all datetime have in each timline case defects incoming/outgoing
                            List<string> times = new List<string>();

                            foreach (var caseData in casesQuery)
                            {
                                times.Add(caseData.DateConcat);
                            }

                            foreach (var defectData in defectsQuery)
                            {
                                times.Add(defectData.DateConcat);
                            }

                            // distinct and order by date
                            times = times.Distinct().ToList();

                            // step 2: for loop list time and get total
                            var handleData = new Dictionary<string, Dictionary<string, int>>();
                            foreach (var time in times)
                            {
                                var findCases = casesQuery.FindAll(x => x.DateConcat == time);
                                if (findCases.Count > 0)
                                {
                                    foreach (var caseData in findCases)
                                    {
                                        if (!handleData.ContainsKey(time))
                                        {
                                            handleData.Add(time, new Dictionary<string, int>());
                                        }

                                        if (!handleData[time].ContainsKey(caseData.Status))
                                        {
                                            handleData[time].Add(caseData.Status, 0);
                                        }

                                        handleData[time][caseData.Status] += caseData.TotalStatus;
                                    }
                                }

                                var findDefects = defectsQuery.FindAll(x => x.DateConcat == time);
                                if (findDefects.Count > 0)
                                {
                                    foreach (var defectData in findDefects)
                                    {
                                        if (!handleData.ContainsKey(time))
                                        {
                                            handleData.Add(time, new Dictionary<string, int>());
                                        }

                                        if (!handleData[time].ContainsKey(defectData.Status))
                                        {
                                            handleData[time].Add(defectData.Status, 0);
                                        }

                                        handleData[time][defectData.Status] += defectData.TotalStatus;
                                    }
                                }
                            }
                            return Ok(handleData);
                        }
                    }
                }
            }
            catch (Exception ce)
            {
                return Ok(message: ce.Message);
            }

            return Ok(result);
        }

        /// <summary>
        /// Func get detail Total Cases and Defects, Defect List Outstanding Items 1st
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (02.06.2023)
        public async Task<ServiceResponse> DLPCasesDefectsProjectOutStandingItem1st(string request)
        {
            using var connectionMainteance = _appMaintenanceDbContext.CreateConnection();
            ResourceManager rm = new ResourceManager("DashboardApi.Resource.QueryMaintenance", Assembly.GetExecutingAssembly());
            Dictionary<string, object> result = new Dictionary<string, object>();

            // get current week and previous week
            var lstWeek = _baseCommonService.GetListWeekNeed(1);

            try
            {
                // init data
                string whereSQLDefects = $"1=1",
                       querySQL = rm.GetString("DLPDetailUnitsOIF1stOutstanding", CultureInfo.CurrentCulture),
                       finalQuery = string.Empty;

                // build where query
                if (!string.IsNullOrEmpty(querySQL))
                {
                    if (!string.IsNullOrWhiteSpace(request))
                    {
                        if ((request.StartsWith("{") && request.EndsWith("}")) || (request.StartsWith("[") && request.EndsWith("]")))
                        {
                            SummaryRequest requestConvert = JsonConvert.DeserializeObject<SummaryRequest>(request);

                            // filter by project
                            if (requestConvert.listProject != null && requestConvert.listProject.Count() > 0)
                            {
                                whereSQLDefects = $"{whereSQLDefects} M.PROJECT_ID IN ('{string.Join("','", requestConvert.listProject.ToArray())}')";
                            }
                        }
                    }
                }

                foreach (var week in lstWeek)
                {
                    //string whereSQL = $"{whereSQLDefects} AND DM.CREATION_DATE >= '{week.Value[0]}' AND DM.CREATION_DATE <= '{week.Value[1]}'";
                    string whereSQL = "1=1";

                    finalQuery += querySQL
                        .Replace("@WHERESQL", whereSQL)
                        .Replace("@STARTWEEK", $"'{week.Value[0].ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}'")
                        .Replace("@ENDWEEK", $"'{week.Value[1].ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}'");
                }

                // query
                using (var resultQuery = await connectionMainteance.QueryMultipleAsync(finalQuery, new { }))
                {
                    if (resultQuery != null)
                    {
                        int index = 1;
                        foreach (var week in lstWeek)
                        {
                            var readResult = ConvertStatusOutstanding((await resultQuery.ReadAsync<TempSummaryCasesDefectsRequest>()).ToList());

                            result.Add($"{week.Value[0].ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)} - {week.Value[1].ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}", new
                            {
                                result = readResult,
                                weekType = index >= lstWeek.Count ? "PreviousWeek" : "ThisWeek"
                            });

                            index++;
                        }
                    }
                }
            }
            catch (Exception ce)
            {
                return Ok(message: ce.Message);
            }

            return Ok(result);
        }

        /// <summary>
        /// Func get detail Total Cases and Defects, Defect List Outstanding Items 2nd
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (02.06.2023)
        public async Task<ServiceResponse> DLPCasesDefectsProjectOutStandingItem2nd(string request)
        {
            using var connectionMainteance = _appMaintenanceDbContext.CreateConnection();
            ResourceManager rm = new ResourceManager("DashboardApi.Resource.QueryMaintenance", Assembly.GetExecutingAssembly());
            Dictionary<string, object> result = new Dictionary<string, object>();

            // get current week and previous week
            var lstWeek = _baseCommonService.GetListWeekNeed(1);

            try
            {
                // init data
                string whereSQLDefects = $"1=1",
                       querySQL = rm.GetString("DLPDetailUnitsOIF2ndOutstanding", CultureInfo.CurrentCulture),
                       finalQuery = string.Empty;

                // build where query
                if (!string.IsNullOrEmpty(querySQL))
                {
                    if (!string.IsNullOrWhiteSpace(request))
                    {
                        if ((request.StartsWith("{") && request.EndsWith("}")) || (request.StartsWith("[") && request.EndsWith("]")))
                        {
                            SummaryRequest requestConvert = JsonConvert.DeserializeObject<SummaryRequest>(request);

                            // filter by project
                            if (requestConvert.listProject != null && requestConvert.listProject.Count() > 0)
                            {
                                whereSQLDefects = $"{whereSQLDefects} M.PROJECT_ID IN ('{string.Join("','", requestConvert.listProject.ToArray())}')";
                            }
                        }
                    }
                }

                foreach (var week in lstWeek)
                {
                    // string whereSQL = $"{whereSQLDefects} AND DM.CREATION_DATE >= '{week.Value[0]}' AND DM.CREATION_DATE <= '{week.Value[1]}'";
                    string whereSQL = "1=1";

                    finalQuery += querySQL
                        .Replace("@WHERESQL", whereSQL)
                        .Replace("@STARTWEEK", $"'{week.Value[0].ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}'")
                        .Replace("@ENDWEEK", $"'{week.Value[1].ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}'");
                }

                // query
                using (var resultQuery = await connectionMainteance.QueryMultipleAsync(finalQuery, new { }))
                {
                    if (resultQuery != null)
                    {
                        int index = 1;
                        foreach (var week in lstWeek)
                        {
                            var readResult = ConvertStatusOutstanding((await resultQuery.ReadAsync<TempSummaryCasesDefectsRequest>()).ToList());

                            result.Add($"{week.Value[0].ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)} - {week.Value[1].ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}", new
                            {
                                result = readResult,
                                weekType = index >= lstWeek.Count ? "PreviousWeek" : "ThisWeek"
                            });

                            index++;
                        }
                    }
                }
            }
            catch (Exception ce)
            {
                return Ok(message: ce.Message);
            }

            return Ok(result);
        }

        /// <summary>
        /// Func get detail Total Cases and Defects by Project
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (02.06.2023)
        public async Task<ServiceResponse> DLPCasesDefectsProjectDefectOverdue(string request)
        {
            using var connectionMainteance = _appMaintenanceDbContext.CreateConnection();
            ResourceManager rm = new ResourceManager("DashboardApi.Resource.QueryMaintenance", Assembly.GetExecutingAssembly());
            Dictionary<string, object> result = new Dictionary<string, object>();

            // get current week and previous week
            var lstWeek = _baseCommonService.GetListWeekNeed(1);

            try
            {
                // init data
                string whereSQLDefects = $"1=1",
                       querySQL = rm.GetString("SummaryDLPTotalDefectOverdueTypeWeek", CultureInfo.CurrentCulture),
                       finalQuery = string.Empty;

                // build where query
                if (!string.IsNullOrEmpty(querySQL))
                {
                    if (!string.IsNullOrWhiteSpace(request))
                    {
                        if ((request.StartsWith("{") && request.EndsWith("}")) || (request.StartsWith("[") && request.EndsWith("]")))
                        {
                            SummaryRequest requestConvert = JsonConvert.DeserializeObject<SummaryRequest>(request);

                            // filter by project
                            if (requestConvert.listProject != null && requestConvert.listProject.Count() > 0)
                            {
                                whereSQLDefects = $"{whereSQLDefects} M.PROJECT_ID IN ('{string.Join("','", requestConvert.listProject.ToArray())}')";
                            }
                        }
                    }
                }

                foreach (var week in lstWeek)
                {
                    string whereSQL = $"DM.CREATION_DATE >= '{week.Value[0]}' AND DM.CREATION_DATE <= '{week.Value[1]}'";
                    finalQuery += querySQL
                        .Replace("@WHERESQLDEFECTS", whereSQL)
                        .Replace("@STARTWEEK", $"'{week.Value[0].ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}'")
                        .Replace("@ENDWEEK", $"'{week.Value[1].ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}'");
                }

                // query
                using (var resultQuery = await connectionMainteance.QueryMultipleAsync(finalQuery, new { }))
                {
                    if (resultQuery != null)
                    {
                        int index = 1;
                        foreach (var week in lstWeek)
                        {
                            var readResult = (await resultQuery.ReadAsync<TempSummaryCasesDefectsRequest>()).ToList().FirstOrDefault();

                            result.Add($"{week.Value[0].ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)} - {week.Value[1].ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}", new
                            {
                                result = readResult,
                                weekType = index >= lstWeek.Count ? "PreviousWeek" : "ThisWeek"
                            });

                            index++;
                        }
                    }
                }
            }
            catch (Exception ce)
            {
                return Ok(message: ce.Message);
            }

            return Ok(result);
        }

        /// <summary>
        /// Func get detail Total Cases and Defects by Project
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (02.06.2023)
        public async Task<ServiceResponse> DLPCasesDefectsProjectWaterSeepagesLeakagesUnits(string request)
        {
            // init value
            using var connectionMainteance = _appMaintenanceDbContext.CreateConnection();
            ResourceManager rm = new ResourceManager("DashboardApi.Resource.QueryMaintenance", Assembly.GetExecutingAssembly());
            Dictionary<string, object> result = new Dictionary<string, object>();

            // get current week and previous week
            var lstWeek = _baseCommonService.GetListWeekNeed(1);

            try
            {
                // init data
                string whereSQLMaintenance = $"1=1",
                       querySQL = rm.GetString("DetailDLPProjectSeepagesChokage", CultureInfo.CurrentCulture);

                // build where query
                if (!string.IsNullOrEmpty(querySQL))
                {
                    if (!string.IsNullOrWhiteSpace(request))
                    {
                        if ((request.StartsWith("{") && request.EndsWith("}")) || (request.StartsWith("[") && request.EndsWith("]")))
                        {
                            SummaryRequest requestConvert = JsonConvert.DeserializeObject<SummaryRequest>(request);

                            // filter by project
                            if (requestConvert.listProject != null && requestConvert.listProject.Count() > 0)
                            {
                                whereSQLMaintenance = $"{whereSQLMaintenance} AND M.PROJECT_ID IN ('{string.Join("','", requestConvert.listProject.ToArray())}')";
                            }
                        }
                    }
                }

                // query
                using (var resultQuery = await connectionMainteance.QueryMultipleAsync(querySQL.Replace("@WHERESQL", whereSQLMaintenance), new { }))
                {
                    if (resultQuery != null)
                    {
                        var total = (await resultQuery.ReadAsync<TempSummaryCasesDefectsRequest>()).ToList();
                        var summary = (await resultQuery.ReadAsync<TempSummaryCasesDefectsRequest>()).ToList();

                        result.Add("TotalSeepagesLeakagesUnitsAndChokage", total);
                        result.Add("Summary", summary);
                    }
                }
            }
            catch (Exception ce)
            {
                return Ok(message: ce.Message);
            }

            return Ok(result);
        }

        /// <summary>
        /// Func get detail Total Cases and Defects by Project
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (02.06.2023)
        public async Task<ServiceResponse> DLPCasesDefectsProjectChokageUnits(string request)
        {
            // init value
            using var connectionMainteance = _appMaintenanceDbContext.CreateConnection();
            ResourceManager rm = new ResourceManager("DashboardApi.Resource.QueryMaintenance", Assembly.GetExecutingAssembly());
            Dictionary<string, object> result = new Dictionary<string, object>();

            // get current week and previous week
            var lstWeek = _baseCommonService.GetListWeekNeed(1);

            try
            {
                // init data
                string whereSQLMaintenance = $"1=1",
                       querySQL = rm.GetString("DetailDLPProjectSeepagesChokage", CultureInfo.CurrentCulture);

                // build where query
                if (!string.IsNullOrEmpty(querySQL))
                {
                    if (!string.IsNullOrWhiteSpace(request))
                    {
                        if ((request.StartsWith("{") && request.EndsWith("}")) || (request.StartsWith("[") && request.EndsWith("]")))
                        {
                            SummaryRequest requestConvert = JsonConvert.DeserializeObject<SummaryRequest>(request);

                            // filter by project
                            if (requestConvert.listProject != null && requestConvert.listProject.Count() > 0)
                            {
                                whereSQLMaintenance = $"{whereSQLMaintenance} AND M.PROJECT_ID IN ('{string.Join("','", requestConvert.listProject.ToArray())}')";
                            }
                        }
                    }
                }

                // query
                using (var resultQuery = await connectionMainteance.QueryMultipleAsync(querySQL.Replace("@WHERESQL", whereSQLMaintenance), new { }))
                {
                    if (resultQuery != null)
                    {
                        var total = (await resultQuery.ReadAsync<TempSummaryCasesDefectsRequest>()).ToList();
                        var summary = (await resultQuery.ReadAsync<TempSummaryCasesDefectsRequest>()).ToList();

                        result.Add("TotalSeepagesLeakagesUnitsAndChokage", total);
                        result.Add("Summary", summary);
                    }
                }
            }
            catch (Exception ce)
            {
                return Ok(message: ce.Message);
            }

            return Ok(result);
        }

        /// <summary>
        /// Func get detail Total Cases and Defects by Project
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (02.06.2023)
        public async Task<ServiceResponse> DLPCasesDefectsProjectTotalWorkersOnSite(string request)
        {
            // init value
            using var connectionMainteance = _appMaintenanceDbContext.CreateConnection();
            ResourceManager rm = new ResourceManager("DashboardApi.Resource.QueryMaintenance", Assembly.GetExecutingAssembly());
            Dictionary<string, object> result = new Dictionary<string, object>();

            // get current week and previous week
            var lstMonth = _baseCommonService.GetListMonthNeed(1);

            try
            {
                // init data
                string whereSQLDefects = $"1=1",
                       whereSQLCases = $"1=1",
                       whereSQLMaintenance = $"1=1",
                       querySQLSummary = rm.GetString("SummaryDLPTotalWorkersOnSite", CultureInfo.CurrentCulture),
                       querySQLTimeline = rm.GetString("DLPTotalWorkersOnSiteTimeline", CultureInfo.CurrentCulture),
                       lastQuery = string.Empty;

                // build where query
                if (!string.IsNullOrEmpty(querySQLSummary) && !string.IsNullOrEmpty(querySQLTimeline))
                {
                    if (!string.IsNullOrWhiteSpace(request))
                    {
                        if ((request.StartsWith("{") && request.EndsWith("}")) || (request.StartsWith("[") && request.EndsWith("]")))
                        {
                            SummaryRequest requestConvert = JsonConvert.DeserializeObject<SummaryRequest>(request);

                            // filter by project
                            if (requestConvert.listProject != null && requestConvert.listProject.Count() > 0)
                            {
                                whereSQLMaintenance = $"{whereSQLMaintenance} AND M.PROJECT_ID IN ('{string.Join("','", requestConvert.listProject.ToArray())}')";
                            }
                        }
                    }
                }

                foreach (var month in lstMonth)
                {
                    lastQuery += querySQLSummary
                        .Replace("@START_MONTH", $"'{month.Value[0].ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}'")
                        .Replace("@END_MONTH", $"'{month.Value[1].ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}'");
                }

                lastQuery += querySQLTimeline;

                // query
                using (var resultQuery = await connectionMainteance.QueryMultipleAsync(lastQuery.Replace("@WHERESQL", whereSQLMaintenance), new { }))
                {
                    if (resultQuery != null)
                    {
                        var thisMonth = (await resultQuery.ReadAsync<MaintenanceMonthlyRequest>()).ToList();
                        var lastMonth = (await resultQuery.ReadAsync<MaintenanceMonthlyRequest>()).ToList();
                        var timeLine = (await resultQuery.ReadAsync<MaintenanceMonthlyRequest>()).ToList();

                        foreach (var time in timeLine)
                        {
                            time.MonthString = $"{time.Month.Day.ToString()}/{time.Month.Month.ToString()}/{time.Month.Year.ToString()}";
                        }

                        result.Add("thisMonth", thisMonth);
                        result.Add("lastMonth", lastMonth);
                        result.Add("timeLine", timeLine);
                    }
                }
            }
            catch (Exception ce)
            {
                return Ok(message: ce.Message);
            }

            return Ok(result);
        }

        /// <summary>
        /// Func get defects overdue exceeding 30 days
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (18.06.2023)
        public async Task<ServiceResponse> DLPSummaryTotalDefectsOverdueExceeding30Days(string request)
        {
            using var connectionMainteance = _appMaintenanceDbContext.CreateConnection();
            ResourceManager rm = new ResourceManager("DashboardApi.Resource.QueryMaintenance", Assembly.GetExecutingAssembly());
            Dictionary<string, object> result = new Dictionary<string, object>();

            // get current week and previous week
            var lstWeek = _baseCommonService.GetListWeekNeed(1);

            try
            {
                // init data
                string whereSQLDefects = $"1=1",
                       whereSQLCases = $"1=1",
                       querySQL = rm.GetString("SummaryDLPTotalDefectOverdueTypeExceeding30Days", CultureInfo.CurrentCulture),
                       finalQuery = string.Empty;
                bool isOverview = true;

                // build where query
                if (!string.IsNullOrEmpty(querySQL))
                {
                    if (!string.IsNullOrWhiteSpace(request))
                    {
                        if ((request.StartsWith("{") && request.EndsWith("}")) || (request.StartsWith("[") && request.EndsWith("]")))
                        {
                            SummaryRequest requestConvert = JsonConvert.DeserializeObject<SummaryRequest>(request);

                            // filter by project
                            if (requestConvert.listProject != null && requestConvert.listProject.Count() > 0)
                            {
                                whereSQLDefects = $"{whereSQLDefects} M.PROJECT_ID IN ('{string.Join("','", requestConvert.listProject.ToArray())}')";
                                whereSQLCases = $"{whereSQLCases} M.PROJECT_ID IN ('{string.Join("','", requestConvert.listProject.ToArray())}')";
                            }

                            // filter by date
                            if (!string.IsNullOrWhiteSpace(requestConvert.gteDate))
                            {
                                whereSQLDefects = $"{whereSQLDefects} DM.CONFIRMATION_DATE <= '{requestConvert.gteDate}')";
                                whereSQLCases = $"{whereSQLCases} DM.CREATION_DATE <= '{requestConvert.gteDate}')";
                            }
                            if (!string.IsNullOrWhiteSpace(requestConvert.lteDate))
                            {
                                whereSQLDefects = $"{whereSQLDefects} DM.CONFIRMATION_DATE >= '{requestConvert.lteDate}')";
                                whereSQLCases = $"{whereSQLCases} DM.CREATION_DATE >= '{requestConvert.gteDate}')";

                            }

                            // filter by project
                            if (requestConvert.TotalNumberDefectStatuses != null && !string.IsNullOrWhiteSpace(requestConvert.TotalNumberDefectStatuses))
                            {
                                if (requestConvert.TotalNumberDefectStatuses.Contains("Project"))
                                {
                                    isOverview = false;
                                }
                            }
                        }
                    }
                }

                // query
                using (var resultQuery = await connectionMainteance.QueryMultipleAsync(querySQL
                    .Replace("@WHERESQLDEFECTS", whereSQLDefects)
                    .Replace("@WHERESQLCASES", whereSQLDefects), new { }))
                {
                    if (resultQuery != null)
                    {
                        var rowData = (await resultQuery.ReadAsync<TempSummaryCasesDefectsRequest>()).ToList();

                        if (isOverview)
                        {
                            var handleData = new Dictionary<string, TempSummaryCasesDefectsRequest>()
                            {
                                {"MOS",  new TempSummaryCasesDefectsRequest() { Tag = "MOS"} },
                                {"RENO",  new TempSummaryCasesDefectsRequest() { Tag = "RENO"}},
                                {"Appt",  new TempSummaryCasesDefectsRequest() { Tag = "Appt"}},
                                {"Other",  new TempSummaryCasesDefectsRequest() { Tag = "Other"}}
                            };

                            foreach (var row in rowData)
                            {
                                if (!string.IsNullOrEmpty(row.Tag))
                                {
                                    if (row.Tag.ToLower().Contains("mos"))
                                    {
                                        handleData["MOS"].TotalStatus += row.TotalStatus;
                                    }
                                    else
                                    {
                                        if (row.Tag.ToLower().Contains("reno"))
                                        {
                                            handleData["RENO"].TotalStatus += row.TotalStatus;
                                        }
                                        else
                                        {
                                            if (row.Tag.ToLower().Contains("appt"))
                                            {
                                                handleData["Appt"].TotalStatus += row.TotalStatus;
                                            }
                                            else
                                            {
                                                handleData["Other"].TotalStatus += row.TotalStatus;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    handleData["Other"].TotalStatus += row.TotalStatus;
                                }
                            }

                            result.Add("Result", handleData);
                        }
                        else
                        {
                            var projectData = rowData.GroupBy(c => new
                            {
                                c.ProjectID,
                                c.ProjectName
                            }).ToList();

                            var handleResult = new List<Dictionary<string, dynamic>>();

                            foreach (var project in projectData)
                            {
                                var handleData = new Dictionary<string, dynamic>()
                                {
                                    {"MOS",  0 },
                                    {"RENO",  0},
                                    {"Appt",  0},
                                    {"Other",  0},
                                    {"ProjectName",  project.Key.ProjectName},
                                    {"ProjectID",  project.Key.ProjectID},
                                };

                                foreach (var row in project)
                                {
                                    if (!string.IsNullOrEmpty(row.Tag))
                                    {
                                        if (row.Tag.ToLower().Contains("mos"))
                                        {
                                            handleData["MOS"] += row.TotalStatus;
                                        }
                                        else
                                        {
                                            if (row.Tag.ToLower().Contains("reno"))
                                            {
                                                handleData["RENO"] += row.TotalStatus;
                                            }
                                            else
                                            {
                                                if (row.Tag.ToLower().Contains("appt"))
                                                {
                                                    handleData["Appt"] += row.TotalStatus;
                                                }
                                                else
                                                {
                                                    handleData["Other"] += row.TotalStatus;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        handleData["Other"] += row.TotalStatus;
                                    }
                                }

                                handleResult.Add(handleData);
                            }

                            return Ok(handleResult);
                        }
                    }
                }
            }
            catch (Exception ce)
            {
                return Ok(message: ce.Message);
            }

            return Ok(result);
        }

        /// <summary>
        /// Func get defects overdue exceeding 30 days
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (18.06.2023)
        public async Task<ServiceResponse> DLPSummaryTotalDefectsOverdueExceeding30DaysDetail(string request)
        {
            using var connectionMainteance = _appMaintenanceDbContext.CreateConnection();
            ResourceManager rm = new ResourceManager("DashboardApi.Resource.QueryMaintenance", Assembly.GetExecutingAssembly());
            Dictionary<string, object> result = new Dictionary<string, object>();

            try
            {
                // init data
                string whereSQLDefects = $"1=1",
                       querySQL = rm.GetString("SummaryDLPTotalDefectOverdueTypeExceeding30DaysDetailCases", CultureInfo.CurrentCulture),
                       finalQuery = string.Empty;

                // build where query
                if (!string.IsNullOrEmpty(querySQL))
                {
                    if (!string.IsNullOrWhiteSpace(request))
                    {
                        if ((request.StartsWith("{") && request.EndsWith("}")) || (request.StartsWith("[") && request.EndsWith("]")))
                        {
                            SummaryRequest requestConvert = JsonConvert.DeserializeObject<SummaryRequest>(request);

                            // filter detail
                            if (!string.IsNullOrWhiteSpace(requestConvert.typeProject))
                            {
                                if (requestConvert.typeProject.ToLower() == "2")
                                {
                                    querySQL = rm.GetString("SummaryDLPTotalDefectOverdueTypeExceeding30DaysDetailDefects", CultureInfo.CurrentCulture);
                                }
                            }

                            // filter by project
                            if (requestConvert.listProject != null && requestConvert.listProject.Count() > 0)
                            {
                                whereSQLDefects = $"{whereSQLDefects} M.PROJECT_ID IN ('{string.Join("','", requestConvert.listProject.ToArray())}')";
                            }
                            // filter by date
                            if (!string.IsNullOrWhiteSpace(requestConvert.gteDate))
                            {
                                whereSQLDefects = $"{whereSQLDefects} DM.CONFIRMATION_DATE <= '{requestConvert.gteDate}')";
                            }
                            if (!string.IsNullOrWhiteSpace(requestConvert.lteDate))
                            {
                                whereSQLDefects = $"{whereSQLDefects} DM.CONFIRMATION_DATE >= '{requestConvert.lteDate}')";
                            }
                        }
                    }
                }

                // query
                using (var resultQuery = await connectionMainteance.QueryMultipleAsync(querySQL.Replace("@WHERESQLDEFECTS", whereSQLDefects), new { }))
                {
                    if (resultQuery != null)
                    {
                        var valueTag = (await resultQuery.ReadAsync<CombineCasesDefectsMonitoringRequest>()).ToList();
                        result.Add("Result", valueTag);
                    }
                }
            }
            catch (Exception ce)
            {
                return Ok(message: ce.Message);
            }

            return Ok(result);
        }

        #endregion

        #region Post DLP

        /// <summary>
        /// Func get Total Number of Cases by Statuses
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (19.06.2023)
        public async Task<ServiceResponse> PostDLPSummaryCasesDefectsStatus(string request)
        {
            using var connectionMainteance = _appMaintenanceDbContext.CreateConnection();
            ResourceManager rm = new ResourceManager("DashboardApi.Resource.QueryMaintenance", Assembly.GetExecutingAssembly());
            Dictionary<string, object> result = new Dictionary<string, object>();

            try
            {
                // init data
                string whereSQLDefects = $"1=1",
                       querySQL = rm.GetString("SummaryPostDLPTotalDefectsStatus", CultureInfo.CurrentCulture),
                       finalQuery = string.Empty;

                // build where query
                if (!string.IsNullOrEmpty(querySQL))
                {
                    if (!string.IsNullOrWhiteSpace(request))
                    {
                        if ((request.StartsWith("{") && request.EndsWith("}")) || (request.StartsWith("[") && request.EndsWith("]")))
                        {
                            SummaryRequest requestConvert = JsonConvert.DeserializeObject<SummaryRequest>(request);

                            // filter by project
                            if (requestConvert.listProject != null && requestConvert.listProject.Count() > 0)
                            {
                                whereSQLDefects = $"{whereSQLDefects} AND M.PROJECT_ID IN ('{string.Join("','", requestConvert.listProject.ToArray())}')";
                            }
                            // filter by date
                            if (!string.IsNullOrWhiteSpace(requestConvert.gteDate))
                            {
                                whereSQLDefects = $"{whereSQLDefects} AND DM.CONFIRMATION_DATE <= '{requestConvert.gteDate}'";
                            }
                            if (!string.IsNullOrWhiteSpace(requestConvert.lteDate))
                            {
                                whereSQLDefects = $"{whereSQLDefects} AND DM.CONFIRMATION_DATE >= '{requestConvert.lteDate}'";
                            }
                        }
                    }
                }

                // query
                using (var resultQuery = await connectionMainteance.QueryMultipleAsync(querySQL.Replace("@WHERESQLDEFECTS", whereSQLDefects), new { }))
                {
                    if (resultQuery != null)
                    {
                        var overviews = (await resultQuery.ReadAsync<TempSummaryCasesDefectsRequest>()).ToList();
                        var timeline = (await resultQuery.ReadAsync<TempSummaryCasesDefectsRequest>()).ToList().GroupBy(x => x.DateConcat);
                        var detail = (await resultQuery.ReadAsync<DefectsMonitoringRequest>()).ToList();

                        result.Add("Overview", overviews);
                        result.Add("Timeline", timeline);
                        result.Add("Detail", detail);
                    }
                }
            }
            catch (Exception ce)
            {
                return Ok(message: ce.Message);
            }

            return Ok(result);
        }

        /// <summary>
        /// Func get Total Numbers of Incoming and Outgoing Defects and Cases
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (19.06.2023)
        public async Task<ServiceResponse> PostDLPSummaryIncomingOutgoing(string request)
        {
            using var connectionMainteance = _appMaintenanceDbContext.CreateConnection();
            ResourceManager rm = new ResourceManager("DashboardApi.Resource.QueryMaintenance", Assembly.GetExecutingAssembly());
            Dictionary<string, object> result = new Dictionary<string, object>();

            try
            {
                // init data
                string whereSQLDefects = $"1=1",
                       whereSQLCases = $"1=1",
                       querySQL = rm.GetString("SummaryPostDLPIncomingOutgoing", CultureInfo.CurrentCulture),
                       finalQuery = string.Empty;

                // build where query
                if (!string.IsNullOrEmpty(querySQL))
                {
                    if (!string.IsNullOrWhiteSpace(request))
                    {
                        if ((request.StartsWith("{") && request.EndsWith("}")) || (request.StartsWith("[") && request.EndsWith("]")))
                        {
                            SummaryRequest requestConvert = JsonConvert.DeserializeObject<SummaryRequest>(request);

                            // filter by project
                            if (requestConvert.listProject != null && requestConvert.listProject.Count() > 0)
                            {
                                whereSQLDefects = $"{whereSQLDefects} AND M.PROJECT_ID IN ('{string.Join("','", requestConvert.listProject.ToArray())}')";
                                whereSQLCases = $"{whereSQLCases} AND M.PROJECT_ID IN ('{string.Join("','", requestConvert.listProject.ToArray())}')";
                            }
                            // filter by date
                            if (!string.IsNullOrWhiteSpace(requestConvert.gteDate))
                            {
                                whereSQLDefects = $"{whereSQLDefects} AND DM.CONFIRMATION_DATE <= '{requestConvert.gteDate}'";
                                whereSQLCases = $"{whereSQLCases} AND CM.CONFIRMATION_DATE <= '{requestConvert.gteDate}'";
                            }
                            if (!string.IsNullOrWhiteSpace(requestConvert.lteDate))
                            {
                                whereSQLDefects = $"{whereSQLDefects} AND DM.CONFIRMATION_DATE >= '{requestConvert.lteDate}'";
                                whereSQLCases = $"{whereSQLCases} AND CM.CONFIRMATION_DATE >= '{requestConvert.lteDate}'";
                            }
                        }
                    }
                }

                // query
                using (var resultQuery = await connectionMainteance.QueryMultipleAsync(querySQL
                    .Replace("@WHERESQLDEFECTS", whereSQLDefects)
                    .Replace("@WHERESQLCASES", whereSQLCases), new { }))
                {
                    if (resultQuery != null)
                    {
                        // defects first
                        var overviewDefects = (await resultQuery.ReadAsync<CombineCasesDefectsMonitoringRequest>()).ToList();
                        var overviewCases = (await resultQuery.ReadAsync<CombineCasesDefectsMonitoringRequest>()).ToList();

                        var timeLineDefectsIncoming = (await resultQuery.ReadAsync<CombineCasesDefectsMonitoringRequest>()).ToList();
                        var timeLineDefectsOutGoing = (await resultQuery.ReadAsync<CombineCasesDefectsMonitoringRequest>()).ToList();

                        var timelineCasesIncoming = (await resultQuery.ReadAsync<CombineCasesDefectsMonitoringRequest>()).ToList();
                        var timelineCasesOutGoing = (await resultQuery.ReadAsync<CombineCasesDefectsMonitoringRequest>()).ToList();

                        // handle data incoming and outgoing group by date
                        var timeLineIncomingOutgoing = CombineInconingOutgoing(timeLineDefectsIncoming, timeLineDefectsOutGoing, timelineCasesIncoming, timelineCasesOutGoing);

                        result.Add("Defects", new
                        {
                            OverviewDefects = overviewDefects,
                        });

                        result.Add("Cases", new
                        {
                            OverviewCases = overviewCases,
                        });

                        result.Add("TimeLine", timeLineIncomingOutgoing);
                    }
                }
            }
            catch (Exception ce)
            {
                return Ok(message: ce.Message);
            }


            return Ok(result);
        }

        /// <summary>
        /// Func get Total Numbers of Defects and Cases
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (19.06.2023)
        public async Task<ServiceResponse> PostDLPSummaryTotalNumber(string request)
        {
            using var connectionMainteance = _appMaintenanceDbContext.CreateConnection();
            ResourceManager rm = new ResourceManager("DashboardApi.Resource.QueryMaintenance", Assembly.GetExecutingAssembly());
            Dictionary<string, object> result = new Dictionary<string, object>();
            bool isOverview = true;

            // get current week and previous week
            var lstWeek = _baseCommonService.GetListWeekNeed(1);
            try
            {
                // init data
                string whereSQLDefects = $"1=1",
                       querySQL = rm.GetString("SummaryPostDLPTotalDefectsCasesOverviewTimeout", CultureInfo.CurrentCulture),
                       finalQuery = string.Empty;

                // build where query
                if (!string.IsNullOrEmpty(querySQL))
                {
                    if (!string.IsNullOrWhiteSpace(request))
                    {
                        if ((request.StartsWith("{") && request.EndsWith("}")) || (request.StartsWith("[") && request.EndsWith("]")))
                        {
                            SummaryRequest requestConvert = JsonConvert.DeserializeObject<SummaryRequest>(request);

                            // filter by project
                            if (requestConvert.listProject != null && requestConvert.listProject.Count() > 0)
                            {
                                whereSQLDefects = $"{whereSQLDefects} AND M.PROJECT_ID IN ('{string.Join("','", requestConvert.listProject.ToArray())}')";
                            }
                            // filter by date
                            if (!string.IsNullOrWhiteSpace(requestConvert.gteDate))
                            {
                                whereSQLDefects = $"{whereSQLDefects} AND CONFIRMATION_DATE <= '{requestConvert.gteDate}'";
                            }
                            if (!string.IsNullOrWhiteSpace(requestConvert.lteDate))
                            {
                                whereSQLDefects = $"{whereSQLDefects} AND CONFIRMATION_DATE >= '{requestConvert.lteDate}'";
                            }
                            // filter by project
                            if (requestConvert.TotalNumberDefectStatuses != null && !string.IsNullOrWhiteSpace(requestConvert.TotalNumberDefectStatuses))
                            {
                                if (requestConvert.TotalNumberDefectStatuses.Contains("Timeline"))
                                {
                                    isOverview = false;
                                    querySQL = rm.GetString("SummaryPostDLPTotalDefectsCasesOverviewTimeoutTimeLine", CultureInfo.CurrentCulture);
                                }
                            }
                        }
                    }
                }

                if (isOverview)
                {
                    foreach (var week in lstWeek)
                    {
                        finalQuery += querySQL
                            .Replace("@WHERESQLDEFECTS", whereSQLDefects)
                            .Replace("@WHERESQLCASES", whereSQLDefects)
                            .Replace("@STARTWEEK", $"'{week.Value[0].ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}'")
                            .Replace("@ENDWEEK", $"'{week.Value[1].ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}'");
                    }
                }
                else
                {
                    finalQuery = querySQL;
                }

                // query
                using (var resultQuery = await connectionMainteance.QueryMultipleAsync(finalQuery.Replace("@WHERESQLDEFECTS", whereSQLDefects).Replace("@WHERESQLCASES", whereSQLDefects), new { }))
                {
                    if (resultQuery != null)
                    {
                        if (isOverview)
                        {
                            int index = 1;
                            foreach (var week in lstWeek)
                            {
                                var caseQuery = (await resultQuery.ReadAsync<TempSummaryCasesDefectsRequest>()).ToList();
                                var caseDefects = (await resultQuery.ReadAsync<TempSummaryCasesDefectsRequest>()).ToList();

                                result.Add(index >= lstWeek.Count ? "PreviousWeek" : "ThisWeek", new
                                {
                                    cases = caseQuery,
                                    defects = caseDefects,
                                });

                                index++;
                            }
                        }
                        else
                        {
                            var handleTimeLine = HandleTimeLineTotalCasesDefects(
                                (await resultQuery.ReadAsync<TempSummaryCasesDefectsRequest>()).ToList(),
                                (await resultQuery.ReadAsync<TempSummaryCasesDefectsRequest>()).ToList());

                            result.Add("TimeLine", handleTimeLine);
                        }

                    }
                }
            }
            catch (Exception ce)
            {
                return Ok(message: ce.Message);
            }

            return Ok(result);
        }

        /// <summary>
        /// Func get Top 10 Defects by Number of Items
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (19.06.2023)
        public async Task<ServiceResponse> PostDLPSummaryTop10Items(string request)
        {
            using var connectionMainteance = _appMaintenanceDbContext.CreateConnection();
            ResourceManager rm = new ResourceManager("DashboardApi.Resource.QueryMaintenance", Assembly.GetExecutingAssembly());
            Dictionary<string, object> result = new Dictionary<string, object>();

            try
            {
                // init data
                string whereSQLDefects = $"1=1",
                       querySQL = rm.GetString("SummaryPostDLPTop10Defects", CultureInfo.CurrentCulture),
                       finalQuery = string.Empty;

                // build where query
                if (!string.IsNullOrEmpty(querySQL))
                {
                    if (!string.IsNullOrWhiteSpace(request))
                    {
                        if ((request.StartsWith("{") && request.EndsWith("}")) || (request.StartsWith("[") && request.EndsWith("]")))
                        {
                            SummaryRequest requestConvert = JsonConvert.DeserializeObject<SummaryRequest>(request);

                            // filter by project
                            if (requestConvert.listProject != null && requestConvert.listProject.Count() > 0)
                            {
                                whereSQLDefects = $"{whereSQLDefects} AND M.PROJECT_ID IN ('{string.Join("','", requestConvert.listProject.ToArray())}')";
                            }
                            // filter by date
                            if (!string.IsNullOrWhiteSpace(requestConvert.gteDate))
                            {
                                whereSQLDefects = $"{whereSQLDefects} AND DM.CONFIRMATION_DATE <= '{requestConvert.gteDate}'";
                            }
                            if (!string.IsNullOrWhiteSpace(requestConvert.lteDate))
                            {
                                whereSQLDefects = $"{whereSQLDefects} AND DM.CONFIRMATION_DATE >= '{requestConvert.lteDate}'";
                            }
                        }
                    }
                }

                // query
                using (var resultQuery = await connectionMainteance.QueryMultipleAsync(querySQL.Replace("@WHERESQLDEFECTS", whereSQLDefects), new { }))
                {
                    if (resultQuery != null)
                    {
                        var data = (await resultQuery.ReadAsync<TempSummaryCasesDefectsRequest>()).ToList();

                        result.Add("data", data);
                    }
                }
            }
            catch (Exception ce)
            {
                return Ok(message: ce.Message);
            }

            return Ok(result);
        }

        /// <summary>
        /// Func get Top 10 Defects by Number of Items
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (19.06.2023)
        public async Task<ServiceResponse> PostDLPSummaryItemByTypeDetail(string request)
        {
            using var connectionMainteance = _appMaintenanceDbContext.CreateConnection();
            ResourceManager rm = new ResourceManager("DashboardApi.Resource.QueryMaintenance", Assembly.GetExecutingAssembly());
            Dictionary<string, object> result = new Dictionary<string, object>();

            try
            {
                // init data
                string whereSQLDefects = $"1=1",
                       querySQL = rm.GetString("SummaryPostDLPTop10DefectsStatus", CultureInfo.CurrentCulture),
                       finalQuery = string.Empty;

                // build where query
                if (!string.IsNullOrEmpty(querySQL))
                {
                    if (!string.IsNullOrWhiteSpace(request))
                    {
                        if ((request.StartsWith("{") && request.EndsWith("}")) || (request.StartsWith("[") && request.EndsWith("]")))
                        {
                            SummaryRequest requestConvert = JsonConvert.DeserializeObject<SummaryRequest>(request);

                            // filter by project
                            if (requestConvert.listProject != null && requestConvert.listProject.Count() > 0)
                            {
                                whereSQLDefects = $"{whereSQLDefects} AND M.PROJECT_ID IN ('{string.Join("','", requestConvert.listProject.ToArray())}')";
                            }
                            // filter by date
                            if (!string.IsNullOrWhiteSpace(requestConvert.gteDate))
                            {
                                whereSQLDefects = $"{whereSQLDefects} AND DM.CONFIRMATION_DATE <= '{requestConvert.gteDate}'";
                            }
                            if (!string.IsNullOrWhiteSpace(requestConvert.lteDate))
                            {
                                whereSQLDefects = $"{whereSQLDefects} AND DM.CONFIRMATION_DATE >= '{requestConvert.lteDate}'";
                            }
                        }
                    }
                }

                // query
                using (var resultQuery = await connectionMainteance.QueryMultipleAsync(querySQL.Replace("@WHERESQLDEFECTS", whereSQLDefects), new { }))
                {
                    if (resultQuery != null)
                    {
                        var data = (await resultQuery.ReadAsync<TempSummaryCasesDefectsRequest>()).GroupBy(x => x.TypeMaintenance).ToList();
                        result.Add("data", data);
                    }
                }
            }
            catch (Exception ce)
            {
                return Ok(message: ce.Message);
            }

            return Ok(result);
        }


        #endregion

        #region Function Common

        /// <summary>
        /// Handle total cases defects
        /// </summary>
        /// <param name="cases"></param>
        /// <param name="defects"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (04.07.2023)
        public List<TempSummaryCasesDefectsRequest> HandleTimeLineTotalCasesDefects(List<TempSummaryCasesDefectsRequest> cases, List<TempSummaryCasesDefectsRequest> defects)
        {
            List<TempSummaryCasesDefectsRequest> result = new List<TempSummaryCasesDefectsRequest>();

            try
            {
                // step 1: get all datetime have in each timline case defects incoming/outgoing
                List<string> times = new List<string>();

                foreach (var ele in cases)
                {
                    times.Add(ele.DateConcat);
                }

                foreach (var ele in defects)
                {
                    times.Add(ele.DateConcat);
                }

                // distinct and order by date
                times = times.OrderBy(x => x).Distinct().ToList();

                // step 2: for loop list time and get total incoming/outgoing
                foreach (var time in times)
                {
                    result.Add(new TempSummaryCasesDefectsRequest
                    {
                        DateConcat = time,
                        TotalCases = cases.Find(x => x.DateConcat == time)?.Total,
                        TotalDefects = defects.Find(x => x.DateConcat == time)?.Total,
                    });
                }
            }
            catch (Exception ce)
            {
                throw ce;
            }

            return result;
        }

        /// <summary>
        /// Handle data for incoming outgoing
        /// </summary>
        /// <param name="timeLineDefectsIncomings"></param>
        /// <param name="timeLineDefectsOutGoings"></param>
        /// <param name="timelineCasesIncomings"></param>
        /// <param name="timelineCasesOutGoings"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (03.07.2023)
        public List<CombineCasesDefectsMonitoringRequest> CombineInconingOutgoing(List<CombineCasesDefectsMonitoringRequest> timeLineDefectsIncomings,
            List<CombineCasesDefectsMonitoringRequest> timeLineDefectsOutGoings,
            List<CombineCasesDefectsMonitoringRequest> timeLineCasesIncomings,
            List<CombineCasesDefectsMonitoringRequest> timeLineCasesOutGoings)
        {
            List<CombineCasesDefectsMonitoringRequest> result = new List<CombineCasesDefectsMonitoringRequest>();

            try
            {
                // step 1: get all datetime have in each timline case defects incoming/outgoing
                List<string> times = new List<string>();

                foreach (var timeLineDefectsIncoming in timeLineDefectsIncomings)
                {
                    times.Add(timeLineDefectsIncoming.DateConcat);
                }

                foreach (var timeLineDefectsOutGoing in timeLineDefectsOutGoings)
                {
                    times.Add(timeLineDefectsOutGoing.DateConcat);
                }

                foreach (var timelineCasesIncoming in timeLineCasesIncomings)
                {
                    times.Add(timelineCasesIncoming.DateConcat);
                }

                foreach (var timelineCasesOutGoing in timeLineCasesOutGoings)
                {
                    times.Add(timelineCasesOutGoing.DateConcat);
                }

                // distinct and order by date
                times = times.OrderBy(x => x).Distinct().ToList();

                // step 2: for loop list time and get total incoming/outgoing
                foreach (var time in times)
                {
                    result.Add(new CombineCasesDefectsMonitoringRequest
                    {
                        DateTemp = time,

                        TimelineIncomingCases = timeLineCasesIncomings.Find(x => x.DateConcat == time)?.Total,
                        TimelineIncomingDefects = timeLineDefectsIncomings.Find(x => x.DateConcat == time)?.Total,

                        TimelineOutgoingCases = timeLineCasesOutGoings.Find(x => x.DateConcat == time)?.Total,
                        TimelineOutgoingDefects = timeLineDefectsOutGoings.Find(x => x.DateConcat == time)?.Total,
                    });
                }
            }
            catch (Exception ce)
            {
                throw ce;
            }

            return result;
        }

        /// <summary>
        /// Func convert status
        /// </summary>
        /// <param name="readResults"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (09.06.2023)
        public List<TempSummaryCasesDefectsRequest> ConvertStatus(List<TempSummaryCasesDefectsRequest> readResults)
        {

            foreach (var readResult in readResults)
            {
                if (!string.IsNullOrEmpty(readResult.Status))
                {
                    switch (readResult.Status.ToLower())
                    {
                        case "closed":
                            readResult.Status = "Defect List Signed Off";
                            break;
                        case "completed":
                            readResult.Status = "Defect List Pending Joint Inspection (Work Completed)";
                            break;
                        case "wip":
                            readResult.Status = "Defect List Outstanding";
                            break;
                        case "ps":
                            readResult.Status = "Defect List Outstanding";
                            break;
                        case "new":
                            readResult.Status = "Defect List New (Pending Joint Inspection)";
                            break;
                        default:
                            readResult.Status = "Other";
                            break;
                    }
                }
                else
                {
                    readResult.Status = "Other";
                }
            }

            return readResults;
        }

        /// <summary>
        /// Func convert status outstanding
        /// </summary>
        /// <param name="readResults"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (09.06.2023)
        public List<TempSummaryCasesDefectsRequest> ConvertStatusOutstanding(List<TempSummaryCasesDefectsRequest> readResults)
        {

            foreach (var readResult in readResults)
            {
                switch (readResult?.Tag?.ToUpper())
                {
                    case "BLANK":
                        readResult.Status = "By keys (work in progress)";
                        break;
                    case "REWK":
                        readResult.Status = "By keys (work in progress)";
                        break;
                    case "RDF":
                        readResult.Status = "By appointments (work in progress)";
                        break;
                    case "UNC":
                        readResult.Status = "By appointments (work in progress)";
                        break;
                    case "CSOR":
                        readResult.Status = "By appointments (work in progress)";
                        break;
                    case "RENO":
                        readResult.Status = "Defect list pending owner providing key (Renovation)";
                        break;
                    case "MOS":
                        readResult.Status = "Defect list Partial sign off (Pending MOS)";
                        break;
                    default:
                        readResult.Status = "Other";
                        break;
                }
            }

            return readResults;
        }
        #endregion
    }
}