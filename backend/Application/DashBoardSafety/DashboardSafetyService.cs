using AutoMapper;
using Dapper;
using DashboardApi.Application.Project;
using DashboardApi.Auth;
using DashboardApi.Auth.PermisionChecker;
using DashboardApi.DatabaseContext.DapperDbContext;
using DashboardApi.Dtos.Safety.Request;
using DashboardApi.Dtos.Safety.Response;
using DashboardApi.HttpConfig;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Resources;
using static DashboardApi.Dtos.Safety.Response.Statistics;

namespace DashboardApi.Application.DashBoardSafety
{
    public class DashboardSafetyService : Service, IDashboardSafetyService
    {
        private readonly SafetyDbContext _safetyContext;
        private readonly IPermissionChecker _permissionChecker;
        private readonly IProjectService _projectService;
        public readonly IMapper _mapper;
        private readonly IOptions<HttpEndpoint> _options;


        public DashboardSafetyService(SafetyDbContext safetyContext, IPermissionChecker permissionChecker, IMapper mapper, IOptions<HttpEndpoint> options, IProjectService projectService)
        {
            _safetyContext = safetyContext;
            _permissionChecker = permissionChecker;
            _mapper = mapper;
            _options = options;
            _projectService = projectService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ServiceResponse> GetStatistic(SearchRequest request)
        {
            #region Get Permission and Date
            //Check permission
            var isHasPermission = _permissionChecker.HasPermission(PermissionConstants.DASHBOARD_SAFETY_VIEW_REPORT).Result;
            if (!isHasPermission)
            {
                return Unauthorized("Sorry, you are not authorized to access this function");
            }

            using var connection = _safetyContext.CreateConnection();
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            var currentDate = request.FromDate != null ? request.FromDate.Value : DateTime.Now;
            var currentYear = currentDate.Year;
            var currentMonth = request.Month != null ? request.Month.Value : currentDate.Month;
            if (currentYear < DateTime.Now.Year)
                currentMonth = 12;
            var yearBefore = currentYear - 1;

            var lastDayOfMonth = new DateTime(currentYear, currentMonth, DateTime.DaysInMonth(currentYear, currentMonth));
            var firstDayOfMonth = new DateTime(currentMonth == 12 ? currentYear : yearBefore, currentMonth == 12 ? 1 : currentMonth + 1, 1);
            #endregion

            #region Get Project with JV
            var projectJV = (await _projectService.GetAllProjects())?.Where(x => x.IsJV == false).ToList();

            #endregion

            #region Get Statistic
            var statistic = GetStatisticEntity(lastDayOfMonth, firstDayOfMonth).ToList();

            if (!string.IsNullOrEmpty(request.ProjectName))
            {
                statistic = statistic.Where(c => c.ProjectName == request.ProjectName).ToList();
            }
            var parameters = new { currentYear = currentYear, fromDate = request.FromDate, projectName = request.ProjectName, ProjectIds = request?.ListProjects?.Select(c => c.ProjectName).ToList(), month = request.Month };
            List<StatisticResponse> statisticCurrentResponses = new List<StatisticResponse>();
            List<ProjectResponse> projectResponses = new List<ProjectResponse>();

            try
            {
                statisticCurrentResponses = statistic.Where(c => c.Date.Year == currentYear).ToList();
                var projectStatistics = statisticCurrentResponses.GroupBy(e => e.ProjectName).Select(g => g.FirstOrDefault()).Select(c => new ProjectResponse() { ProjectId = c.ProjectId, ProjectName = c.ProjectName }).OrderBy(c => c.ProjectName).ToList();
                if (projectStatistics.Any())
                {
                    projectResponses.AddRange(projectStatistics);
                }
                if (request.Month != null)
                {
                    statisticCurrentResponses = statisticCurrentResponses.Where(c => c.Date.Month == request.Month).ToList();
                }
            }
            catch (Exception ex)
            {

            }

            if (connection.State != ConnectionState.Closed)
            {
                connection.Close();
            }
            var report = new StatisticReport();

            var count = statisticCurrentResponses != null ? statisticCurrentResponses.ToList().Count() : 0;
            var d = statisticCurrentResponses.Sum(c => c.AveDailyClientAndRepPersonnel);

            var statisticTmp = await GetStatisticReportQuery(request, currentYear, currentMonth);

            report.WhStrength = Math.Round(statisticTmp.WhStrength, 0);
            report.ClientRPStrength = Math.Round(statisticTmp.ClientRPStrength, 0);
            report.SubconStrength = Math.Round(statisticTmp.SubconStrength, 0);

            report.WhManhoursWorker = Math.Round(statisticCurrentResponses.Sum(c => c.TotalWHManhoursWorked), 0);
            report.ClientRPWorker = Math.Round(statisticCurrentResponses.Sum(c => c.TotalClientWorked), 0);
            report.SubconManhoursWorked = Math.Round(statisticCurrentResponses.Sum(c => c.TotalSCMHWorked), 0);

            //Get Incident Register
            int maxMonth = statistic != null ? statistic.Select(c => c.Date).OrderByDescending(c => c).FirstOrDefault().Month : 0;
            if (maxMonth > 0 && maxMonth < DateTime.Now.Month && currentYear == DateTime.Now.Year && request.Month == null)
            {
                currentMonth = maxMonth;
                lastDayOfMonth = new DateTime(currentYear, currentMonth, DateTime.DaysInMonth(currentYear, currentMonth));
                firstDayOfMonth = new DateTime(currentMonth == 12 ? currentYear : yearBefore, currentMonth == 12 ? 1 : currentMonth + 1, 1);
                //Get Statistic
                statistic = GetStatisticEntity(lastDayOfMonth, firstDayOfMonth).ToList();
            }
            #endregion

            #region Safety - Total incident
            var incidentSql = "select * from \"IncidentRegisterEntities\" where is_deleted <> true  and date_created::date <=@lastDayOfMonth";
            var parameters2 = new { currentYear = currentYear, yearBefore = yearBefore, lastDayOfMonth = lastDayOfMonth };

            //Get data from table Incident Register Entities
            List<IncidentRegister> incidents = (await connection.QueryAsync<IncidentRegister>(incidentSql, parameters2)).ToList(); connection.Close();

            var incidentCurrentYears = incidents?.Where(c => c.DateCreated.Year == currentYear).ToList();
            var projects = incidentCurrentYears.GroupBy(e => e.ProjectName).Select(g => g.FirstOrDefault()).Select(c => new ProjectResponse() { ProjectId = c.ProjectId, ProjectName = c.ProjectName }).OrderBy(c => c.ProjectName).ToList();
            if (projects.Any())
            {
                projectResponses.AddRange(projects);
            }
            if (!string.IsNullOrEmpty(request.ProjectName))
            {
                incidentCurrentYears = incidentCurrentYears.Where(c => c.ProjectName == request.ProjectName).ToList();
                incidents = incidents.Where(c => c.ProjectName == request.ProjectName).ToList();
            }
            if (request.Month != null)
            {
                incidentCurrentYears = incidentCurrentYears.Where(c => c.DateCreated.Month == request.Month).ToList();
            }

            //Number of Fatality
            var fatality = incidentCurrentYears.Where(c => c.IncidentClassification == "Fatality").ToList();
            report.NumberOfFatality = fatality.Count;

            //Number of Reportable Accidents
            var reportableAccidents = incidentCurrentYears.Where(c => c.IncidentClassification == "Reportable Accident").ToList();
            report.NumberOfReportableAccidents = reportableAccidents.Count;

            //Number of Dangerous Occurrence
            var dangerousOccurrences = incidentCurrentYears.Where(c => c.IncidentClassification == "Dangerous Occurrence").ToList();
            report.NumberOfDangerousOccurrence = dangerousOccurrences.Count;

            //Number of Lost Time Injury
            var lostTimeInjury = incidentCurrentYears.Where(c => c.IncidentClassification == "Lost Time Injury").ToList();
            report.NumberOfLostTimeInjury = lostTimeInjury.Count;

            //Number of Light Duty Case
            var lightDutyCase = incidentCurrentYears.Where(c => c.IncidentClassification == "Light Duty Case").ToList();
            report.NumberOfLightDutyCase = lightDutyCase.Count;

            //Number of Near Miss
            var nearMiss = incidentCurrentYears.Where(c => c.IncidentClassification == "Near Miss").ToList();
            report.NumberOfNearMiss = nearMiss != null ? nearMiss.Count() : 0;
            var permanentDisability = incidentCurrentYears.Where(c => c.IncidentClassification == "Permanent Disability").ToList();
            #endregion

            // cal full project
            report = CalculateWIRASR(request, report, incidents, statistic, firstDayOfMonth, lastDayOfMonth, currentDate);

            if (projectJV != null && projectJV.Count() > 0)
            {
                // Lọc incidents, statistic dựa trên ProjectId có trong projectJV
                var incidentsWH = incidents
                    .Where(incident => projectJV.Any(project => project?.Id == incident?.ProjectId))
                    .ToList();

                var statisticWH = statistic
                    .Where(incident => projectJV.Any(project => project?.Id == incident?.ProjectId))
                    .ToList();

                var reportJV = new StatisticReport();

                reportJV = CalculateWIRASR(request, reportJV, incidentsWH, statisticWH, firstDayOfMonth, lastDayOfMonth, currentDate);

                report.WorkplaceInjuryRateJV = reportJV.WorkplaceInjuryRate;
                report.AccidentSeverityRateJV = reportJV.AccidentSeverityRate;
            }
            else
            {
                report.WorkplaceInjuryRateJV = report.WorkplaceInjuryRate;
                report.AccidentSeverityRateJV = report.AccidentSeverityRate;
            }

            return Ok(new { projectJV = projectJV, statistic = report, projects = projectResponses.GroupBy(c => c.ProjectId).Select(c => c.FirstOrDefault()) });
        }

        /// <summary>
        /// Calculate WIR ASR
        /// </summary>
        /// <param name="request"></param>
        /// <param name="report"></param>
        /// <param name="incidents"></param>
        /// <param name="statistic"></param>
        /// <param name="firstDayOfMonth"></param>
        /// <param name="lastDayOfMonth"></param>
        /// <param name="currentDate"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (20.11.2024)
        private StatisticReport CalculateWIRASR(SearchRequest request, StatisticReport report,
            List<IncidentRegister> incidents, List<StatisticResponse> statistic,
            DateTime firstDayOfMonth, DateTime lastDayOfMonth, DateTime currentDate)
        {
            var incidentsRolling12Month = new List<IncidentRegister>();

            #region WIR

            // Check condition for project and 12 month
            if (!string.IsNullOrEmpty(request.ProjectName))
            {
                incidents = incidents.Where(c => c.ProjectName == request.ProjectName).ToList();
            }
            if (incidents != null)
            {
                incidentsRolling12Month = incidents.Where(c => c.DateCreated.Date >= firstDayOfMonth && c.DateCreated.Date <= lastDayOfMonth).ToList();
            }

            // check has 12 months
            var checkIs12Months = incidentsRolling12Month.Select(c => c.DateCreated.Date.Month).Distinct().ToList();
            if (checkIs12Months.Count() < 12)
            {
                report.WorkplaceInjuryRate = -1;
                report.AccidentSeverityRate = -1;

                return report;
            }

            //Number of Fatality
            var fatalityRolling12Month = incidentsRolling12Month.Where(c => c.IncidentClassification == "Fatality").ToList();

            //Number of Reportable Accidents
            var reportableAccidentsRolling12Month = incidentsRolling12Month.Where(c => c.IncidentClassification == "Reportable Accident").ToList();

            // Number of Permanent Disability
            var permanentDisabilityRolling12Month = incidentsRolling12Month.Where(c => c.IncidentClassification == "Permanent Disability").ToList();

            // Get total incidents
            var totalAccidents = fatalityRolling12Month.Count + (reportableAccidentsRolling12Month.Count) + permanentDisabilityRolling12Month.Count;

            // Get Accumulation 
            var accumulation = GetAccumulations(currentDate).GroupBy(c => c.ProjectId).Select(g => g.FirstOrDefault());

            // Get total statics WHManhours, ClientWorked, SCMHWorked
            var totalMHWorker = statistic.Sum(c => c.TotalWHManhoursWorked + c.TotalClientWorked + c.TotalSCMHWorked);

            // Get total statis daily Ave Personnel
            double dailyAvePersonnel = statistic.Sum(c => c.AveDailyWHPersonnel + c.AveDailyClientAndRepPersonnel + c.AveDailySubconPersonnel);

            // Get WIR
            double wir = totalMHWorker > 0 ? ((totalAccidents * 100000) / (dailyAvePersonnel / 12)) : 0;
            report.WorkplaceInjuryRate = Math.Round(wir, 2);
            #endregion

            #region ASR

            // Get sum medical
            var medicalLeaveOfFatalityASR = fatalityRolling12Month.Count > 0 ? fatalityRolling12Month.Sum(c => c.MedicalLeaveDay) : 0;
            var medicalLeaveOfPermanentDisabilityASR = reportableAccidentsRolling12Month.Count > 0 ? reportableAccidentsRolling12Month.Sum(c => c.MedicalLeaveDay) : 0;
            var medicalLeaveOfReportableASR = permanentDisabilityRolling12Month.Count > 0 ? permanentDisabilityRolling12Month.Sum(c => c.MedicalLeaveDay) : 0;

            // Get asr
            var asr = totalMHWorker > 0 ? ((medicalLeaveOfFatalityASR + medicalLeaveOfPermanentDisabilityASR + medicalLeaveOfReportableASR) * 1000000 / totalMHWorker) : 0;
            report.AccidentSeverityRate = Math.Round(asr, 2);
            #endregion

            return report;
        }

        /// <summary>
        /// GetStatisticReportQuery
        /// </summary>
        /// <param name="request"></param>
        /// <param name="currentYear"></param>
        /// <param name="currentMonth"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (24.10.2024)
        public async Task<StatisticReport> GetStatisticReportQuery(SearchRequest request, int currentYear, int currentMonth)
        {
            var report = new StatisticReport();

            try
            {
                using var connection = _safetyContext.CreateConnection();
                ResourceManager rm = new ResourceManager("DashboardApi.Resource.QuerySafety", Assembly.GetExecutingAssembly());
                // init data
                string whereSQL = $"",
                        querySQL = rm.GetString("StatisticReportV1", CultureInfo.CurrentCulture);

                if (!string.IsNullOrEmpty(request.ProjectName))
                {
                    whereSQL += " AND LOWER(PROJECT_NAME) = LOWER(@PROJECTNAME)";
                }
                if (currentYear != null)
                {
                    whereSQL += " AND EXTRACT(YEAR FROM DATE) = @CURRENTYEAR ";
                }
                if (currentMonth != null)
                {
                    whereSQL += " AND EXTRACT(MONTH FROM DATE) <= @CURRENTMONTH ";
                }

                // query
                using (var resultQuery = await connection.QueryMultipleAsync(querySQL.Replace("@WHERECLAUSE", whereSQL), new
                {
                    PROJECTNAME = request.ProjectName,
                    CURRENTYEAR = currentYear,
                    CURRENTMONTH = currentMonth,
                }))
                {
                    report = (await resultQuery.ReadAsync<StatisticReport>()).ToList().FirstOrDefault();
                }
            }
            catch (Exception)
            {

                throw;
            }

            return report;
        }

        public List<IncidentRegister> GetIncidentRegisters(int currentYear, SearchRequest request)
        {
            using var connection = _safetyContext.CreateConnection();
            var incidentSql =
                "select * from \"IncidentRegisterEntities\" " +
                "where is_deleted <> true and extract(year from date_created)<=@currentYear";
            //if (!string.IsNullOrEmpty(request.ProjectName))
            //{
            //    incidentSql += Environment.NewLine + @$" and project_name=@projectName";
            //}
            //if(request.ListProjects!=null &&  request.ListProjects.Count > 0)
            //   incidentSql += " and project_name = any(@ProjectIds) ";

            //if (request.Month != null)
            //{
            //    incidentSql += Environment.NewLine + @$"
            //        and extract(month from date_created)=@month";
            //}

            //if(request.Month!=null)
            //    incidentSql += Environment.NewLine + @$" and extract(month from date_created)=@month";
            var parameters = new { currentYear = currentYear, fromDate = request.FromDate, projectName = request.ProjectName, ProjectIds = request?.ListProjects?.Select(c => c.ProjectName).ToList(), month = request.Month };
            //Get data from table Incident Register Entities
            List<IncidentRegister> incidentRegisters =
                connection.QueryAsync<IncidentRegister>(incidentSql, parameters).Result.ToList();
            return incidentRegisters;
        }
        public List<IncidentRegister> GetIncidentRegisters2(int currentYear, SearchRequest request)
        {
            using var connection = _safetyContext.CreateConnection();
            var incidentSql =
                "select * from \"IncidentRegisterEntities\" " +
                "where is_deleted <> true and extract(year from date_created)=@currentYear";
            //if (!string.IsNullOrEmpty(request.ProjectName))
            //{
            //    incidentSql += Environment.NewLine + @$" and project_name=@projectName";
            //}
            //if(request.ListProjects!=null &&  request.ListProjects.Count > 0)
            //   incidentSql += " and project_name = any(@ProjectIds) ";

            //if (request.Month != null)
            //{
            //    incidentSql += Environment.NewLine + @$"
            //        and extract(month from date_created)=@month";
            //}

            //if(request.Month!=null)
            //    incidentSql += Environment.NewLine + @$" and extract(month from date_created)=@month";
            var parameters = new { currentYear = currentYear, fromDate = request.FromDate, projectName = request.ProjectName, ProjectIds = request?.ListProjects?.Select(c => c.ProjectName).ToList(), month = request.Month };
            //Get data from table Incident Register Entities
            List<IncidentRegister> incidentRegisters =
                connection.QueryAsync<IncidentRegister>(incidentSql, parameters).Result.ToList();
            return incidentRegisters;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentYear"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public async Task<List<SummaryOfPerformanceScoreV2>> GetAuditEntities(int currentYear, int? month = null)
        {
            var result = new List<SummaryOfPerformanceScoreV2>();
            try
            {
                using var connection = _safetyContext.CreateConnection();
                ResourceManager rm = new ResourceManager("DashboardApi.Resource.QuerySafety", Assembly.GetExecutingAssembly());
                // init data
                string whereSQL = $" AND EXTRACT(YEAR FROM DATE_CREATED)=@CURRENTYEAR ",
                        querySQL = rm.GetString("SummaryPCAuditScore", CultureInfo.CurrentCulture);

                if (month != null)
                {
                    whereSQL += $" AND EXTRACT(MONTH FROM DATE_CREATED)=@CURRENTMONTH ";
                }

                // query
                using (var resultQuery = await connection.QueryMultipleAsync(querySQL.Replace("@WHERECLAUSE", whereSQL), new
                {
                    CURRENTYEAR = currentYear,
                    CURRENTMONTH = month,
                }))
                {
                    result = (await resultQuery.ReadAsync<SummaryOfPerformanceScoreV2>()).ToList();
                }

                connection.Close();
            }
            catch (Exception ce)
            {
                throw;
            }

            return result;
        }
        public List<AccumulationEntity> GetAccumulations(DateTime date)
        {
            try
            {
                using var connection = _safetyContext.CreateConnection();
                var incidentSql =
                    "select * from accumulation_entity where is_deleted <> true ";
                var parameters = new { currentDate = date };
                //Get data from table Incident Register Entities
                List<AccumulationEntity> accumulations = connection.QueryAsync<AccumulationEntity>(incidentSql, parameters).Result.ToList();
                accumulations = accumulations.Where(c => c.StartDate.Year <= 2021 && c.EndDate.Year <= 2021).ToList();
                return accumulations;
            }
            catch (Exception ex)
            {
                return new List<AccumulationEntity>();
            }
        }

        public async Task<ServiceResponse> GetTotalIncidentNumbersByHazardTypeReport(SearchRequest request)
        {
            //Check permission
            var isHasPermission = _permissionChecker.HasPermission(PermissionConstants.DASHBOARD_SAFETY_VIEW_REPORT).Result;
            if (!isHasPermission)
            {
                return Unauthorized("Sorry, you are not authorized to access this function");
            }
            using var connection = _safetyContext.CreateConnection();
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            var incidentSql =
                "select * from \"IncidentRegisterEntities\" " +
                "where is_deleted <> true and (extract(year from date_created)=@currentYear or extract(year from date_created)=@beforeYear) ";
            var currentYear = DateTime.Now.Year;
            int? currentMonth = null;
            if (request.FromDate != null)
            {
                currentYear = request.FromDate.Value.Year;
            }
            if (request.Month != null)
            {
                currentMonth = request.Month.Value;
                incidentSql += " and extract(month from date_created)=@currentMonth";
            }
            if (request.ProjectName != null)
            {
                incidentSql += " and project_name=@projectName";
            }
            var beforeYear = currentYear - 1;
            var parameters = new { currentYear = currentYear, beforeYear = beforeYear, currentMonth = currentMonth, projectName = request.ProjectName };
            List<IncidentNumbersByHazardTypeReport> incidentNumbersByHazardTypeReport = new List<IncidentNumbersByHazardTypeReport>();

            //Get data from table Incident Register Entities
            List<IncidentRegister> incidentRegisters =
                (await connection.QueryAsync<IncidentRegister>(incidentSql, parameters)).ToList();
            connection.Close();

            //Get list Hazard Type
            var listHazardTypes = incidentRegisters.GroupBy(c => c.NatureOfHazard).Select(c => c.FirstOrDefault()).Select(c => c.NatureOfHazard).Where(c => c != null);
            var listCurrentYears = incidentRegisters.Where(c => c.DateCreated.Year == currentYear);
            var listBeforeYears = incidentRegisters.Where(c => c.DateCreated.Year == beforeYear);

            foreach (var hazardType in listHazardTypes)
            {
                var currentIncidentByTypes = listCurrentYears.Where(c => c.NatureOfHazard == hazardType).ToList();
                var beforeIncidentByTypes = listBeforeYears.Where(c => c.NatureOfHazard == hazardType).ToList();
                IncidentNumbersByHazardTypeReport record = new IncidentNumbersByHazardTypeReport()
                {
                    NameOfHazardType = hazardType,
                    CurrentYear = currentIncidentByTypes.Count(),
                    BeforeYear = beforeIncidentByTypes.Count(),
                };
                if (record.CurrentYear > 0 || record.BeforeYear > 0)
                {
                    incidentNumbersByHazardTypeReport.Add(record);
                }
            }
            return Ok(new { reports = incidentNumbersByHazardTypeReport.OrderByDescending(x => x.CurrentYear), currentYear = currentYear, beforeYear = beforeYear });
        }

        public async Task<ServiceResponse> GetTotalIncidentNumbersByProjectsReport(SearchRequest request)
        {
            //Check permission
            var isHasPermission = _permissionChecker.HasPermission(PermissionConstants.DASHBOARD_SAFETY_VIEW_REPORT).Result;
            if (!isHasPermission)
            {
                return Unauthorized("Sorry, you are not authorized to access this function");
            }
            using var connection = _safetyContext.CreateConnection();
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            var incidentSql =
                "select * from \"IncidentRegisterEntities\" " +
                "where is_deleted <> true and extract(year from date_created)=@currentYear";
            var currentYear = DateTime.Now.Year;
            int? currentMonth = null;
            if (request.FromDate != null)
            {
                currentYear = request.FromDate.Value.Year;
            }
            if (request.Month != null)
            {
                currentMonth = request.Month.Value;
                incidentSql += " and extract(month from date_created)=@currentMonth";
            }
            if (request.ProjectName != null)
            {
                incidentSql += " and project_name=@projectName";
            }
            var parameters = new { currentYear = currentYear, currentMonth = currentMonth, projectName = request.ProjectName };
            //Get data from table Incident Register Entities
            List<IncidentRegister> incidentRegisters =
                (await connection.QueryAsync<IncidentRegister>(incidentSql, parameters)).ToList();
            connection.Close();
            //Get list Hazard Type
            var listProjects = incidentRegisters.GroupBy(c => c.ProjectId).Select(c => c.FirstOrDefault())
                .Select(c => new { ProjectId = c.ProjectId, ProjectName = c.ProjectName });
            var currentDate = DateTime.Now;
            var lastDayOfMonth = new DateTime(currentDate.Year, 12, DateTime.DaysInMonth(currentDate.Year, 12));
            List<IncidentNumbersByProject> incidentNumbersByProjectReport = new List<IncidentNumbersByProject>();
            foreach (var project in listProjects)
            {
                var list = incidentRegisters.Where(c => c.ProjectId == project.ProjectId);
                var reportableAccidents =
                    list.Where(c => c.IncidentClassification == "Reportable Accident").ToList();
                IncidentNumbersByProject record = new IncidentNumbersByProject()
                {
                    ProjectName = project.ProjectName,
                    ReportableAccident = reportableAccidents.Count(),
                    ManDayLost = reportableAccidents.Sum(c => c.MedicalLeaveDay),
                };

                if (record?.ReportableAccident > 0 && record?.ManDayLost > 0)
                {
                    incidentNumbersByProjectReport.Add(record);
                }
            }

            return Ok(incidentNumbersByProjectReport);
        }
        public async Task<ServiceResponse> GetTotalAuthorityComplianceReport(SearchRequest request)
        {
            //Check permission
            var isHasPermission = _permissionChecker.HasPermission(PermissionConstants.DASHBOARD_SAFETY_VIEW_REPORT).Result;
            if (!isHasPermission)
            {
                return Unauthorized("Sorry, you are not authorized to access this function");
            }
            using var connection = _safetyContext.CreateConnection();
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            var incidentSql =
                "select * from \"LegalNCRegisterEntity\" " +
                "where is_deleted <> true and extract(year from date_created)=@currentYear";
            var currentYear = DateTime.Now.Year;
            int? currentMonth = null;
            if (request.FromDate != null)
            {
                currentYear = request.FromDate.Value.Year;
            }
            if (request.Month != null)
            {
                currentMonth = request.Month.Value;
                incidentSql += " and extract(month from date_created)=@currentMonth";
            }
            if (request.ProjectName != null)
            {
                incidentSql += " and project_name=@projectName";
            }
            var parameters = new { currentYear = currentYear, currentMonth = currentMonth, projectName = request.ProjectName };
            //Get data from table Incident Register Entities
            List<LegalNCRegister> incidentRegisters =
                (await connection.QueryAsync<LegalNCRegister>(incidentSql, parameters)).ToList();
            connection.Close();
            //Get list Hazard Type
            var listAuthoritys = incidentRegisters.GroupBy(c => c.Authority).Select(c => c.FirstOrDefault())
                .Select(c => c.Authority);

            List<AuthorityCompliance> authorityCompliances = new List<AuthorityCompliance>();
            foreach (var authority in listAuthoritys)
            {
                var list = incidentRegisters.Where(c => c.Authority == authority);
                var reason = list.GroupBy(c => new { c.Authority, c.Reason }).Select(c => new AuthorityCompliance
                {
                    Name = authority + "-" + c.FirstOrDefault().Reason,
                    Fine = c.Where(c => c.Type == "Fine").ToList().Sum(c => c.FineAmount),
                    SWO = c.Where(c => c.Type == "SWO").ToList().Count(),
                    NNC = c.Where(c => c.Type == "NNC").ToList().Count(),
                    Demerit = c.Where(c => c.Type == "Demerit Points").Count(),
                    FineOfLine = c.Where(c => c.Type == "Fine").ToList().Sum(c => c.FineAmount),
                    NEARodent = c.Where(c => c.Type == "NEA").ToList().Sum(c => c.NEARodent),
                    NEAWdph = c.Where(c => c.Type == "NEA").ToList().Sum(c => c.NEAWdph),
                });
                authorityCompliances.AddRange(reason);
            }

            return Ok(authorityCompliances);

        }

        public List<StatisticResponse> GetStatisticEntity(DateTime toDate, DateTime? fromDate = null)
        {
            var currentYear = toDate.Year;
            using var connection = _safetyContext.CreateConnection();
            var incidentSql =
                "select * from \"statistic_entity\" " +
                "where is_deleted <> true";

            incidentSql += fromDate != null ? " and date::date >=@fromDate and date::date <=@toDate " : " and date::date <=@toDate ";
            incidentSql += " order by date;";

            var parameters = new { currentYear = currentYear, fromDate = fromDate, toDate = toDate };
            //Get data from table Incident Register Entities
            List<StatisticResponse> statisticResponses =
                (connection.QueryAsync<StatisticResponse>(incidentSql, parameters)).Result.ToList();

            connection.Close();
            return statisticResponses;
        }

        public List<StatisticResponse> GetStatisticEntityToCalculateTotalMH(DateTime toDate)
        {
            var currentYear = toDate.Year;
            using var connection = _safetyContext.CreateConnection();
            var incidentSql =
                "select * from \"statistic_entity\" " +
                "where is_deleted <> true";
            incidentSql += " and date::date <= @toDate";
            var parameters = new { currentYear = currentYear, toDate = toDate };
            //Get data from table Incident Register Entities
            List<StatisticResponse> statisticResponses =
                (connection.QueryAsync<StatisticResponse>(incidentSql, parameters)).Result.ToList();
            return statisticResponses;
        }
        public List<LegalNCRegister> GetLegalNCRegister(DateTime date)
        {
            var currentYear = date.Year;
            using var connection = _safetyContext.CreateConnection();
            var incidentSql =
                "select * from \"LegalNCRegisterEntity\" " +
                "where is_deleted <> true and extract(year from date_created)=@currentYear";
            var parameters = new { currentYear = currentYear };
            //Get data from table Incident Register Entities
            List<LegalNCRegister> legalNCRegister =
                (connection.QueryAsync<LegalNCRegister>(incidentSql, parameters)).Result.ToList();
            return legalNCRegister;
        }
        public AvailableData GetAvailableData(int currentYear)
        {
            using var connection = _safetyContext.CreateConnection();
            var incidentSql =
                "select * from \"AvailableDataEntities\" " +
                "where is_deleted <> true and extract(year from date_created)=@currentYear";
            var parameters = new { currentYear = currentYear };
            //Get data from table Incident Register Entities
            AvailableData availableData =
                (connection.QueryAsync<AvailableData>(incidentSql, parameters)).Result.FirstOrDefault();
            return availableData;
        }

        public async Task<ServiceResponse> GetSummaryOfPerformanceScoreReportV2(SearchRequest request)
        {
            //Check permission
            var isHasPermission = _permissionChecker.HasPermission(PermissionConstants.DASHBOARD_SAFETY_VIEW_REPORT).Result;
            if (!isHasPermission)
            {
                return Unauthorized("Sorry, you are not authorized to access this function");
            }

            var listSummaryOfPerformanceScores = await GetAuditEntities(request.FromDate != null ? request.FromDate.Value.Year : DateTime.Now.Year, request.Month != null ? request.Month.Value : null);

            return Ok(new { report = listSummaryOfPerformanceScores });
        }

        public void CalculateAuthorityScore(List<LegalNCRegister> legalNCRegisterOfProjects, ref SummaryOfPerformanceScore score)
        {
            //MOM
            var momLegalNCRegister = GetLegalNCRegisterByType(legalNCRegisterOfProjects, "MOM");
            if (momLegalNCRegister.DemeritPoint == 0 && momLegalNCRegister.NNC == 0 && momLegalNCRegister.SWO == 0)
                score.WellAboveExpectation += 12.5;
            else if (momLegalNCRegister.Fine == 0)
                score.JustAboveExpectation += 9.375;
            else if (momLegalNCRegister.Fine == 1)
                score.MeetExpectation += 6.25;
            else if (momLegalNCRegister.DemeritPoint > 1)
                score.BelowExpectation += 3.125;

            //NEA
            var pubLegalNCRegister = GetLegalNCRegisterByType(legalNCRegisterOfProjects, "PUB");
            if (pubLegalNCRegister.SWO == 0 && pubLegalNCRegister.Fine == 0)
                score.WellAboveExpectation += 2.5;
            else if (pubLegalNCRegister.Fine == 0)
                score.JustAboveExpectation += 1.25;
            else if (pubLegalNCRegister.Fine == 1)
                score.MeetExpectation += 0.625;
            else if (pubLegalNCRegister.DemeritPoint > 1)
                score.BelowExpectation += 3.125;

            //NEA Noise
            var nea_Noise_LegalNCRegister = GetLegalNCRegisterByType(legalNCRegisterOfProjects, "NEA", "Noise");
            if (nea_Noise_LegalNCRegister.RestrictionOfWorkHours == 0 && nea_Noise_LegalNCRegister.Fine == 0)
                score.WellAboveExpectation += 5;
            else if (nea_Noise_LegalNCRegister.Fine == 1)
                score.JustAboveExpectation += 3.75;
            else if (nea_Noise_LegalNCRegister.Fine == 2)
                score.MeetExpectation += 2.5;
            else if (nea_Noise_LegalNCRegister.Fine == 3)
                score.BelowExpectation += 1.25;

            //NEA Mosquito
            var nea_Mosquito_LegalNCRegister = GetLegalNCRegisterByType(legalNCRegisterOfProjects, "NEA", "Vector", "Mosquito");
            if (nea_Mosquito_LegalNCRegister.SWO == 0 && nea_Mosquito_LegalNCRegister.Fine == 0)
                score.WellAboveExpectation += 5;
            else if (nea_Mosquito_LegalNCRegister.SWO == 0 && nea_Mosquito_LegalNCRegister.Fine == 1)
                score.JustAboveExpectation += 3.75;
            else if (nea_Mosquito_LegalNCRegister.SWO == 0 && nea_Mosquito_LegalNCRegister.Fine == 2)
                score.MeetExpectation += 2.5;
            else if (nea_Mosquito_LegalNCRegister.SWO == 1)
                score.BelowExpectation += 1.25;
        }

        public void CalculateAuditScore(List<Audit> auditOfProjects, ref SummaryOfPerformanceScore score)
        {
            var wellAbove_housekeeping = auditOfProjects.Where(c => c.AuditScore == 10).ToList();
            score.WellAboveExpectation += wellAbove_housekeeping.Count() + 10;
            var justAbove_housekeeping = auditOfProjects.Where(c => c.AuditScore == 7.5).ToList();
            score.JustAboveExpectation += wellAbove_housekeeping.Count() + 7.5;
            var meet_housekeeping = auditOfProjects.Where(c => c.AuditScore == 5).ToList();
            score.MeetExpectation += wellAbove_housekeeping.Count() + 5;
            var below_housekeeping = auditOfProjects.Where(c => c.AuditScore == 2.5).ToList();
            score.BelowExpectation += wellAbove_housekeeping.Count() + 2.5;

            var wellAbove_deduction = auditOfProjects.Where(c => c.AuditFindingsDeductionRating == 15).ToList();
            score.WellAboveExpectation += wellAbove_housekeeping.Count() + 15;
            var justAbove_deduction = auditOfProjects.Where(c => c.AuditFindingsDeductionRating == 11.25).ToList();
            score.JustAboveExpectation += wellAbove_housekeeping.Count() + 11.25;
            var meet_deduction = auditOfProjects.Where(c => c.AuditFindingsDeductionRating == 7.5).ToList();
            score.MeetExpectation += wellAbove_housekeeping.Count() + 5;
            var below_deduction = auditOfProjects.Where(c => c.AuditFindingsDeductionRating == 3.75).ToList();
            score.BelowExpectation += wellAbove_housekeeping.Count() + 3.75;

            var wellAbove_pcAuditScore = auditOfProjects.Where(c => c.AuditScoreRating == 25).ToList();
            score.WellAboveExpectation += wellAbove_housekeeping.Count() + 25;
            var justAbove_pcAuditScore = auditOfProjects.Where(c => c.AuditScoreRating == 18.75).ToList();
            score.JustAboveExpectation += wellAbove_housekeeping.Count() + 18.75;
            var meet_pcAuditScore = auditOfProjects.Where(c => c.AuditScoreRating == 12.5).ToList();
            score.MeetExpectation += wellAbove_housekeeping.Count() + 12.5;
            var below_pcAuditScore = auditOfProjects.Where(c => c.AuditScoreRating == 6.25).ToList();
            score.BelowExpectation += wellAbove_housekeeping.Count() + 6.25;
        }

        public LegalNCRegisterTypeResponse GetLegalNCRegisterByType(List<LegalNCRegister> legalNCRegisters, string authority, string? reason = null, string remark = null)
        {
            legalNCRegisters = legalNCRegisters.Where(c => c.Authority == authority).ToList();
            if (reason != null)
                legalNCRegisters = legalNCRegisters.Where(c => c.Reason == reason).ToList();
            if (!string.IsNullOrEmpty(remark))
                legalNCRegisters = legalNCRegisters.Where(c => !string.IsNullOrEmpty(c.Remark) && c.Remark.ToLower().Contains(remark.ToLower())).ToList();
            var demeritPoint = legalNCRegisters.Where(c => c.Type == "Demerit Points").ToList().Count();
            var nnc = legalNCRegisters.Where(c => c.Type == "NNC").ToList().Count();
            var fine = legalNCRegisters.Where(c => c.Type == "Fine").ToList().Count();
            var swo = legalNCRegisters.Where(c => c.Type == "SWO").ToList().Count();
            var restrictionOfWorkHours = legalNCRegisters.Where(c => c.Type == "SWO").ToList().Count();
            var result = new LegalNCRegisterTypeResponse()
            {
                DemeritPoint = demeritPoint,
                NNC = nnc,
                Fine = fine,
                SWO = swo,
                RestrictionOfWorkHours = restrictionOfWorkHours
            };
            return result;
        }

        public double CalculateASR(List<IncidentRegister> incidentRegisters, List<StatisticResponse> statisticEntities, AccumulationEntity accumulation)
        {
            var fatality = incidentRegisters.Where(c => c.IncidentClassification == "Fatality");
            var reportableAccidents = incidentRegisters.Where(c => c.IncidentClassification == "Reportable Accident");
            var permanentDisability = incidentRegisters.Where(c => c.IncidentClassification == "Permanent Disability");

            var medicalLeaveOfFatality = fatality != null ? fatality.Sum(c => c.MedicalLeaveDay) : 0;
            var medicalLeaveOfPermanentDisability = permanentDisability != null ? permanentDisability.Sum(c => c.MedicalLeaveDay) : 0;
            var medicalLeaveOfReportable = reportableAccidents != null ? reportableAccidents.Sum(c => c.MedicalLeaveDay) : 0;

            var totalMHWorker = statisticEntities?.Count() > 0 ? statisticEntities.Average(c => c.TotalWHManhoursWorked + c.TotalClientWorked + c.TotalSCMHWorked) : 0;
            var cumulativeWHWorked = totalMHWorker + (accumulation != null ? accumulation.CumulativeMHWorked : 0);

            var asr = cumulativeWHWorked > 0 ? (medicalLeaveOfFatality + medicalLeaveOfPermanentDisability + medicalLeaveOfReportable) * 1000000 / cumulativeWHWorked : 0;
            var accidentSeverityRate = Math.Round(asr, 2);

            return accidentSeverityRate;
        }

        public double CalculateAFR(List<IncidentRegister> incidentRegisters, List<StatisticResponse> statisticEntities, AccumulationEntity accumulation)
        {
            var fatality = incidentRegisters.Where(c => c.IncidentClassification == "Fatality").ToList().Count();
            var reportableAccidents = incidentRegisters.Where(c => c.IncidentClassification == "Reportable Accident").ToList().Count();
            var permanentDisability = incidentRegisters.Where(c => c.IncidentClassification == "Permanent Disability").ToList().Count();

            var totalMHWorker = statisticEntities?.Count() > 0 ? statisticEntities.Average(c => c.TotalWHManhoursWorked + c.TotalClientWorked + c.TotalSCMHWorked) : 0;
            var cumulativeWHWorked = totalMHWorker + (accumulation != null ? accumulation.CumulativeMHWorked : 0);
            var cumulativeReportableAccidents = accumulation != null ? accumulation.CumulativeReportableAccidents : 0;
            var asr = cumulativeWHWorked > 0 ? ((fatality + reportableAccidents + permanentDisability + cumulativeReportableAccidents) * 1000000) / cumulativeWHWorked : 0;
            var accidentSeverityRate = Math.Round(asr, 2);

            return accidentSeverityRate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="incidentRegisters"></param>
        /// <param name="statisticEntities"></param>
        /// <param name="accumulation"></param>
        /// <param name="dateTime"></param>
        /// <param name="indexMonth"></param>
        /// <returns></returns>
        public double CalculateWIRV1(List<IncidentRegister> incidentRegisters, List<StatisticResponse> statisticEntities, AccumulationEntity accumulation, DateTime? dateTime = null)
        {
            var aveDailyWHPersonnel = statisticEntities.Sum(c => c.AveDailyWHPersonnel);
            var aveDailyClientAndRepPersonnel = statisticEntities.Sum(c => c.AveDailyClientAndRepPersonnel);
            var aveDailySubconPersonnel = statisticEntities.Sum(c => c.AveDailySubconPersonnel);

            double dailyAvePersonnel = aveDailyWHPersonnel + aveDailyClientAndRepPersonnel + aveDailySubconPersonnel;
            if (dateTime != null)
            {
                dailyAvePersonnel = dailyAvePersonnel / (dateTime.Value.Month);
            }
            else
            {
                dailyAvePersonnel = dailyAvePersonnel / 12;
            }
            var fatality = incidentRegisters.Where(c => c.IncidentClassification == "Fatality").ToList().Count;
            var reportableAccidents = incidentRegisters.Where(c => c.IncidentClassification == "Reportable Accident").ToList().Count;
            var permanentDisability = incidentRegisters.Where(c => c.IncidentClassification == "Permanent Disability").ToList().Count;

            var totalAccidents = fatality + reportableAccidents + permanentDisability;

            double workInjuryRate = dailyAvePersonnel > 0 ? (((totalAccidents) * 100000) / dailyAvePersonnel) : 0;
            return Math.Round(workInjuryRate, 2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="incidentRegisters"></param>
        /// <param name="statisticEntities"></param>
        /// <param name="accumulation"></param>
        /// <param name="dateTime"></param>
        /// <param name="indexMonth"></param>
        /// <returns></returns>
        public double CalculateWIRV3(List<IncidentRegister> incidentRegisters, List<StatisticResponse> statisticEntities, AccumulationEntity accumulation, DateTime? dateTime = null)
        {
            var aveDailyWHPersonnel = statisticEntities?.Count() > 0 ? statisticEntities.Average(c => c.AveDailyWHPersonnel) : 0;
            var aveDailyClientAndRepPersonnel = statisticEntities?.Count() > 0 ? statisticEntities.Average(c => c.AveDailyClientAndRepPersonnel) : 0;
            var aveDailySubconPersonnel = statisticEntities?.Count() > 0 ? statisticEntities.Average(c => c.AveDailySubconPersonnel) : 0;

            double dailyAvePersonnel = aveDailyWHPersonnel + aveDailyClientAndRepPersonnel + aveDailySubconPersonnel;

            var fatality = incidentRegisters.Where(c => c.IncidentClassification == "Fatality").ToList().Count;
            var reportableAccidents = incidentRegisters.Where(c => c.IncidentClassification == "Reportable Accident").ToList().Count;
            var permanentDisability = incidentRegisters.Where(c => c.IncidentClassification == "Permanent Disability").ToList().Count;

            var totalAccidents = fatality + reportableAccidents + permanentDisability;

            double workInjuryRate = dailyAvePersonnel > 0 ? (((totalAccidents) * 100000) / dailyAvePersonnel) : 0;
            return Math.Round(workInjuryRate, 2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="incidentRegisters"></param>
        /// <param name="statisticEntities"></param>
        /// <param name="accumulation"></param>
        /// <param name="dateTime"></param>
        /// <param name="indexMonth"></param>
        /// <returns></returns>
        public double CalculateWIRV2(List<IncidentRegister> incidentRegisters, double dailyAvePersonnel,
            AccumulationEntity accumulation, DateTime? dateTime = null)
        {
            var fatality = incidentRegisters.Where(c => c.IncidentClassification == "Fatality").ToList().Count;
            var reportableAccidents = incidentRegisters.Where(c => c.IncidentClassification == "Reportable Accident").ToList().Count;
            var permanentDisability = incidentRegisters.Where(c => c.IncidentClassification == "Permanent Disability").ToList().Count;
            var totalAccidents = fatality + reportableAccidents + permanentDisability;

            double workInjuryRate = dailyAvePersonnel > 0 ? (((totalAccidents) * 100000) / dailyAvePersonnel) : 0;
            return Math.Round(workInjuryRate, 2);
        }

        public async Task<List<ProjectResponse>> GetProjects()
        {
            string url = _options.Value.Https + "/projects/lambda-query-project";
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "application/json";

            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.100 Safari/537.36";
            var converter = new ExpandoObjectConverter();
            dynamic message = null;
            var listProject = new List<ProjectResponse>();
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
                            ProjectId = ((IDictionary<string, object>)item)["id"]?.ToString(),
                            ProjectName = name,
                        };
                        listProject.Add(obj);
                    };
                }
            }
            return listProject;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentYear"></param>
        /// <param name="isWHJV"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (21.10.2024)
        public async Task<List<StatisticDailyPersonnelResponse>> GetDailyAvePersonnelWIR(int currentYear, bool isWH = false)
        {
            var result = new List<StatisticDailyPersonnelResponse>();

            try
            {
                using var connection = _safetyContext.CreateConnection();
                ResourceManager rm = new ResourceManager("DashboardApi.Resource.QuerySafety", Assembly.GetExecutingAssembly());
                // init data
                string whereSQL = $"1=1",
                        querySQL = rm.GetString("CalculateDailyWIR", CultureInfo.CurrentCulture);

                if (isWH)
                {
                    whereSQL += " AND (LOWER(PROJECT_NAME) <> 'bay' AND LOWER(PROJECT_NAME) <> 'cr106') ";
                }

                // query
                using (var resultQuery = await connection.QueryMultipleAsync(querySQL.Replace("@WHERECLAUSE", whereSQL), new
                {
                    CURRENTYEAR = currentYear
                }))
                {
                    result = (await resultQuery.ReadAsync<StatisticDailyPersonnelResponse>()).ToList();
                }

                connection.Close();
                return result;
            }
            catch (Exception ce)
            {
                return result;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ServiceResponse> GetTotalIncidentNumbersByMonthReport(SearchRequest request)
        {
            // init
            var list = new List<IncidentNumbersByMonth>();
            double totalWIRWH = 0;
            double totalWIRWHJV = 0;
            var currentYear = DateTime.Now.Year;
            int currentMonth = DateTime.Now.Month;

            //Check permission
            var isHasPermission = _permissionChecker.HasPermission(PermissionConstants.DASHBOARD_SAFETY_VIEW_REPORT).Result;
            if (!isHasPermission)
            {
                return Unauthorized("Sorry, you are not authorized to access this function");
            }

            using var connection = _safetyContext.CreateConnection();
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            if (request.FromDate != null)
            {
                currentYear = request.FromDate.Value.Year;
                if (currentYear < DateTime.Now.Year)
                    currentMonth = 12;
            }

            if (request.Month != null)
            {
                currentMonth = request.Month.Value;
            }

            var yearBefore = currentYear - 1;
            var lastDayOfMonth = new DateTime(currentYear, currentMonth, DateTime.DaysInMonth(currentYear, currentMonth));
            var firstDayOfMonth = new DateTime(currentMonth == 12 ? currentYear : yearBefore, currentMonth == 12 ? 1 : currentMonth + 1, 1);

            //Get Statistic
            var statistic = GetStatisticEntity(lastDayOfMonth, firstDayOfMonth).ToList();
            var statisticAll = GetStatisticEntity(lastDayOfMonth, lastDayOfMonth.AddYears(-2)).ToList();
            int maxMonth = statistic != null ? statistic.Select(c => c.Date).OrderByDescending(c => c).FirstOrDefault().Month : 0;

            if (maxMonth > 0 && maxMonth < DateTime.Now.Year && currentYear == DateTime.Now.Year && request.Month == null)
            {
                currentMonth = maxMonth;
                lastDayOfMonth = new DateTime(currentYear, currentMonth, DateTime.DaysInMonth(currentYear, currentMonth));
                firstDayOfMonth = new DateTime(currentMonth == 12 ? currentYear : yearBefore, currentMonth == 12 ? 1 : currentMonth + 1, 1);
                statistic = GetStatisticEntity(lastDayOfMonth, firstDayOfMonth).ToList();
            }

            var incidentSql =
                "select * from \"IncidentRegisterEntities\" " +
                "where is_deleted <> true  and date_created::date <=@lastDayOfMonth " +
                "order by date_created";

            // and extract(year from date_created)>=@yearBefore
            var parameters = new { currentYear = currentYear, yearBefore = yearBefore, lastDayOfMonth = lastDayOfMonth, firstDayOfMonth = firstDayOfMonth };

            //Get data from table Incident Register Entities
            List<IncidentRegister> incidentRegisters =
                (await connection.QueryAsync<IncidentRegister>(incidentSql, parameters)).ToList();
            connection.Close();

            var incidentRegistersAll = incidentRegisters.Where(c => c.DateCreated.Date >= lastDayOfMonth.AddYears(-2) && c.DateCreated.Date <= lastDayOfMonth).ToList();

            if (incidentRegisters != null)
            {
                incidentRegisters = incidentRegisters.Where(c => c.DateCreated.Date >= firstDayOfMonth && c.DateCreated.Date <= lastDayOfMonth).ToList();
            }

            List<IncidentRegister> incidentRegisterCurrents = incidentRegistersAll.Where(c => c.DateCreated.Year == currentYear).ToList();
            var statisticCurrents = statistic.Where(c => c.Date.Year == currentYear).ToList();
            var months = incidentRegisterCurrents.Select(c => new { c.DateCreated.Year, c.DateCreated.Month }).GroupBy(c => c.Month).Select(e => e.FirstOrDefault()).Select(c => c.Month).OrderBy(c => c);

            //Get Accumulation
            var accumulations = GetAccumulations(lastDayOfMonth);
            var availableData = GetAvailableData(currentYear);
            if (availableData == null)
            {
                availableData = GetAvailableData(currentYear - 1);
            }

            var nationalAverage_50Percent = availableData != null ? availableData.NationalAverage : 0;
            var accumulationGroup = accumulations.GroupBy(c => c.ProjectId).Select(c => c.FirstOrDefault());
            var accumulationJVGroup = accumulations.Where(c => c.ProjectName == "BAY" || c.ProjectName == "CR106").GroupBy(c => c.ProjectId).Select(c => c.FirstOrDefault());
            var accumulation = new AccumulationEntity()
            {
                CumulativeReportableAccidents = accumulationGroup != null ? accumulationGroup.Sum(c => c.CumulativeReportableAccidents) : 0,
            };

            var accumulationJV = new AccumulationEntity()
            {
                CumulativeReportableAccidents = accumulationJVGroup != null ? accumulationJVGroup.Sum(c => c.CumulativeReportableAccidents) : 0,
            };

            DateTime firstDayOfYear = new DateTime(currentYear, 1, 1);
            var dailyAvePersonnelWH = await GetDailyAvePersonnelWIR(currentYear);
            var dailyAvePersonnelWHJV = await GetDailyAvePersonnelWIR(currentYear, true);

            for (int i = 1; i <= maxMonth; i++)
            {
                var lastDayOfMonthItem = new DateTime(currentYear, i, DateTime.DaysInMonth(currentYear, i));
                var fristDayBefore12Month = lastDayOfMonthItem.AddMonths(-12).AddDays(1);

                var incidentRegisterOfMonth = incidentRegisterCurrents.Where(c => c.DateCreated.Date <= lastDayOfMonthItem).ToList();

                var whReportableAccidents = incidentRegisterOfMonth.Where(c => (c.IncidentClassification == "Reportable Accident" || c.IncidentClassification == "Fatality" || c.IncidentClassification == "Permanent Disability") && c.DateCreated.Year == currentYear && c.DateCreated.Month == i).ToList().Count();
                var whReportableAccident_jvs = incidentRegisterOfMonth.Where(c => (c.IncidentClassification == "Reportable Accident" || c.IncidentClassification == "Fatality" || c.IncidentClassification == "Permanent Disability") && (c.ProjectName == "BAY" || c.ProjectName == "CR106") && c.DateCreated.Year == currentYear && c.DateCreated.Month == i).ToList().Count();

                // calculate 12 month from current month
                var incidentRegisterOfMonthWIR = incidentRegistersAll.Where(c => c.DateCreated.Date >= fristDayBefore12Month && c.DateCreated.Date <= lastDayOfMonthItem).ToList();
                var statisticOfMonth = statistic.Where(c => c.Date.Date <= lastDayOfMonthItem && c?.Date.Date >= firstDayOfYear).ToList();

                var dailyWWH = dailyAvePersonnelWH.Find(x => x.Month.Month == i);
                var dailyWWHJV = dailyAvePersonnelWHJV.Find(x => x.Month.Month == i);

                var wirWHJV = CalculateWIRV2(incidentRegisterOfMonthWIR, dailyWWH != null ? dailyWWH.AvgLast12Months : 0, accumulation, new DateTime(currentYear, i, 1));
                var wirWH = CalculateWIRV2(incidentRegisterOfMonthWIR.Where(c => c.ProjectName != "BAY" && c.ProjectName != "CR106").ToList(), dailyWWHJV != null ? dailyWWHJV.AvgLast12Months : 0, accumulationJV, new DateTime(currentYear, i, 1));

                var wirWHJV2 = CalculateWIRV1(incidentRegisterOfMonth, statisticOfMonth, accumulation, new DateTime(currentYear, i, 1));
                var wirWH2 = CalculateWIRV1(incidentRegisterOfMonth.Where(c => c.ProjectName != "BAY" && c.ProjectName != "CR106").ToList(), statisticOfMonth.Where(c => c.ProjectName != "BAY" && c.ProjectName != "CR106").ToList(), accumulationJV, new DateTime(currentYear, i, 1));

                IncidentNumbersByMonth incident = new IncidentNumbersByMonth()
                {
                    Name = new DateTime(currentYear, i, 1).ToString("MMM"),
                    WH = whReportableAccidents,
                    WHJV = whReportableAccident_jvs,

                    WIR = wirWH2,
                    JVWIR = wirWHJV2,

                    Total_WIR = wirWH,
                    Total_JVWIR = wirWHJV,

                    Nation_Average = nationalAverage_50Percent
                };

                list.Add(incident);
            }

            double maxValueWIR = 0;
            double maxValueWIRJV = 0;

            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i].Total_WIR > 0 || list[i].Total_JVWIR > 0)
                {
                    maxValueWIR = list[i].Total_WIR;
                    maxValueWIRJV = list[i].Total_JVWIR;
                    break;
                }
            }

            for (int i = list.Count - 2; i >= 0; i--)
            {
                list[i].Total_WIR = maxValueWIR;
                list[i].Total_JVWIR = maxValueWIRJV;
            }

            return Ok(list);
        }

        public async Task<ServiceResponse> GetDetailOfTotalIncidentNumbersByHazardTypeReport(SearchRequest request)
        {
            //Check permission
            var isHasPermission = _permissionChecker.HasPermission(PermissionConstants.DASHBOARD_SAFETY_VIEW_REPORT).Result;
            if (!isHasPermission)
            {
                return Unauthorized("Sorry, you are not authorized to access this function");
            }
            var currentYear = DateTime.Now.Year;
            using var connection = _safetyContext.CreateConnection();
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            var incidentSql =
                "select * from \"IncidentRegisterEntities\" " +
                "where is_deleted <> true  and extract(year from date_created)=@currentYear";

            var parameters = new { currentYear = currentYear };
            //Get data from table Incident Register Entities
            List<IncidentRegister> incidentRegisters =
                (await connection.QueryAsync<IncidentRegister>(incidentSql, parameters)).ToList();
            connection.Close();
            var listCurrentYears = incidentRegisters.Where(c => c.DateCreated.Year == currentYear);
            var listInjureds = listCurrentYears.GroupBy(c => c.InjuredPart).Select(c => c.FirstOrDefault())
                .Select(c => c.InjuredPart).Where(c => c != null);
            List<IncidentNumbersByInjuredPartReport> incidentNumbersByHazardTypeReport =
                new List<IncidentNumbersByInjuredPartReport>();
            string[] listGrays =
                { "Head", "Ear", "Nose", "Mouth", "Teeth", "Face", "Neck", "Chest", "Abdommen", "Back" };
            string[] listBlues = { "Eye", "Other", "Multiple Injuries" };
            string[] listOrange = { "Shoulder", "Arm", "Elbow", "Forearm", "Hand", "Wrist" };
            string[] listBoldBlues = { "Palm" };
            string[] listReds = { "Finger" };
            string[] listPurples = { "Groin", "Leg", "Knee", "Ankle", "Foot", "Toe", };

            foreach (var injureds in listInjureds)
            {
                var current_IncidentByTypes = listCurrentYears.Where(c => c.InjuredPart == injureds).ToList();
                IncidentNumbersByInjuredPartReport record = new IncidentNumbersByInjuredPartReport()
                {
                    NameOfInjuredPart = injureds,
                    NumberOfInjuredPart = current_IncidentByTypes.Count(),
                    Type = listGrays.Contains(injureds) ? 1 :
                        listBlues.Contains(injureds) ? 2 :
                        listOrange.Contains(injureds) ? 3 :
                        listBoldBlues.Contains(injureds) ? 4 :
                        listReds.Contains(injureds) ? 5 : 6
                };
                incidentNumbersByHazardTypeReport.Add(record);
            }

            return Ok(incidentNumbersByHazardTypeReport.OrderBy(c => c.Type));
        }

        public async Task<ServiceResponse> GetWir(SearchRequest request)
        {
            //Check permission
            var isHasPermission = _permissionChecker.HasPermission(PermissionConstants.DASHBOARD_SAFETY_VIEW_REPORT).Result;
            if (!isHasPermission)
            {
                return Unauthorized("Sorry, you are not authorized to access this function");
            }

            var isAllMonth = request?.Month == null;
            var currentDate = request.FromDate != null ? request.FromDate.Value : DateTime.Now;
            var currentYear = currentDate.Year;
            var currentMonth = request.Month != null ? request.Month.Value : currentDate.Month;
            if (currentYear < DateTime.Now.Year)
            {
                currentMonth = 12;
            }
            var lastDayOfMonth = new DateTime(currentYear, currentMonth, DateTime.DaysInMonth(DateTime.Now.Year, currentMonth));
            var firstDayOfMonth = isAllMonth ? new DateTime(currentYear, 1, 1) : new DateTime(currentYear, lastDayOfMonth.Month, 1);

            //Get Accumulation
            var result = new List<WIRASRAFRDetail>();

            using var connection = _safetyContext.CreateConnection();
            ResourceManager rm = new ResourceManager("DashboardApi.Resource.QuerySafety", Assembly.GetExecutingAssembly());
            // init data
            string querySQL = rm.GetString("WIRASRAFRDetail", CultureInfo.CurrentCulture);

            // query
            using (var resultQuery = await connection.QueryMultipleAsync(querySQL, new
            {
                CURRENT_YEAR = currentYear,
                FROM_DATE = firstDayOfMonth,
                TO_DATE = lastDayOfMonth,
            }))
            {
                result = (await resultQuery.ReadAsync<WIRASRAFRDetail>()).ToList();
            }

            if (isAllMonth)
            {
                foreach (var r in result)
                {
                    r.AFR = r.SumAFR;
                    r.ASR = r.SumASR;
                }
            }

            connection.Close();
            connection.Dispose();

            return Ok(result);
        }

        public async Task<ServiceResponse> GetMOMNoticeOfNonCF(SearchRequest request)
        {
            //Check permission
            var isHasPermission = _permissionChecker.HasPermission(PermissionConstants.DASHBOARD_SAFETY_VIEW_REPORT).Result;
            if (!isHasPermission)
            {
                return Unauthorized("Sorry, you are not authorized to access this function");
            }

            var currentYear = DateTime.Now.Year;
            var legalNCRegisters = new List<LegalNCRegister>();
            var neaOffencesByMonth = new List<NEAOffencesByMonth>();
            var legalMOMNotiecs = new List<MOMNotice>();
            var neaByMonth = new List<NEA>();
            using var connection = _safetyContext.CreateConnection();
            ResourceManager rm = new ResourceManager("DashboardApi.Resource.QuerySafety", Assembly.GetExecutingAssembly());
            // init data
            string querySQL = rm.GetString("MOMNoticeQuery", CultureInfo.CurrentCulture);

            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            //Get data from table Incident Register Entities
            using (var resultQuery = await connection.QueryMultipleAsync(querySQL, new { CURRENTYEAR = currentYear }))
            {
                // 0. get all
                legalNCRegisters = (await resultQuery.ReadAsync<LegalNCRegister>()).ToList();

                // 1. MOM - Notice of Non-Compliance Fine
                legalMOMNotiecs = (await resultQuery.ReadAsync<MOMNotice>()).ToList();

                // 2. NEA – Notice of Composition Fine and NTAC (By Month)
                neaByMonth = (await resultQuery.ReadAsync<NEA>()).ToList();

                // 3. NEA Offences By Month(Pending Receive CF)
                neaOffencesByMonth = (await resultQuery.ReadAsync<NEAOffencesByMonth>())
                    .OrderByDescending(o => o.Noise + o.Rodent + o.WDPH + o.Vector).ToList();
            }
            connection.Close();

            //2. NEA – Notice of Composition Fine and NTAC (By Month)
            var momProjectReports = new MOMProjectReport();
            // Chuyển đổi thành cấu trúc yêu cầu
            momProjectReports.MOMNotices = neaByMonth
                    .GroupBy(nea => nea.Month)
                    .Select(g => new
                    {
                        month = g.Key,
                        Projects = g.ToDictionary(
                            nea => nea.ProjectName,
                            nea => nea.TotalFineAmount)
                    })
                    .Select(entry => new Dictionary<string, object>(entry.Projects.Select(p => new KeyValuePair<string, object>(p.Key, p.Value)))
                    {
                        { "month", entry.month }
                    }).ToList();
            momProjectReports.Projects = neaByMonth.Select(x => x.ProjectName).Distinct().ToList();

            var report = new MOMReport()
            {
                MOMNotices = legalMOMNotiecs,
                NEAOffencesByMonths = neaOffencesByMonth,
                MOMProjectReport = momProjectReports
            };

            return Ok(report);
        }
    }
}
