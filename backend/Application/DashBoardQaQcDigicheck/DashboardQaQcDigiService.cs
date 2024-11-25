using AutoMapper;
using Dapper;
using DashboardApi.Application.Project;
using DashboardApi.Application.Project.Blocks;
using DashboardApi.Auth.CurrentUser;
using DashboardApi.Auth.PermisionChecker;
using DashboardApi.DatabaseContext.DapperDbContext;
using DashboardApi.Dtos.QaQc.Responses;
using DashboardApi.HttpConfig;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace DashboardApi.Application.DashBoardQaQcDigicheck
{
    public class DashboardQaQcDigiService : Service, IDashboardQaQcDigicheckService
    {
        private readonly DigiCheckDapperContext _dapperDigi;
        private readonly IPermissionChecker _permissionChecker;
        public readonly IMapper _mapper;
        public readonly IBlockService _blockService;
        public readonly IUnitsService _unitsService;
        public readonly ICurrentUser _currentUser;
        private static List<string> listIdUnitBcaInspected;
        private static List<FormCheckItem> listCheckDetail;
        private static List<EntQaQcBcaInspection> listUnitBcaInspected;

        public DashboardQaQcDigiService(
            DigiCheckDapperContext dapperJot,
            IPermissionChecker permissionChecker,
            IMapper mapper,
            IUnitsService unitsService,
            ICurrentUser currentUser,
            IBlockService blockService)
        {
            _dapperDigi = dapperJot;
            _permissionChecker = permissionChecker;
            _mapper = mapper;
            _blockService = blockService;
            _currentUser = currentUser;
            _unitsService = unitsService;
        }


        /// <summary>
        /// QAQC BCA INSPECTION DATA
        /// QAQC HANDOVER DATA
        /// </summary>
        /// <param name="SiteId"></param>
        /// <returns></returns>
        public async Task<ServiceResponse> QaQcGetBcaInspection(string SiteId, string[] strataBlock)
        {
            // init data
            List<object> listInspector = new List<object>(),
                         listScore = new List<object>(),
                         listStatic = new List<object>();
            List<BlockResponse> lstBlock = new List<BlockResponse>();

            if (string.IsNullOrEmpty(SiteId))
            {
                return Ok(new { ListInpsector = listInspector, ListScore = listScore, ListStatic = listStatic }, message: $"QaQcGetBcaInspection {SiteId}");
            }

            try
            {
                using var connection = _dapperDigi.CreateConnection();

                lstBlock = await _blockService.GetBlock(SiteId);
                if (strataBlock.Count() > 0)
                {
                    lstBlock = lstBlock.Where(x => strataBlock.Contains(x.Id))?.ToList();
                }

                var units = (await _unitsService.GetUnits(SiteId))?.Where(x => !string.IsNullOrEmpty(x.FullName));
                var lstUnit = units.Select(x => x.FullName).ToList();

                string queryOnlyBLK = string.Empty;
                if (lstBlock.Count > 0)
                {
                    queryOnlyBLK = $" AND (module_unit_block.BLOCK LIKE '{string.Join("' OR module_unit_block.BLOCK LIKE '", lstBlock.Select(x => $"BLK{x.Name.TrimStart('0')}").ToList())}' OR " +
                        $"module_unit_block.BLOCK LIKE '{string.Join("' OR module_unit_block.BLOCK LIKE '", lstBlock.Select(x => $"{x.Name}").ToList())}')";
                }

                var inspectedHistorySql =
                    "select module_step.status, module_step_history.histories from module_unit_block " +
                    "inner join module_step on module_id = module_unit_block.id " +
                    "inner join module_step_history on module_step.id = module_step_history.module_step_id " +
                    "inner join step on module_step.step_id = step.id " +
                    "inner join form_template on step.form_id = form_template.id " +
                    "where module_unit_block.is_deleted IS NOT TRUE and module_step.is_deleted IS NOT TRUE " +
                    "and module_step_history.is_deleted IS NOT TRUE and module_unit_block.site_id = @siteId " +
                    "and module_step.status = 99 and form_template.name = 'QAQC-Unit-BCA-Inspection'" +
                    queryOnlyBLK;

                var parameters = new { siteId = SiteId };
                List<EntQaQcBcaInspection> listUnitBcaInspected = (await connection.QueryAsync<EntQaQcBcaInspection>(inspectedHistorySql, parameters)).ToList();
                List<string> listInspectorName = listUnitBcaInspected
                    .Select(x => new List<string>() { x.HistoriesJson[0].Value, x.HistoriesJson[1].Value })
                    .SelectMany(item => item).Distinct().OrderBy(z => z).ToList();

                foreach (var inspector in listInspectorName)
                {
                    //total score
                    var listFilter = listUnitBcaInspected.Where(x =>
                        x.HistoriesJson[0].Value == inspector || x.HistoriesJson[1].Value == inspector);
                    double score = 0;
                    int count = 0;
                    count = listFilter.Count();
                    if (count > 0)
                    {
                        score = listFilter.Sum(item => Double.Parse(item.HistoriesJson[2].Value.Replace('-', '.').Replace(" ", ""))) / count;
                    }

                    //score as accessor 1
                    var listFilter1 = listUnitBcaInspected.Where(x => x.HistoriesJson[0].Value == inspector);
                    double score1 = 0;
                    int count1 = 0;
                    count1 = listFilter1.Count();
                    if (count1 > 0)
                    {
                        score1 = listFilter1.Sum(item => Double.Parse(item.HistoriesJson[2].Value.Replace('-', '.').Replace(" ", ""))) / count1;
                    }

                    //score as accessor 2
                    var listFilter2 = listUnitBcaInspected.Where(x => x.HistoriesJson[1].Value == inspector);
                    double score2 = 0;
                    int count2 = 0;
                    count2 = listFilter2.Count();
                    if (count2 > 0)
                    {
                        score2 = listFilter2.Sum(item => Double.Parse(item.HistoriesJson[2].Value.Replace('-', '.').Replace(" ", ""))) / count2;
                    }

                    var o = new
                    {
                        name = inspector,
                        count1 = count1,
                        avgScore1 = Math.Round(score1, 2),
                        count2 = count2,
                        avgScore2 = Math.Round(score2, 2),
                        count = count,
                        avgScore = Math.Round(score, 2)
                    };
                    listInspector.Add(o);
                }

                //Static for dashboard
                // get block by id project
                //var lstBlock = await _blockService.GetBlock(SiteId);
                //var units = (await _unitsService.GetUnits(SiteId))?.Where(x => !string.IsNullOrEmpty(x.FullName));
                //var lstUnit = units.Select(x => x.FullName).ToList();
                //string queryOnlyBLK = string.Empty;

                //if (strataBlock != null && strataBlock.Length > 0 && lstBlock?.Count > 0)
                //{
                //queryOnlyBLK = $" AND (MUB.BLOCK LIKE '%{string.Join("' OR MUB.BLOCK LIKE '%", lstBlock.Select(x => $"BLK{x.Name.TrimStart('0')}").ToList())}' OR " +
                //    $"MUB.BLOCK LIKE '%{string.Join("%' OR MUB.BLOCK LIKE '%", lstBlock.Select(x => $"{x.Name}").ToList())}%')";
                //}

                //Static for dashboard
                string whereTotal = $"WHERE module_unit_block.site_id=@SiteId AND module_unit_block.is_deleted=false {queryOnlyBLK}";
                if (lstBlock.Count > 0)
                {
                    whereTotal += queryOnlyBLK;
                }

                var lstUnitNameTotal = (await connection.QueryAsync<string>($"SELECT module_unit_block.NAME FROM module_unit_block {whereTotal}", new { SiteId = SiteId, }));
                lstUnitNameTotal = lstUnitNameTotal?.Where(item => !string.IsNullOrEmpty(lstUnit.Find(x => x.ToLower().Trim().Contains(getUnit(item?.ToLower().Trim()))))).ToList();
                var total_unit = lstUnitNameTotal?.Count();

                //get List of score by range
                List<double> listScoreRaw = listUnitBcaInspected.Select(o => Double.Parse(o.HistoriesJson[2].Value.Replace('-', '.').Replace(" ", ""))).ToList();

                var o1 = new { name = "<92", count = listScoreRaw.Count(c => c < 92) };
                listScore.Add(o1);
                var o2 = new { name = "92-92.9", count = listScoreRaw.Count(c => c >= 92 && c < 93) };
                listScore.Add(o2);
                var o3 = new { name = "93-93.9", count = listScoreRaw.Count(c => c >= 93 && c < 94) };
                listScore.Add(o3);
                var o4 = new { name = "94-94.9", count = listScoreRaw.Count(c => c >= 94 && c < 95) };
                listScore.Add(o4);
                var o5 = new { name = ">=95", count = listScoreRaw.Count(c => c >= 95) };
                listScore.Add(o5);


                //Static for dashboard
                var ls1 = new { key = "Average score", value = Math.Round(listScoreRaw.DefaultIfEmpty(0).Average(), 2) };
                listStatic.Add(ls1);

                int unitCompleted = listUnitBcaInspected.Count(u => u.status == 99);
                var ls2 = new { key = "No of unit completed", value = unitCompleted + "/" + total_unit };
                listStatic.Add(ls2);

                var ls3 = new
                {
                    key = "No of unit archived target (>=93)",
                    value = listScoreRaw.Count(c => c >= 93) + "/" + unitCompleted
                };
                listStatic.Add(ls3);
            }
            catch (Exception ce)
            {
                return Ok(new { ListInpsector = listInspector, ListScore = listScore, ListStatic = listStatic }, message: $"QaQcGetBcaInspection {ce.Message}");
            }


            return Ok(new { ListInpsector = listInspector, ListScore = listScore, ListStatic = listStatic });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        static string TrimLeadingZero(string input)
        {
            // Check if the first character is '0' and the length is greater than 1
            if (input.StartsWith("0") && input.Length > 1)
            {
                // Remove the first character
                return input.Substring(1);
            }
            return input;
        }

        /// <summary>
        /// Get handed over
        /// </summary>
        /// <param name="SiteId"></param>
        /// <returns></returns>
        /// Unit handed over
        /// Unit HandOver
        public async Task<ServiceResponse> QaQcGetHandedOver(string SiteId, string[] strataBlock)
        {
            using var connection = _dapperDigi.CreateConnection();
            List<object> listStatic = new List<object>();
            List<string> lstUnit = new List<string>();
            List<EntQaQcHandedOver> listUnit = new List<EntQaQcHandedOver>();
            List<BlockResponse> lstBlock = new List<BlockResponse>();
            List<dynamic> listRawHOInspected = new List<dynamic>();
            List<dynamic> listRawHONotInspected = new List<dynamic>();

            Dictionary<string, string> lstProjectForm = new Dictionary<string, string>()
            {
                {"Handy", "MTU5MTY3NDI0ODUyMy1IYW5keQ"}
            };

            try
            {
                ResourceManager rm = new ResourceManager("DashboardApi.Resource.QueryQMDashboard", Assembly.GetExecutingAssembly());

                // int query blk
                string queryOnlyBLK = string.Empty;

                // get block by id project
                lstBlock = await _blockService.GetBlock(SiteId);
                var units = (await _unitsService.GetUnits(SiteId))?.Where(x => !string.IsNullOrEmpty(x.FullName));
                lstUnit = units.Select(x => x.FullName).ToList();

                if (strataBlock.Count() > 0)
                {
                    lstBlock = lstBlock.Where(x => strataBlock.Contains(x.Id))?.ToList();
                }

                if (strataBlock?.Count() > 0 && lstBlock?.Count > 0)
                {
                    queryOnlyBLK = $" AND (MUB.BLOCK = '{string.Join("' OR MUB.BLOCK = '", lstBlock.Select(x => $"BLK{x.Name.TrimStart('0')}").ToList())}' OR " +
                        $"MUB.BLOCK = '{string.Join("' OR MUB.BLOCK = '", lstBlock.Where(x => x.Name.StartsWith("0") && x.Name?.Length > 1).Select(x => $"{TrimLeadingZero(x.Name)}").ToList())}' OR " +
                        $"MUB.BLOCK = '{string.Join("' OR MUB.BLOCK = '", lstBlock.Select(x => $"{x.Name}").ToList())}')";
                }

                string form_template = "QAQC-Unit-Handover",
                       form_template_bca = "QAQC-Unit-BCA-Inspection";

                bool flagIsOffice = false;

                if (lstBlock.Find(x => x.Name.ToLower().Contains("Office".ToLower()) || x.Name.ToLower().Contains("Retail".ToLower())) != null)
                {
                    form_template = "QAQC_Handover_Office";
                }

                /*if (lstProjectForm.Where(x => x.Value == SiteId).ToList().Count > 0)
                {
                    form_template = "QAQC-Unit-Handover";
                }*/

                //GET DATA OF BCA INSPECTED
                var inspectedHistorySql =
                   "SELECT MUB.id,MUB.name, module_step.status, module_step_history.histories FROM module_unit_block MUB " +
                   "INNER JOIN module_step ON module_id = MUB.id " +
                   "INNER JOIN module_step_history ON module_step.id = module_step_history.module_step_id " +
                   "INNER JOIN step ON module_step.step_id = step.id " +
                   "INNER JOIN form_template ON step.form_id = form_template.id " +
                   "WHERE MUB.is_deleted = false AND module_step.is_deleted = false " +
                   "AND module_step_history.is_deleted = false AND MUB.site_id = @siteId " +
                   $"AND module_step.status = 99 AND form_template.name = '{form_template_bca}' {queryOnlyBLK}";

                var parameters = new { siteId = SiteId };
                listUnitBcaInspected = (await connection.QueryAsync<EntQaQcBcaInspection>(inspectedHistorySql, parameters)).ToList();
                listIdUnitBcaInspected = listUnitBcaInspected.Select(o => o.id).ToList();

                var unitMainApp = units.Select(x => x.Name).ToList();
                var unitDigi = listUnitBcaInspected.Select(x => x.name).ToList();

                var temp = unitDigi.Where(x => unitMainApp.Where(y => y != x.Replace("#", "")).ToList().Count > 0).ToList();
                var temp2 = unitDigi.Select(x => x.Replace("#", "")).Except(unitMainApp);

                //GET LIST OF UNIT HAS HANDED OVER
                var form = await connection.QueryFirstOrDefaultAsync($"SELECT id, detail FROM form_template WHERE name ='{form_template}'");
                listCheckDetail = JsonConvert.DeserializeObject<List<FormCheckItem>>(form.detail);

                var paramsHO = new { siteId = SiteId, FORM_TEMPLATE = form_template };
                string listHOQuery = rm.GetString("LstHOQuery", CultureInfo.CurrentCulture);

                if (strataBlock.Count() > 0 && lstBlock.Count > 0)
                {
                    listHOQuery += $" {queryOnlyBLK}";
                }

                listRawHOInspected = (await connection.QueryAsync(listHOQuery, paramsHO)).ToList();
                List<EntQaQcHandedOver> listHOInspected = new List<EntQaQcHandedOver>();
                foreach (var item in listRawHOInspected)
                {
                    string tempName = getUnit(item.name?.ToLower().Trim());
                    if (string.IsNullOrEmpty(lstUnit.Find(x => x.ToLower().Trim().Contains(tempName))))
                    {
                        continue;
                    }

                    EntQaQcHandedOver ent = new EntQaQcHandedOver();

                    try
                    {
                        var getinfo = getInfoFromChecks(item.status, item.histories);
                        ent.date = item.date;
                        ent.ArchiRepresentative = getinfo.Item1;
                        ent.QMRepresentative = getinfo.Item2;
                        ent.Block = item.block;
                        ent.Level = item.level;
                        ent.Unit = getUnit(item.name);
                        ent.Status = getinfo.Item3;
                        ent.listItemsNotAcceptted = getinfo.Item4;
                        ent.Comment = getinfo.Item5;
                        ent.BCAInspected = getBcaInspected(item.id);
                        ent.score = getScore(item.name, flagIsOffice);

                        ent.PlanStart = item.plan_start;
                        ent.PlanEnd = item.plan_end;
                        ent.ActualStart = item.actual_start;
                        ent.ActualEnd = item.actual_end;
                        ent.Status_Unit = item.status_unit;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Data histories is empty: {item}, {e.Message}");
                    }

                    var checkIndex = listHOInspected.FindIndex(x => x.Block == ent.Block && x.Level == ent.Level && x.Unit == ent.Unit);

                    if (checkIndex != -1)
                    {
                        if (listHOInspected[checkIndex]?.date <= ent.date)
                        {
                            listHOInspected[checkIndex] = ent;
                        }
                    }
                    else
                    {
                        listHOInspected.Add(ent);
                    }

                }

                string[] ListUnitIds = listRawHOInspected.Select(x => x.id as string).ToArray();

                //Static for dashboard
                string whereHONotQueryl = $"WHERE MUB.SITE_ID = @SiteId AND MUB.IS_DELETED = FALSE AND NOT(MUB.ID = ANY(@Ids)) ";
                if (strataBlock.Count() > 0 && lstBlock.Count > 0)
                {
                    whereHONotQueryl += queryOnlyBLK;
                }
                string listHONotQuery = $"SELECT MUB.BLOCK, MUB.LEVEL, MUB.NAME, MUB.PLAN_START, MUB.PLAN_END, MUB.ACTUAL_START, MUB.ACTUAL_END, MUB.STATUS " +
                    $"FROM MODULE_UNIT_BLOCK MUB {whereHONotQueryl} ORDER BY MUB.BLOCK, MUB.LEVEL;";

                listRawHONotInspected = (await connection.QueryAsync(listHONotQuery, new
                {
                    SiteId = SiteId,
                    Ids = ListUnitIds
                })).ToList();

                List<EntQaQcHandedOver> listHONotInspected = new List<EntQaQcHandedOver>();
                var seen = new Dictionary<string, bool>();  // Dictionary to keep track of seen keys

                foreach (var item in listRawHONotInspected)
                {
                    string tempName = getUnit(item.name?.ToLower().Trim());
                    if (string.IsNullOrEmpty(lstUnit.Find(x => x.ToLower().Trim().Contains(tempName))))
                    {
                        continue;
                    }

                    EntQaQcHandedOver ent = new EntQaQcHandedOver()
                    {
                        Block = item?.block,
                        Level = item?.level,
                        Unit = getUnit(item.name),
                        Status = 0,
                        BCAInspected = false
                    };


                    var checkIndex = listHOInspected.FindIndex(x => x.Block == ent.Block && x.Level == ent.Level && x.Unit == ent.Unit);

                    if (checkIndex != -1)
                    {
                        if (listHOInspected[checkIndex]?.date <= ent.date)
                        {
                            listHOInspected[checkIndex] = ent;
                        }
                    }
                    else
                    {
                        listHOInspected.Add(ent);
                    }
                }

                listUnit = listHOInspected.Concat(listHONotInspected)
                    .OrderByDescending(x => x.Block)
                    .ThenByDescending(x => x.Level)
                    .ThenByDescending(x => x.Unit)
                    .ToList();

                //Static for dashboard
                string whereTotal = $"WHERE MUB.site_id=@SiteId AND MUB.is_deleted=false ";
                if (strataBlock.Count() > 0 && lstBlock.Count > 0)
                {
                    whereTotal += queryOnlyBLK;
                }

                var lstUnitNameTotal = (await connection.QueryAsync<string>(
                    $"SELECT MUB.NAME FROM module_unit_block MUB {whereTotal}",
                    new { SiteId = SiteId, }));

                lstUnitNameTotal = lstUnitNameTotal?.Where(item => !string.IsNullOrEmpty(lstUnit.Find(x => x.ToLower().Trim().Contains(getUnit(item?.ToLower().Trim()))))).ToList();
                var total_unit = lstUnitNameTotal?.Count();

                int untiBCAInspected = listIdUnitBcaInspected.Count();
                var ls1 = new { key = "BCA Inspected", value = untiBCAInspected + "/" + total_unit };
                listStatic.Add(ls1);

                int unitConditionallyHandedOver = listUnit.Count(u => u.Status == 98), // handover without condition
                    unitHandedOved = listHOInspected.Count(u => u.Status == 99 || u.Status == 98) - unitConditionallyHandedOver;

                int total_ho = unitHandedOved + unitConditionallyHandedOver;

                var ls4 = new { key = "Total handover", value = (unitHandedOved + unitConditionallyHandedOver) + "/" + (total_unit) };
                listStatic.Add(ls4);

                var ls2 = new { key = "Handed over without condition", value = unitHandedOved + "/" + total_unit };
                listStatic.Add(ls2);

                var ls3 = new { key = "Handed over with condition", value = unitConditionallyHandedOver + "/" + total_ho };
                listStatic.Add(ls3);

            }
            catch (Exception ce)
            {
                return Ok(new { ListStatic = listStatic, listUnit = listUnit, lstBlock = lstBlock }, message:
                    $"QaQcGetHandedOver Last time main function: {ce.Message} " +
                    $", List unit available: {JsonConvert.SerializeObject(lstUnit)}, strataBlock: {JsonConvert.SerializeObject(strataBlock)}" +
                    $", listIdUnitBcaInspected: {JsonConvert.SerializeObject(listIdUnitBcaInspected)}" +
                    $", listCheckDetail: {JsonConvert.SerializeObject(listCheckDetail)}" +
                    $", listRawHOInspected: {JsonConvert.SerializeObject(listRawHOInspected)}" +
                    $", listRawHONotInspected: {JsonConvert.SerializeObject(listRawHONotInspected)}");
            }

            return Ok(new { ListStatic = listStatic, listUnit = listUnit.OrderBy(x => x.Block).ToList(), lstBlock = lstBlock.OrderBy(x => x.Name).ToList() });
        }

        /// <summary>
        /// Get handed over
        /// </summary>
        /// <param name="SiteId"></param>
        /// <returns></returns>
        public async Task<ServiceResponse> QaQcGetHandedOverBlock(string SiteId)
        {
            using var connection = _dapperDigi.CreateConnection();
            var lstBlockAvaliableResult = new List<BlockResponse>();
            var lstBlock = new List<BlockResponse>();

            try
            {
                lstBlock = (await _blockService.GetBlock(SiteId)).OrderBy(x => x.Name).ToList();

                // query list modult
                string lstBlockAvaliableQuery = $"SELECT MUB.BLOCK " +
                    $"FROM MODULE_UNIT_BLOCK MUB WHERE MUB.SITE_ID = @SiteId AND MUB.IS_DELETED = FALSE GROUP BY BLOCK;";

                var lstBlockAvaliables = (await connection.QueryAsync(lstBlockAvaliableQuery, new
                {
                    SiteId = SiteId
                })).ToList();

                foreach (var lstBlockAvaliable in lstBlockAvaliables)
                {
                    var idBlock = lstBlock?.Find(x => lstBlockAvaliable?.block?.ToString().Replace("BLK", "").ToLower().TrimStart('0') == x?.Name?.ToString()?.TrimStart('0').ToLower());

                    if (idBlock != null)
                    {
                        lstBlockAvaliableResult.Add(idBlock);
                    }
                }
            }
            catch (Exception ce)
            {
                return Ok(new { LstBlockAvaliableResult = lstBlockAvaliableResult.Distinct().ToList(), lstBlock = lstBlock }, message:
                    $"QaQcGetHandedOver Last time block: {ce.Message} ");
            }

            return Ok(new { LstBlockAvaliableResult = lstBlockAvaliableResult.Distinct().OrderBy(x => x.Name).ToList(), lstBlock = lstBlock.Distinct().OrderBy(x => x.Name).ToList() });
        }


        /// <summary>
        /// func get score
        /// </summary>
        /// <param name="UnitName"></param>
        /// <param name="flagIsOffice">flag is office form is check according to reject or accept</param>
        /// <returns></returns>
        private static Decimal getScore(string UnitName, bool flagIsOffice)
        {
            try
            {
                decimal score = 0;
                EntQaQcBcaInspection ent = listUnitBcaInspected.FirstOrDefault(u => u.name == UnitName);

                if (ent != null)
                {
                    var checks = ent.histories;
                    decimal.TryParse(JsonConvert.DeserializeObject<History>(checks != null ? (checks[2] != null ? checks[2] : "{}") : "{}").Value, out score);
                    score = Math.Round(score, 2);
                }


                return score;
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        private static Tuple<string, string, int, List<string>, string> getInfoFromChecks(int originStatus,
            dynamic checks)
        {
            List<string> issueList = new List<string>();
            List<History> listChecks = new List<History>();
            int status = originStatus;
            string ArchiName = string.Empty,
                   QMName = string.Empty,
                   comment = string.Empty;

            try
            {
                foreach (string check in checks)
                {
                    listChecks.Add(JsonConvert.DeserializeObject<History>(check));
                }

                //find archi repsentative
                string idArchi = listCheckDetail?.Find(x => x.name.Contains("Name of Archi & M&E Representative"))?.id;

                var tempArchiName = listChecks.Find(x => x.Id == idArchi)?.Value;
                ArchiName = tempArchiName != null ? tempArchiName : "";

                //find QM repsentative
                string idQM = listCheckDetail?.Find(x => x.name.Contains("Name of QA & QM Representative"))?.id;
                var tempQMName = listChecks?.Find(x => x.Id == idQM)?.Value;
                QMName = tempQMName != null ? tempQMName : "";


                //find status and issue
                if (originStatus == 99)
                {
                    foreach (History item in listChecks)
                    {
                        if (item.Value != null && item.Value.Contains("Rejected"))
                        {
                            status = 98;
                            string a = listCheckDetail.Find(x => x.id == item.Id)?.name?.Replace("\t", String.Empty);
                            issueList.Add(a);
                        }
                    }
                }

                //find Comment
                string idComment = listCheckDetail.Find(x => x.name.Contains("Comments")).id;

                var tempComment = listChecks.Find(x => x.Id == idComment)?.Value;
                comment = tempComment != null ? tempComment : "";
            }
            catch (Exception ce)
            {
                Console.WriteLine($"getInfoFromChecks: {ce.Message}");
            }
            return Tuple.Create(ArchiName, QMName, status, issueList, comment);
        }


        private static bool getBcaInspected(string Id)
        {
            bool check = false;

            if (listIdUnitBcaInspected.Contains(Id))
            {
                check = true;
            }

            return check;
        }

        /// <summary>
        /// Func get full name unit
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (10.06.2023)
        private static string getUnitFullName(string Name)
        {
            string unit = "";

            try
            {
                if (Name != null)
                {
                    unit = Name.Substring(Name.IndexOf("-") + 1);
                }
            }
            catch (Exception ce)
            {
                throw ce;
            }

            return unit;
        }

        /// <summary>
        /// func get short name unit
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (10.06.2023)
        private static string getUnit(string Name)
        {
            string unit = "";

            try
            {
                if (Name != null)
                {
                    unit = Name.Split("-").Last();
                }
            }
            catch (Exception ce)
            {
                throw ce;
            }

            return unit;
        }
    }
}