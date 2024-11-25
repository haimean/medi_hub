using AutoMapper;
using Dapper;
using DashboardApi.Auth.PermisionChecker;
using DashboardApi.DatabaseContext.DapperDbContext;
using DashboardApi.Dtos.QaQc.Requests;
using DashboardApi.Dtos.QaQc.Responses;
using DashboardApi.Dtos.QMWeekly;
using DashboardApi.HttpConfig;
using DashboardApi.Models;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace DashboardApi.Application.DashBoardQaQcJot
{
    public class DashboardQaQcJotService : Service, IDashboardQaQcJotService
    {
        private readonly JotDapperContext _dapperJot;
        private readonly IPermissionChecker _permissionChecker;
        public readonly IMapper _mapper;
        public readonly DigiCheckDapperContext _digicheckDbContext;
        public readonly QaQcDapperContext _qaqcDbContext;

        public DashboardQaQcJotService(JotDapperContext dapperJot,
            IPermissionChecker permissionChecker,
            IMapper mapper,
            DigiCheckDapperContext digiCheckDapperContext,
            QaQcDapperContext qaqcDbContext
        )
        {
            _dapperJot = dapperJot;
            _permissionChecker = permissionChecker;
            _digicheckDbContext = digiCheckDapperContext;
            _qaqcDbContext = qaqcDbContext;
            _mapper = mapper;
        }

        //QAQC SUMMARY PAGES DATA
        public async Task<ServiceResponse> QaQcJotGetSummaryData()
        {
            using var connection = _dapperJot.CreateConnection();
            var cm_sql = "SELECT CREATED_AT, PROJECT, YEAR, MONTH, NO_OF_YES, NO_OF_NO FROM QAQC_COMMON_CHECK ORDER BY CREATED_AT DESC;";

            var data_cm = (await connection.QueryAsync<EntQaQcCommonSum>(cm_sql)).ToList();

            var ct_sql = "SELECT CREATED_AT, project, year,month, no_of_no, no_of_1st , no_of_1plus " +
                         "FROM qaqc_critical_check  ORDER BY CREATED_AT DESC";
            var data_ct = (await connection.QueryAsync<EntQaQcCritical>(ct_sql)).ToList();

            return Ok(new { data_cm = data_cm, data_ct = data_ct });
        }

        //QAQC COMMON CHECK DATA
        public async Task<ServiceResponse> QaQcJotGetCommonCheckData()
        {
            using var connection = _dapperJot.CreateConnection();
            var cm_sql = "SELECT PROJECT, DISCIPLINE, SHORTFORM, TRADE, YEAR, MONTH, NO_OF_YES, NO_OF_NO, CREATED_AT, CHECK_LIST FROM QAQC_COMMON_CHECK;";
            var data_cm = (await connection.QueryAsync<EntQaQcCommonDetail>(cm_sql)).ToList();

            return Ok(new { data_cm = data_cm });
        }

        public async Task<ServiceResponse> QaQcJotGetCriticalCheckData()
        {
            using var connection = _dapperJot.CreateConnection();

            var ct_sql = "SELECT PROJECT, DISCIPLINE, SHORTFORM, TRADE, YEAR, MONTH, NO_OF_NO, NO_OF_1ST, NO_OF_1PLUS, CREATED_AT, CHECK_LIST FROM QAQC_CRITICAL_CHECK;";
            var data_ct = (await connection.QueryAsync<EntQaQcCritical>(ct_sql)).ToList();

            return Ok(new { data_ct = data_ct });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ServiceResponse> QMHandoverSchedule(BaseRequest request)
        {
            try
            {
                ResourceManager rm = new ResourceManager("DashboardApi.Resource.QueryQMDashboard", Assembly.GetExecutingAssembly());
                var targetQMWeekly = new List<QMWeeklyReport>();
                var result = new List<CombineQMWeekly>();
                List<TargetQMWeekly> actualUnitHandover = new List<TargetQMWeekly>(),
                                    actualBCAInspection = new List<TargetQMWeekly>(),
                                    actualBCAInspected = new List<TargetQMWeekly>(),
                                    actualBCAAccessmentScore = new List<TargetQMWeekly>();

                if (request != null && !string.IsNullOrEmpty(request.projectId))
                {
                    var enableActualKeyin = false;
                    // request.projectId = "MTYzMzk1MDIxMTY2Ny1UR1c";

                    if (request.projectId == "MTYzMzk1MDIxMTY2Ny1UR1c" || request.projectId == "MTYyMDYzMDk4MTY3NS1QaG9lbml4")
                    {
                        enableActualKeyin = true;
                    }

                    // 1. get query dashboard
                    string whereSQL = $"1=1",
                           queryTargetUnitHandover = rm.GetString("TargetQMWeekly", CultureInfo.CurrentCulture),
                           queryActualUnitHandover = rm.GetString("ActualQMWeekly", CultureInfo.CurrentCulture);

                    // query with params
                    using var connectionQAQC = _qaqcDbContext.CreateConnection();
                    using var connectionDigicheck = _digicheckDbContext.CreateConnection();

                    // get week
                    var weekQuery = GetWeeksDictionary(request.lteDate != null ? (DateTime)request.lteDate : DateTime.MinValue, request.gteDate != null ? (DateTime)request.gteDate : DateTime.MinValue);

                    // query
                    using (var resultQuery = await connectionQAQC.QueryMultipleAsync(queryTargetUnitHandover, new { SiteId = request.projectId }))
                    {
                        targetQMWeekly = (await resultQuery.ReadAsync<QMWeeklyReport>()).ToList();
                    }

                    using (var resultQuery = await connectionDigicheck.QueryMultipleAsync(queryActualUnitHandover, new { SITE_ID = request.projectId }))
                    {
                        // Actual Unit Handover to QM(OVERALL)
                        actualUnitHandover = (await resultQuery.ReadAsync<TargetQMWeekly>()).ToList();

                        // Actual PREPARED & READY FOR BCA INSPECTION
                        actualBCAInspection = (await resultQuery.ReadAsync<TargetQMWeekly>()).ToList();

                        // Actual BCA Inspected (BCA Assessment)
                        actualBCAInspected = (await resultQuery.ReadAsync<TargetQMWeekly>()).ToList();

                        // Actual BCA Assessment Score
                        actualBCAAccessmentScore = (await resultQuery.ReadAsync<TargetQMWeekly>()).ToList();
                    }

                    foreach (var week in weekQuery.Select((value, i) => new { i, value }))
                    {

                        var temp = new CombineQMWeekly();
                        temp.WeekDateForm = week.value.Value.startDate;
                        temp.WeekDateTo = week.value.Value.endDate;

                        // 1. target qm weekly
                        var temp1 = targetQMWeekly.Find(x => week.value.Value.startDate.Date == x.WeekDateForm.Date && x.WeekDateTo.Date <= week.value.Value.endDate.Date);
                        if (temp1 != null)
                        {
                            temp.QMWeeklyReport = temp1;
                            temp.Remark = temp1?.Remark?.ToString();
                        }

                        // 2. Actual Unit Handover to QM(OVERALL)
                        if (enableActualKeyin && temp1 != null && temp1.ActualUnitHandover > 0)
                        {
                            // if has value actual key-in
                            temp.UnitHandover = temp1.ActualUnitHandover;
                        }
                        else
                        {
                            var temp2 = actualUnitHandover.Find(x => week.value.Value.startDate.Date == x.WeekDateForm && x.WeekDateTo.Value.Date <= week.value.Value.endDate.Date);
                            if (temp2 != null)
                            {
                                temp.UnitHandover = temp2.Unit;
                            }
                        }
                        temp.CumUnitHandover = (temp.UnitHandover != null ? temp.UnitHandover : 0) + (result.ElementAtOrDefault(week.i - 1) != null ? (result[week.i - 1].CumUnitHandover != null ? result[week.i - 1].CumUnitHandover : 0) : 0);

                        // 3. Actual PREPARED & READY FOR BCA INSPECTION
                        if (enableActualKeyin && temp1 != null && temp1.ActualPreparedRealy >= 0)
                        {
                            // if has value actual key-in
                            temp.BCAInspection = temp1.ActualPreparedRealy;
                        }
                        else
                        {
                            var temp3 = actualBCAInspection.Find(x => week.value.Value.startDate.Date == x.WeekDateForm.Value.Date && x.WeekDateTo.Value.Date <= week.value.Value.endDate.Date);
                            if (temp3 != null)
                            {
                                temp.BCAInspection = temp3.Unit;
                            }
                        }
                        temp.CumBCAInspection = (temp.BCAInspection != null ? temp.BCAInspection : 0) + (result.ElementAtOrDefault(week.i - 1) != null ? (result[week.i - 1].CumBCAInspection != null ? result[week.i - 1].CumBCAInspection : 0) : 0);

                        // 4. Actual BCA Inspected (BCA Assessment)
                        if (enableActualKeyin && temp1 != null && temp1.ActualBCAInspected > 0)
                        {
                            temp.BCAInspected = temp1.ActualBCAInspected;
                        }
                        else
                        {
                            var temp4 = actualBCAInspected.Find(x => week.value.Value.startDate.Date == x.WeekDateForm.Value.Date && x.WeekDateTo.Value.Date <= week.value.Value.endDate.Date);
                            if (temp4 != null)
                            {
                                temp.BCAInspected = temp4.Unit;
                            }
                        }
                        temp.CumBCAInspected = (temp.BCAInspected != null ? temp.BCAInspected : 0) + (result.ElementAtOrDefault(week.i - 1) != null ? (result[week.i - 1].CumBCAInspected != null ? result[week.i - 1].CumBCAInspected : 0) : 0);
                        temp.CumBCAInspected = Math.Round((double)temp.CumBCAInspected, 2);

                        // 5. Actual BCA Assessment Score
                        if (enableActualKeyin && temp1 != null && temp1.AvgBCAAssessmentScore > 0)
                        {
                            temp.AvgBCAAssessmentScore = temp1.BCAAssmentScore;
                        }
                        else
                        {
                            var temp5 = actualBCAAccessmentScore.Find(x => week.value.Value.startDate.Date == x.WeekDateForm.Value.Date && x.WeekDateTo.Value.Date <= week.value.Value.endDate.Date);
                            if (temp5 != null)
                            {
                                temp.AvgBCAAssessmentScore = temp5.Unit;
                            }
                        }
                        // temp.AvgBCAAssessmentScore = temp.BCAInspected > 0 ? temp.AvgBCAAssessmentScore / temp.BCAInspected : 0;

                        // calulate cum score
                        temp.CumBCAAssessmentScore = (temp.AvgBCAAssessmentScore != null ? temp.AvgBCAAssessmentScore : 0) + (result.ElementAtOrDefault(week.i - 1) != null ? (result[week.i - 1].CumBCAAssessmentScore != null ? result[week.i - 1].CumBCAAssessmentScore : 0) : 0);

                        result.Add(temp);
                    }

                    result.Reverse();

                    connectionDigicheck.Close();
                    connectionQAQC.Close();
                    connectionDigicheck.Dispose();
                    connectionQAQC.Dispose();

                    return Ok(result);
                }
                else
                {
                    return Ok(message: "Need project - to for api");
                }
            }
            catch (Exception ce)
            {
                return BadRequest(message: ce.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        protected Dictionary<string, (DateTime startDate, DateTime endDate)> GetWeeksDictionary(DateTime startDate, DateTime endDate)
        {
            startDate = new DateTime(2019, 1, 1);
            Dictionary<string, (DateTime startDate, DateTime endDate)> weeksDictionary = new Dictionary<string, (DateTime, DateTime)>();

            // Adjust startDate to the beginning of the week (Monday)
            if (endDate == null || endDate == DateTime.MinValue)
            {
                // get end of current week
                endDate = GetEndOfWeek(DateTime.Today);
            }
            if (startDate == null || startDate == DateTime.MinValue)
            {
                startDate = GetEndOfWeek(DateTime.Now.AddYears(-5));
            }

            startDate = startDate.AddDays(-(int)startDate.DayOfWeek + (int)DayOfWeek.Monday);

            int weekNumber = 1;

            while (startDate <= endDate)
            {
                DateTime weekEndDate = startDate.AddDays(6);

                // Ensure the end date of the week does not exceed the given end date
                if (weekEndDate > endDate)
                {
                    weekEndDate = endDate;
                }

                weeksDictionary.Add($"Week{weekNumber}", (startDate, weekEndDate));

                startDate = startDate.AddDays(7);
                weekNumber++;
            }

            return weeksDictionary;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected DateTime GetEndOfWeek(DateTime today)
        {
            int daysUntilEndOfWeek = DayOfWeek.Sunday - today.DayOfWeek;

            if (daysUntilEndOfWeek < 0)
            {
                daysUntilEndOfWeek += 7;
            }

            DateTime endOfWeek = today.AddDays(daysUntilEndOfWeek);
            return endOfWeek;
        }
    }
}
