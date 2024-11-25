using AutoMapper;
using Dapper;
using DashboardApi.Application.Project;
using DashboardApi.Auth.CurrentUser;
using DashboardApi.Auth.PermisionChecker;
using DashboardApi.Common;
using DashboardApi.DatabaseContext.DapperDbContext;
using DashboardApi.Dtos.QaQc.Requests;
using DashboardApi.Dtos.QaQc.Responses;
using DashboardApi.HttpConfig;
using DashboardApi.Infastructure.QAQC;
using DashboardApi.Models;
using Newtonsoft.Json;
using QAQCApi.AttributeCustom;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace DashboardApi.Application.DashBoardQaQc.V2
{
    public class DashboardQaQcServiceV2 : Service, IDashboardQaQcServiceV2
    {
        private readonly QaQcDapperContext _qaqcDapper;
        private readonly IPermissionChecker _permissionChecker;
        public readonly IMapper _mapper;
        public readonly ICurrentUser _currentUser;
        private readonly IDashboardQAQCRepositoryV2 _dashboardQAQCRepositoryV2;
        private readonly IProjectService _projectService;

        public DashboardQaQcServiceV2(QaQcDapperContext qaqcDapper,
            IPermissionChecker permissionChecker,
            IMapper mapper,
            AppMainDapperContext appMainDapper,
            ICurrentUser currentUser,
            WorkerDapperContext workerDapper,
            DigiCheckDapperContext digiCheckDapper,
            IDashboardQAQCRepositoryV2 dashboardQAQCRepositoryV2,
            IProjectService projectService)
        {
            _qaqcDapper = qaqcDapper;
            _permissionChecker = permissionChecker;
            _mapper = mapper;
            _currentUser = currentUser;
            _dashboardQAQCRepositoryV2 = dashboardQAQCRepositoryV2;
            _projectService = projectService;
        }

        #region Summary Project KPR

        /// <summary>
        /// Build query sql where with filter date and project
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (08.04.2023)
        protected string BuildQuerySQL(SummaryRequest request, string typeData = "created_at", bool isCumAvg = false, string typeProject = "project")
        {
            string whereSQL = string.Empty;

            // get all project from table projects
            string lstProjectName = (request.listProject != null && request.listProject.Length >= 0) ? string.Join("','", request.listProject) : "";
            string lstProjectId = (request.listProjectId != null && request.listProjectId.Length >= 0) ? string.Join("','", request.listProjectId) : "";

            if (!string.IsNullOrEmpty(request.gteDate))
            {
                whereSQL = $"{whereSQL} AND {typeData} <= @GteDate ";
            }

            if (!string.IsNullOrEmpty(request.lteDate) && !isCumAvg)
            {
                whereSQL = $"{whereSQL} AND {typeData} >= @LteDate ";
            }

            if (typeProject == "project_id")
            {
                whereSQL = !string.IsNullOrEmpty(lstProjectId) ? $"{whereSQL} AND {typeProject} IN (@ListProject)" : "";
            }
            else
            {
                whereSQL = !string.IsNullOrEmpty(lstProjectName) ? $"{whereSQL} AND {typeProject} IN (@ListProject)" : "";
            }

            if (whereSQL != "")
            {
                whereSQL = "WHERE " + whereSQL.Substring(5);
            }

            return whereSQL;
        }

        /// <summary>
        /// calculate score group by project
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (07.04.2023)
        protected Dictionary<string, object> CalculateProjectScore(Dictionary<string, object> datas)
        {
            // init project
            Dictionary<string, object> result = new Dictionary<string, object>();
            List<string> lstProject = new List<string>();

            try
            {
                // group by project
                // critical
                var criticalProjects = (datas["criticals"] as List<SummaryCriticalCommon>).GroupBy(x => x.project).ToList();
                // common
                var commonProjects = (datas["commons"] as List<SummaryCriticalCommon>).GroupBy(x => x.project).ToList();
                //pqa
                var pqaProjects = (datas["pqas"] as List<SummaryQAQCTab>).GroupBy(x => x.project).ToList();
                //iqa
                var iqaProjects = (datas["iqas"] as List<SummaryQAQCTab>).GroupBy(x => x.project).ToList();
                //rework
                var reworkProjects = (datas["reworks"] as List<SummaryQAQCTab>).GroupBy(x => x.project).ToList();
                //defects
                var defectsProjects = (datas["defects"] as List<SummaryQAQCTab>).GroupBy(x => x.project).ToList();

                // convert to percent
                foreach (var criticalProject in criticalProjects)
                {
                    // add to list project
                    lstProject.Add(criticalProject.Key);
                    var getGuildeLines = GetGuidelinesScoring("Critical", criticalProject.FirstOrDefault().percentCriticalCheck);

                    // assign to score covnert
                    criticalProject.FirstOrDefault().kprValue = Convert.ToInt32(!string.IsNullOrEmpty(getGuildeLines["QualityKPR"]) ? getGuildeLines["QualityKPR"] : 0);
                    criticalProject.FirstOrDefault().kprText = getGuildeLines["QualityKPRText"];
                    criticalProject.FirstOrDefault().color = getGuildeLines["QualityKPRColor"];
                }

                foreach (var commonProject in commonProjects)
                {
                    // add to list project
                    lstProject.Add(commonProject.Key);
                    var getGuildeLines = GetGuidelinesScoring("Common", commonProject.FirstOrDefault().percentCommonCheck);

                    // assign to score covnert
                    commonProject.FirstOrDefault().kprValue = Convert.ToInt32(!string.IsNullOrEmpty(getGuildeLines["QualityKPR"]) ? getGuildeLines["QualityKPR"] : 0);
                    commonProject.FirstOrDefault().kprText = getGuildeLines["QualityKPRText"];
                    commonProject.FirstOrDefault().color = getGuildeLines["QualityKPRColor"];
                }

                foreach (var pqaProject in pqaProjects)
                {
                    // add to list project
                    lstProject.Add(pqaProject.Key);
                    var getGuildeLines = GetGuidelinesScoring("PQAIQA", pqaProject.FirstOrDefault().score_avg);

                    // assign to score covnert
                    pqaProject.FirstOrDefault().kprValue = Convert.ToInt32(!string.IsNullOrEmpty(getGuildeLines["QualityKPR"]) ? getGuildeLines["QualityKPR"] : 0);
                    pqaProject.FirstOrDefault().kprText = getGuildeLines["QualityKPRText"];
                    pqaProject.FirstOrDefault().color = getGuildeLines["QualityKPRColor"];
                }

                foreach (var iqaProject in iqaProjects)
                {
                    // add to list project
                    lstProject.Add(iqaProject.Key);
                    var getGuildeLines = GetGuidelinesScoring("PQAIQA", iqaProject.FirstOrDefault().score_avg);

                    // assign to score covnert
                    iqaProject.FirstOrDefault().kprValue = Convert.ToInt32(!string.IsNullOrEmpty(getGuildeLines["QualityKPR"]) ? getGuildeLines["QualityKPR"] : 0);
                    iqaProject.FirstOrDefault().kprText = getGuildeLines["QualityKPRText"];
                    iqaProject.FirstOrDefault().color = getGuildeLines["QualityKPRColor"];
                }

                foreach (var reworkProject in reworkProjects)
                {
                    // add to list project
                    lstProject.Add(reworkProject.Key);
                    var getGuildeLines = GetGuidelinesScoring("Rework", reworkProject.FirstOrDefault().score_avg);

                    // assign to score covnert
                    reworkProject.FirstOrDefault().kprValue = Convert.ToInt32(!string.IsNullOrEmpty(getGuildeLines["QualityKPR"]) ? getGuildeLines["QualityKPR"] : 0);
                    reworkProject.FirstOrDefault().kprText = getGuildeLines["QualityKPRText"];
                    reworkProject.FirstOrDefault().color = getGuildeLines["QualityKPRColor"];
                }

                foreach (var defectsProject in defectsProjects)
                {
                    // add to list project
                    lstProject.Add(defectsProject.Key);
                    var getGuildeLines = GetGuidelinesScoring("PQAIQA", defectsProject.FirstOrDefault().score_avg);

                    // assign to score covnert
                    defectsProject.FirstOrDefault().kprValue = Convert.ToInt32(!string.IsNullOrEmpty(getGuildeLines["QualityKPR"]) ? getGuildeLines["QualityKPR"] : 0);
                    defectsProject.FirstOrDefault().kprText = getGuildeLines["QualityKPRText"];
                    defectsProject.FirstOrDefault().color = getGuildeLines["QualityKPRColor"];
                }

                // remove duplicate value
                lstProject = lstProject?.Distinct().ToList();

                // for loop lst project
                foreach (var project in lstProject)
                {
                    double valueCritical = criticalProjects.Find(x => x.FirstOrDefault().project == project) != null ? criticalProjects.Find(x => x.FirstOrDefault().project == project).FirstOrDefault().kprValue : 0;

                    double valueCommom = commonProjects.Find(x => x.FirstOrDefault().project == project) != null ? commonProjects.Find(x => x.FirstOrDefault().project == project).FirstOrDefault().kprValue : 0;

                    double valuePQA = pqaProjects.Find(x => x.FirstOrDefault().project == project) != null ? pqaProjects.Find(x => x.FirstOrDefault().project == project).FirstOrDefault().kprValue : 0;
                    double valueIQA = iqaProjects.Find(x => x.FirstOrDefault().project == project) != null ? iqaProjects.Find(x => x.FirstOrDefault().project == project).FirstOrDefault().kprValue : 0;

                    double valueRework = reworkProjects.Find(x => x.FirstOrDefault().project == project) != null ? reworkProjects.Find(x => x.FirstOrDefault().project == project).FirstOrDefault().kprValue : 0;
                    double valueDefect = defectsProjects.Find(x => x.FirstOrDefault().project == project) != null ? defectsProjects.Find(x => x.FirstOrDefault().project == project).FirstOrDefault().kprValue : 0;

                    // calulate with critical common 50%
                    double process = (((valueCritical + valueCommom) / 2) * 50) / 100;
                    // calulate with pqa iqa 25%
                    double system = (((valuePQA + valueIQA) / 2) * 25) / 100;
                    // calulate with reputation 25%
                    double reputation = (((valueRework + valueDefect) / 2) * 25) / 100;


                    // assign to project name
                    result.Add(project != null ? project : "null", (process + system + reputation));
                }
            }
            catch (Exception ce)
            {
                Console.Write(ce.Message.ToString());
            }

            return result;
        }

        /// <summary>
        /// filter data by month for building and civil
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (12.04.2023)
        public Dictionary<string, Dictionary<string, List<string>>> DataFilterMonthBuildingCivil(Dictionary<string, List<DateTime>> lstDateTime, SummaryRequest request)
        {
            // init data
            Dictionary<string, Dictionary<string, List<string>>> result = new Dictionary<string, Dictionary<string, List<string>>>();
            Dictionary<string, List<string>> temp = new Dictionary<string, List<string>>();
            List<string> querySQLWA = new List<string>(),
                         querySQLQAQC = new List<string>();

            try
            {
                foreach (var month in lstDateTime)
                {
                    List<string> querySQL = new List<string>();

                    // build where query
                    string whereSQL = BuildQuerySQL(new SummaryRequest
                    {
                        lteDate = month.Value[0].ToString(),
                        gteDate = month.Value[1].ToString(),
                        listProject = request.listProject,
                    });
                    whereSQL = whereSQL.Replace("@LteDate", $"'{month.Value[0]}'").Replace("@GteDate", $"'{month.Value[1]}'");

                    string queryPQA = BuildQuerySQL(new SummaryRequest
                    {
                        lteDate = month.Value[0].ToString(),
                        gteDate = month.Value[1].ToString(),
                        listProject = request.listProject
                    }, "last_update", typeProject: "project_actual");
                    queryPQA = queryPQA.Replace("@LteDate", $"'{month.Value[0]}'").Replace("@GteDate", $"'{month.Value[1]}'");

                    string queryIQA = BuildQuerySQL(new SummaryRequest
                    {
                        lteDate = month.Value[0].ToString(),
                        gteDate = month.Value[1].ToString(),
                        listProject = request.listProject
                    }, typeProject: "project_actual");
                    queryIQA = queryIQA.Replace("@LteDate", $"'{month.Value[0]}'").Replace("@GteDate", $"'{month.Value[1]}'");

                    // build query critical, common
                    string queryCriticalCommon = $"SELECT PROJECT, SUM(NO_OF_NO) AS TOTAL_NO, 0 AS TOTAL_1ST, SUM(NO_OF_YES) AS TOTAL_YES, 0 AS TOTAL_1PLUS, SUM(NO_OF_NO + NO_OF_YES) AS TOTAL, 'QAQC_COMMON_CHECK' AS ProjectType, '{month.Key}' AS month_name " +
                        $"FROM QAQC_COMMON_CHECK {whereSQL} GROUP BY PROJECT " +
                        $"UNION SELECT PROJECT, SUM(NO_OF_NO) AS TOTAL_NO, SUM(NO_OF_1ST) AS TOTAL_1ST, 0 AS TOTAL_YES, SUM(NO_OF_1PLUS) AS TOTAL_1PLUS, SUM(NO_OF_NO + NO_OF_1ST + NO_OF_1PLUS) AS TOTAL, 'QAQC_CRITICAL_CHECK' AS ProjectType, '{month.Key}' AS month_name " +
                        $"FROM QAQC_CRITICAL_CHECK {whereSQL} GROUP BY PROJECT ";

                    // build query pqa iqa
                    string queryQAQCTable = $"SELECT PQA.PROJECT AS Project, COUNT(PQA.ID_REPORT) AS TOTAL_PROJECT, SUM(PQA.OVERALL_SCORE) AS SUM_OVERALL_SCORE, (SUM(PQA.OVERALL_SCORE) / COUNT(PQA.ID)) AS SCORE_AVG, 'PQA' AS ProjectType, '{month.Key}' AS month_name " +
                        $"FROM PQA_REPORTDATA_ENTITY AS PQA {queryPQA} GROUP BY PQA.PROJECT UNION " +
                        $"SELECT IQA.project_actual AS Project, COUNT(IQA.IQA_NO) AS TOTAL_PROJECT, SUM(IQA.IQA_SCORE) AS SUM_OVERALL_SCORE, (SUM(IQA.IQA_SCORE) / COUNT(IQA.ID)) AS SCORE_AVG, 'IQA' AS ProjectType, '{month.Key}' AS month_name " +
                        $"FROM IQA_SCHEDULE_ENTITY AS IQA {queryIQA.Replace("PROJECT_NAME IN", "project_actual IN")} GROUP BY IQA.project_actual UNION " +
                        $"SELECT RW.PROJECT AS Project, COUNT(RW.ID) AS TOTAL_PROJECT, SUM(RW.REWORK_SCORE) AS SUM_OVERALL_SCORE, (SUM(RW.REWORK_SCORE) / COUNT(RW.ID)) AS SCORE_AVG, 'Rework' AS ProjectType, '{month.Key}' AS month_name " +
                        $"FROM REDUCE_REWORK AS RW {whereSQL} GROUP BY RW.PROJECT UNION " +
                        $"SELECT DF.PROJECT AS Project, COUNT(DF.ID) AS TOTAL_PROJECT, SUM(DF.DEFECT_SCORE) AS SUM_OVERALL_SCORE, (SUM(DF.DEFECT_SCORE) / COUNT(DF.ID)) AS SCORE_AVG, 'Defects' AS ProjectType, '{month.Key}' AS month_name " +
                        $"FROM DEFECT_SCORE AS DF {whereSQL} GROUP BY DF.PROJECT ";

                    // build query for cum avg month
                    // build where query
                    string whereSQLCumAvg = BuildQuerySQL(new SummaryRequest
                    {
                        lteDate = month.Value[0].ToString(),
                        gteDate = month.Value[1].ToString(),
                        listProject = request.listProject,
                    }, isCumAvg: true);
                    whereSQLCumAvg = whereSQLCumAvg.Replace("@LteDate", $"'{month.Value[0]}'").Replace("@GteDate", $"'{month.Value[1]}'");

                    string queryPQACumAvg = BuildQuerySQL(new SummaryRequest
                    {
                        lteDate = month.Value[0].ToString(),
                        gteDate = month.Value[1].ToString(),
                        listProject = request.listProject
                    }, "last_update", true, typeProject: "project_actual");
                    queryPQACumAvg = queryPQACumAvg.Replace("@LteDate", $"'{month.Value[0]}'").Replace("@GteDate", $"'{month.Value[1]}'");

                    string queryIQACumAvg = BuildQuerySQL(new SummaryRequest
                    {
                        lteDate = month.Value[0].ToString(),
                        gteDate = month.Value[1].ToString(),
                        listProject = request.listProject
                    }, isCumAvg: true, typeProject: "PROJECT_ACTUAL");
                    queryIQACumAvg = queryIQACumAvg.Replace("@LteDate", $"'{month.Value[0]}'").Replace("@GteDate", $"'{month.Value[1]}'");

                    // build query critical, common
                    string queryCriticalCommonCumAvg = $"SELECT PROJECT, SUM(NO_OF_NO) AS TOTAL_NO, 0 AS TOTAL_1ST, SUM(NO_OF_YES) AS TOTAL_YES, 0 AS TOTAL_1PLUS, SUM(NO_OF_NO + NO_OF_YES) AS TOTAL, 'QAQC_COMMON_CHECK' AS ProjectType, 'Cum-{month.Key}' AS month_name " +
                        $"FROM QAQC_COMMON_CHECK {whereSQLCumAvg} GROUP BY PROJECT " +
                        $"UNION SELECT PROJECT, SUM(NO_OF_NO) AS TOTAL_NO, SUM(NO_OF_1ST) AS TOTAL_1ST, 0 AS TOTAL_YES, SUM(NO_OF_1PLUS) AS TOTAL_1PLUS, SUM(NO_OF_NO + NO_OF_1ST + NO_OF_1PLUS) AS TOTAL, 'QAQC_CRITICAL_CHECK' AS ProjectType, 'Cum-{month.Key}' AS month_name " +
                        $"FROM QAQC_CRITICAL_CHECK {whereSQLCumAvg} GROUP BY PROJECT ";

                    // build query pqa iqa
                    string queryQAQCTableCumAvg = $"SELECT PQA.PROJECT_ACTUAL AS Project, COUNT(PQA.ID_REPORT) AS TOTAL_PROJECT, SUM(PQA.OVERALL_SCORE) AS SUM_OVERALL_SCORE, (SUM(PQA.OVERALL_SCORE) / COUNT(PQA.ID)) AS SCORE_AVG, 'PQA' AS ProjectType, 'Cum-{month.Key}' AS month_name " +
                        $"FROM PQA_REPORTDATA_ENTITY AS PQA {queryPQACumAvg} GROUP BY PQA.PROJECT_ACTUAL UNION " +
                        $"SELECT IQA.project_actual AS Project, COUNT(IQA.IQA_NO) AS TOTAL_PROJECT, SUM(IQA.IQA_SCORE) AS SUM_OVERALL_SCORE, (SUM(IQA.IQA_SCORE) / COUNT(IQA.ID)) AS SCORE_AVG, 'IQA' AS ProjectType, 'Cum-{month.Key}' AS month_name " +
                        $"FROM IQA_SCHEDULE_ENTITY AS IQA {queryIQACumAvg.Replace("PROJECT_NAME IN", "project_actual IN")} GROUP BY IQA.project_actual UNION " +
                        $"SELECT RW.PROJECT AS Project, COUNT(RW.ID) AS TOTAL_PROJECT, SUM(RW.REWORK_SCORE) AS SUM_OVERALL_SCORE, (SUM(RW.REWORK_SCORE) / COUNT(RW.ID)) AS SCORE_AVG, 'Rework' AS ProjectType, 'Cum-{month.Key}' AS month_name " +
                        $"FROM REDUCE_REWORK AS RW {whereSQLCumAvg} GROUP BY RW.PROJECT UNION " +
                        $"SELECT DF.PROJECT AS Project, COUNT(DF.ID) AS TOTAL_PROJECT, SUM(DF.DEFECT_SCORE) AS SUM_OVERALL_SCORE, (SUM(DF.DEFECT_SCORE) / COUNT(DF.ID)) AS SCORE_AVG, 'Defects' AS ProjectType, 'Cum-{month.Key}' AS month_name " +
                        $"FROM DEFECT_SCORE AS DF {whereSQLCumAvg} GROUP BY DF.PROJECT ";



                    // push month
                    querySQL.Add(queryCriticalCommon);
                    querySQL.Add(queryCriticalCommonCumAvg);

                    querySQL.Add(queryQAQCTable);
                    querySQL.Add(queryQAQCTableCumAvg);

                    // push to arr
                    querySQLWA.Add(queryCriticalCommon);
                    querySQLWA.Add(queryCriticalCommonCumAvg);

                    querySQLQAQC.Add(queryQAQCTable);
                    querySQLQAQC.Add(queryQAQCTableCumAvg);

                    // push to dic
                    temp.Add(month.Key, querySQL);
                }

                // add to result
                result.Add("MonthQuery", temp);
                result.Add("GroupMonthQuery", new Dictionary<string, List<string>>()
                {
                    {"WA", new List<string>(){ string.Join(" UNION ", querySQLWA.ToArray()) } },
                    {"QAQC", new List<string>(){ string.Join(" UNION ", querySQLQAQC.ToArray()) } },
                });
            }
            catch (Exception ce)
            {
                result.Add("MonthQuery", temp);
                result.Add("GroupMonthQuery", new Dictionary<string, List<string>>()
                {
                    {"WA", new List<string>(){ string.Join(" UNION ", querySQLWA.ToArray()) } },
                    {"QAQC", new List<string>(){ string.Join(" UNION ", querySQLQAQC.ToArray()) } },
                });
            }

            return result;
        }

        /// <summary>
        /// Get data summary Projects Quality Performance against KPI
        /// summary critical, common
        /// summary PQA, IQA
        /// summary rework, defects
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (07.04.2023)
        public async Task<ServiceResponse> SummaryProjectsQualityPerformancerequest(string request)
        {
            Dictionary<string, Dictionary<string, object>> result = new Dictionary<string, Dictionary<string, object>>()
            {
                {"Process", new Dictionary<string, object> {} },
                {"System", new Dictionary<string, object> {} },
                {"Reputation", new Dictionary<string, object> {} },
                {"ListProjectScoreRow", new Dictionary<string, object> {} },
                {"MonthData", new Dictionary<string, object> {} }
            };
            ResourceManager rm = new ResourceManager("DashboardApi.Resource.QueryDigicheck", Assembly.GetExecutingAssembly());

            // check params
            if (string.IsNullOrEmpty(request))
            {
                return Ok(
                   result,
                   message: "Params request is not correct"
                );
            }

            try
            {
                // init data
                SummaryRequest requestConvert = JsonConvert.DeserializeObject<SummaryRequest>(request);
                Dictionary<string, Dictionary<string, List<string>>> queryByMonth = new Dictionary<string, Dictionary<string, List<string>>>();

                // build where query
                string whereSQL = BuildQuerySQL(requestConvert),
                       queryIQA = BuildQuerySQL(requestConvert, typeProject: "project_actual"),
                       queryPQA = BuildQuerySQL(requestConvert, "last_update", typeProject: "project_actual"),
                       queryQAQCTable = string.Empty,
                       queryPQADigiCheck = BuildQuerySQL(requestConvert, typeProject: "project_id"),
                       queryCriticalCommon = rm.GetString("SummaryCriticalCommonDigicheck", CultureInfo.CurrentCulture),
                       queryDigiTable = rm.GetString("SummaryReworkDefect", CultureInfo.CurrentCulture);

                // get list project and mapping
                var projectAppSetting = await _projectService.GetAllProjects();

                // build query pqa iqa
                queryQAQCTable = $"SELECT PQA.project_actual AS Project, COUNT(PQA.ID_REPORT) AS TOTAL_PROJECT, SUM(PQA.OVERALL_SCORE) AS SUM_OVERALL_SCORE, (SUM(PQA.OVERALL_SCORE) / COUNT(PQA.ID)) AS SCORE_AVG, 'PQA' AS ProjectType, 'Summary' AS month_name " +
                    $"FROM PQA_REPORTDATA_ENTITY AS PQA {queryPQA} GROUP BY PQA.project_actual UNION " +
                    $"SELECT IQA.project_actual AS Project, COUNT(IQA.IQA_NO) AS TOTAL_PROJECT, SUM(IQA.IQA_SCORE) AS SUM_OVERALL_SCORE, (SUM(IQA.IQA_SCORE) / COUNT(IQA.ID)) AS SCORE_AVG, 'IQA' AS ProjectType, 'Summary' AS month_name " +
                    $"FROM IQA_SCHEDULE_ENTITY AS IQA {queryIQA.Replace("project_actual IN", "PROJECT_NAME IN")} GROUP BY IQA.project_actual ";

                queryDigiTable = queryDigiTable.Replace("@WHERECLAUSE", whereSQL);

                // new version pqa from digicheck
                string pqaDigicheckQuery = $"SELECT PQA.PROJECT_ID AS PROJECT_ID, COUNT(PQA.ID) AS TOTAL_PROJECT, SUM(PQA.TOTAL_SCORE) AS SUM_OVERALL_SCORE, (SUM(PQA.TOTAL_SCORE) / COUNT(PQA.ID)) AS SCORE_AVG, 'PQA' AS PROJECTTYPE, 'Summary' AS MONTH_NAME FROM SUBAPP_PQA AS PQA {queryPQADigiCheck} GROUP BY PQA.PROJECT_ID;";


                // step 1: get summary critical, common summary by filter date and project
                string[] typeProject = { "Building", "Civil" };

                if (typeProject.Any(requestConvert.typeProject.Contains))
                {
                    // get current month and previous 2 month
                    var lstMonth = GetListMonthNeed(requestConvert.numberMonthPrevious);

                    if (lstMonth.Count > 0)
                    {
                        queryByMonth = DataFilterMonthBuildingCivil(lstMonth, requestConvert);

                        if (queryByMonth.ContainsKey("GroupMonthQuery"))
                        {
                            queryCriticalCommon = $"{queryCriticalCommon} UNION {queryByMonth["GroupMonthQuery"]["WA"].FirstOrDefault()} ORDER BY Project;";
                            queryQAQCTable = $"{queryQAQCTable} UNION {queryByMonth["GroupMonthQuery"]["QAQC"].FirstOrDefault()} ORDER BY Project;";
                            queryDigiTable = $"{queryDigiTable} UNION {queryByMonth["GroupMonthQuery"]["QAQC"].FirstOrDefault()} ORDER BY Project;";
                        }
                    }
                }

                // call func query summary critical common
                var summaryCriticalCommon = await _dashboardQAQCRepositoryV2.GetSummaryCriticalCommonDigicheck(queryCriticalCommon, requestConvert);
                var summaryQAQCTb = await _dashboardQAQCRepositoryV2.QaQcGetSummaryPQAIQAReworkDefect(queryQAQCTable, requestConvert);
                var pqaDigicheck = await _dashboardQAQCRepositoryV2.QaQcGetSummaryPQAIQAReworkDefectDigicheck(queryDigiTable, requestConvert);

                foreach (var pqa in pqaDigicheck)
                {
                    var temp = projectAppSetting?.Find(x => x.Id == (pqa?.projectId));
                    if (temp != null)
                    {
                        pqa.project = temp.IssueKey;
                    }
                }

                foreach (var project in projectAppSetting)
                {
                    // pqa
                    var tempPQA = pqaDigicheck.Where(x => x.projectId == project.Id).ToList();
                    foreach (var pqa in tempPQA)
                    {
                        if (pqa != null)
                        {
                            pqa.project = project.IssueKey;
                        }
                    }


                    // critical, common
                    var tempCritical = summaryCriticalCommon.Where(x => x.project_id == project.Id).ToList();
                    foreach (var crccc in tempCritical)
                    {
                        if (crccc != null)
                        {
                            crccc.project = project.IssueKey;
                        }
                    }
                }

                // query after get data
                List<SummaryCriticalCommon> criticals = summaryCriticalCommon.Where(x => x.month_name == "Summary" && x.ProjectType == "QAQC_CRITICAL_CHECK").ToList(),
                                            commons = summaryCriticalCommon.Where(x => x.month_name == "Summary" && x.ProjectType == "QAQC_COMMON_CHECK").ToList();

                //List<SummaryQAQCTab> summaryCommon = summaryQAQCTb.Where(x => x.month_name == "Summary").ToList(),
                //                     pqas = pqaDigicheck.ToList(),
                //                     iqas = summaryCommon.Where(x => x.ProjectType == "IQA").ToList(),
                //                     reworks = summaryCommon.Where(x => x.ProjectType == "Rework").ToList(),
                //                     defects = summaryCommon.Where(x => x.ProjectType == "Defects").ToList();

                List<SummaryQAQCTab> summaryCommon = summaryQAQCTb.Where(x => x.month_name == "Summary").ToList(),
                                     pqas = summaryCommon.Where(x => x.ProjectType == "PQA").ToList(),
                                     iqas = summaryCommon.Where(x => x.ProjectType == "IQA").ToList(),
                                     reworks = pqaDigicheck.Where(x => x.ProjectType == "Rework").ToList(),
                                     defects = pqaDigicheck.Where(x => x.ProjectType == "Defects").ToList();

                Dictionary<string, object> lstResultQuery = new Dictionary<string, object>()
                {
                    {"criticals", criticals },
                    {"commons", commons },
                    {"pqas", pqas },
                    {"iqas", iqas },
                    {"reworks", reworks },
                    {"defects", defects },
                };

                // handle data summary common
                result["Process"] = new Dictionary<string, object>()
                {
                    {"Critical",  HandleStatusScoreingCriticalCommon(criticals, "Critical")},
                    {"Common",  HandleStatusScoreingCriticalCommon(commons, "Common")},
                };
                result["System"] = new Dictionary<string, object>()
                {
                    {"PQA",  HandleStatusScoreingQAQCTab(pqas, "PQA")},
                    {"IQA", HandleStatusScoreingQAQCTab(iqas, "IQA")},
                };
                result["Reputation"] = new Dictionary<string, object>()
                {
                    {"Rework",  HandleStatusScoreingQAQCTab(reworks, "Rework")},
                    {"Defects",  HandleStatusScoreingQAQCTab(defects, "Defects")},
                };
                result["ListProjectScoreRow"] = lstResultQuery;

                if (requestConvert.isOvervall)
                {
                    Dictionary<string, object> projectGroupScore = CalculateProjectScore(lstResultQuery);
                    result.Add("ProjectQualityKeyPerformanceResult", projectGroupScore);
                }

                // add result to building and civil
                result = HandleDataBulidingCivilReport(result, requestConvert, queryByMonth, summaryCriticalCommon, summaryQAQCTb, typeProject);
            }
            catch (Exception ce)
            {
                return Ok(
                   result,
                   message: "Something wrong with params request"
                );
            }

            return Ok(result);
        }

        /// <summary>ProjectQualityKeyPerformanceResult
        /// Function handle data for tab building project and civil project
        /// </summary>
        /// <param name="result"></param>
        /// <param name="requestConvert"></param>
        /// <param name="queryByMonth"></param>
        /// <param name="summaryCriticalCommon"></param>
        /// <param name="summaryQAQCTb"></param>
        /// <param name="typeProject"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (13.04.2023)
        public Dictionary<string, Dictionary<string, object>> HandleDataBulidingCivilReport(
            Dictionary<string, Dictionary<string, object>> result,
            SummaryRequest requestConvert,
            Dictionary<string, Dictionary<string, List<string>>> queryByMonth,
            List<SummaryCriticalCommon> summaryCriticalCommon,
            List<SummaryQAQCTab> summaryQAQCTb,
            string[] typeProject)
        {
            try
            {
                // add result to building and civil
                if (typeProject.Any(requestConvert.typeProject.Contains))
                {
                    if (queryByMonth.ContainsKey("GroupMonthQuery"))
                    {
                        foreach (var month in queryByMonth["MonthQuery"])
                        {
                            // query after get data
                            List<SummaryCriticalCommon> criticalsMonth = summaryCriticalCommon.Where(x => x.month_name == month.Key && x.ProjectType == "QAQC_CRITICAL_CHECK").ToList(),
                                                        commonsMonth = summaryCriticalCommon.Where(x => x.month_name == month.Key && x.ProjectType == "QAQC_COMMON_CHECK").ToList(),
                                                        criticalsMonthCum = summaryCriticalCommon.Where(x => x.month_name == $"Cum-{month.Key}" && x.ProjectType == "QAQC_CRITICAL_CHECK").ToList(),
                                                        commonsMonthCum = summaryCriticalCommon.Where(x => x.month_name == $"Cum-{month.Key}" && x.ProjectType == "QAQC_COMMON_CHECK").ToList();

                            List<SummaryQAQCTab> summaryMonth = summaryQAQCTb.Where(x => x.month_name == month.Key).ToList(),
                                                 pqasMonth = summaryMonth.Where(x => x.ProjectType == "PQA").ToList(),
                                                 iqasMonth = summaryMonth.Where(x => x.ProjectType == "IQA").ToList(),
                                                 reworksMonth = summaryMonth.Where(x => x.ProjectType == "Rework").ToList(),
                                                 defectsMonth = summaryMonth.Where(x => x.ProjectType == "Defects").ToList(),
                                                 summaryMonthCum = summaryQAQCTb.Where(x => x.month_name == $"Cum-{month.Key}").ToList(),
                                                 pqasMonthCum = summaryMonth.Where(x => x.ProjectType == "PQA").ToList(),
                                                 iqasMonthCum = summaryMonth.Where(x => x.ProjectType == "IQA").ToList(),
                                                 reworksMonthCum = summaryMonth.Where(x => x.ProjectType == "Rework").ToList(),
                                                 defectsMonthCum = summaryMonth.Where(x => x.ProjectType == "Defects").ToList();

                            Dictionary<string, object> lstResultQueryMonth = new Dictionary<string, object>()
                            {
                                {"criticals", criticalsMonth },
                                {"commons", commonsMonth },
                                {"pqas", pqasMonth },
                                {"iqas", iqasMonth },
                                {"reworks", reworksMonth },
                                {"defects", defectsMonth },

                                {"cum-criticals", criticalsMonthCum },
                                {"cum-commons", commonsMonthCum },
                                {"cum-pqas", pqasMonthCum },
                                {"cum-iqas", iqasMonthCum },
                                {"cum-reworks", reworksMonthCum },
                                {"cum-defects", defectsMonthCum },
                            };

                            Dictionary<string, object> projectGroupScore = CalculateProjectScore(lstResultQueryMonth);

                            // handle data summary common
                            result["MonthData"][month.Key] = new Dictionary<string, object>()
                            {
                                {"Process",  new Dictionary<string, object>()
                                    {
                                        {"Critical",  HandleStatusScoreingCriticalCommon(criticalsMonth, "Critical")},
                                        {"Common",  HandleStatusScoreingCriticalCommon(commonsMonth, "Common")},
                                        {"Cum-Critical",  HandleStatusScoreingCriticalCommon(criticalsMonthCum, "Critical")},
                                        {"Cum-Common",  HandleStatusScoreingCriticalCommon(commonsMonthCum, "Common")}
                                    }
                                },
                                {"System",  new Dictionary<string, object>()
                                    {
                                        {"PQA",  HandleStatusScoreingQAQCTab(pqasMonth, "PQA")},
                                        {"IQA", HandleStatusScoreingQAQCTab(iqasMonth, "IQA")},
                                        {"Cum-PQA",  HandleStatusScoreingQAQCTab(pqasMonthCum, "PQA")},
                                        {"Cum-IQA", HandleStatusScoreingQAQCTab(iqasMonthCum, "IQA")}
                                    }
                                },
                                {"Reputation",  new Dictionary<string, object>()
                                    {
                                        {"Rework",  HandleStatusScoreingQAQCTab(reworksMonth, "Rework")},
                                        {"Defects",  HandleStatusScoreingQAQCTab(defectsMonth, "Defects")},
                                        {"Cum-Rework",  HandleStatusScoreingQAQCTab(reworksMonthCum, "Rework")},
                                        {"Cum-Defects",  HandleStatusScoreingQAQCTab(defectsMonthCum, "Defects")},
                                    }
                                },
                                {"ListProjectScoreRow",  lstResultQueryMonth},
                                {"ProjectQualityKeyPerformanceResult",  projectGroupScore},
                            };
                        }
                    }
                }
            }
            catch (Exception ce)
            {
                return result;
            }

            return result;
        }

        /// <summary>
        /// Resource guidelines score
        /// </summary>
        /// <param name="typeScore"></param>
        /// <param name="score"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (08.04.2023)
        public Dictionary<string, string> GetGuidelinesScoring(string typeScore, double score)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            result.Add("score", score.ToString());

            var enumType = typeof(ProjectKPR);

            if (Double.IsNaN(score))
            {
                result.Add("QualityKPR", "");
                result.Add("QualityKPRText", "");
                result.Add("QualityKPRColor", "");

                return result;
            }

            switch (typeScore)
            {
                case "Critical":
                    if (score <= 85)
                    {
                        result.Add("QualityKPR", ((int)ProjectKPR.UNA).ToString());
                        result.Add("QualityKPRText", enumType.GetField(nameof(ProjectKPR.UNA)).GetCustomAttribute<Text>(false).Name);
                        result.Add("QualityKPRColor", enumType.GetField(nameof(ProjectKPR.UNA)).GetCustomAttribute<Color>(false).Name);
                    }
                    if (score > 85 && score <= 93)
                    {
                        result.Add("QualityKPR", ((int)ProjectKPR.BEX).ToString());
                        result.Add("QualityKPRText", enumType.GetField(nameof(ProjectKPR.BEX)).GetCustomAttribute<Text>(false).Name);
                        result.Add("QualityKPRColor", enumType.GetField(nameof(ProjectKPR.BEX)).GetCustomAttribute<Color>(false).Name);
                    }
                    if (score > 93 && score <= 95)
                    {
                        result.Add("QualityKPR", ((int)ProjectKPR.MEX).ToString());
                        result.Add("QualityKPRText", enumType.GetField(nameof(ProjectKPR.MEX)).GetCustomAttribute<Text>(false).Name);
                        result.Add("QualityKPRColor", enumType.GetField(nameof(ProjectKPR.MEX)).GetCustomAttribute<Color>(false).Name);
                    }
                    if (score > 95 && score <= 98)
                    {
                        result.Add("QualityKPR", ((int)ProjectKPR.JAE).ToString());
                        result.Add("QualityKPRText", enumType.GetField(nameof(ProjectKPR.JAE)).GetCustomAttribute<Text>(false).Name);
                        result.Add("QualityKPRColor", enumType.GetField(nameof(ProjectKPR.JAE)).GetCustomAttribute<Color>(false).Name);
                    }
                    if (score > 98)
                    {
                        result.Add("QualityKPR", ((int)ProjectKPR.EEX).ToString());
                        result.Add("QualityKPRText", enumType.GetField(nameof(ProjectKPR.EEX)).GetCustomAttribute<Text>(false).Name);
                        result.Add("QualityKPRColor", enumType.GetField(nameof(ProjectKPR.EEX)).GetCustomAttribute<Color>(false).Name);
                    }
                    break;
                case "Common":
                    if (score >= 12)
                    {
                        result.Add("QualityKPR", ((int)ProjectKPR.UNA).ToString());
                        result.Add("QualityKPRText", enumType.GetField(nameof(ProjectKPR.UNA)).GetCustomAttribute<Text>(false).Name);
                        result.Add("QualityKPRColor", enumType.GetField(nameof(ProjectKPR.UNA)).GetCustomAttribute<Color>(false).Name);
                    }
                    if (score >= 8 && score < 12)
                    {
                        result.Add("QualityKPR", ((int)ProjectKPR.BEX).ToString());
                        result.Add("QualityKPRText", enumType.GetField(nameof(ProjectKPR.BEX)).GetCustomAttribute<Text>(false).Name);
                        result.Add("QualityKPRColor", enumType.GetField(nameof(ProjectKPR.BEX)).GetCustomAttribute<Color>(false).Name);
                    }
                    if (score >= 5 && score < 8)
                    {
                        result.Add("QualityKPR", ((int)ProjectKPR.MEX).ToString());
                        result.Add("QualityKPRText", enumType.GetField(nameof(ProjectKPR.MEX)).GetCustomAttribute<Text>(false).Name);
                        result.Add("QualityKPRColor", enumType.GetField(nameof(ProjectKPR.MEX)).GetCustomAttribute<Color>(false).Name);
                    }
                    if (score >= 2.5 && score < 5)
                    {
                        result.Add("QualityKPR", ((int)ProjectKPR.JAE).ToString());
                        result.Add("QualityKPRText", enumType.GetField(nameof(ProjectKPR.JAE)).GetCustomAttribute<Text>(false).Name);
                        result.Add("QualityKPRColor", enumType.GetField(nameof(ProjectKPR.JAE)).GetCustomAttribute<Color>(false).Name);
                    }
                    if (score < 2.5)
                    {
                        result.Add("QualityKPR", ((int)ProjectKPR.EEX).ToString());
                        result.Add("QualityKPRText", enumType.GetField(nameof(ProjectKPR.EEX)).GetCustomAttribute<Text>(false).Name);
                        result.Add("QualityKPRColor", enumType.GetField(nameof(ProjectKPR.EEX)).GetCustomAttribute<Color>(false).Name);
                    }
                    break;
                case "PQAIQA":
                    if (score <= 60)
                    {
                        result.Add("QualityKPR", ((int)ProjectKPR.UNA).ToString());
                        result.Add("QualityKPRText", enumType.GetField(nameof(ProjectKPR.UNA)).GetCustomAttribute<Text>(false).Name);
                        result.Add("QualityKPRColor", enumType.GetField(nameof(ProjectKPR.UNA)).GetCustomAttribute<Color>(false).Name);
                    }
                    if (score > 60 && score <= 75)
                    {
                        result.Add("QualityKPR", ((int)ProjectKPR.BEX).ToString());
                        result.Add("QualityKPRText", enumType.GetField(nameof(ProjectKPR.BEX)).GetCustomAttribute<Text>(false).Name);
                        result.Add("QualityKPRColor", enumType.GetField(nameof(ProjectKPR.BEX)).GetCustomAttribute<Color>(false).Name);
                    }
                    if (score > 75 && score <= 85)
                    {
                        result.Add("QualityKPR", ((int)ProjectKPR.MEX).ToString());
                        result.Add("QualityKPRText", enumType.GetField(nameof(ProjectKPR.MEX)).GetCustomAttribute<Text>(false).Name);
                        result.Add("QualityKPRColor", enumType.GetField(nameof(ProjectKPR.MEX)).GetCustomAttribute<Color>(false).Name);
                    }
                    if (score > 85 && score <= 93)
                    {
                        result.Add("QualityKPR", ((int)ProjectKPR.JAE).ToString());
                        result.Add("QualityKPRText", enumType.GetField(nameof(ProjectKPR.JAE)).GetCustomAttribute<Text>(false).Name);
                        result.Add("QualityKPRColor", enumType.GetField(nameof(ProjectKPR.JAE)).GetCustomAttribute<Color>(false).Name);
                    }
                    if (score > 93)
                    {
                        result.Add("QualityKPR", ((int)ProjectKPR.EEX).ToString());
                        result.Add("QualityKPRText", enumType.GetField(nameof(ProjectKPR.EEX)).GetCustomAttribute<Text>(false).Name);
                        result.Add("QualityKPRColor", enumType.GetField(nameof(ProjectKPR.EEX)).GetCustomAttribute<Color>(false).Name);
                    }
                    break;
                case "Rework":
                    if (score >= 6)
                    {
                        result.Add("QualityKPR", ((int)ProjectKPR.UNA).ToString());
                        result.Add("QualityKPRText", enumType.GetField(nameof(ProjectKPR.UNA)).GetCustomAttribute<Text>(false).Name);
                        result.Add("QualityKPRColor", enumType.GetField(nameof(ProjectKPR.UNA)).GetCustomAttribute<Color>(false).Name);
                    }
                    if (score >= 4 && score < 6)
                    {
                        result.Add("QualityKPR", ((int)ProjectKPR.BEX).ToString());
                        result.Add("QualityKPRText", enumType.GetField(nameof(ProjectKPR.BEX)).GetCustomAttribute<Text>(false).Name);
                        result.Add("QualityKPRColor", enumType.GetField(nameof(ProjectKPR.BEX)).GetCustomAttribute<Color>(false).Name);
                    }
                    if (score >= 2 && score < 4)
                    {
                        result.Add("QualityKPR", ((int)ProjectKPR.MEX).ToString());
                        result.Add("QualityKPRText", enumType.GetField(nameof(ProjectKPR.MEX)).GetCustomAttribute<Text>(false).Name);
                        result.Add("QualityKPRColor", enumType.GetField(nameof(ProjectKPR.MEX)).GetCustomAttribute<Color>(false).Name);
                    }
                    if (score >= 1 && score < 2)
                    {
                        result.Add("QualityKPR", ((int)ProjectKPR.JAE).ToString());
                        result.Add("QualityKPRText", enumType.GetField(nameof(ProjectKPR.JAE)).GetCustomAttribute<Text>(false).Name);
                        result.Add("QualityKPRColor", enumType.GetField(nameof(ProjectKPR.JAE)).GetCustomAttribute<Color>(false).Name);
                    }
                    if (score < 1)
                    {
                        result.Add("QualityKPR", ((int)ProjectKPR.EEX).ToString());
                        result.Add("QualityKPRText", enumType.GetField(nameof(ProjectKPR.EEX)).GetCustomAttribute<Text>(false).Name);
                        result.Add("QualityKPRColor", enumType.GetField(nameof(ProjectKPR.EEX)).GetCustomAttribute<Color>(false).Name);
                    }
                    break;
                default:
                    result.Add("QualityKPR", "");
                    result.Add("QualityKPRText", "");
                    result.Add("QualityKPRColor", "");
                    break;
            }

            return result;
        }

        /// <summary>
        /// handle data status for score status
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (08.04.2023)
        protected Dictionary<string, object> HandleStatusScoreingCriticalCommon(List<SummaryCriticalCommon> datas, string type)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            double totalScore = 0;
            double avgScore = 0;

            try
            {
                if (type == "Critical")
                {
                    totalScore = datas.Sum(x => x.percentCriticalCheck);
                }
                else
                {
                    totalScore = datas.Sum(x => x.percentCommonCheck);
                }

                avgScore = totalScore / datas.Count();

                Dictionary<string, string> guideLinesKPR = GetGuidelinesScoring(type, avgScore);

                // add value
                result = new Dictionary<string, object>()
                {
                    {"ProjectType", type},
                    {"score", avgScore.ToString()},
                    {"QualityKPR", guideLinesKPR.ContainsKey("QualityKPR") ? guideLinesKPR["QualityKPR"] : 0},
                    {"QualityKPRText",  guideLinesKPR.ContainsKey("QualityKPRText") ? guideLinesKPR["QualityKPRText"] : ""},
                    {"QualityKPRColor", guideLinesKPR.ContainsKey("QualityKPRColor") ? guideLinesKPR["QualityKPRColor"] : ""}
                };
            }
            catch (Exception ce)
            {
                Console.WriteLine($"HandleStatusScoreing: {ce.Message}");
            }

            return result;
        }

        /// <summary>
        /// handle data status for score status
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (08.04.2023)
        protected Dictionary<string, object> HandleStatusScoreingQAQCTab(List<SummaryQAQCTab> datas, string type, string guidelines = "PQAIQA")
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            double totalScore = 0;
            double avgScore = 0;

            try
            {
                totalScore = datas.Sum(x => x.score_avg);

                avgScore = totalScore / datas.Count();

                Dictionary<string, string> guideLinesKPR = GetGuidelinesScoring(guidelines, avgScore);

                // add value
                result = new Dictionary<string, object>()
                {
                    {"ProjectType", type},
                    {"score", avgScore.ToString()},
                    {"QualityKPR", guideLinesKPR.ContainsKey("QualityKPR") ? guideLinesKPR["QualityKPR"] : 0},
                    {"QualityKPRText",  guideLinesKPR.ContainsKey("QualityKPRText") ? guideLinesKPR["QualityKPRText"] : ""},
                    {"QualityKPRColor", guideLinesKPR.ContainsKey("QualityKPRColor") ? guideLinesKPR["QualityKPRColor"] : ""}
                };
            }
            catch (Exception ce)
            {
                Console.WriteLine($"HandleStatusScoreing: {ce.Message}");
            }

            return result;
        }
        #endregion

        #region Common function
        /// <summary>
        /// get list pervious month
        /// </summary>
        /// <param name="numberPreviousMonth"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (13.03.2023)
        public Dictionary<string, List<DateTime>> GetListMonthNeed(int numberPreviousMonth)
        {
            Dictionary<string, List<DateTime>> lstMonth = new Dictionary<string, List<DateTime>>();

            var today = DateTime.Today;
            var currentMonth = new DateTime(today.Year, today.Month, 1);

            if (numberPreviousMonth >= 0)
            {
                for (int i = 0; i <= numberPreviousMonth; i++)
                {
                    var startOfMonth = currentMonth.AddMonths(-i);

                    int daysInMonth = DateTime.DaysInMonth(year: startOfMonth.Year, month: startOfMonth.Month);
                    var lastOfMonth = new DateTime(startOfMonth.Year, startOfMonth.Month, daysInMonth);

                    List<DateTime> temp = new List<DateTime>()
                    {
                        startOfMonth,
                        lastOfMonth
                    };

                    lstMonth.Add(startOfMonth.ToString("MMM-yy", CultureInfo.InvariantCulture), temp);
                }
            }

            return lstMonth;
        }

        #endregion

        #region QM Project KPR per month
        /// <summary>
        /// Get project KPR follow month and project
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (14.04.2023)
        public async Task<ServiceResponse> ProjectsKPRMonth(string request)
        {
            Dictionary<string, Dictionary<string, object>> result = new Dictionary<string, Dictionary<string, object>>()
            {
                {"MonthData", new Dictionary<string, object> {} },
                {"ThisMonth", new Dictionary<string, object> {} },
                {"LastMonth", new Dictionary<string, object> {} }
            };

            // check params
            if (string.IsNullOrEmpty(request))
            {
                return Ok(
                   result,
                   message: "Params request is not correct"
                );
            }

            try
            {
                // init data
                SummaryRequest requestConvert = JsonConvert.DeserializeObject<SummaryRequest>(request);
                Dictionary<string, Dictionary<string, List<string>>> queryByMonth = new Dictionary<string, Dictionary<string, List<string>>>();

                // build where query
                string whereSQL = BuildQuerySQL(requestConvert),
                       queryIQA = BuildQuerySQL(requestConvert, typeProject: "project_actual"),
                       queryPQA = BuildQuerySQL(requestConvert, "last_update", typeProject: "project_actual"),
                       queryCriticalCommon = string.Empty,
                       queryQAQCTable = string.Empty;

                // build query critical, common
                queryCriticalCommon = $"SELECT PROJECT, SUM(NO_OF_NO) AS TOTAL_NO, 0 AS TOTAL_1ST, SUM(NO_OF_YES) AS TOTAL_YES, 0 AS TOTAL_1PLUS, SUM(NO_OF_NO + NO_OF_YES) AS TOTAL, 'QAQC_COMMON_CHECK' AS ProjectType, 'Summary' AS month_name " +
                    $"FROM QAQC_COMMON_CHECK {whereSQL} GROUP BY PROJECT " +
                    $"UNION SELECT PROJECT, SUM(NO_OF_NO) AS TOTAL_NO, SUM(NO_OF_1ST) AS TOTAL_1ST, 0 AS TOTAL_YES, SUM(NO_OF_1PLUS) AS TOTAL_1PLUS, SUM(NO_OF_NO + NO_OF_1ST + NO_OF_1PLUS) AS TOTAL, 'QAQC_CRITICAL_CHECK' AS ProjectType, 'Summary' AS month_name " +
                    $"FROM QAQC_CRITICAL_CHECK {whereSQL} GROUP BY PROJECT ";

                // build query pqa iqa
                queryQAQCTable = $"SELECT PQA.project_actual AS Project, COUNT(PQA.ID_REPORT) AS TOTAL_PROJECT, SUM(PQA.OVERALL_SCORE) AS SUM_OVERALL_SCORE, (SUM(PQA.OVERALL_SCORE) / COUNT(PQA.ID)) AS SCORE_AVG, 'PQA' AS ProjectType, 'Summary' AS month_name " +
                    $"FROM PQA_REPORTDATA_ENTITY AS PQA {queryPQA} GROUP BY PQA.project_actual UNION " +
                    $"SELECT IQA.project_actual AS Project, COUNT(IQA.IQA_NO) AS TOTAL_PROJECT, SUM(IQA.IQA_SCORE) AS SUM_OVERALL_SCORE, (SUM(IQA.IQA_SCORE) / COUNT(IQA.ID)) AS SCORE_AVG, 'IQA' AS ProjectType, 'Summary' AS month_name " +
                    $"FROM IQA_SCHEDULE_ENTITY AS IQA {queryIQA.Replace("PROJECT_NAME IN", "project_actual IN")} GROUP BY IQA.project_actual UNION " +
                    $"SELECT RW.PROJECT AS Project, COUNT(RW.ID) AS TOTAL_PROJECT, SUM(RW.REWORK_SCORE) AS SUM_OVERALL_SCORE, (SUM(RW.REWORK_SCORE) / COUNT(RW.ID)) AS SCORE_AVG, 'Rework' AS ProjectType, 'Summary' AS month_name " +
                    $"FROM REDUCE_REWORK AS RW {whereSQL} GROUP BY RW.PROJECT UNION " +
                    $"SELECT DF.PROJECT AS Project, COUNT(DF.ID) AS TOTAL_PROJECT, SUM(DF.DEFECT_SCORE) AS SUM_OVERALL_SCORE, (SUM(DF.DEFECT_SCORE) / COUNT(DF.ID)) AS SCORE_AVG, 'Defects' AS ProjectType, 'Summary' AS month_name " +
                    $"FROM DEFECT_SCORE AS DF {whereSQL} GROUP BY DF.PROJECT ";

                // step 1: get summary critical, common summary by filter date and project
                string[] typeProject = { "Building", "Civil" };

                // get current month and previous 2 month
                var lstMonth = GetListMonthNeed(requestConvert.numberMonthPrevious);

                if (lstMonth.Count > 0)
                {
                    // add this month and last month
                    result["ThisMonth"] = new Dictionary<string, object>()
                    {
                        {lstMonth.Keys.ToList()[0], lstMonth[lstMonth.Keys.ToList()[0]]}
                    };

                    result["LastMonth"] = new Dictionary<string, object>()
                    {
                        {lstMonth.Keys.ToList()[1], lstMonth[lstMonth.Keys.ToList()[1]]}
                    };

                    // build query month
                    queryByMonth = DataFilterMonthBuildingCivil(lstMonth, requestConvert);

                    if (queryByMonth.ContainsKey("GroupMonthQuery"))
                    {
                        queryCriticalCommon = $"{queryCriticalCommon} UNION {queryByMonth["GroupMonthQuery"]["WA"].FirstOrDefault()} ORDER BY Project;";
                        queryQAQCTable = $"{queryQAQCTable} UNION {queryByMonth["GroupMonthQuery"]["QAQC"].FirstOrDefault()} ORDER BY Project;";
                    }
                }

                // call func query summary critical common
                var summaryCriticalCommon = await _dashboardQAQCRepositoryV2.WorkerAppGetSummaryPQOCriticalCommon(queryCriticalCommon, requestConvert);
                var summaryQAQCTb = await _dashboardQAQCRepositoryV2.QaQcGetSummaryPQAIQAReworkDefect(queryQAQCTable, requestConvert);

                // add result to building and civil
                result = HandleDataBulidingCivilReport(result, requestConvert, queryByMonth, summaryCriticalCommon, summaryQAQCTb, typeProject);
            }
            catch (Exception ce)
            {
                return Ok(
                   result,
                   message: "Something wrong with params request"
                );
            }

            return Ok(result);
        }
        #endregion

        /// <summary>
        /// func get reason comment
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (17.04.2023)
        public async Task<ServiceResponse> ReasonComment(string request)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();

            // check params
            if (string.IsNullOrEmpty(request))
            {
                return Ok(
                   result,
                   message: "Params request is not correct"
                );
            }

            try
            {
                // get reason comment by project id
                // init data
                SummaryRequest requestConvert = JsonConvert.DeserializeObject<SummaryRequest>(request);

                if (!string.IsNullOrEmpty(requestConvert.projectID))
                {
                    using var connection = _qaqcDapper.CreateConnection();

                    var queryData = (await connection.QueryAsync<KPRReason>("SELECT * FROM kpr_reason_entity WHERE project_id = @projectID", new
                    {
                        projectID = requestConvert.projectID
                    })).ToList();

                    return Ok(queryData);
                }
            }
            catch (Exception ce)
            {
                return Ok(result, message: ce.Message);
            }

            return Ok(result);
        }

    }
}