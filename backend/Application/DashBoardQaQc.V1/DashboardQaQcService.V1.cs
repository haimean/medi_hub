using AutoMapper;
using Dapper;
using DashboardApi.Application.Project;
using DashboardApi.Application.Project.Response;
using DashboardApi.Auth.CurrentUser;
using DashboardApi.Auth.PermisionChecker;
using DashboardApi.DatabaseContext.DapperDbContext;
using DashboardApi.Dtos.QaQc.Requests;
using DashboardApi.Dtos.QaQc.Responses;
using DashboardApi.HttpConfig;
using DashboardApi.Infastructure.QAQC;
using DashboardApi.Models;
using DashboardApi.Utils;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace DashboardApi.Application.DashBoardQaQc.V1
{
    public class DashboardQaQcServiceV1 : Service, IDashboardQaQcServiceV1
    {
        private readonly QaQcDapperContext _qaqcDapper;
        private readonly AppMainDapperContext _appMainDapper;
        private readonly IPermissionChecker _permissionChecker;
        public readonly IMapper _mapper;
        public readonly ICurrentUser _currentUser;
        private readonly WorkerDapperContext _workerDapper;
        private readonly DigiCheckDapperContext _digiCheckDapper;
        private readonly IProjectService _projectService;
        private readonly IDashboardQAQCRepositoryV2 _dashboardQAQCRepositoryV2;


        public DashboardQaQcServiceV1(QaQcDapperContext qaqcDapper,
            IPermissionChecker permissionChecker,
            IMapper mapper,
            AppMainDapperContext appMainDapper,
            ICurrentUser currentUser,
            WorkerDapperContext workerDapper,
            IProjectService projectService,
            DigiCheckDapperContext digiCheckDapper,
            IDashboardQAQCRepositoryV2 dashboardQAQCRepositoryV2)
        {
            _qaqcDapper = qaqcDapper;
            _permissionChecker = permissionChecker;
            _mapper = mapper;
            _appMainDapper = appMainDapper;
            _currentUser = currentUser;
            _workerDapper = workerDapper;
            _digiCheckDapper = digiCheckDapper;
            _projectService = projectService;
            _dashboardQAQCRepositoryV2 = dashboardQAQCRepositoryV2;
        }

        public async Task<ServiceResponse> QaQcGetSummaryData(SummaryRequest request)
        {
            ResourceManager rm = new ResourceManager("DashboardApi.Resource.QueryDigicheck", Assembly.GetExecutingAssembly());
            using var connection = _digiCheckDapper.CreateConnection();
            string commonCheckSql;
            string criticalCheckSql;
            string whereSqlCritical = " 1=1 ",
                   whereSqlCommon = " 1=1 ",
                   queryCritical = rm.GetString("CriticalQueryDigicheck", CultureInfo.CurrentCulture),
                   queryCommon = rm.GetString("CommonQueryDigicheck", CultureInfo.CurrentCulture);

            // get all project from table projects
            string lstProjectName = (request.listProjectId != null && request.listProjectId.Length >= 0) ? string.Join("','", request.listProjectId) : "";

            var projectAppSetting = await _projectService.GetAllProjects();


            if (request.lteDate != null)
            {
                whereSqlCritical += "and crc.form_date >= '" + request.lteDate + "' ";
                whereSqlCommon += "and cc.created_at >= '" + request.lteDate + "' ";
            }

            if (request.gteDate != null)
            {
                whereSqlCritical += "and crc.form_date <= '" + request.gteDate + "' ";
                whereSqlCommon += "and cc.created_at <= '" + request.gteDate + "' ";
            }

            // filter project follow project type
            if (!string.IsNullOrEmpty(lstProjectName))
            {
                whereSqlCritical += whereSqlCritical != "" ? $" AND crc.project_id IN ('{lstProjectName}')" : $"WHERE crc.project_id IN ('{lstProjectName}')";
                whereSqlCommon += whereSqlCommon != "" ? $" AND cc.project_id IN ('{lstProjectName}')" : $"WHERE cc.project_id IN ('{lstProjectName}')";
            }

            var commonCheck = (await connection.QueryAsync<EntQaQcCommonCheck>(queryCommon.Replace("@WHERECLAUSECOMMON", whereSqlCommon))).ToList();
            var criticalCheck = (await connection.QueryAsync<EntQaQcCriticalCheck>(queryCritical.Replace("@WHERECLAUSECRITICAL", whereSqlCritical))).ToList();

            foreach (var project in projectAppSetting)
            {
                // commonCheck
                var tempPQA = commonCheck.Find(x => x.project_id == (project.Id));
                if (tempPQA != null)
                {
                    tempPQA.project = project.IssueKey;
                }

                // criticalCheck
                var tempCritical = criticalCheck.Find(x => x.project_id == (project.Id));
                if (tempCritical != null)
                {
                    tempCritical.project = project.IssueKey;
                }
            }

            Dictionary<string, object> resultCommon = new Dictionary<string, object>();
            resultCommon["commonCheck"] = commonCheck.OrderBy(x => x.project).ToList();

            Dictionary<string, object> resultCritical = new Dictionary<string, object>();
            resultCritical["criticalCheck"] = criticalCheck.OrderBy(x => x.project).ToList();

            return Ok(new
            {
                commonCheck = resultCommon.FirstOrDefault().Value,
                criticalCheck = resultCritical.FirstOrDefault().Value
            });
        }

        public async Task<ServiceResponse> QaQcCriticalCommonTrades()
        {
            var trades = new List<string>();

            try
            {
                using var connection = _digiCheckDapper.CreateConnection();
                trades = (await connection.QueryAsync<string>("SELECT DISTINCT TRADE FROM SUBAPP_CRC WHERE IS_DELETED IS NOT TRUE UNION SELECT DISTINCT TRADE FROM SUBAPP_CC WHERE IS_DELETED IS NOT TRUE;")).ToList();

            }
            catch (Exception ce)
            {

            }

            return Ok(trades);
        }


        #region Handle Logic data

        public async Task<ServiceResponse> QaQcGetCriticalDataChecks(CriticalRequest request)
        {
            string whereSql = "";
            if (request.project != null && request.project.Count > 0)
            {
                whereSql += $"and project IN ('{string.Join("','", request.project)}')";
            }

            if (request.discipline != null)
            {
                if (request.discipline != "" && request.discipline != "All Trades")
                {
                    if (request.discipline != "None")
                    {
                        whereSql += "and discipline = '" + request.discipline + "' ";
                    }
                    else
                    {
                        whereSql += "and discipline is null ";
                    }
                }
            }

            if (request.lteDate != null)
            {
                if (request.lteDate != "")
                {
                    whereSql += "and created_at >= '" + request.lteDate + "' ";
                }
            }

            if (request.gteDate != null)
            {
                if (request.gteDate != "")
                {
                    whereSql += "and created_at <= '" + request.gteDate + "' ";
                }
            }

            if (whereSql != "")
            {
                whereSql = "where " + whereSql.Substring(3);
            }

            var checks = QaQcGetCriticalDetailChecks(whereSql, request.trade, "qaqc_critical_check").Result;

            return Ok(new
            {
                checks = checks["checks"]
            });
        }

        public async Task<ServiceResponse> QaQcGetCriticalData(CriticalRequest request)
        {
            string whereSql = "";
            string whereSqlV2 = "";
            if (request.project.Count > 0)
            {
                whereSql += $"and project IN ('{string.Join("','", request.project)}')";
                whereSqlV2 += $"and project_id IN ('{string.Join("','", request.projectId)}')";
            }

            if (!string.IsNullOrEmpty(request.discipline))
            {
                if (request.discipline != "All Trades")
                {
                    if (request.discipline != "None")
                    {
                        whereSql += "and discipline = '" + request.discipline + "' ";
                        whereSqlV2 += "and discipline = '" + request.discipline + "' ";
                    }
                    else
                    {
                        whereSql += "and discipline is null ";
                        whereSqlV2 += "and discipline is null ";
                    }
                }
            }

            if (!string.IsNullOrEmpty(request.lteDate))
            {
                whereSql += "and created_at >= '" + request.lteDate + "' ";
                whereSqlV2 += "and created_at >= '" + request.lteDate + "' ";
            }

            if (!string.IsNullOrEmpty(request.gteDate))
            {
                whereSql += "and created_at <= '" + request.gteDate + "' ";
                whereSqlV2 += "and created_at <= '" + request.gteDate + "' ";
            }

            if (whereSql != "")
            {
                whereSql = "where " + whereSql.Substring(3);
            }

            var project = await QaQcGetCriticalDataProject(whereSqlV2);
            var date = await QaQcGetCriticalDataDate(whereSql);
            var checks = await QaQcGetCriticalDetailChecks(whereSql, request.trade, "qaqc_critical_check");
            var tableSummary = await _permissionChecker.HasPermission(PermissionDefine.VIEW_QAQC_CRITICAL_TABLE_SUMMARY);
            var chartProject = await _permissionChecker.HasPermission(PermissionDefine.VIEW_QAQC_CRITICAL_CHART_PROJECT);
            using var connection = _workerDapper.CreateConnection();
            var projectSql =
                "SELECT \"project\" FROM qaqc_common_check " +
                "GROUP BY \"project\" ORDER BY project ; ";
            var projects = (await connection.QueryAsync<string>(projectSql)).ToList();
            var disciplineSql =
                "SELECT \"discipline\" FROM qaqc_common_check " +
                "GROUP BY \"discipline\" ORDER BY discipline ; ";
            var disciplines = (await connection.QueryAsync<string>(disciplineSql)).ToList();
            return Ok(new
            {
                failItems = project["listFailByTrade"],
                failByMonth = date["listFailByDate"],
                listTrade = project["list_trade"],
                lineLabels = project["line_labels"],
                tradeSelected = project["tradeSelected"],
                projects = projects,
                disciplines = disciplines,
                checks = checks["checks"],
                tableSummary = tableSummary,
                chartProject = chartProject
            });
        }

        public async Task<ServiceResponse> QaQcGetCriticalDataV2(CriticalRequest request)
        {
            ResourceManager rm = new ResourceManager("DashboardApi.Resource.QueryDigicheck", Assembly.GetExecutingAssembly());
            using var connection = _digiCheckDapper.CreateConnection();

            string whereSql = " 1=1 ";
            if (request.project?.Count > 0)
            {
                whereSql += $"and RT.project_id IN ('{string.Join("','", request?.projectId)}')";
            }

            if (!string.IsNullOrEmpty(request.tradeSelected))
            {
                if (request.tradeSelected != "All Trades")
                {
                    if (request.tradeSelected != "None")
                    {
                        whereSql += "and LOWER(RT.TRADE) = LOWER('" + request.tradeSelected + "') ";
                    }
                }
            }

            if (!string.IsNullOrEmpty(request.lteDate))
            {
                whereSql += "and RT.created_at >= '" + request.lteDate + "' ";
            }

            if (!string.IsNullOrEmpty(request.gteDate))
            {
                whereSql += "and RT.created_at <= '" + request.gteDate + "' ";
            }

            // get data group by trade and sub trade total yes, no
            var queryTradeSubTrades = rm.GetString("QAQCGroupByTradeSubTrade", CultureInfo.CurrentCulture);
            var queryTradeSubTradesChecklist = rm.GetString("QAQCGroupByTradeSubTradeChecklist", CultureInfo.CurrentCulture);
            var queryTradeSubTradesPerMonth = rm.GetString("QAQCGroupByTradeSubTradePerMonth", CultureInfo.CurrentCulture);
            var queryTradeSubTradesPerProject = rm.GetString("QAQCGroupByTradeSubTradePerProject", CultureInfo.CurrentCulture);

            var groupByTrades = new List<EntQaQcCriticalDetailV2>();
            var checklist = new List<EntQaQcCriticalDetailV2>();
            var perMonth = new List<EntQaQcCriticalDetailV2>();
            var subTrade = new List<EntQaQcCriticalDetailV2>();
            var perProject = new List<EntQaQcCriticalDetailV2>();

            using (var resultQuery = await connection.QueryMultipleAsync(($"{queryTradeSubTrades} {queryTradeSubTradesChecklist} {queryTradeSubTradesPerMonth} {queryTradeSubTradesPerProject}".Replace("@WHERECLAUSE", whereSql)), new { }))
            {
                groupByTrades = (await resultQuery.ReadAsync<EntQaQcCriticalDetailV2>()).ToList();
                checklist = (await resultQuery.ReadAsync<EntQaQcCriticalDetailV2>()).ToList();
                perMonth = (await resultQuery.ReadAsync<EntQaQcCriticalDetailV2>()).ToList();
                subTrade = (await resultQuery.ReadAsync<EntQaQcCriticalDetailV2>()).ToList();
                perProject = (await resultQuery.ReadAsync<EntQaQcCriticalDetailV2>()).ToList();
            }

            // get all project
            var projectAppSetting = await _projectService.GetAllProjects();
            foreach (var item in perProject)
            {
                var actualProject = projectAppSetting.Find(x => x.Id == item.project_id);
                item.project = actualProject != null ? actualProject.IssueKey : null;
            }


            // Handle data by month
            var groupMonths = perMonth.GroupBy(x => x.per_month).ToList();
            var handleMonth = new List<Dictionary<string, object>>();

            foreach (var month in groupMonths)
            {
                var temp = new Dictionary<string, object>();
                temp.Add("name", month?.Key);

                foreach (var item in month)
                {
                    temp.Add(!string.IsNullOrEmpty(item.sub_trade) ? item.sub_trade : "No name", item.total_no);
                }

                handleMonth.Add(temp);
            }

            return Ok(new
            {
                failItems = groupByTrades,
                checkList = checklist,
                failByMonth = handleMonth,
                lineLabels = subTrade,
                perProject = perProject,
            });
        }

        public async Task<ServiceResponse> QaQcGetCommonDataV2(CommonCheckRequest request)
        {
            ResourceManager rm = new ResourceManager("DashboardApi.Resource.QueryDigicheck", Assembly.GetExecutingAssembly());
            using var connection = _digiCheckDapper.CreateConnection();

            string whereSql = " 1=1 ";
            if (request.project?.Count > 0)
            {
                whereSql += $"and RT.project_id IN ('{string.Join("','", request?.projectId)}')";
            }

            if (!string.IsNullOrEmpty(request.tradeSelected))
            {
                if (request.tradeSelected != "All Trades")
                {
                    if (request.tradeSelected != "None")
                    {
                        whereSql += "and LOWER(RT.TRADE) = LOWER('" + request.tradeSelected + "') ";
                    }
                }
            }

            if (!string.IsNullOrEmpty(request.lteDate))
            {
                whereSql += "and RT.created_at >= '" + request.lteDate + "' ";
            }

            if (!string.IsNullOrEmpty(request.gteDate))
            {
                whereSql += "and RT.created_at <= '" + request.gteDate + "' ";
            }

            // get data group by trade and sub trade total yes, no
            var queryTradeSubTrades = rm.GetString("QAQCCommonGroupByTradeSubTrade", CultureInfo.CurrentCulture);
            var queryTradeSubTradesChecklist = rm.GetString("QAQCCommonGroupByTradeSubTradeChecklist", CultureInfo.CurrentCulture);
            var queryTradeSubTradesPerMonth = rm.GetString("QAQCCommonGroupByTradeSubTradePerMonth", CultureInfo.CurrentCulture);
            var queryTradeSubTradesPerProject = rm.GetString("QAQCCommonGroupByTradeSubTradePerProject", CultureInfo.CurrentCulture);

            var groupByTrades = new List<EntQaQcCommonDetail>();
            var checklist = new List<EntQaQcCommonDetail>();
            var perMonth = new List<EntQaQcCommonDetail>();
            var subTrade = new List<EntQaQcCommonDetail>();
            var perProject = new List<EntQaQcCommonDetail>();

            using (var resultQuery = await connection.QueryMultipleAsync(($"{queryTradeSubTrades} {queryTradeSubTradesChecklist} {queryTradeSubTradesPerMonth} {queryTradeSubTradesPerProject}".Replace("@WHERECLAUSE", whereSql)), new { }))
            {
                groupByTrades = (await resultQuery.ReadAsync<EntQaQcCommonDetail>()).ToList();
                checklist = (await resultQuery.ReadAsync<EntQaQcCommonDetail>()).ToList();
                perMonth = (await resultQuery.ReadAsync<EntQaQcCommonDetail>()).ToList();
                subTrade = (await resultQuery.ReadAsync<EntQaQcCommonDetail>()).ToList();
                perProject = (await resultQuery.ReadAsync<EntQaQcCommonDetail>()).ToList();
            }

            // get all project
            var projectAppSetting = await _projectService.GetAllProjects();
            foreach (var item in perProject)
            {
                var actualProject = projectAppSetting.Find(x => x.Id == item.project_id);
                item.project = actualProject != null ? actualProject.IssueKey : null;
            }


            // Handle data by month
            var groupMonths = perMonth.GroupBy(x => x.per_month).ToList();
            var handleMonth = new List<Dictionary<string, object>>();

            foreach (var month in groupMonths)
            {
                var temp = new Dictionary<string, object>();
                temp.Add("name", month?.Key);

                foreach (var item in month)
                {
                    if (item.sub_trade != null)
                    {
                        temp.Add(item.sub_trade, item.total_no);
                    }
                }

                handleMonth.Add(temp);
            }

            return Ok(new
            {
                failItems = groupByTrades,
                checkList = checklist,
                failByMonth = handleMonth,
                lineLabels = subTrade,
                perProject = perProject,
            });
        }


        public async Task<ServiceResponse> QaQcGetReworkData(ReworkRequest request)
        {
            string whereSql = "";
            if (request.project != null)
            {
                whereSql += "and project = '" + request.project + "'";
            }

            if (request.subcontractor != null)
            {
                whereSql += "and subcontractor = '" + request.subcontractor + "'";
            }

            if (request.lteDate != null)
            {
                whereSql += "and created_at >= '" + request.lteDate + "' ";
            }

            if (request.gteDate != null)
            {
                whereSql += "and created_at <= '" + request.gteDate + "' ";
            }

            if (whereSql != "")
            {
                whereSql = "where " + whereSql.Substring(3);
            }

            var subcontractor = QaQcGetReworkDataSub(whereSql).Result;
            var date = QaQcGetReworkDataDate(whereSql).Result;
            using var connection = _workerDapper.CreateConnection();
            var projectSql =
                "SELECT project FROM qaqc_rework " +
                "GROUP BY project ORDER BY project ; ";

            var projects = (await connection.QueryAsync<string>(projectSql)).ToList();

            return Ok(new
            {
                countItems = subcontractor["countItems"],
                countByMonth = date["countByMonth"],
                listSub = subcontractor["listSub"],
                lineLabels = subcontractor["lineLabels"],
                subSelected = subcontractor["subSelected"],
                projects = projects
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// ModifiBy: PQ Huy (29.03.2023) - improve performance
        public async Task<ServiceResponse> QaQcGetObservationData(ObservationRequest request)
        {
            try
            {
                // init connection
                using var connection = _workerDapper.CreateConnection();

                // setup query
                string whereSql = "";
                if (request.project != null)
                {
                    if (request.project != "")
                    {
                        whereSql += "and project_code = '" + request.project + "'";
                    }
                }

                if (request.sxf != null)
                {
                    if (request.sxf != "")
                    {
                        whereSql += "and sxf = '" + request.sxf + "'";
                    }
                }

                if (request.lteDate != null)
                {
                    if (request.lteDate != "")
                    {
                        whereSql += "and created_at >= '" + request.lteDate + "' ";
                    }
                }

                if (request.gteDate != null)
                {
                    if (request.gteDate != "")
                    {
                        whereSql += "and created_at <= '" + request.gteDate + "' ";
                    }
                }

                if (whereSql != "")
                {
                    whereSql = "where " + whereSql.Substring(3);
                }

                // call data sxf
                var sxf = QaQcGetObservationDataSxf(whereSql).Result;

                // call data date
                var date = QaQcGetObservationDataDate(whereSql).Result;

                // check permission
                var chartProject = await _permissionChecker.HasPermission(PermissionDefine.VIEW_QAQC_OBSERVATION_CHART_PROJECT);
                var chartSxfMonth = await _permissionChecker.HasPermission(PermissionDefine.VIEW_QAQC_OBSERVATION_CHART_SXF_MONTH);

                // call data project
                string projectSql = "SELECT project_code FROM qaqc_observation GROUP BY project_code ORDER BY project_code ";
                var projects = (await connection.QueryAsync<string>(projectSql)).ToList();

                // call data constractor
                var sub = QaQcGetObservationDataSub(whereSql).Result;

                // return value
                return Ok(new
                {
                    countItems = sxf["countItems"],
                    countByMonth = date["countByMonth"],
                    listSub = sxf["listSxf"],
                    lineLabels = sxf["lineLabels"],
                    subSelected = sxf["sxfSelected"],
                    projects = projects,
                    sub = sub["sub"],
                    chartProject = chartProject,
                    chartSxfMonth = chartSxfMonth
                });
            }
            catch (Exception ce)
            {
                // return value
                return Ok(new { }, message: ce.Message);
            }
        }

        public async Task<ServiceResponse> QaQcGetViolationData(ViolationRequest request)
        {
            string whereSql = "";
            if (request.project != null)
            {
                whereSql += "and project = '" + request.project + "'";
            }

            if (request.subcontractor != null)
            {
                whereSql += "and subcontractor = '" + request.subcontractor + "'";
            }

            if (request.lteDate != null)
            {
                whereSql += "and created_at >= '" + request.lteDate + "' ";
            }

            if (request.gteDate != null)
            {
                whereSql += "and created_at <= '" + request.gteDate + "' ";
            }

            if (whereSql != "")
            {
                whereSql = "where " + whereSql.Substring(3);
            }

            var subcontractor = QaQcGetViolationDataSub(whereSql).Result;
            var date = QaQcGetViolationDataDate(whereSql).Result;
            using var connection = _workerDapper.CreateConnection();
            var projectSql =
                "SELECT project FROM qaqc_rework " +
                "GROUP BY project ORDER BY project ; ";
            // DateTime localDate1 = DateTime.Now;
            var projects = (await connection.QueryAsync<string>(projectSql)).ToList();
            return Ok(new
            {
                countItems = subcontractor["countItems"],
                countByMonth = date["countByMonth"],
                listSub = subcontractor["listSub"],
                lineLabels = subcontractor["lineLabels"],
                subSelected = subcontractor["subSelected"],
                projects = projects,
                subcontractor = subcontractor
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="whereSql"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, object>> QaQcGetCriticalDataProject(string whereSql)
        {
            ResourceManager rm = new ResourceManager("DashboardApi.Resource.QueryDigicheck", Assembly.GetExecutingAssembly());
            var result = new Dictionary<string, object>();
            if (!await _permissionChecker.HasPermission(PermissionDefine.VIEW_QAQC_CRITICAL_PROJECT))
            {
                result["listFailByTrade"] = false;
                result["list_trade"] = new List<string>();
                result["line_labels"] = new List<string>();
                result["tradeSelected"] = "";
            }
            else
            {
                using var connection = _digiCheckDapper.CreateConnection();
                var failByTradeSql = rm.GetString("QaQcGetCriticalDataProject1", CultureInfo.CurrentCulture);

                var failByTradeList = (await connection.QueryAsync<EntQaQcCriticalDetail>(failByTradeSql.Replace("@WHERECLAUSE", $" 1=1 {whereSql}"))).ToList();
                var listFailByTrade = new List<Dictionary<string, object>>();
                var list_trade = new List<string>();
                var line_labels = new List<Dictionary<string, object>>();
                var iColor = 0;
                foreach (var e in failByTradeList)
                {
                    if (e.total <= 0) continue;
                    var failByTrade = new Dictionary<string, object>();
                    failByTrade["discipline"] = e.discipline;
                    failByTrade["name"] = e.trade;
                    failByTrade["total"] = e.total;
                    failByTrade["short_name"] = e.shortform;
                    failByTrade["total_no"] = e.no_of_no;
                    failByTrade["total_1st"] = e.no_of_1st;
                    failByTrade["total_1plus"] = e.no_of_1plus;
                    failByTrade["percent"] = e.percent;
                    var dataByProjectSql = rm.GetString("QaQcGetCriticalDataProject2", CultureInfo.CurrentCulture);
                    var projectByProject = (await connection.QueryAsync<EntQaQcCriticalByProject>(dataByProjectSql.Replace("@WHERECLAUSE", $"SST.SUB_TRADE = '{e.trade}'"))).ToList();

                    foreach (var f in projectByProject)
                    {
                        var projectPercentName = f.project + "_percent";
                        if (f.percent > 0)
                        {
                            failByTrade[projectPercentName] = f.percent;
                        }
                        else
                        {
                            failByTrade[projectPercentName] = 0;
                        }
                    }

                    if (failByTrade["name"] != "")
                    {
                        listFailByTrade.Add(failByTrade);
                    }

                    list_trade.Add(e.trade);
                    var lineBaseTrade = new Dictionary<string, object>();
                    lineBaseTrade["key"] = e.shortform;
                    lineBaseTrade["trade_full_name"] = e.trade;
                    try
                    {
                        lineBaseTrade["color"] = CommoncheckColor.getColor(iColor);
                    }
                    catch (Exception exception)
                    {
                        lineBaseTrade["color"] = "Red";
                    }

                    iColor++;
                    line_labels.Add(lineBaseTrade);
                }

                result["listFailByTrade"] = listFailByTrade;
                result["list_trade"] = list_trade;
                result["line_labels"] = line_labels;
                try
                {
                    result["tradeSelected"] = list_trade.Count > 0 ? list_trade[0] : "";
                }
                catch (Exception e)
                {
                    result["tradeSelected"] = "";
                }
            }

            return result;
        }

        public async Task<Dictionary<string, object>> QaQcGetCriticalDataDate(string whereSql)
        {
            var result = new Dictionary<string, object>();
            if (!await _permissionChecker.HasPermission(PermissionDefine.VIEW_QAQC_CRITICAL_DATE))
            {
                result["listFailByDate"] = false;
            }
            else
            {
                using var connection = _workerDapper.CreateConnection();
                var failByDateSql =
                    "SELECT to_char( \"created_at\", 'YYYYMM') as yyyymm, to_char( \"created_at\", 'MM/YY') " +
                    "as mmyy from qaqc_critical_check " + whereSql + " group by YYYYMM, MMYY order by yyyymm;";
                var failByDateList = (await connection.QueryAsync<EntQaQcCommonCheckGroupDate>(failByDateSql)).ToList();
                var groupByDate = "SELECT \"project\"  " +
                                  "FROM qaqc_common_check " +
                                  "GROUP BY \"project\"; ";
                var listFailByDate = new List<Dictionary<string, object>>();
                foreach (var e in failByDateList)
                {
                    // if (e.total <= 5) continue;
                    var failByDate = new Dictionary<string, object>();
                    failByDate["name"] = e.mmyy;
                    var dataByTradeSql =
                        "select trade, SUM(no_of_no) as no_of_no, sum(no_of_1st) as no_of_1st, sum(no_of_1plus) as no_of_1plus, sum(no_of_no + no_of_1st + no_of_1plus) " +
                        "as total from (select *, to_char( \"created_at\", 'YYYYMM') as YYYYMM from qaqc_critical_check) AS YM where YM.YYYYMM = '" +
                        e.yyyymm + "' GROUP BY trade";
                    var dataByTrade =
                        (await connection.QueryAsync<EntQaQcCriticalByTrade>(dataByTradeSql)).ToList();
                    foreach (var f in dataByTrade)
                    {
                        if (f.trade != null)
                        {
                            if (f.total > 0)
                            {
                                failByDate[f.trade] = f.percent * 10;
                            }
                            else
                            {
                                failByDate[f.trade] = 0;
                            }
                        }
                    }

                    listFailByDate.Add(failByDate);
                }

                result["listFailByDate"] = listFailByDate;
            }

            return result;
        }

        public async Task<Dictionary<string, object>> QaQcGetCommonCheckDataTrade(string whereSql)
        {
            var result = new Dictionary<string, object>();
            if (!await _permissionChecker.HasPermission(PermissionDefine.VIEW_QAQC_COMMON_TRADE))
            {
                result["listFailByTrade"] = false;
                result["list_trade"] = new List<string>();
                result["line_labels"] = new List<string>();
                result["tradeSelected"] = "";
            }
            else
            {
                using var connection = _workerDapper.CreateConnection();
                var commonCheckSql =
                    "SELECT trade,discipline,shortform, SUM (no_of_yes) AS no_of_yes, SUM (no_of_no) AS no_of_no,  " +
                    "SUM (no_of_yes + no_of_no) AS total FROM qaqc_common_check " + whereSql +
                    " GROUP BY trade,discipline,shortform ORDER BY discipline, trade";
                var commonCheckList = (await connection.QueryAsync<EntQaQcCommonDetail>(commonCheckSql)).ToList();
                var listFailByTrade = new List<Dictionary<string, object>>();
                var list_trade = new List<string>();
                var line_labels = new List<Dictionary<string, object>>();
                var iColor = 0;
                var dataByTradeSql = "SELECT project, trade,sum(no_of_yes) as no_of_yes, sum(no_of_no) as no_of_no " +
                                       "FROM qaqc_common_check " + " group by project,trade";
                List<EntQaQcCommonCheckByProject> query = (await connection.QueryAsync<EntQaQcCommonCheckByProject>(dataByTradeSql)).ToList(); ;


                foreach (var e in commonCheckList)
                {
                    if (e.total <= 5) continue;
                    var failByTrade = new Dictionary<string, object>();
                    failByTrade["discipline"] = e.discipline;
                    failByTrade["name"] = e.trade;
                    failByTrade["total"] = e.total;
                    failByTrade["short_name"] = e.shortform;
                    failByTrade["total_no"] = e.no_of_no;
                    failByTrade["total_yes"] = e.no_of_yes;
                    failByTrade["discipline"] = e.discipline;
                    failByTrade["percent"] = 100 - e.percent;
                    //var dataByTradeSql = "SELECT project, sum(no_of_yes) as no_of_yes, sum(no_of_no) as no_of_no " +
                    //                     "FROM qaqc_common_check " +
                    //                     "WHERE trade = '" + e.trade + "' group by project";
                    var projectByTrade = query.Where(c => c.trade == e.trade).ToList();
                    //    (await connection.QueryAsync<EntQaQcCommonCheckByProject>(dataByTradeSql)).ToList();

                    foreach (var f in projectByTrade)
                    {
                        var projectPercentName = f.project + "_percent";
                        if (f.percent > 0)
                        {
                            failByTrade[projectPercentName] = f.percent;
                        }
                        else
                        {
                            failByTrade[projectPercentName] = 0;
                        }
                    }

                    if (failByTrade["name"] != "")
                    {
                        listFailByTrade.Add(failByTrade);
                    }

                    list_trade.Add(e.trade);
                    var lineBaseTrade = new Dictionary<string, object>();
                    lineBaseTrade["key"] = e.shortform;
                    lineBaseTrade["trade_full_name"] = e.trade;
                    try
                    {
                        lineBaseTrade["color"] = CommoncheckColor.getColor(iColor);
                    }
                    catch (Exception exception)
                    {
                        lineBaseTrade["color"] = "Red";
                    }

                    iColor++;
                    line_labels.Add(lineBaseTrade);
                }

                result["listFailByTrade"] = listFailByTrade;
                result["list_trade"] = list_trade;
                result["line_labels"] = line_labels;
                try
                {
                    result["tradeSelected"] = list_trade.Count > 0 ? list_trade[0] : "";
                }
                catch (Exception e)
                {
                    result["tradeSelected"] = "";
                }
            }

            return result;
        }

        public async Task<Dictionary<string, object>> QaQcGetCommonCheckDataDate(string whereSql)
        {
            var result = new Dictionary<string, object>();
            if (!await _permissionChecker.HasPermission(PermissionDefine.VIEW_QAQC_COMMON_DATE))
            {
                result["listFailByDate"] = false;
            }
            else
            {
                using var connection = _workerDapper.CreateConnection();
                var tradeDateSql =
                    "SELECT  to_char( created_at, 'YYYYMM') as yyyymm, to_char( created_at, 'MM/YY') " +
                    "as mmyy from qaqc_common_check " + whereSql + " group by YYYYMM, MMYY order by yyyymm;";
                var tradeByDateList = (await connection.QueryAsync<EntQaQcCommonCheckGroupDate>(tradeDateSql)).ToList();
                var groupByDate = "SELECT project  " +
                                  "FROM qaqc_common_check " +
                                  "GROUP BY project";
                var listFailByDate = new List<Dictionary<string, object>>();
                foreach (var e in tradeByDateList)
                {
                    var failByDate = new Dictionary<string, object>();
                    failByDate["name"] = e.mmyy;
                    var dataByShortformSql =
                        "select \"shortform\" as shortForm, SUM(\"no_of_yes\") as no_of_yes, sum(\"no_of_no\") as no_of_no, sum(\"no_of_yes\"+\"no_of_no\") " +
                        "as total from (select *, to_char( \"created_at\", 'YYYYMM') as YYYMM from qaqc_common_check) AS YM where YM.YYYMM = '" +
                        e.yyyymm + "' GROUP BY \"shortform\"";
                    ;
                    var dataByShortform =
                        (await connection.QueryAsync<EntQaQcCommonCheckByShortform>(dataByShortformSql)).ToList();
                    foreach (var f in dataByShortform)
                    {
                        if (f?.shortform != null)
                        {
                            if (f.total > 0)
                            {
                                failByDate[f?.shortform] = 100 - f.percent * 10;
                            }
                            else
                            {
                                failByDate[f?.shortform] = 100;
                            }
                        }
                    }

                    listFailByDate.Add(failByDate);
                }

                result["listFailByDate"] = listFailByDate;
            }

            return result;
        }

        public async Task<Dictionary<string, object>> QaQcGetCommonDetailChecks(string whereSql, string trade,
            string tableName)
        {
            var result = new Dictionary<string, object>();

            if (!await _permissionChecker.HasPermission(PermissionDefine.VIEW_QAQC_COMMON_CHECK))
            {
                result["checks"] = false;
            }
            else
            {
                using var connection = _workerDapper.CreateConnection();
                if (whereSql != "")
                {
                    if (trade != "" && !trade.IsNullOrEmpty())
                    {
                        whereSql = whereSql + " and trade = '" + trade + "' ";
                    }
                }
                else
                {
                    if (trade != "" && trade != null)
                    {
                        whereSql = "where trade = '" + trade + "' ";
                    }
                }

                var tradeDataSql = "SELECT check_list FROM " + tableName + " " + whereSql;
                var tradeDataString = await connection.QueryAsync<string>(tradeDataSql);
                // unwind total data to list
                Dictionary<string, CommonCheckDto.UnwindTrade> unwindTradeMap =
                    new Dictionary<string, CommonCheckDto.UnwindTrade>();
                List<string> checkList = new List<string>();
                if (tradeDataString.Count() > 0)
                {
                    foreach (var e in tradeDataString)
                    {
                        if (e != null)
                        {
                            checkList = JsonConvert.DeserializeObject<Dictionary<string, string>>(e)
                                .Select(f => f.Key).ToList();
                            break;
                        }
                    }
                }

                foreach (var e in tradeDataString)
                {
                    if (e != null)
                    {
                        Dictionary<string, string>
                            unwindData = JsonConvert.DeserializeObject<Dictionary<string, string>>(e);
                        foreach (var f in unwindData)
                        {
                            string key = f.Key;
                            string value = f.Value;
                            CommonCheckDto.UnwindTrade unwindTrade = new CommonCheckDto.UnwindTrade();
                            bool isConcluded = unwindTradeMap.ContainsKey(f.Key);
                            if (isConcluded)
                            {
                                unwindTrade = unwindTradeMap[f.Key];
                            }

                            bool checkText = false;
                            string firstWord = f.Key.Split(" ")[0];
                            checkText = int.TryParse(firstWord, out _);
                            var text = value.ToLower();
                            if (text.Contains("no") && checkText == false)
                            {
                                unwindTrade.total_no += 1;
                            }

                            if (text.Contains("yes") && checkText == false)
                            {
                                unwindTrade.total_yes += 1;
                            }

                            if (!isConcluded)
                            {
                                unwindTrade.name = key;
                                unwindTradeMap.Add(key, unwindTrade);
                            }
                        }
                    }
                }

                List<CommonCheckDto.UnwindTrade> checks = new List<CommonCheckDto.UnwindTrade>();
                foreach (var e in checkList)
                {
                    checks.Add(unwindTradeMap[e]);
                }

                // result["checkList"] = checkList;
                result["checks"] = checks;
            }

            return result;
        }

        public async Task<Dictionary<string, object>> QaQcGetCriticalDetailChecks(string whereSql, string trade,
            string tableName)
        {
            var result = new Dictionary<string, object>();
            if (!await _permissionChecker.HasPermission(PermissionDefine.VIEW_QAQC_CRITICAL_CHECK))
            {
                result["checkList"] = false;
                result["checks"] = false;
            }
            else
            {
                using var connection = _workerDapper.CreateConnection();
                if (whereSql != "")
                {
                    if (trade != "" && !trade.IsNullOrEmpty())
                    {
                        whereSql = whereSql + " and trade = '" + trade + "' ";
                    }
                }
                else
                {
                    if (trade != "" && trade != null)
                    {
                        whereSql = "where trade = '" + trade + "' ";
                    }
                }

                var tradeDataSql = "SELECT check_list FROM " + tableName + " " + whereSql;
                var tradeDataString = await connection.QueryAsync<string>(tradeDataSql);
                // unwind total data to list
                Dictionary<string, CommonCheckDto.UnwindTrade> unwindTradeMap =
                    new Dictionary<string, CommonCheckDto.UnwindTrade>();
                List<string> checkList = new List<string>();
                if (tradeDataString.Count() > 0)
                {
                    foreach (var e in tradeDataString)
                    {
                        if (e != null)
                        {
                            checkList = JsonConvert.DeserializeObject<Dictionary<string, string>>(e)
                                .Select(f => f.Key).ToList();
                            break;
                        }
                    }
                }

                foreach (var e in tradeDataString)
                {
                    if (e != null)
                    {
                        Dictionary<string, string>
                            unwindData = JsonConvert.DeserializeObject<Dictionary<string, string>>(e);
                        foreach (var f in unwindData)
                        {
                            string key = f.Key;
                            string value = f.Value;
                            CommonCheckDto.UnwindTrade unwindTrade = new CommonCheckDto.UnwindTrade();
                            bool isConcluded = unwindTradeMap.ContainsKey(f.Key);
                            if (isConcluded)
                            {
                                unwindTrade = unwindTradeMap[f.Key];
                            }

                            bool checkText = false;
                            string firstWord = f.Key.Split(" ")[0];
                            checkText = int.TryParse(firstWord, out _);
                            var text = value.ToLower();
                            if (text.Contains("no") && checkText == false)
                            {
                                unwindTrade.total_no += 1;
                            }

                            if (text.Contains("yes") && checkText == false)
                            {
                                unwindTrade.total_yes += 1;
                            }

                            if (!isConcluded)
                            {
                                unwindTrade.name = key;
                                unwindTradeMap.Add(key, unwindTrade);
                            }
                        }
                    }
                }

                List<CommonCheckDto.UnwindTrade> checks = new List<CommonCheckDto.UnwindTrade>();
                foreach (var e in checkList)
                {
                    checks.Add(unwindTradeMap[e]);
                }

                result["checkList"] = checkList;
                result["checks"] = checks;
            }

            return result;
        }

        public async Task<Dictionary<string, object>> QaQcGetReworkDataSub(string whereSql)
        {
            var result = new Dictionary<string, object>();
            using var connection = _workerDapper.CreateConnection();
            var reworkSql = "select subcontractor, count (*) as total from qaqc_rework " +
                            whereSql + " group by subcontractor order by subcontractor";
            var reworkList = (await connection.QueryAsync<EntQaQcRework>(reworkSql)).ToList();
            var listCountBySub = new List<Dictionary<string, object>>();
            var list_trade = new List<string>();
            var line_labels = new List<Dictionary<string, object>>();
            var iColor = 0;
            foreach (var e in reworkList)
            {
                var rework = new Dictionary<string, object>();
                rework["name"] = e.subcontractor;
                if (rework["name"] != null)
                {
                    rework["short_name"] = getShortNameOfTrade(e.subcontractor);
                }
                else
                {
                    rework["short_name"] = "NA";
                }

                rework["total"] = e.total;
                var dataByProjectSql = "select project, count(*) from qaqc_rework where subcontractor = '" +
                                       e.subcontractor + "' group by project";
                var projectByProject =
                    (await connection.QueryAsync<EntQaQcReworkByProject>(dataByProjectSql)).ToList();
                foreach (var f in projectByProject)
                {
                    var projectCountName = f.project + "_count";
                    rework[projectCountName] = f.total;
                }

                if (e.total > 0)
                {
                    listCountBySub.Add(rework);
                }

                list_trade.Add(e.subcontractor);
                var lineBaseTrade = new Dictionary<string, object>();
                lineBaseTrade["key"] = e.subcontractor;
                try
                {
                    lineBaseTrade["color"] = CommoncheckColor.getColor(iColor);
                }
                catch (Exception exception)
                {
                    lineBaseTrade["color"] = "Red";
                }

                // lineBaseTrade["color"] = CommoncheckColor.getColor(iColor);
                iColor++;
                line_labels.Add(lineBaseTrade);
            }

            result["countItems"] = listCountBySub;
            result["listSub"] = list_trade;
            result["lineLabels"] = line_labels;
            try
            {
                result["subSelected"] = list_trade[0];
            }
            catch (Exception e)
            {
                result["subSelected"] = "";
            }

            return result;
        }

        public async Task<Dictionary<string, object>> QaQcGetReworkDataDate(string whereSql)
        {
            var result = new Dictionary<string, object>();
            using var connection = _workerDapper.CreateConnection();
            var subcontractorByDateSql =
                "SELECT  to_char( created_at, 'YYYYMM') as yyyymm, to_char( created_at, 'MM/YY') " +
                "as mmyy, count(*) as total from qaqc_rework " + whereSql +
                " group by yyyymm, mmyy order by yyyymm;";
            var subcontractorByDateList =
                (await connection.QueryAsync<EntQaQcReworkDate>(subcontractorByDateSql)).ToList();
            var groupByDate = "SELECT \"project\"  " +
                              "FROM qaqc_common_check " +
                              "GROUP BY \"project\"; ";
            var listSubcontractorByDate = new List<Dictionary<string, object>>();
            foreach (var e in subcontractorByDateList)
            {
                // if (e.total <= 5) continue;
                var subcontractorByDate = new Dictionary<string, object>();
                subcontractorByDate["name"] = e.mmyy;
                subcontractorByDate["All"] = e.total;
                var dataBySubcontractorSql =
                    "select subcontractor, count(*) as total from (select *, to_char( created_at, 'YYYYMM') as " +
                    "YYYYMM, to_char( created_at, 'MMYY') as MMYY from qaqc_rework) AS YM where YM.MMYY = '" + e.mmyy +
                    "' GROUP BY subcontractor";
                var dataBySubcontractor =
                    (await connection.QueryAsync<EntQaQcReworkBySubcontractor>(dataBySubcontractorSql)).ToList();
                foreach (var f in dataBySubcontractor)
                {
                    subcontractorByDate[f.subcontractor] = f.total;
                }

                listSubcontractorByDate.Add(subcontractorByDate);
            }

            result["countByMonth"] = listSubcontractorByDate;
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="whereSql"></param>
        /// <returns></returns>
        /// Improve performance
        /// ModifyBy: PQ Huy (29.03.2023)
        public async Task<Dictionary<string, object>> QaQcGetObservationDataSxf(string whereSql)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            if (!await _permissionChecker.HasPermission(PermissionDefine.VIEW_QAQC_OBSERVATION_SXF))
            {
                result["countItems"] = false;
                result["listSxf"] = new List<string>();
                result["lineLabels"] = new List<string>();
                result["sxfSelected"] = "";
            }
            else
            {
                // init value
                using var connection = _workerDapper.CreateConnection();
                List<Dictionary<string, object>> listCountProjectBySxf = new List<Dictionary<string, object>>();
                List<string> lstSxf = new List<string>();
                List<Dictionary<string, object>> line_labels = new List<Dictionary<string, object>>();
                Dictionary<string, object> firstLineBase = new Dictionary<string, object>();
                int iColor = 1;

                firstLineBase["key"] = "All";
                firstLineBase["color"] = CommoncheckColor.getColor(0);
                line_labels.Add(firstLineBase);

                // query project sxf
                string queryProjectSxf = $"select sxf, count (*) as total, project_code as project from qaqc_observation {whereSql} group by sxf, project_code order by sxf, project_code;";
                List<EntQaQcObservation> sxfProjectList = (await connection.QueryAsync<EntQaQcObservation>(queryProjectSxf)).ToList();

                // group sxf
                var listSxf = sxfProjectList.GroupBy(p => p.sxf).ToList();

                // get list sxf, project, sum total and total by project
                foreach (var sxfName in listSxf)
                {
                    Dictionary<string, object> handleProjectSxf = new Dictionary<string, object>();

                    handleProjectSxf.Add("name", sxfName.Key?.ToString());
                    handleProjectSxf.Add("total", sxfName.Sum(x => x.total));
                    lstSxf.Add(sxfName.Key?.ToString());

                    foreach (var project in sxfName)
                    {
                        handleProjectSxf.Add($"{project.project}_count", project.total);
                    }

                    // add to list
                    listCountProjectBySxf.Add(handleProjectSxf);

                    var lineBaseTrade = new Dictionary<string, object>();
                    lineBaseTrade["key"] = sxfName.Key?.ToString();
                    try
                    {
                        lineBaseTrade["color"] = CommoncheckColor.getColor(iColor);
                    }
                    catch (Exception exception)
                    {
                        lineBaseTrade["color"] = "Red";
                    }

                    iColor++;
                    line_labels.Add(lineBaseTrade);
                }

                // add countItems to result
                result.Add("countItems", listCountProjectBySxf);

                // add list sxf
                result.Add("listSxf", lstSxf);

                // add lineLabels
                result.Add("lineLabels", line_labels);

                try
                {
                    result.Add("sxfSelected", lstSxf?.Count > 0 ? lstSxf[0] : "");
                }
                catch (Exception e)
                {
                    result.Add("sxfSelected", "");
                }
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="whereSql"></param>
        /// <returns></returns>
        /// Improve performance
        /// ModifyBy: PQ Huy (29.03.2023)
        public async Task<Dictionary<string, object>> QaQcGetObservationDataDate(string whereSql)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            List<Dictionary<string, object>> listSxf = new List<Dictionary<string, object>>();

            if (!await _permissionChecker.HasPermission(PermissionDefine.VIEW_QAQC_OBSERVATION_DATE))
            {
                result["countByMonth"] = false;
            }
            else
            {
                using var connection = _workerDapper.CreateConnection();

                // query data
                string querySQL = $"SELECT to_char( created_at, 'YYYYMM') as ym, to_char( created_at, 'MM/YY') as my, sxf, count(*) as total_sxf " +
                    $"from qaqc_observation {whereSql} group by ym, my, sxf order by ym, sxf;";

                List<EntQaQcObservationDate> dataQuery = (await connection.QueryAsync<EntQaQcObservationDate>(querySQL)).ToList();

                // group by data by month (my)
                var groupDataYMs = dataQuery?.GroupBy(x => x.my)?.ToList();

                // add each other value
                foreach (var groupDataYM in groupDataYMs)
                {
                    Dictionary<string, object> dataSxf = new Dictionary<string, object>();

                    // default value
                    dataSxf.Add("name", groupDataYM?.Key?.ToString());
                    dataSxf.Add("All", groupDataYM?.Sum(x => x?.total_sxf));

                    // add value sxf
                    foreach (var sxfData in groupDataYM)
                    {
                        dataSxf.Add(sxfData.sxf?.ToString(), sxfData?.total_sxf);
                    }

                    // add to list
                    listSxf.Add(dataSxf);
                }

                // add to dic
                result.Add("countByMonth", listSxf);
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="whereSql"></param>
        /// <returns></returns>
        /// Improve performance
        /// ModifyBy: PQ Huy (29.03.2023)
        public async Task<Dictionary<string, object>> QaQcGetObservationDataSub(string whereSql)
        {
            var result = new Dictionary<string, object>();
            if (!await _permissionChecker.HasPermission(PermissionDefine.VIEW_QAQC_OBSERVATION_SUB))
            {
                result["sub"] = false;
            }
            else
            {
                using var connection = _workerDapper.CreateConnection();
                var subList = new List<EntQaQcObservationContractor>();

                var listContractorSql = $"select \"Attention (Subcontractor):\" as contractor, (array_agg(sxf))[1] AS sxf from qaqc_observation {whereSql} group by \"Attention (Subcontractor):\"";
                var listContractor = (await connection.QueryAsync<EntQaQcContractor>(listContractorSql)).ToList();

                // list query
                string lstSxf = string.Join(",", listContractor.Select(x => $"'{x.sxf}'").ToArray());

                // handle query where
                var whereSxfSql = $"{whereSql}";

                if (!string.IsNullOrEmpty(lstSxf))
                {
                    whereSxfSql += $" and sxf in ({lstSxf})";
                }

                if (whereSql == "")
                {
                    whereSxfSql = $" where sxf in ({lstSxf})";
                }

                // query db
                string queryProject = $"select project_code as project,  count(*), sxf from qaqc_observation {whereSxfSql} group by project_code, sxf ";
                var projectCount = (await connection.QueryAsync<EntQaQcContractorByProject>(queryProject)).ToList();


                foreach (var c in listContractor)
                {
                    // init
                    var entQaQcObservationContractor = new EntQaQcObservationContractor();

                    // get correct sxf
                    var projectBySxf = projectCount.Where(x => x.sxf == c.sxf).ToList();

                    // add name
                    entQaQcObservationContractor.name = c.contractor;

                    // count total
                    entQaQcObservationContractor.total = projectBySxf.Aggregate(0, (total, x) => total + x.count);


                    var perPjt = "";

                    foreach (var p in projectBySxf)
                    {
                        if (p.count > 0)
                        {
                            perPjt += p.project + ":" + p.count + " ";
                        }
                    }

                    entQaQcObservationContractor.pjtBreakdown = perPjt;
                    subList.Add(entQaQcObservationContractor);
                }

                result["sub"] = subList;
            }

            return result;
        }

        public async Task<ServiceResponse> QaQcGetObservationStaticReport()
        {
            var result = new Dictionary<string, object>();
            result.Add("staticReport", false);

            try
            {
                if (!await _permissionChecker.HasPermission(PermissionDefine.VIEW_QAQC_OBSERVATION_STATIC_REPORT))
                {
                    result["staticReport"] = false;
                }
                else
                {
                    using var connection = _workerDapper.CreateConnection();
                    var staticReportSql = "select COUNT(*) AS reports, " +
                                          "COUNT(*) FILTER ( where \"Status of Observation:\" = 'Draft') AS totalDraft," +
                                          "COUNT(*) FILTER ( where \"Status of Observation:\" = 'Accepted by QA') AS totalQa,+ " +
                                          "COUNT(*) FILTER ( where \"Status of Observation:\" = 'Accepted by PM') AS totalPm " +
                                          "FROM qaqc_observation";

                    var staticReport = (await connection?.QueryAsync<EntQaQcObservationStaticReport>(staticReportSql)).ToList();

                    result["staticReport"] = staticReport[0];
                }
            }
            catch (Exception ce)
            {
                return Ok(new
                {
                    staticReport = result["staticReport"]
                }, message: $"{ce.Message}");
            }

            return Ok(new
            {
                staticReport = result["staticReport"]
            });
        }

        public async Task<Dictionary<string, object>> QaQcGetViolationDataSub(string whereSql)
        {
            var result = new Dictionary<string, object>();
            using var connection = _workerDapper.CreateConnection();
            var violationSql =
                "select subcontractor, count (*) as total,  sum(demerit_point) as demerit_point, sum(withholding_amount) as withholding_amount " +
                "from qaqc_violation " + whereSql + " group by subcontractor order by subcontractor";
            var violationList = (await connection.QueryAsync<EntQaQcViolation>(violationSql)).ToList();
            var listCountBySub = new List<Dictionary<string, object>>();
            var list_sub = new List<string>();
            var line_labels = new List<Dictionary<string, object>>();
            var iColor = 0;
            foreach (var e in violationList)
            {
                var violation = new Dictionary<string, object>();
                violation["name"] = e.subcontractor;
                if (violation["name"] != null)
                {
                    violation["short_name"] = getShortNameOfTrade(e.subcontractor);
                }
                else
                {
                    violation["short_name"] = "NA";
                }

                violation["total"] = e.total;
                violation["demerit_point"] = e.demeritPoint;
                violation["withholding_amount"] = e.withholdingAmount;

                var dataByProjectSql = "select project, count(*) from qaqc_violation where subcontractor = '" +
                                       e.subcontractor + "' group by project";
                var projectByProject =
                    (await connection.QueryAsync<EntQaQcViolationByProject>(dataByProjectSql)).ToList();
                foreach (var f in projectByProject)
                {
                    var projectCountName = f.project + "_count";
                    violation[projectCountName] = f.total;

                    var projectDemeritName = f.project + "_demerit_point";
                    violation[projectDemeritName] = f.demerit_point;

                    var projectWithholdingAmountName = f.project + "_withholding_amount";
                    violation[projectWithholdingAmountName] = f.withholding_amount;
                }

                if (e.total > 0)
                {
                    listCountBySub.Add(violation);
                }

                list_sub.Add(e.subcontractor);
                var lineBaseTrade = new Dictionary<string, object>();
                lineBaseTrade["key"] = e.subcontractor;
                try
                {
                    lineBaseTrade["color"] = CommoncheckColor.getColor(iColor);
                }
                catch (Exception exception)
                {
                    lineBaseTrade["color"] = "Red";
                }

                // lineBaseTrade["color"] = CommoncheckColor.getColor(iColor);
                iColor++;
                line_labels.Add(lineBaseTrade);
            }

            result["countItems"] = listCountBySub;
            result["listSub"] = list_sub;
            result["lineLabels"] = line_labels;
            try
            {
                result["subSelected"] = list_sub[0];
            }
            catch (Exception e)
            {
                result["subSelected"] = "";
            }

            return result;
        }

        public async Task<Dictionary<string, object>> QaQcGetViolationDataDate(string whereSql)
        {
            var result = new Dictionary<string, object>();
            using var connection = _workerDapper.CreateConnection();
            var subcontractorByDateSql =
                "SELECT sum(demerit_point) as demerit_point,sum(withholding_amount) as withholding_amount,  to_char( created_at, 'YYYYMM') as yyyymm, to_char( created_at, 'MM/YY') " +
                "as mmyy, count(*) as total from qaqc_violation " + whereSql +
                " group by yyyymm, mmyy order by yyyymm;";
            var subcontractorByDateList =
                (await connection.QueryAsync<EntQaQcViolationDate>(subcontractorByDateSql)).ToList();
            var groupByDate = "SELECT \"project\"  " +
                              "FROM qaqc_violation " +
                              "GROUP BY \"project\"; ";
            var listSubcontractorByDate = new List<Dictionary<string, object>>();
            foreach (var e in subcontractorByDateList)
            {
                var subcontractorByDate = new Dictionary<string, object>();
                subcontractorByDate["name"] = e.mmyy;
                subcontractorByDate["All"] = e.total;
                subcontractorByDate["demerit_point"] = e.demerit_point;
                subcontractorByDate["withholding_amount"] = e.withholding_amount;
                var dataBySubcontractorSql =
                    "select subcontractor, count(*) as total, sum(demerit_point) as demerit_point, sum(withholding_amount) as withholding_amount" +
                    " from (select *, to_char( created_at, 'YYYYMM') as " +
                    "YYYYMM, to_char( created_at, 'MMYY') as MMYY from qaqc_violation) AS YM where YM.MMYY = '" +
                    e.mmyy +
                    "' GROUP BY subcontractor";
                var dataBySubcontractor =
                    (await connection.QueryAsync<EntQaQcViolationBySubcontractor>(dataBySubcontractorSql)).ToList();
                foreach (var f in dataBySubcontractor)
                {
                    var keyCount = f.subcontractor + "_count";
                    subcontractorByDate[keyCount] = f.total;
                    var keyDemerit = f.subcontractor + "_demerit";
                    subcontractorByDate[keyDemerit] = f.demerit_point;
                    var keyWithholding = f.subcontractor + "_withholding";
                    subcontractorByDate[keyWithholding] = f.withholding_amount;
                }

                listSubcontractorByDate.Add(subcontractorByDate);
            }

            result["countByMonth"] = listSubcontractorByDate;
            return result;
        }

        /// <summary>
        /// Function get data summary critical and common in dashboard
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (24.02.2023)
        public async Task<ServiceResponse> QaQcGetSummaryDataDashboardCritical(SummaryRequest request)
        {
            try
            {
                // init connection
                using var connectionAppMain = _appMainDapper.CreateConnection();
                using var connection = _workerDapper.CreateConnection();
                DateTime now = DateTime.Now;

                // get all project from table projects
                var lstProjectRequest = request.listProject.ToList<string>();
                string lstProjectName = string.Join("','", request.listProject);


                var projectsData = (List<Projects>)await connectionAppMain.QueryAsync<Projects>($"SELECT * FROM projects AS pr WHERE pr.type = '{request?.typeProject}' OR pr.type = 'PreCast'");
                if (request.listProject.Length <= 0)
                {
                    lstProjectName = projectsData.Select(p => p.Name).ToList()?.Count > 0 ? string.Join("','", projectsData.Select(p => p.Name).ToList()) : string.Empty;
                }
                else
                {
                    var lstProjectFilter = lstProjectRequest.Where(project => projectsData.Select(x => x.Name).ToList<string>().Contains(project)).ToList<string>();
                    lstProjectName = string.Join("','", lstProjectFilter);
                }

                // get all critical and common check
                string criticalCheckSqlFirst = string.Empty,
                       criticalCheckSqlSecond = string.Empty,
                       whereSqlFirst = "",
                       whereSqlSecond = "";

                // check monthly
                if (request?.typeKPR == "Monthly")
                {
                    // get current monthly set start date to current date
                    var thisMonthStart = new DateTime(now.Year, now.Month, 1);
                    var thisMonthEnd = thisMonthStart.AddMonths(1).AddDays(-1);


                    whereSqlFirst += "and created_at >= '" + thisMonthStart.ToString() + "' ";
                    whereSqlFirst += "and created_at <= '" + thisMonthEnd.ToString() + "' ";

                    // get previous monthly set start date to current date
                    var lastMonthStart = thisMonthStart.AddMonths(-1);
                    var lastMonthEnd = lastMonthStart.AddMonths(1).AddDays(-1);

                    whereSqlSecond += "and created_at >= '" + lastMonthStart.ToString() + "' ";
                    whereSqlSecond += "and created_at <= '" + lastMonthEnd.ToString() + "' ";
                }
                else
                {
                    if (request?.typeKPR == "Cumulative")
                    {
                        // get current monthly set start date to current date
                        var thisMonthStart = new DateTime(now.Year, now.Month, 1);
                        var thisMonthEnd = thisMonthStart.AddMonths(1).AddDays(-1);


                        whereSqlFirst += "and created_at >= '" + thisMonthStart.ToString() + "' ";
                        whereSqlFirst += "and created_at <= '" + thisMonthEnd.ToString() + "' ";

                        // get previous monthly set start date to current date
                        var lastMonthStart = thisMonthStart.AddMonths(-1);
                        var lastMonthEnd = lastMonthStart.AddMonths(1).AddDays(-1);

                        // whereSqlSecond += "and created_at >= '" + lastMonthStart.ToString() + "' ";
                        whereSqlSecond += "and created_at <= '" + thisMonthStart.ToString() + "' ";
                    }
                }

                if (request.lteDate != null && request.lteDate != "")
                {
                    whereSqlFirst += "and created_at >= '" + request.lteDate + "' ";
                }

                if (request.gteDate != null && request.gteDate != "")
                {
                    whereSqlFirst += "and created_at <= '" + request.gteDate + "' ";
                }

                // filter project follow project type
                if (!string.IsNullOrEmpty(lstProjectName))
                {
                    whereSqlFirst += $"and project IN ('{lstProjectName}')";
                    whereSqlSecond += $"and project IN ('{lstProjectName}')";
                }

                if (whereSqlFirst != "")
                {
                    whereSqlFirst = "where " + whereSqlFirst.Substring(3);
                }
                if (whereSqlSecond != "")
                {
                    whereSqlSecond = "where " + whereSqlSecond.Substring(3);
                }

                criticalCheckSqlFirst = "select project, sum(no_of_no) as total_no, sum(no_of_1st) as total_1st," +
                               " sum(no_of_1plus) as total_1plus,sum(no_of_no + no_of_1st + no_of_1plus)" +
                               " as total from qaqc_critical_check " + whereSqlFirst + " group by project order by  project;";
                criticalCheckSqlSecond = "select project, sum(no_of_no) as total_no, sum(no_of_1st) as total_1st," +
                               " sum(no_of_1plus) as total_1plus,sum(no_of_no + no_of_1st + no_of_1plus)" +
                               " as total from qaqc_critical_check " + whereSqlSecond + " group by project order by  project;";

                // query data
                var criticalCheckFirst = QaQcGetSummaryCritical(criticalCheckSqlFirst).Result;
                var criticalCheckSecond = QaQcGetSummaryCritical(criticalCheckSqlSecond).Result;

                return Ok(new
                {
                    criticalCheckFirst = criticalCheckFirst["criticalCheck"],
                    criticalCheckSecond = criticalCheckSecond["criticalCheck"],
                });
            }
            catch (Exception ce)
            {
                Console.WriteLine(ce.Message);
                return BadRequest(ce.Message);
            }
        }

        /// <summary>
        /// Function get data summary critical and common in dashboard
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (24.02.2023)
        public async Task<ServiceResponse> QaQcGetSummaryDataDashboardCommon(SummaryRequest request)
        {
            try
            {
                // init connection
                using var connectionAppMain = _appMainDapper.CreateConnection();
                using var connection = _workerDapper.CreateConnection();
                DateTime now = DateTime.Now;

                // get all project from table projects
                var lstProjectRequest = request.listProject.ToList<string>();
                string lstProjectName = string.Join("','", request.listProject);


                var projectsData = (List<Projects>)await connectionAppMain.QueryAsync<Projects>($"SELECT * FROM projects AS pr WHERE pr.type = '{request?.typeProject}' OR pr.type = 'PreCast'");
                if (request.listProject.Length <= 0)
                {
                    lstProjectName = projectsData.Select(p => p.Name).ToList()?.Count > 0 ? string.Join("','", projectsData.Select(p => p.Name).ToList()) : string.Empty;
                }
                else
                {
                    var lstProjectFilter = lstProjectRequest.Where(project => projectsData.Select(x => x.Name).ToList<string>().Contains(project)).ToList<string>();
                    lstProjectName = string.Join("','", lstProjectFilter);
                }

                // get all critical and common check
                string commonCheckSqlFirst = string.Empty,
                       commonCheckSqlSecond = string.Empty,
                       whereSqlFirst = "",
                       whereSqlSecond = "";

                // check monthly
                if (request?.typeKPR == "Monthly")
                {
                    // get current monthly set start date to current date
                    var thisMonthStart = new DateTime(now.Year, now.Month, 1);
                    var thisMonthEnd = thisMonthStart.AddMonths(1).AddDays(-1);


                    whereSqlFirst += "and created_at >= '" + thisMonthStart.ToString() + "' ";
                    whereSqlFirst += "and created_at <= '" + thisMonthEnd.ToString() + "' ";

                    // get previous monthly set start date to current date
                    var lastMonthStart = thisMonthStart.AddMonths(-1);
                    var lastMonthEnd = lastMonthStart.AddMonths(1).AddDays(-1);

                    whereSqlSecond += "and created_at >= '" + lastMonthStart.ToString() + "' ";
                    whereSqlSecond += "and created_at <= '" + lastMonthEnd.ToString() + "' ";
                }
                else
                {
                    if (request?.typeKPR == "Cumulative")
                    {
                        // get current monthly set start date to current date
                        var thisMonthStart = new DateTime(now.Year, now.Month, 1);
                        var thisMonthEnd = thisMonthStart.AddMonths(1).AddDays(-1);


                        whereSqlFirst += "and created_at >= '" + thisMonthStart.ToString() + "' ";
                        whereSqlFirst += "and created_at <= '" + thisMonthEnd.ToString() + "' ";

                        // get previous monthly set start date to current date
                        var lastMonthStart = thisMonthStart.AddMonths(-1);
                        var lastMonthEnd = lastMonthStart.AddMonths(1).AddDays(-1);

                        // whereSqlSecond += "and created_at >= '" + lastMonthStart.ToString() + "' ";
                        whereSqlSecond += "and created_at <= '" + thisMonthStart.ToString() + "' ";
                    }
                }

                if (request.lteDate != null && request.lteDate != "")
                {
                    whereSqlFirst += "and created_at >= '" + request.lteDate + "' ";
                }

                if (request.gteDate != null && request.gteDate != "")
                {
                    whereSqlFirst += "and created_at <= '" + request.gteDate + "' ";
                }

                // filter project follow project type
                if (!string.IsNullOrEmpty(lstProjectName))
                {
                    whereSqlFirst += $"and project IN ('{lstProjectName}')";
                    whereSqlSecond += $"and project IN ('{lstProjectName}')";
                }

                if (whereSqlFirst != "")
                {
                    whereSqlFirst = "where " + whereSqlFirst.Substring(3);
                }
                if (whereSqlSecond != "")
                {
                    whereSqlSecond = "where " + whereSqlSecond.Substring(3);
                }

                commonCheckSqlFirst = "select project, sum(no_of_no) as total_no, sum(no_of_yes) as total_yes, " +
                             "sum(no_of_no + no_of_yes) as total from qaqc_common_check " + whereSqlFirst +
                             " group by project order by  project;";
                commonCheckSqlSecond = "select project, sum(no_of_no) as total_no, sum(no_of_yes) as total_yes, " +
                             "sum(no_of_no + no_of_yes) as total from qaqc_common_check " + whereSqlSecond +
                             " group by project order by  project;";

                // query data
                var commonCheckFirst = QaQcGetSummaryCommon(commonCheckSqlFirst).Result;
                var commonCheckSecond = QaQcGetSummaryCommon(commonCheckSqlSecond).Result;

                return Ok(new
                {
                    commonCheckFirst = commonCheckFirst["commonCheck"],
                    commonCheckSecond = commonCheckSecond["commonCheck"],
                });
            }
            catch (Exception ce)
            {
                Console.WriteLine(ce.Message);
                return BadRequest(ce.Message);
            }
        }


        /// <summary>
        /// Function get data summary critical and common in dashboard
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (24.02.2023)
        public async Task<ServiceResponse> QaQcGetSummaryDataDashboardCombine(SummaryRequest request)
        {
            try
            {
                // init connection
                using var connectionAppMain = _appMainDapper.CreateConnection();
                using var connection = _workerDapper.CreateConnection();
                DateTime now = DateTime.Now;

                // get all project from table projects
                var lstProjectRequest = request.listProject.ToList<string>();
                string lstProjectName = string.Join("','", request.listProject);


                var projectsData = (List<Projects>)await connectionAppMain.QueryAsync<Projects>($"SELECT * FROM projects AS pr WHERE pr.type = '{request?.typeProject}' OR pr.type = 'PreCast'");
                if (request.listProject.Length <= 0)
                {
                    lstProjectName = projectsData.Select(p => p.Name).ToList()?.Count > 0 ? string.Join("','", projectsData.Select(p => p.Name).ToList()) : string.Empty;
                }
                else
                {
                    var lstProjectFilter = lstProjectRequest.Where(project => projectsData.Select(x => x.Name).ToList<string>().Contains(project)).ToList<string>();
                    lstProjectName = string.Join("','", lstProjectFilter);
                }

                // get all critical and common check
                string criticalCheckSqlFirst = string.Empty,
                       criticalCheckSqlSecond = string.Empty,
                       commonCheckSqlFirst = string.Empty,
                       commonCheckSqlSecond = string.Empty,
                       whereSqlFirst = "",
                       whereSqlSecond = "";

                // check monthly
                if (request?.typeKPR == "Monthly")
                {
                    // get current monthly set start date to current date
                    var thisMonthStart = new DateTime(now.Year, now.Month, 1);
                    var thisMonthEnd = thisMonthStart.AddMonths(1).AddDays(-1);


                    whereSqlFirst += "and created_at >= '" + thisMonthStart.ToString() + "' ";
                    whereSqlFirst += "and created_at <= '" + thisMonthEnd.ToString() + "' ";

                    // get previous monthly set start date to current date
                    var lastMonthStart = thisMonthStart.AddMonths(-1);
                    var lastMonthEnd = lastMonthStart.AddMonths(1).AddDays(-1);

                    whereSqlSecond += "and created_at >= '" + lastMonthStart.ToString() + "' ";
                    whereSqlSecond += "and created_at <= '" + lastMonthEnd.ToString() + "' ";
                }
                else
                {
                    if (request?.typeKPR == "Cumulative")
                    {
                        // get current monthly set start date to current date
                        var thisMonthStart = new DateTime(now.Year, now.Month, 1);
                        var thisMonthEnd = thisMonthStart.AddMonths(1).AddDays(-1);


                        whereSqlFirst += "and created_at >= '" + thisMonthStart.ToString() + "' ";
                        whereSqlFirst += "and created_at <= '" + thisMonthEnd.ToString() + "' ";

                        // get previous monthly set start date to current date
                        var lastMonthStart = thisMonthStart.AddMonths(-1);
                        var lastMonthEnd = lastMonthStart.AddMonths(1).AddDays(-1);

                        // whereSqlSecond += "and created_at >= '" + lastMonthStart.ToString() + "' ";
                        whereSqlSecond += "and created_at <= '" + thisMonthStart.ToString() + "' ";
                    }
                }

                if (request.lteDate != null && request.lteDate != "")
                {
                    whereSqlFirst += "and created_at >= '" + request.lteDate + "' ";
                }

                if (request.gteDate != null && request.gteDate != "")
                {
                    whereSqlFirst += "and created_at <= '" + request.gteDate + "' ";
                }

                // filter project follow project type
                if (string.IsNullOrEmpty(lstProjectName))
                {
                    lstProjectName = "NULL";
                }

                // filter project follow project type
                if (!string.IsNullOrEmpty(lstProjectName))
                {
                    whereSqlFirst += $"and project IN ('{lstProjectName}')";
                    whereSqlSecond += $"and project IN ('{lstProjectName}')";
                }

                if (whereSqlFirst != "")
                {
                    whereSqlFirst = "where " + whereSqlFirst.Substring(3);
                }
                if (whereSqlSecond != "")
                {
                    whereSqlSecond = "where " + whereSqlSecond.Substring(3);
                }

                // test
                criticalCheckSqlFirst = "select project, sum(no_of_no) as total_no, sum(no_of_1st) as total_1st," +
                               " sum(no_of_1plus) as total_1plus,sum(no_of_no + no_of_1st + no_of_1plus)" +
                               " as total from qaqc_critical_check " + whereSqlFirst + " group by project order by  project;";
                criticalCheckSqlSecond = "select project, sum(no_of_no) as total_no, sum(no_of_1st) as total_1st," +
                               " sum(no_of_1plus) as total_1plus,sum(no_of_no + no_of_1st + no_of_1plus)" +
                               " as total from qaqc_critical_check " + whereSqlSecond + " group by project order by  project;";

                commonCheckSqlFirst = "select project, sum(no_of_no) as total_no, sum(no_of_yes) as total_yes, " +
                             "sum(no_of_no + no_of_yes) as total from qaqc_common_check " + whereSqlFirst +
                             " group by project order by  project;";
                commonCheckSqlSecond = "select project, sum(no_of_no) as total_no, sum(no_of_yes) as total_yes, " +
                             "sum(no_of_no + no_of_yes) as total from qaqc_common_check " + whereSqlSecond +
                             " group by project order by  project;";

                // query data critical
                var criticalCheckFirst = QaQcGetSummaryCritical(criticalCheckSqlFirst).Result;
                var criticalCheckSecond = QaQcGetSummaryCritical(criticalCheckSqlSecond).Result;

                // query data common
                var commonCheckFirst = QaQcGetSummaryCommon(commonCheckSqlFirst).Result;
                var commonCheckSecond = QaQcGetSummaryCommon(commonCheckSqlSecond).Result;

                return Ok(new
                {
                    criticalCheckFirst = criticalCheckFirst["criticalCheck"],
                    criticalCheckSecond = criticalCheckSecond["criticalCheck"],

                    commonCheckFirst = commonCheckFirst["commonCheck"],
                    commonCheckSecond = commonCheckSecond["commonCheck"]
                });
            }
            catch (Exception ce)
            {
                Console.WriteLine(ce.Message);
                return BadRequest(ce.Message);
            }
        }

        /// <summary>
        /// Function get data summary quality kpr
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (06.03.2023)
        public async Task<ServiceResponse> QaQcGetSummaryDataDashboardQualityKPR(SummaryRequest request)
        {
            try
            {
                // init connection
                using var connectionAppMain = _appMainDapper.CreateConnection();
                using var connection = _workerDapper.CreateConnection();
                DateTime now = DateTime.Now;

                // get all project from table projects
                var lstProjectRequest = request.listProject.ToList<string>();
                string lstProjectName = string.Join("','", request.listProject);

                var projectsData = (List<Projects>)await connectionAppMain.QueryAsync<Projects>($"SELECT * FROM projects AS pr WHERE pr.type = '{request?.typeProject}' OR pr.type = 'PreCast'");
                if (request.listProject.Length <= 0)
                {
                    lstProjectName = projectsData.Select(p => p.Name).ToList()?.Count > 0 ? string.Join("','", projectsData.Select(p => p.Name).ToList()) : string.Empty;
                }
                else
                {
                    var lstProjectFilter = lstProjectRequest.Where(project => projectsData.Select(x => x.Name).ToList<string>().Contains(project)).ToList<string>();
                    lstProjectName = string.Join("','", lstProjectFilter);
                }

                // get all critical and common check
                string pqaFirst = string.Empty,
                       pqaSecond = string.Empty,
                       iqaFirst = string.Empty,
                       iqaSecond = string.Empty,
                       wherePQAFirst = "",
                       wherePQASecond = "",
                       whereIQAFirst = "",
                       whereIQASecond = "";

                // check monthly
                if (request?.typeKPR == "Monthly")
                {
                    // get current monthly set start date to current date
                    var thisMonthStart = new DateTime(now.Year, now.Month, 1);
                    var thisMonthEnd = thisMonthStart.AddMonths(1).AddDays(-1);

                    whereIQAFirst += $"and created_at >= '{thisMonthStart.ToString()}' and created_at <= '{thisMonthEnd.ToString()}' ";
                    wherePQAFirst += $"and last_update >= '{thisMonthStart.ToString()}' and last_update <= '{thisMonthEnd.ToString()}' ";

                    // get previous monthly set start date to current date
                    var lastMonthStart = thisMonthStart.AddMonths(-1);
                    var lastMonthEnd = lastMonthStart.AddMonths(1).AddDays(-1);

                    whereIQASecond += $"and created_at >= '{lastMonthStart.ToString()}' and created_at <= '{lastMonthEnd.ToString()}' ";
                    wherePQASecond += $"and last_update >= '{lastMonthStart.ToString()}' and last_update <= '{lastMonthEnd.ToString()}' ";
                }
                else
                {
                    if (request?.typeKPR == "Cumulative")
                    {
                        // get current monthly set start date to current date
                        var thisMonthStart = new DateTime(now.Year, now.Month, 1);
                        var thisMonthEnd = thisMonthStart.AddMonths(1).AddDays(-1);

                        whereIQAFirst += $"and created_at >= '{thisMonthStart.ToString()}' and created_at <= '{thisMonthEnd.ToString()}' ";
                        wherePQAFirst += $"and last_update >= '{thisMonthStart.ToString()}' and last_update <= '{thisMonthEnd.ToString()}' ";

                        // get previous monthly set start date to current date
                        var lastMonthStart = thisMonthStart.AddMonths(-1);
                        var lastMonthEnd = lastMonthStart.AddMonths(1).AddDays(-1);

                        whereIQASecond += "and created_at <= '" + thisMonthStart.ToString() + "' ";
                        wherePQASecond += "and last_update <= '" + thisMonthStart.ToString() + "' ";
                    }
                }

                if (request.lteDate != null && request.lteDate != "")
                {
                    whereIQAFirst += "and created_at >= '" + request.lteDate + "' ";
                    wherePQAFirst += "and last_update >= '" + request.lteDate + "' ";
                }

                if (request.gteDate != null && request.gteDate != "")
                {
                    whereIQASecond += "and created_at <= '" + request.gteDate + "' ";
                    wherePQASecond += "and last_update <= '" + request.gteDate + "' ";
                }

                // filter project follow project type
                if (string.IsNullOrEmpty(lstProjectName))
                {
                    lstProjectName = "NULL";

                }

                string projectQuery = $"and project_actual IN ('{lstProjectName}') ";

                whereIQAFirst += projectQuery;
                whereIQASecond += projectQuery;

                projectQuery = $"and project_actual IN ('{lstProjectName}') ";

                wherePQAFirst += projectQuery;
                wherePQASecond += projectQuery;


                if (whereIQAFirst != "")
                {
                    whereIQAFirst = "where " + whereIQAFirst.Substring(3);
                }
                if (wherePQAFirst != "")
                {
                    wherePQAFirst = "where " + wherePQAFirst.Substring(3);
                }

                if (whereIQASecond != "")
                {
                    whereIQASecond = "where " + whereIQASecond.Substring(3);
                }
                if (wherePQASecond != "")
                {
                    wherePQASecond = "where " + wherePQASecond.Substring(3);
                }

                pqaFirst = $"SELECT pqa.PROJECT_ACTUAL_ACTUAL, COUNT(pqa.id_report) AS total_project, SUM(pqa.overall_score) AS sum_overall_score, (SUM(pqa.overall_score) / COUNT(pqa.id)) AS score_avg " +
                    $"FROM pqa_reportdata_entity AS pqa {wherePQAFirst} GROUP BY pqa.PROJECT_ACTUAL_ACTUAL ORDER BY project;";
                pqaSecond = $"SELECT pqa.PROJECT_ACTUAL_ACTUAL, COUNT(pqa.id_report) AS total_project, SUM(pqa.overall_score) AS sum_overall_score, (SUM(pqa.overall_score) / COUNT(pqa.id)) AS score_avg " +
                    $"FROM pqa_reportdata_entity AS pqa {wherePQASecond} GROUP BY pqa.PROJECT_ACTUAL_ACTUAL ORDER BY project;";

                iqaFirst = $"SELECT iqa.PROJECT_ACTUAL, COUNT(iqa.iqa_no) AS total_project, SUM(iqa.iqa_score) AS sum_overall_score, (SUM(iqa.iqa_score) / COUNT(iqa.id)) AS score_avg " +
                    $"FROM iqa_schedule_entity AS iqa {whereIQAFirst}" +
                    $"GROUP BY iqa.PROJECT_ACTUAL ORDER BY iqa.PROJECT_ACTUAL;";

                iqaSecond = $"SELECT iqa.PROJECT_ACTUAL, COUNT(iqa.iqa_no) AS total_project, SUM(iqa.iqa_score) AS sum_overall_score, (SUM(iqa.iqa_score) / COUNT(iqa.id)) AS score_avg " +
                    $"FROM iqa_schedule_entity AS iqa {whereIQASecond}" +
                    $"GROUP BY iqa.PROJECT_ACTUAL ORDER BY iqa.PROJECT_ACTUAL;";

                // query data critical
                var pqaDataFirst = QaQcPQASummary(pqaFirst).Result;
                var pqaDataSecond = QaQcPQASummary(pqaSecond).Result;

                // query data common
                var iqaDataFirst = QaQcIQASummary(iqaFirst).Result;
                var iqaDataSecond = QaQcIQASummary(iqaSecond).Result;

                return Ok(new
                {
                    pqaFirst = pqaDataFirst["pqaData"],
                    pqaSecond = pqaDataSecond["pqaData"],

                    iqaFirst = iqaDataFirst["iqaData"],
                    iqaSecond = iqaDataSecond["iqaData"]
                });
            }
            catch (Exception ce)
            {
                Console.WriteLine(ce.Message);
                return BadRequest(ce.Message);
            }
        }

        /// <summary>
        /// Build query pqa ipa
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (10.03.2023)
        public List<string> BuildQueryPQAIQA(SummaryRequest request)
        {
            DateTime now = DateTime.Now;
            string pqaFirst = string.Empty,
                    pqaSecond = string.Empty,
                    iqaFirst = string.Empty,
                    iqaSecond = string.Empty,
                    wherePQAFirst = "",
                    wherePQASecond = "",
                    whereIQAFirst = "",
                    whereIQASecond = "";

            // get current monthly set start date to current date
            var thisMonthStart = new DateTime(now.Year, now.Month, 1);
            var thisMonthEnd = thisMonthStart.AddMonths(1).AddDays(-1);

            whereIQAFirst += $"and created_at >= '{thisMonthStart.ToString()}' and created_at <= '{thisMonthEnd.ToString()}' ";
            wherePQAFirst += $"and last_update >= '{thisMonthStart.ToString()}' and last_update <= '{thisMonthEnd.ToString()}' ";

            // check monthly
            // get previous monthly set start date to current date
            var lastMonthStart = thisMonthStart.AddMonths(-1);
            var lastMonthEnd = lastMonthStart.AddMonths(1).AddDays(-1);

            if (request.gteDate != null && request.lteDate != null)
            {
                whereIQASecond += $"and created_at >= '{lastMonthStart.ToString()}' and created_at <= '{lastMonthEnd.ToString()}'";
                wherePQASecond += $"and last_update >= '{lastMonthStart.ToString()}' and last_update <= '{lastMonthEnd.ToString()}'";
            }
            else
            {
                if (request.gteDate == null && request.lteDate == null)
                {
                    whereIQASecond += $"and created_at < '{thisMonthStart.ToString()}'";
                    wherePQASecond += $"and last_update < '{thisMonthStart.ToString()}'";
                }

                if (request.gteDate != null)
                {
                    whereIQASecond += "and created_at >= '" + request.gteDate + "' ";
                    wherePQASecond += "and last_update >= '" + request.gteDate + "' ";
                }

                if (request.lteDate != null)
                {
                    whereIQASecond += "and created_at <= '" + request.lteDate + "' ";
                    wherePQASecond += "and last_update <= '" + request.lteDate + "' ";
                }
            }

            string projectQueryIQA = $"and project_name = '{request.projectName}' ";
            string projectQueryPQA = $"and project = '{request.projectName}' ";

            whereIQAFirst += projectQueryIQA;
            whereIQASecond += projectQueryIQA;

            wherePQAFirst += projectQueryPQA;
            wherePQASecond += projectQueryPQA;


            if (whereIQAFirst != "")
            {
                whereIQAFirst = "where " + whereIQAFirst.Substring(3);
            }
            if (wherePQAFirst != "")
            {
                wherePQAFirst = "where " + wherePQAFirst.Substring(3);
            }

            if (whereIQASecond != "")
            {
                whereIQASecond = "where " + whereIQASecond.Substring(3);
            }
            if (wherePQASecond != "")
            {
                wherePQASecond = "where " + wherePQASecond.Substring(3);
            }

            pqaFirst = $"SELECT pqa.PROJECT_ACTUAL_ACTUAL, COUNT(pqa.id_report) AS total_project, SUM(pqa.overall_score) AS sum_overall_score, (SUM(pqa.overall_score) / COUNT(pqa.id)) AS score_avg " +
                $"FROM pqa_reportdata_entity AS pqa {wherePQAFirst} GROUP BY pqa.PROJECT_ACTUAL_ACTUAL ORDER BY project;";
            pqaSecond = $"SELECT pqa.PROJECT_ACTUAL_ACTUAL, COUNT(pqa.id_report) AS total_project, SUM(pqa.overall_score) AS sum_overall_score, (SUM(pqa.overall_score) / COUNT(pqa.id)) AS score_avg " +
                $"FROM pqa_reportdata_entity AS pqa {wherePQASecond} GROUP BY pqa.PROJECT_ACTUAL_ACTUAL ORDER BY project;";

            iqaFirst = $"SELECT iqa.PROJECT_ACTUAL, COUNT(iqa.iqa_no) AS total_project, SUM(iqa.iqa_score) AS sum_overall_score, (SUM(iqa.iqa_score) / COUNT(iqa.id)) AS score_avg " +
                $"FROM iqa_schedule_entity AS iqa {whereIQAFirst}" +
                $"GROUP BY iqa.PROJECT_ACTUAL ORDER BY iqa.PROJECT_ACTUAL;";
            iqaSecond = $"SELECT iqa.PROJECT_ACTUAL, COUNT(iqa.iqa_no) AS total_project, SUM(iqa.iqa_score) AS sum_overall_score, (SUM(iqa.iqa_score) / COUNT(iqa.id)) AS score_avg " +
                $"FROM iqa_schedule_entity AS iqa {whereIQASecond}" +
                $"GROUP BY iqa.PROJECT_ACTUAL ORDER BY iqa.PROJECT_ACTUAL;";


            return new List<string> { pqaFirst, pqaSecond, iqaFirst, iqaSecond };
        }

        /// <summary>
        /// Build query critical common
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (10.03.2023)
        public List<string> BuildQueryCriticalCommon(SummaryRequest request)
        {
            DateTime now = DateTime.Now;

            // get all critical and common check
            string criticalCheckSqlFirst = string.Empty,
                   criticalCheckSqlSecond = string.Empty,
                   commonCheckSqlFirst = string.Empty,
                   commonCheckSqlSecond = string.Empty,
                   whereSqlFirst = "",
                   whereSqlSecond = "";

            // get current monthly set start date to current date
            var thisMonthStart = new DateTime(now.Year, now.Month, 1);
            var thisMonthEnd = thisMonthStart.AddMonths(1).AddDays(-1);

            whereSqlFirst += $"and created_at >= '{thisMonthStart.ToString()}' and created_at <= '{thisMonthEnd.ToString()}' ";

            // check monthly
            // get previous monthly set start date to current date
            var lastMonthStart = thisMonthStart.AddMonths(-1);
            var lastMonthEnd = lastMonthStart.AddMonths(1).AddDays(-1);

            if (request.gteDate != null && request.lteDate != null)
            {
                whereSqlSecond += $"and created_at >= '{lastMonthStart.ToString()}' and created_at <= '{lastMonthEnd.ToString()}'";
            }
            else
            {
                if (request.gteDate == null && request.lteDate == null)
                {
                    whereSqlSecond += $"and created_at < '{thisMonthStart.ToString()}'";
                }

                if (request.gteDate != null)
                {
                    whereSqlSecond += "and created_at >= '" + request.gteDate + "' ";
                }

                if (request.lteDate != null)
                {
                    whereSqlSecond += "and created_at <= '" + request.lteDate + "' ";
                }
            }



            whereSqlFirst += $"and project = '{request.projectName}'";
            whereSqlSecond += $"and project = '{request.projectName}'";

            if (whereSqlFirst != "")
            {
                whereSqlFirst = "where " + whereSqlFirst.Substring(3);
            }
            if (whereSqlSecond != "")
            {
                whereSqlSecond = "where " + whereSqlSecond.Substring(3);
            }

            // test
            criticalCheckSqlFirst = "select project, sum(no_of_no) as total_no, sum(no_of_1st) as total_1st," +
                           " sum(no_of_1plus) as total_1plus,sum(no_of_no + no_of_1st + no_of_1plus)" +
                           " as total from qaqc_critical_check " + whereSqlFirst + " group by project order by  project;";
            criticalCheckSqlSecond = "select project, sum(no_of_no) as total_no, sum(no_of_1st) as total_1st," +
                           " sum(no_of_1plus) as total_1plus,sum(no_of_no + no_of_1st + no_of_1plus)" +
                           " as total from qaqc_critical_check " + whereSqlSecond + " group by project order by  project;";

            commonCheckSqlFirst = "select project, sum(no_of_no) as total_no, sum(no_of_yes) as total_yes, " +
                         "sum(no_of_no + no_of_yes) as total from qaqc_common_check " + whereSqlFirst +
                         " group by project order by  project;";
            commonCheckSqlSecond = "select project, sum(no_of_no) as total_no, sum(no_of_yes) as total_yes, " +
                         "sum(no_of_no + no_of_yes) as total from qaqc_common_check " + whereSqlSecond +
                         " group by project order by  project;";

            return new List<string> { criticalCheckSqlFirst, criticalCheckSqlSecond, commonCheckSqlFirst, commonCheckSqlSecond };
        }

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
                for (int i = 1; i <= numberPreviousMonth; i++)
                {
                    var startOfMonth = currentMonth.AddMonths(-i);

                    int daysInMonth = DateTime.DaysInMonth(year: startOfMonth.Year, month: startOfMonth.Month);
                    var lastOfMonth = new DateTime(startOfMonth.Year, startOfMonth.Month, daysInMonth);

                    List<DateTime> temp = new List<DateTime>()
                    {
                        startOfMonth,
                        lastOfMonth
                    };

                    lstMonth.Add(startOfMonth.ToString("MMM", CultureInfo.InvariantCulture), temp);
                }
            }

            return lstMonth;
        }

        /// <summary>
        /// get previous month for report kpr from current month
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (15.03.2023)
        public async Task<ServiceResponse> QaQcGetDetailDataDashboardQualityKPRMonth(SummaryRequest request)
        {
            try
            {
                // init connection
                using var connectionAppMain = _appMainDapper.CreateConnection();
                using var connection = _workerDapper.CreateConnection();
                DateTime now = DateTime.Now;

                // get all pqa iqa
                string pqaFirst = string.Empty,
                        pqaSecond = string.Empty,
                        iqaFirst = string.Empty,
                        iqaSecond = string.Empty,
                        criticalCheckSqlFirst = string.Empty,
                        criticalCheckSqlSecond = string.Empty,
                        commonCheckSqlFirst = string.Empty,
                        commonCheckSqlSecond = string.Empty;

                // get list month need to get
                var lstMonth = GetListMonthNeed(request.numberMonthPrevious <= 12 ? request.numberMonthPrevious : 12);

                // get data pqa iqa, critical common by month and project
                var listQuery = ListDataQualityKPR(lstMonth, request.projectName);

                // query data
                Dictionary<string, Dictionary<string, object>> result = new Dictionary<string, Dictionary<string, object>>();

                foreach (var queryMonth in listQuery)
                {
                    // get data
                    var resultPQAIQA = PQAIQAQuery(queryMonth.Value["queryPQA"].ToString(), queryMonth.Value["queryIQA"].ToString()).Result;

                    // get data
                    var resultCriticalCommon = CriticalCommonQuery(queryMonth.Value["queryCritical"].ToString(), queryMonth.Value["queryCommon"].ToString()).Result;

                    result.Add(queryMonth.Key, new Dictionary<string, object>(){
                        {"PQAIQA", resultPQAIQA},
                        {"CriticalCommon", resultCriticalCommon}
                    });
                }

                return Ok(new
                {
                    result
                });
            }
            catch (Exception ce)
            {
                Console.WriteLine(ce.Message);
                return BadRequest(ce.Message);
            }
        }

        /// <summary>
        /// Function get data summary quality kpr
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (10.03.2023)
        public Dictionary<string, Dictionary<string, object>> ListDataQualityKPR(Dictionary<string, List<DateTime>> dateTimes, string projectName)
        {
            Dictionary<string, Dictionary<string, object>> listQuery = new Dictionary<string, Dictionary<string, object>>();

            try
            {
                foreach (var month in dateTimes)
                {
                    Dictionary<string, object> monthQuery = new Dictionary<string, object>();

                    string wherePQA = $"WHERE PROJECT_ACTUAL = '{projectName}'",
                        whereIQA = $"WHERE PROJECT_ACTUAL = '{projectName}'",
                        whereCritical = $"WHERE project = '{projectName}'",
                        whereCommon = $"WHERE project = '{projectName}'";

                    if (string.IsNullOrEmpty(projectName))
                    {
                        wherePQA = whereIQA = "WHERE 1=1 ";
                        whereCritical = whereCommon = "WHERE 1=1 ";
                    }

                    // PQA IQA
                    wherePQA = $"{wherePQA} AND last_update >= '{month.Value[0]}'  AND last_update <= '{month.Value[1]}'";
                    whereIQA = $"{whereIQA} AND created_at >= '{month.Value[0]}'  AND created_at <= '{month.Value[1]}'";

                    // critical common
                    whereCritical = $"{whereCritical} AND created_at >= '{month.Value[0]}'  AND created_at <= '{month.Value[1]}'";
                    whereCommon = $"{whereCommon} AND created_at >= '{month.Value[0]}'  AND created_at <= '{month.Value[1]}'";

                    string queryPQA = $"SELECT pqa.PROJECT_ACTUAL, COUNT(pqa.id_report) AS total_project, SUM(pqa.overall_score) AS sum_overall_score, (SUM(pqa.overall_score) / COUNT(pqa.id)) AS score_avg " +
                        $"FROM pqa_reportdata_entity AS pqa {wherePQA} GROUP BY pqa.PROJECT_ACTUAL ORDER BY project;",
                        queryIQA = $"SELECT iqa.PROJECT_ACTUAL, COUNT(iqa.iqa_no) AS total_project, SUM(iqa.iqa_score) AS sum_overall_score, (SUM(iqa.iqa_score) / COUNT(iqa.id)) AS score_avg " +
                        $"FROM iqa_schedule_entity AS iqa {whereIQA} GROUP BY iqa.PROJECT_ACTUAL ORDER BY iqa.PROJECT_ACTUAL;",
                        queryCritical = $"select project, sum(no_of_no) as total_no, sum(no_of_1st) as total_1st, sum(no_of_1plus) as total_1plus,sum(no_of_no + no_of_1st + no_of_1plus) as total " +
                        $"from qaqc_critical_check {whereCritical} group by project order by  project;",
                        queryCommon = $"select project, sum(no_of_no) as total_no, sum(no_of_yes) as total_yes, sum(no_of_no + no_of_yes) as total " +
                        $"from qaqc_common_check {whereCommon} group by project order by  project;";

                    // add to dic
                    monthQuery.Add("queryPQA", queryPQA);
                    monthQuery.Add("queryIQA", queryIQA);
                    monthQuery.Add("queryCritical", queryCritical);
                    monthQuery.Add("queryCommon", queryCommon);

                    // push to list
                    listQuery.Add(month.Key, monthQuery);
                }
            }
            catch (Exception ce)
            {
                Console.WriteLine(ce.Message);
            }

            return listQuery;
        }

        /// <summary>
        /// Function get data summary quality kpr
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (10.03.2023)
        public async Task<ServiceResponse> QaQcGetDetailDataDashboardQualityKPR(SummaryRequest request)
        {
            try
            {
                // init connection
                using var connectionAppMain = _appMainDapper.CreateConnection();
                using var connection = _workerDapper.CreateConnection();
                DateTime now = DateTime.Now;

                // get all pqa iqa
                string pqaFirst = string.Empty,
                        pqaSecond = string.Empty,
                        iqaFirst = string.Empty,
                        iqaSecond = string.Empty,
                        criticalCheckSqlFirst = string.Empty,
                        criticalCheckSqlSecond = string.Empty,
                        commonCheckSqlFirst = string.Empty,
                        commonCheckSqlSecond = string.Empty;

                List<string> queryBuildPQAIQA = BuildQueryPQAIQA(request);

                pqaFirst = queryBuildPQAIQA[0];
                pqaSecond = queryBuildPQAIQA[1];
                iqaFirst = queryBuildPQAIQA[2];
                iqaSecond = queryBuildPQAIQA[3];

                var pqaDataFirst = QaQcPQASummary(pqaFirst).Result;
                var pqaDataSecond = QaQcPQASummary(pqaSecond).Result;
                var iqaDataFirst = QaQcIQASummary(iqaFirst).Result;
                var iqaDataSecond = QaQcIQASummary(iqaSecond).Result;

                #region get all critical & common
                // query data critical
                List<string> queryBuildCriticalCommon = BuildQueryCriticalCommon(request);

                criticalCheckSqlFirst = queryBuildCriticalCommon[0];
                criticalCheckSqlSecond = queryBuildCriticalCommon[1];
                commonCheckSqlFirst = queryBuildCriticalCommon[2];
                commonCheckSqlSecond = queryBuildCriticalCommon[3];

                var criticalCheckFirst = QaQcGetSummaryCritical(criticalCheckSqlFirst).Result;
                var criticalCheckSecond = QaQcGetSummaryCritical(criticalCheckSqlSecond).Result;
                var commonCheckFirst = QaQcGetSummaryCommon(commonCheckSqlFirst).Result;
                var commonCheckSecond = QaQcGetSummaryCommon(commonCheckSqlSecond).Result;
                #endregion

                return Ok(new
                {
                    pqaFirst = pqaDataFirst["pqaData"],
                    pqaSecond = pqaDataSecond["pqaData"],
                    iqaFirst = iqaDataFirst["iqaData"],
                    iqaSecond = iqaDataSecond["iqaData"],

                    criticalCheckFirst = criticalCheckFirst["criticalCheck"],
                    criticalCheckSecond = criticalCheckSecond["criticalCheck"],
                    commonCheckFirst = commonCheckFirst["commonCheck"],
                    commonCheckSecond = commonCheckSecond["commonCheck"]
                });
            }
            catch (Exception ce)
            {
                Console.WriteLine(ce.Message);
                return BadRequest(ce.Message);
            }
        }

        #endregion
        /// <summary>
        /// Get user project
        /// </summary>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (01.03.2023)
        public async Task<ServiceResponse> QaQcGetUserProject()
        {
            List<ProjectResponse> userProjects = new List<ProjectResponse>();

            try
            {
                userProjects = await _projectService.GetProjectsByUser();
            }
            catch (Exception ce)
            {
                Console.WriteLine(ce.Message);
                return Ok(new
                {
                    UserProject = userProjects
                },
                message: $"ce.Message: {ce.Message}, userID={_currentUser?.GetId()}");
            }

            return Ok(new
            {
                UserProject = userProjects
            });
        }


        public async Task<ServiceResponse> QaQcGetCommonDataChecks(CommonCheckRequest request)
        {
            string whereSql = "";
            if (request.project != null)
            {
                if (request.project.Count > 0)
                {
                    whereSql += $"and project IN ('{string.Join("','", request.project)}')";
                }
            }

            if (request.discipline != null)
            {
                if (request.discipline != "" && request.discipline != "All Trades")
                {
                    if (request.discipline != "None")
                    {
                        whereSql += "and discipline = '" + request.discipline + "' ";
                    }
                    else
                    {
                        whereSql += "and discipline is null ";
                    }
                }
            }

            if (request.lteDate != null)
            {
                if (request.lteDate != "")
                {
                    whereSql += "and created_at >= '" + request.lteDate + "' ";
                }
            }

            if (request.gteDate != null)
            {
                if (request.gteDate != "")
                {
                    whereSql += "and created_at <= '" + request.gteDate + "' ";
                }
            }

            if (whereSql != "")
            {
                whereSql = "where " + whereSql.Substring(3);
            }

            var checks = QaQcGetCommonDetailChecks(whereSql, request.trade, "qaqc_common_check").Result;

            return Ok(new
            {
                checks = checks["checks"],
            });
        }


        public async Task<ServiceResponse> QaQcGetCommonData(CommonCheckRequest request)
        {
            string whereSql = "";
            if (request.project != null)
            {
                if (request.project.Count > 0)
                {
                    whereSql += $"and project IN ('{string.Join("','", request.project)}')";
                }
            }

            if (request.discipline != null)
            {
                if (request.discipline != "" && request.discipline != "All Trades")
                {
                    if (request.discipline != "None")
                    {
                        whereSql += "and discipline = '" + request.discipline + "' ";
                    }
                    else
                    {
                        whereSql += "and discipline is null ";
                    }
                }
            }

            if (request.lteDate != null)
            {
                if (request.lteDate != "")
                {
                    whereSql += "and created_at >= '" + request.lteDate + "' ";
                }
            }

            if (request.gteDate != null)
            {
                if (request.gteDate != "")
                {
                    whereSql += "and created_at <= '" + request.gteDate + "' ";
                }
            }

            if (whereSql != "")
            {
                whereSql = "where " + whereSql.Substring(3);
            }

            var trade = QaQcGetCommonCheckDataTrade(whereSql).Result;
            var date = QaQcGetCommonCheckDataDate(whereSql).Result;
            var checks = QaQcGetCommonDetailChecks(whereSql, request.trade, "qaqc_common_check").Result;

            var tableSummary = await _permissionChecker.HasPermission(PermissionDefine.VIEW_QAQC_COMMON_TABLE_SUMMARY);
            var chartProject = await _permissionChecker.HasPermission(PermissionDefine.VIEW_QAQC_COMMON_CHART_PROJECT);

            using var connection = _workerDapper.CreateConnection();
            var projectSql = "SELECT project FROM qaqc_common_check GROUP BY project ORDER BY project";

            // DateTime localDate1 = DateTime.Now;
            var projects = (await connection.QueryAsync<string>(projectSql)).ToList();

            var disciplineSql = "SELECT discipline FROM qaqc_common_check GROUP BY discipline ORDER BY discipline";

            var disciplines = (await connection.QueryAsync<string>(disciplineSql)).ToList();
            return Ok(new
            {
                failItems = trade["listFailByTrade"],
                failByMonth = date["listFailByDate"],
                listTrade = trade["list_trade"],
                lineLabels = trade["line_labels"],
                tradeSelected = trade["tradeSelected"],
                projects = projects,
                disciplines = disciplines,
                checks = checks["checks"],
                tableSummary = tableSummary,
                chartProject = chartProject
            });
        }

        #region Query database

        public async Task<Dictionary<string, object>> QaQcGetSummaryCommon(string sql)
        {
            var result = new Dictionary<string, object>();
            if (!await _permissionChecker.HasPermission(PermissionDefine.VIEW_QAQC_COMMON_SUMMARY))
            {
                result["commonCheck"] = false;
            }
            else
            {
                using var connection = _workerDapper.CreateConnection();
                var commonCheck = (await connection.QueryAsync<EntQaQcCommonCheck>(sql)).ToList();
                result["commonCheck"] = commonCheck;
            }

            return result;
        }

        public async Task<Dictionary<string, object>> QaQcGetSummaryCritical(string sql)
        {
            var result = new Dictionary<string, object>();
            if (!await _permissionChecker.HasPermission(PermissionDefine.VIEW_QAQC_CRITICAL_SUMMARY))
            {
                result["criticalCheck"] = false;
            }
            else
            {
                using var connection = _workerDapper.CreateConnection();
                var criticalCheck = (await connection.QueryAsync<EntQaQcCriticalCheck>(sql)).ToList();
                result["criticalCheck"] = criticalCheck;
            }

            return result;
        }

        /// <summary>
        /// Get data score PQA
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (07.03.2023)
        public async Task<Dictionary<string, object>> PQAIQAQuery(string sqlPQA, string sqlIQA)
        {
            var result = new Dictionary<string, object>();
            using var connection = _qaqcDapper.CreateConnection();

            var pqa = (await connection.QueryAsync<PQARequest>(sqlPQA)).ToList();
            var iqa = (await connection.QueryAsync<IQARequest>(sqlIQA)).ToList();

            result.Add("pqaData", (pqa));
            result.Add("iqaData", (iqa));

            return result;
        }

        /// <summary>
        /// Get data score PQA
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (07.03.2023)
        public async Task<Dictionary<string, object>> CriticalCommonQuery(string sqlCritical, string sqlCommon)
        {
            var result = new Dictionary<string, object>();
            using var connection = _workerDapper.CreateConnection();

            var critical = (await connection.QueryAsync<EntQaQcCriticalCheck>(sqlCritical)).ToList();
            var common = (await connection.QueryAsync<EntQaQcCommonCheck>(sqlCommon)).ToList();

            result.Add("criticalData", (critical));
            result.Add("commonData", (common));

            return result;
        }

        /// <summary>
        /// Get data score PQA
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (07.03.2023)
        public async Task<Dictionary<string, object>> QaQcPQASummary(string sql)
        {
            var result = new Dictionary<string, object>();

            try
            {
                using var connection = _qaqcDapper.CreateConnection();
                var pqa = (await connection.QueryAsync<PQARequest>(sql)).ToList();
                result["pqaData"] = pqa;
            }
            catch (Exception ce)
            {
                Console.Write(ce.Message);
            }

            return result;
        }

        /// <summary>
        /// Get data score IQA
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (07.03.2023)
        public async Task<Dictionary<string, object>> QaQcIQASummary(string sql)
        {
            var result = new Dictionary<string, object>();

            using var connection = _qaqcDapper.CreateConnection();
            var iqa = (await connection.QueryAsync<IQARequest>(sql)).ToList();
            result["iqaData"] = iqa;

            return result;
        }

        #endregion

        public string getShortNameOfTrade(string originalName)
        {
            string short_name = "";
            string[] list_word = originalName.Split(" ");
            if (list_word.Length > 1)
            {
                for (int i = 0; i < list_word.Length; i++)
                {
                    string e = list_word[i];
                    if (e != "/" && e != "/")
                    {
                        if (i == 0)
                        {
                            short_name = e.Substring(0, Math.Min(e.Length, 4)) + "-";
                        }
                        else
                        {
                            if (i < 4)
                            {
                                short_name += e.Substring(0, 1);
                            }
                        }
                    }
                }
            }

            if (list_word.Length == 1)
            {
                if (originalName.Length > 5)
                {
                    short_name = originalName.Substring(0, 4);
                }
            }

            return short_name;
        }

        #region QM Weekly report
        /// <summary>
        /// func get 5 week previous nearest
        /// </summary>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (23.03.2023)
        public List<QMWeeklyReport> getPreviousWeek()
        {
            // get 5 week previous nearest
            DateTime date = DateTime.Now;

            DateTime mondayOfLastWeek5 = date.AddDays(-(int)date.DayOfWeek - 6);
            DateTime staturdayOfLastWeek5 = date.AddDays(-(int)date.DayOfWeek - 1);

            DateTime mondayOfLastWeek4 = mondayOfLastWeek5.AddDays(-(int)mondayOfLastWeek5.DayOfWeek - 6);
            DateTime staturdayOfLastWeek4 = mondayOfLastWeek5.AddDays(-(int)mondayOfLastWeek5.DayOfWeek - 1);

            DateTime mondayOfLastWeek3 = mondayOfLastWeek4.AddDays(-(int)mondayOfLastWeek4.DayOfWeek - 6);
            DateTime staturdayOfLastWeek3 = mondayOfLastWeek4.AddDays(-(int)mondayOfLastWeek4.DayOfWeek - 1);

            DateTime mondayOfLastWeek2 = mondayOfLastWeek3.AddDays(-(int)mondayOfLastWeek3.DayOfWeek - 6);
            DateTime staturdayOfLastWeek2 = mondayOfLastWeek3.AddDays(-(int)mondayOfLastWeek3.DayOfWeek - 1);

            DateTime mondayOfLastWeek1 = mondayOfLastWeek2.AddDays(-(int)mondayOfLastWeek2.DayOfWeek - 6);
            DateTime staturdayOfLastWeek1 = mondayOfLastWeek2.AddDays(-(int)mondayOfLastWeek2.DayOfWeek - 1);

            // init week date time
            return new List<QMWeeklyReport>()
            {
                new QMWeeklyReport () {
                    WeekDateForm = mondayOfLastWeek5,
                    WeekDateTo = staturdayOfLastWeek5,
                },
                new QMWeeklyReport () {
                    WeekDateForm = mondayOfLastWeek4,
                    WeekDateTo = staturdayOfLastWeek4,
                },
                new QMWeeklyReport () {
                    WeekDateForm = mondayOfLastWeek3,
                    WeekDateTo = staturdayOfLastWeek3,
                },
                new QMWeeklyReport () {
                    WeekDateForm = mondayOfLastWeek2,
                    WeekDateTo = staturdayOfLastWeek2,
                },
                new QMWeeklyReport () {
                    WeekDateForm = mondayOfLastWeek1,
                    WeekDateTo = staturdayOfLastWeek1,
                }
            };
        }

        /// <summary>
        /// get data target in table qm weekly
        /// </summary>
        /// <param name="qms"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (31.03.2023)
        protected async Task<List<QMWeeklyReport>> TargetByProjectAndDate(List<QMWeeklyReport> qms, SummaryRequest request)
        {
            using var connection = _qaqcDapper.CreateConnection();

            List<QMWeeklyReport> result = new List<QMWeeklyReport>(qms);

            try
            {
                // list query base
                List<string> querySQLMonths = new List<string>();

                // query summary
                string querySQLMonthSummary = string.Empty;

                // improve performance
                foreach (var qm in qms.Select((value, index) => new { index, value }))
                {
                    string idProject = !string.IsNullOrEmpty(request?.projectID) ? $"'{request?.projectID}'" : "null",
                           querySQL = $"SELECT * FROM qm_weekly_report " +
                                    $"WHERE CAST(week_date_form AS DATE) >= '{qm.value.WeekDateForm.ToString("yyyy/MM/dd")}' " +
                                    $"AND CAST(week_date_to AS DATE) <= '{qm.value.WeekDateTo.ToString("yyyy/MM/dd")}' " +
                                    $"AND is_deleted IS NOT TRUE AND project_id = {idProject}";

                    querySQLMonths.Add(querySQL);

                    if (qm.index == qms.Count - 1)
                    {
                        // get cum target (get by last date)
                        querySQLMonthSummary = $"SELECT SUM(target_prepared_realy) AS CumActualPreparedRealy, SUM(target_unit_handover) AS CumTargetUnitHandover, SUM(target_prepared_realy) AS CumTargetPreparedRealy, SUM(target_bca_inspected) AS CumTargetBCAInspected, " +
                            $"SUM(target_cso) AS CumTargetCSO, SUM(actual_cso) AS CumActualCSO, SUM(target_urc) AS CumTargetURC, SUM(actual_urc) AS CumActualURC FROM qm_weekly_report " +
                            $"WHERE CAST(week_date_to AS DATE) <= '{qm.value.WeekDateTo.ToString("yyyy/MM/dd")}' AND is_deleted IS NOT TRUE AND project_id = {idProject};";
                    }
                }

                // query data month
                List<QMWeeklyReport> dataMonths = (await connection.QueryAsync<QMWeeklyReport>(string.Join(" UNION ", querySQLMonths.ToArray()))).ToList();

                // query data summary
                QMWeeklyReport dataMonthSummary = (await connection.QueryAsync<QMWeeklyReport>(querySQLMonthSummary)).FirstOrDefault();

                // assign result and date
                foreach (var qm in qms.Select((value, index) => new { index, value }))
                {
                    var tempMonth = dataMonths.Where(x =>
                        (
                            x.WeekDateForm.ToString("yyyy/MM/dd/", CultureInfo.InvariantCulture) == qm.value.WeekDateForm.ToString("yyyy/MM/dd/", CultureInfo.InvariantCulture) &&
                            x.WeekDateTo.ToString("yyyy/MM/dd/", CultureInfo.InvariantCulture) == qm.value.WeekDateTo.ToString("yyyy/MM/dd/", CultureInfo.InvariantCulture)
                        )
                    ).FirstOrDefault();

                    if (tempMonth != null)
                    {
                        // assign result and date
                        result[qm.index] = tempMonth != null ? tempMonth : new QMWeeklyReport();
                        result[qm.index].WeekDateForm = qm.value.WeekDateForm;
                        result[qm.index].WeekDateTo = qm.value.WeekDateTo;
                    }
                }

                result.Add(dataMonthSummary != null ? dataMonthSummary : new QMWeeklyReport());
            }
            catch (Exception ce)
            {
                Console.WriteLine(ce.Message);
            }

            return result;
        }

        /// <summary>
        /// get data actual in table qm weekly
        /// </summary>
        /// <param name="qms"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        protected async Task<List<QMWeeklyReport>> ActualByProjectAndDate(List<QMWeeklyReport> qms, SummaryRequest request)
        {
            using var connection = _digiCheckDapper.CreateConnection();

            List<QMWeeklyReport> resultTemp = new List<QMWeeklyReport>(qms);

            try
            {
                // build query
                List<string> querySQLMonths = new List<string>();

                // list query Average BCA 
                List<string> querySQLMonthsAverageBCA = new List<string>();

                // query summary
                List<string> querySQLMonthSummary = new List<string>();
                string querySQLMonthSummaryAverageBCA = string.Empty;

                foreach (var qm in qms.Select((value, index) => new { index, value }))
                {
                    // handover actual
                    string queryHanoverUnit = $"SELECT COUNT(MUB.ID), CAST('{qm.value.WeekDateForm}' AS DATE) AS WeekDateFrom, CAST('{qm.value.WeekDateTo}' AS DATE) AS WeekDateTo, 'queryHanoverUnit' AS NameOf FROM MODULE_UNIT_BLOCK MUB " +
                            $"LEFT JOIN MODULE_STEP MS ON MUB.ID = MS.MODULE_ID INNER JOIN MODULE_STEP_HISTORY MSH ON MS.ID = MSH.MODULE_STEP_ID " +
                            $"WHERE MUB.SITE_ID = @SiteId AND MUB.IS_DELETED = FALSE " +
                            $"AND MS.SITE_ID = @SiteId AND MS.IS_DELETED = FALSE AND MSH.ROLE = 'QAQC' " +
                            $"AND CAST(MS.UPDATED_AT AS DATE) >= CAST('{qm.value.WeekDateForm}' AS DATE)" +
                            $"AND CAST(MS.UPDATED_AT AS DATE) <= CAST('{qm.value.WeekDateTo}' AS DATE) ";

                    // Prepared & Ready For BCA Inspection
                    string queryBCAInspection = $"select COUNT(*), CAST('{qm.value.WeekDateForm}' AS DATE) AS WeekDateFrom, CAST('{qm.value.WeekDateTo}' AS DATE) AS WeekDateTo, 'queryBCAInspection' AS NameOf from module_unit_block " +
                        $"LEFT join module_step on module_id = module_unit_block.id " +
                        $"LEFT join module_step_history on module_step.id = module_step_history.module_step_id " +
                        $"LEFT join step on module_step.step_id = step.id " +
                        $"LEFT join form_template on step.form_id = form_template.id " +
                        $"where module_unit_block.is_deleted IS NOT TRUE and module_step.is_deleted IS NOT TRUE " +
                        $"AND CAST(module_step_history.created_at AS DATE) >= CAST('{qm.value.WeekDateForm}' AS DATE)" +
                        $"AND CAST(module_step_history.created_at AS DATE) <= CAST('{qm.value.WeekDateTo}' AS DATE) " +
                        $"and module_step_history.is_deleted IS NOT TRUE and module_unit_block.site_id = @siteId " +
                        $"and (module_step.status = 0 or module_step.status = 1) and form_template.name = 'QAQC-Unit-BCA-Inspection'";

                    // BCA Inspacted
                    string queryBCAInspected = $"select COUNT(*), CAST('{qm.value.WeekDateForm}' AS DATE) AS WeekDateFrom, CAST('{qm.value.WeekDateTo}' AS DATE) AS WeekDateTo, 'queryBCAInspected' AS NameOf from module_unit_block " +
                        $"LEFT join module_step on module_id = module_unit_block.id " +
                        $"LEFT join module_step_history on module_step.id = module_step_history.module_step_id " +
                        $"LEFT join step on module_step.step_id = step.id " +
                        $"LEFT join form_template on step.form_id = form_template.id " +
                        $"where module_unit_block.is_deleted IS NOT TRUE and module_step.is_deleted IS NOT TRUE " +
                        $"AND CAST(module_step_history.created_at AS DATE) >= CAST('{qm.value.WeekDateForm}' AS DATE)" +
                        $"AND CAST(module_step_history.created_at AS DATE) <= CAST('{qm.value.WeekDateTo}' AS DATE) " +
                        $"and module_step_history.is_deleted IS NOT TRUE and module_unit_block.site_id = @siteId " +
                        $"and module_step.status = 99 and form_template.name = 'QAQC-Unit-BCA-Inspection'";

                    // Average BCA Assessment Score 
                    var queryAvgBCAScore = "SELECT MS.STATUS, MSH.HISTORIES " +
                        "FROM MODULE_UNIT_BLOCK MUB " +
                        "LEFT JOIN MODULE_STEP MS ON MS.MODULE_ID = MUB.ID " +
                        "LEFT JOIN MODULE_STEP_HISTORY MSH ON MS.ID = MSH.MODULE_STEP_ID " +
                        "LEFT JOIN STEP ON MS.STEP_ID = STEP.ID " +
                        "LEFT JOIN FORM_TEMPLATE FT ON STEP.FORM_ID = FT.ID " +
                        "WHERE MUB.SITE_ID = @SITEID AND CAST(MSH.CREATED_AT AS DATE) >= CAST(@TIMESTART AS DATE) " +
                        "AND CAST(MSH.CREATED_AT AS DATE) <= CAST(@TIMEEND AS DATE) " +
                        "AND MUB.IS_DELETED IS NOT TRUE AND MS.IS_DELETED IS NOT TRUE AND MSH.IS_DELETED IS NOT TRUE " +
                        "AND MS.STATUS = 99 AND FT.NAME = 'QAQC-Unit-BCA-Inspection';";

                    List<EntQaQcBcaInspection> listUnitBcaInspected = (await connection.QueryAsync<EntQaQcBcaInspection>(queryAvgBCAScore, new
                    {
                        SiteId = request.projectID,
                        TimeStart = qm.value.WeekDateForm,
                        TimeEnd = qm.value.WeekDateTo
                    })).ToList();

                    //get List of score by range
                    List<float> listScoreRaw = listUnitBcaInspected.Select(o => float.Parse(o.HistoriesJson[2].Value.Replace('-', '.').Replace(" ", ""))).ToList();

                    // assign result and date
                    resultTemp[qm.index].AvgBCAAssessmentScore = listScoreRaw.DefaultIfEmpty(0).Average();

                    // add query to list
                    querySQLMonths.Add(queryHanoverUnit);
                    querySQLMonths.Add(queryBCAInspection);
                    querySQLMonths.Add(queryBCAInspected);

                    querySQLMonthsAverageBCA.Add(queryAvgBCAScore);

                    // get all target and actual and last week, so we will plus before week with current target this is result cum target
                    if (qm.index == qms.Count - 1)
                    {
                        // get cum target (get by last date) ActualUnitHandover
                        string querySummaryActualUnitHandover = $"SELECT COUNT(MUB.ID) AS total, 'CumActualUnitHandover' AS NameOf FROM MODULE_UNIT_BLOCK MUB " +
                            $"INNER JOIN MODULE_STEP MS ON MUB.ID = MS.MODULE_ID INNER JOIN MODULE_STEP_HISTORY MSH ON MS.ID = MSH.MODULE_STEP_ID " +
                            $"WHERE MUB.SITE_ID = @SiteId AND MUB.IS_DELETED = FALSE " +
                            $"AND MS.SITE_ID = @SiteId AND MS.IS_DELETED = FALSE AND MSH.ROLE = 'QAQC' " +
                            $"AND CAST(MS.UPDATED_AT AS DATE) <= CAST('{qm.value.WeekDateTo}' AS DATE) ";

                        // get cum target (get by last date) bcaInspectionActual
                        string querySummaryActualActualPreparedRealy = $"select count(*) AS total, 'CumActualPreparedRealy' AS NameOf from module_unit_block " +
                            $"LEFT join module_step on module_id = module_unit_block.id " +
                            $"LEFT join module_step_history on module_step.id = module_step_history.module_step_id " +
                            $"LEFT join step on module_step.step_id = step.id " +
                            $"LEFT join form_template on step.form_id = form_template.id " +
                            $"where module_unit_block.is_deleted IS NOT TRUE and module_step.is_deleted IS NOT TRUE " +
                            $"AND CAST(module_step_history.created_at AS DATE) <= CAST('{qm.value.WeekDateTo}' AS DATE)" +
                            $"and module_step_history.is_deleted IS NOT TRUE and module_unit_block.site_id = @siteId " +
                            $"and (module_step.status = 0 or module_step.status = 1) and form_template.name = 'QAQC-Unit-BCA-Inspection'";

                        // get cum target (get by last date) bcdInspectedActual
                        string querySummaryActualActualBCAInspected = "select COUNT(*) AS total, 'CumActualBCAInspected' AS NameOf from module_unit_block " +
                            $"LEFT join module_step on module_id = module_unit_block.id " +
                            $"LEFT join module_step_history on module_step.id = module_step_history.module_step_id " +
                            $"LEFT join step on module_step.step_id = step.id " +
                            $"LEFT join form_template on step.form_id = form_template.id " +
                            $"where module_unit_block.is_deleted IS NOT TRUE and module_step.is_deleted IS NOT TRUE " +
                            $"AND CAST(module_step_history.created_at AS DATE) <= CAST('{qm.value.WeekDateTo}' AS DATE) " +
                            $"and module_step_history.is_deleted IS NOT TRUE and module_unit_block.site_id = @siteId " +
                            $"and module_step.status = 99 and form_template.name = 'QAQC-Unit-BCA-Inspection'";

                        querySQLMonthSummary.Add(querySummaryActualUnitHandover);
                        querySQLMonthSummary.Add(querySummaryActualActualPreparedRealy);
                        querySQLMonthSummary.Add(querySummaryActualActualBCAInspected);

                        // Cum average BCA Assessment Score
                        var queryCumAvgBCAScore =
                            "select module_step.status, module_step_history.histories from module_unit_block " +
                            "LEFT join module_step on module_id = module_unit_block.id " +
                            "LEFT join module_step_history on module_step.id = module_step_history.module_step_id " +
                            "LEFT join step on module_step.step_id = step.id " +
                            "LEFT join form_template on step.form_id = form_template.id " +
                            "where module_unit_block.is_deleted IS NOT TRUE and module_step.is_deleted IS NOT TRUE " +
                            "and module_step_history.is_deleted IS NOT TRUE and module_unit_block.site_id = @siteId " +
                            "AND CAST(module_step_history.created_at AS DATE) <= CAST(@TimeEnd AS DATE) " +
                            "and module_step.status = 99 and form_template.name = 'QAQC-Unit-BCA-Inspection'";
                        List<EntQaQcBcaInspection> listCumUnitBcaInspected = (await connection.QueryAsync<EntQaQcBcaInspection>(queryCumAvgBCAScore, new
                        {
                            SiteId = request.projectID,
                            TimeStart = qm.value.WeekDateForm,
                            TimeEnd = qm.value.WeekDateTo
                        })).ToList();

                        //get List of score by range
                        List<float> listCumScoreRaw = listCumUnitBcaInspected.Select(o => float.Parse(o.HistoriesJson[2].Value.Replace('-', '.').Replace(" ", ""))).ToList();

                        // add result
                        QMWeeklyReport tempSummaryActualUnitHandover = new QMWeeklyReport();
                        tempSummaryActualUnitHandover.AvgCumBCAAssessmentScore = listCumScoreRaw.DefaultIfEmpty(0).Average();

                        resultTemp.Add(tempSummaryActualUnitHandover);
                    }
                }

                // get data querySQLMonths
                List<ModuleStepCombineRequest> dataSQLMonths = (await connection.QueryAsync<ModuleStepCombineRequest>(string.Join(" UNION ", querySQLMonths.ToArray()), new
                {
                    SiteId = request.projectID,
                })).ToList();

                // query sum querySQLMonthSummary
                List<QMWeeklyReport> dataQLMonthSummary = (await connection.QueryAsync<QMWeeklyReport>(string.Join(" UNION ", querySQLMonthSummary.ToArray()), new
                {
                    SiteId = request.projectID,
                })).ToList();

                // assign data to result
                foreach (var qm in resultTemp.Select((value, index) => new { index, value }))
                {
                    if (qm.index == qms.Count)
                    {
                        resultTemp[qm.index].CumActualUnitHandover = dataQLMonthSummary.Where(x => x.NameOf == "CumActualUnitHandover").FirstOrDefault().total;
                        resultTemp[qm.index].CumActualPreparedRealy = dataQLMonthSummary.Where(x => x.NameOf == "CumActualPreparedRealy").FirstOrDefault().total;
                        resultTemp[qm.index].CumActualBCAInspected = dataQLMonthSummary.Where(x => x.NameOf == "CumActualBCAInspected").FirstOrDefault().total;
                    }
                    else
                    {
                        // filter by week
                        var groupByDate = dataSQLMonths.Where(x =>
                            (x.WeekDateFrom.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture) == qm.value.WeekDateForm.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture) &&
                            x.WeekDateTo.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture) == qm.value.WeekDateTo.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture))
                        ).ToList();

                        resultTemp[qm.index].ActualUnitHandover = groupByDate.Where(x => x.NameOf == "queryHanoverUnit").FirstOrDefault().count;
                        resultTemp[qm.index].ActualPreparedRealy = groupByDate.Where(x => x.NameOf == "queryBCAInspection").FirstOrDefault().count;
                        resultTemp[qm.index].ActualBCAInspected = groupByDate.Where(x => x.NameOf == "queryBCAInspected").FirstOrDefault().count;
                    }
                }

            }
            catch (Exception ce)
            {
                Console.WriteLine(ce.Message);
            }

            return resultTemp;
        }

        /// <summary>
        /// Func calculate qm weekly report
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CretaedBy: PQ Huy (22.03.2023)
        public async Task<ServiceResponse> QMWeeklyReport(SummaryRequest request)
        {
            List<QMWeeklyReport> qms = getPreviousWeek();

            // Step 1: inti class
            Dictionary<string, List<QMWeeklyReport>> qMWeeklyReports = new Dictionary<string, List<QMWeeklyReport>>()
            {
                {"UnitHandover", qms },
                {"BCAInspection", qms },
                {"BCAAssessment", qms },
                {"CSO", qms },
                {"URC", qms },
            };

            try
            {
                if (string.IsNullOrEmpty(request?.projectID) && string.IsNullOrEmpty(request?.projectName))
                {
                    return Ok(qMWeeklyReports);
                }

                // Step 2: Get data target by project and date
                List<QMWeeklyReport> targetQMWeeklyReport = await TargetByProjectAndDate(qms, request);

                // Step 3: Get data actual by project and date
                List<QMWeeklyReport> actualByProjectAndDate = await ActualByProjectAndDate(qms, request);

                return Ok(
                    new
                    {
                        TargetQMWeeklyReport = targetQMWeeklyReport,
                        ActualByProjectAndDate = actualByProjectAndDate
                    }
                );
            }
            catch (Exception ce)
            {
                return Ok(
                     new
                     {
                         TargetQMWeeklyReport = new List<QMWeeklyReport>(),
                         ActualByProjectAndDate = new List<QMWeeklyReport>()
                     }, message: ce.Message
                 );
            }
        }
        #endregion


        ///comment in Dashboard
        public async Task<ServiceResponse> GetComment()
        {
            List<Comment> SortQuery = new List<Comment>();
            try
            {
                using var connection = _qaqcDapper.CreateConnection();
                var Listcomments = "SELECT * FROM qaqc_comment WHERE \"IsDeleted\"=false ORDER BY created_at DESC";
                List<Comment> query = (await connection.QueryAsync<Comment>(Listcomments)).ToList();
                SortQuery = query != null ? query.GroupBy(c => c.project).Select(x => x.FirstOrDefault()).ToList() : new List<Comment>();

            }
            catch (Exception ex)
            {
                return Ok(message: ex.Message);
            }
            return Ok(SortQuery);
        }
        public async Task<ServiceResponse> GetScore(ScorereworkAndDefece score)
        {
            using var connection = _qaqcDapper.CreateConnection();
            var SocreLastMonth = "";
            var SocreThisMonth = "SELECT SUM(defe.defect_score) AS defectScore,defe.project,Sum(rew.rework_score) AS reworkScore \r\n " +
                "FROM defect_score defe join reduce_rework rew on  defe.project_id= rew.\"IDProject\" " +
                "\r\n WHERE defe.project_id='" + score.project_id + "' AND (defe.is_deleted=false AND rew.is_deleted=false)" +
                "\r\nand to_char(NOW(), 'YYYY-MM')=to_char(defe.date,'YYYY-MM') GROUP BY defe.project";
            if (score.date != null)
            {
                SocreLastMonth = "SELECT sum(defe.defect_score) AS defectScore,defe.project,SUM(rew.rework_score ) AS reworkScore \r\n" +
              "FROM defect_score defe INNER JOIN reduce_rework rew ON  defe.project_id= rew.\"IDProject\" " +
              "\r\n WHERE defe.project_id='" + score.project_id + "' AND (defe.is_deleted=false and rew.is_deleted=false)" +
              "\r\nand( defe.date BETWEEN '" + score.date + "'and Now()) group by defe.project";
            }
            else
            {
                SocreLastMonth = "SELECT sum(defe.defect_score) AS defectScore,defe.project,SUM(rew.rework_score ) AS reworkScore \r\n" +
              "FROM defect_score defe INNER JOIN reduce_rework rew ON  defe.project_id= rew.\"IDProject\" " +
              "\r\n WHERE defe.project_id='" + score.project_id + "' AND (defe.is_deleted=false and rew.is_deleted=false)" +
              "\r\n and defe.date<date_trunc('month',Now())::date  group by defe.project group by defe.project";
            }

            var scoreThisMonth = (await connection.QueryAsync<Score>(SocreThisMonth)).ToList();
            var scoreLastMonth = (await connection.QueryAsync<Score>(SocreLastMonth)).ToList();
            return Ok(new
            {
                _scoreThisMonth = scoreThisMonth,
                _scoreLastMonth = scoreLastMonth
            });

        }
    }
}