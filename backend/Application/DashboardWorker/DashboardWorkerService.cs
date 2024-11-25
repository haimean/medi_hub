using AutoMapper;
using Dapper;
using DashboardApi.Application.Email;
using DashboardApi.Application.Email.Request;
using DashboardApi.Auth;
using DashboardApi.Auth.PermisionChecker;
using DashboardApi.DatabaseContext.AppDbcontext;
using DashboardApi.DatabaseContext.DapperDbContext;
using DashboardApi.Dtos.DashboardWorker.Response;
using DashboardApi.Dtos.QaQc.Responses;
using DashboardApi.HttpConfig;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Core.Operations;
using System.Net.Http.Headers;
using System.Text.Json;

namespace DashboardApi.Application.DashboardWorker
{
    public class DashboardWorkerService : Service, IDashboardWorkerService
    {
        private readonly AppMainDapperContext _appMainDapperContext;
        private readonly WorkerDbContext _db;
        private readonly WorkerDapperContext _workerDapper;
        private readonly ResourceCommonDapperContext _resourceCommonDapper;
        private readonly DigiCheckDapperContext _digiDapper;
        private readonly QaQcDapperContext _qaQcDapperContext;
        private readonly IPermissionChecker _permissionChecker;
        private readonly IOptions<HttpEndpoint> _options;
        private readonly IEmailService _emailService;
        public readonly IMapper _mapper;

        public DashboardWorkerService(WorkerDbContext db, WorkerDapperContext workerDapper,
            AppMainDapperContext appMainDapperContext,
            DigiCheckDapperContext digiDapper,
            IPermissionChecker permissionChecker,
            IMapper mapper,
            ResourceCommonDapperContext resourceCommonDapper,
            QaQcDapperContext qaQcDapperContext,
            IOptions<HttpEndpoint> options,
            IEmailService emailService)
        {
            _appMainDapperContext = appMainDapperContext;
            _db = db;
            _workerDapper = workerDapper;
            _digiDapper = digiDapper;
            _permissionChecker = permissionChecker;
            _mapper = mapper;
            _resourceCommonDapper = resourceCommonDapper;
            _qaQcDapperContext = qaQcDapperContext;
            _options = options;
            _emailService = emailService;
        }

        /// <summary>
        /// Get QM Cost Report
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (21.06.2023)
        public async Task<ServiceResponse> GetCostReport(string projectId, string request)
        {
            using var workerConnection = _workerDapper.CreateConnection();
            using var connectionQaqc = _qaQcDapperContext.CreateConnection();

            List<string> blockUnitInspecteds = new List<string>();
            SummaryCostCode data = new SummaryCostCode();
            string quyeryAverageScore = "SELECT SUM(\"Accommodation_fee\") AS rework_score, SUM(\"Transportdation_fee\") AS rework " +
                "FROM allowance WHERE projectid=@projectId and allowance.is_deleted IS NOT TRUE;";
            string messageData = "Success";
            string typeBlock = "Unit";

            try
            {
                if (!string.IsNullOrWhiteSpace(request))
                {
                    if ((request.StartsWith("{") && request.EndsWith("}")) || (request.StartsWith("[") && request.EndsWith("]")))
                    {
                        var blockRequest = JsonSerializer.Deserialize<Block>(request);

                        if (blockRequest != null)
                        {
                            typeBlock = blockRequest.name;
                        }
                    }
                }

                blockUnitInspecteds = await GetListInspected(projectId, typeBlock);
                string querySQL = "SELECT metadata FROM \"wa_dashboardData\" WHERE key='dataSummaryCostCodeWh' AND \"projectId\"=@projectId;";

                var s = await workerConnection.QuerySingleOrDefaultAsync<string>(querySQL, new { projectId = projectId });
                data = JsonSerializer.Deserialize<SummaryCostCode>(s != null ? s : "{}");
                List<string> descriptionQM = new List<string>()
                {
                    "BCA Inspection",
                    "Buffing/ Polishing",
                    "General Cleaning",
                    "March up",
                    "March Touch up",
                    "Marble Touch up",
                    "Putty (Localised)",
                    "Sanding and Touchup Work",
                    "Sanding",
                    "Silicon",
                    "Final cleaning",
                    "Reconditioning",
                    "Pointing",
                    "Silicon (",
                    "Water Proofing",
                    "Conquas Preparation",
                    "Any Other Work",
                    "Silicon Work",
                    "Painting Work"
                };

                data.costCodeDatasById = data.costCodeDatasById.Where(x => descriptionQM.Any(s => x.description.ToLower().Contains(s.ToLower()))).ToList();

                var t = await connectionQaqc.QuerySingleOrDefaultAsync(quyeryAverageScore, new { projectId = projectId });
                var rework = new
                {
                    reworkScore = t.rework_score,
                    rework = t.rework,
                };

                workerConnection.Close();
                workerConnection.Dispose();
                connectionQaqc.Dispose();
                connectionQaqc.Dispose();

                return Ok(new
                {
                    blockUnitInspecteds = blockUnitInspecteds,
                    blockUnitCosts = data,
                    rework
                }, message: $"{messageData}");
            }
            catch (Exception ce)
            {
                return Ok(new
                {
                    blockUnitInspecteds = blockUnitInspecteds,
                    blockUnitCosts = data,
                    rework = new
                    {
                        reworkScore = 0,
                        rework = 0,
                    }
                }, message: $"{ce.Message}, query: {quyeryAverageScore}, projectID: {projectId}");
            }

        }
        public async Task<List<DashboardProjectApplicable>> DashboardProjectApplicable()
        {
            //var accessToken = _currentUser.GetToken();
            string url = _options.Value.Https +"/qaqc/dashboardprojectapplicable";

            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.100 Safari/537.36");

            var res = await client.GetAsync(url);

            var dt = await res.Content.ReadFromJsonAsync<HttpRespon<List<DashboardProjectApplicable>>>();

            return dt.Data;
        }

        //FILTER QM DATA FROM WORKER APP
        /// <summary>
        /// Filter QM worker app
        /// </summary>
        /// <returns></returns>
        /// CreatedBy: NT Hieu (09.09.2024)
        /// QM Productivity
        public async Task<ServiceResponse> QaQcFilterQmFromWorkerApp()
        {
            try
            {
                // get the list of projects have permission to display in dashboard
                var applicableProjects = await DashboardProjectApplicable();
                var applicalbeProjectIds = applicableProjects.Where(p => p.Applicable.qm)
                                                             .Select(p => new { p.ProjectID, p.ProjectName})
                                                             .ToList();

                using var connectionWorker = _workerDapper.CreateConnection();

                string queryAllocation = "SELECT CONCAT('BLK', BLOCK, '_', UNIT) AS BLKUNITNAME, BLOCK AS BLOCKNAME, UNIT AS UNITNAME, NO_OF_SAMPLE, LEVEL AS LEVEL_NAME, DESCRIPTION, WORKER_NAME AS \"nameWorker\", HOURS AS TOTALHOURS, date, SUPERVISOR AS NAMESUPERVISOR, PROJECT_ID AS PROJECTID " +
                    "FROM WORK_TIME_DETAIL WHERE UNIT IS NOT NULL ORDER BY BLKUNITNAME;";
                List<UnitResponseSQL> listAllocation = (await connectionWorker.QueryAsync<UnitResponseSQL>(queryAllocation)).ToList();

                connectionWorker.Close();

                connectionWorker.Dispose();

                var lstProjectId = listAllocation?.GroupBy(x => x.projectId).ToList();
                var projectData = new List<Dictionary<string, object>>();

                foreach (var projectGroup in lstProjectId)
                {
                    using var connectionResourceCommon = _resourceCommonDapper.CreateConnection();

                    string currentProjectId = projectGroup.Key;

                    var applicableProject = applicalbeProjectIds.FirstOrDefault(p => p.ProjectID == currentProjectId);

                    if (applicableProject == null)
                        continue;

                    var projectName = applicableProject.ProjectName;

                    List<UnitResponseSQL> projectAllocation = projectGroup.ToList();

                    // handover date condition
                    DateTime currentDate = DateTime.Now;
                    var filteredAllocations = projectAllocation
                        .Where(x => x.date <= currentDate && x.date >= currentDate.AddDays(-7))
                        .ToList();

                    if (!filteredAllocations.Any())
                        continue;

                    // get the list of strata
                    string queryStrata = "SELECT PROJECT_ID AS PROJECTID, NAME AS STRATANAME, BLOCK_IDS AS BLOCKID, PRODUCTIVITY_TARGET AS PRODUCTIVITYTARGET " +
                        "FROM STRATAS WHERE PROJECT_ID = @PROJECTID AND PROJECT_ID IS NOT NULL AND IS_DELETED IS NOT TRUE ORDER BY STRATANAME;";
                    List<UnitResponseSQL> listStrata = (await connectionResourceCommon.QueryAsync<UnitResponseSQL>(queryStrata, new
                    {
                        ProjectId = currentProjectId
                    })).ToList();

                    connectionResourceCommon.Close();

                    connectionResourceCommon.Dispose();

                    var strataData = new List<Dictionary<string, object>>();

                    foreach (var strataGroup in listStrata.GroupBy(x => x.strataName))
                    {
                        string currentStrata = strataGroup.Key;
                        var productivityTarget = strataGroup.Select(x => x.productivityTarget).FirstOrDefault();

                        var blockData = new List<Dictionary<string, object>>();

                        var lstBlockName = projectAllocation?.GroupBy(x => x.blockName).Select(x => x.Key).ToList();

                        foreach (string blkName in lstBlockName)
                        {
                            var unitData = new List<object>();

                            var lstBlockUnitName = projectAllocation?
                                .Where(x => x.blockName == blkName)
                                .GroupBy(x => x.blkunitname)
                                .Select(x => x.Key)
                                .ToList();

                            var mapU = new Dictionary<string, List<UnitResponseSQL>>();
                            mapU = listAllocation?.GroupBy(x => x?.blkunitname).ToDictionary(g => g?.Key, g => g?.ToList());
                            foreach (string unit in lstBlockUnitName)
                            {
                                UnitInfo ui = new UnitInfo();
                                var allocations = projectAllocation.Where(x => x.blkunitname == unit).ToList();

                                if (allocations.Count > 0)
                                {
                                    ui.unitName = allocations[0].unitName;
                                    ui.blockName = allocations[0].blockName;
                                    ui.noOfSample = allocations[0].noOfSample;

                                    //qm work
                                    double qmwork_hours = allocations.Where(e =>
                                        e.description.ToLower().Contains("BCA Inspection".ToLower()) ||
                                        e.description.ToLower().Contains("Buffing/ Polishing".ToLower()) ||
                                        e.description.ToLower().Contains("General Cleaning".ToLower()) ||
                                        e.description.ToLower().Contains("March up".ToLower()) ||
                                        e.description.ToLower().Contains("March Touch up".ToLower()) ||
                                        e.description.ToLower().Contains("Marble Touch up".ToLower()) ||
                                        e.description.ToLower().Contains("Putty (Localised)".ToLower()) ||
                                        e.description.ToLower().Contains("Sanding and Touchup Work".ToLower()) ||
                                        e.description.ToLower().Contains("Sanding".ToLower()) ||
                                        e.description.ToLower().Contains("Silicon".ToLower())
                                    ).Select(x => x.totalHours).Sum();

                                    ui.qmwork = Math.Round(qmwork_hours, 2);

                                    //recon work
                                    double rework_hours = allocations.Where(e =>
                                                                                e.description.ToLower().Contains("Final cleaning".ToLower()) ||
                                                                                e.description.ToLower().Contains("Reconditioning".ToLower())
                                                                            ).Select(x => x.totalHours).Sum();

                                    ui.reconwork = Math.Round(rework_hours, 1);

                                    //backcharge work
                                    double bc_hours = allocations.Where(e =>
                                                                        e.description.ToLower().Contains("Pointing".ToLower()) ||
                                                                        e.description.ToLower().Contains("Silicon (".ToLower())
                                                                    ).Select(x => x.totalHours).Sum();
                                    ui.bcwork = Math.Round(bc_hours, 1);

                                    //water proofing
                                    double waterPoofing = allocations.Where(e =>
                                                                        e.description.ToLower().Contains("Water Proofing".ToLower())
                                                                    ).Select(x => x.totalHours).Sum();
                                    ui.waterProofing = Math.Round(bc_hours, 1);

                                    //other work
                                    double other_hours = allocations.Where(e =>
                                        e.description == "Other Work"
                                    ).Select(x => x.totalHours).Sum();
                                    ui.otherwork = Math.Round(other_hours, 1);

                                    //other %
                                    ui.ms = 0;
                                    ui.msQR = 0;
                                    ui.total = Math.Round(qmwork_hours + rework_hours + bc_hours + other_hours, 1);

                                    if (ui.noOfSample > 0)
                                    {
                                        double ms = (double)(ui.total / ui.noOfSample / 11);
                                        ui.ms = Math.Round(ms, 1);

                                        double msQR = (double)((ui.qmwork + ui.reconwork) / ui.noOfSample / 11);
                                        ui.msQR = Math.Round(msQR, 1);
                                    }

                                    if (ui.ms > productivityTarget)
                                    {
                                        unitData.Add(new
                                        {
                                            blockName = ui.blockName,
                                            unitName = ui.unitName,
                                            msQR = ui.msQR,
                                            ms = ui.ms
                                        });
                                    }
                                }
                            }

                            blockData.Add(new Dictionary<string, object>
                            {
                                {
                                    blkName,
                                    unitData
                                }
                            });
                        }

                        strataData.Add(new Dictionary<string, object>()
                        {
                            {currentStrata, blockData },
                            {projectName, projectName }
                        });
                    }
                    projectData.Add(new Dictionary<string, object> {
                        {currentProjectId, strataData }
                    });
                }


                // 5. get list users have permission QAQC ADMIN with project
                using var connectionAppMain = _appMainDapperContext.CreateConnection();

                string queryPermissionChecker = "SELECT DISTINCT USERS.EMAIL, USERS.SHORT_NAME, USERS.NAME, PROJECTS.ID AS projectId, PROJECTS.NAME AS projectName " +
                    "FROM USER_PROJECT_APP_ROLE " +
                    "JOIN USERS ON USERS.ID = USER_PROJECT_APP_ROLE.USER_ID " +
                    "JOIN PROJECTS ON PROJECTS.ID = USER_PROJECT_APP_ROLE.PROJECT_ID " +
                    "JOIN APPS ON APPS.ID = USER_PROJECT_APP_ROLE.APP_ID " +
                    "JOIN ROLE_PERMISSION ON ROLE_PERMISSION.ID = USER_PROJECT_APP_ROLE.ROLE_ID " +
                    "WHERE USER_PROJECT_APP_ROLE.IS_DELETED != TRUE " +
                    "AND ROLE_PERMISSION.IS_DELETED != TRUE " +
                    "AND PROJECTS.IS_DELETED != TRUE " +
                    "AND USERS.IS_DELETED != TRUE " +
                    "AND APPS.IS_DELETED != TRUE " +
                    "AND APPS.NAME = 'QAQC' " +
                    "AND ROLE_PERMISSION.NAME = 'QAQC ADMIN'";

                List<UserProject> userPermissionWithProjects = (await connectionAppMain.QueryAsync<UserProject>(queryPermissionChecker)).ToList();

                connectionAppMain.Close();

                connectionAppMain.Dispose();

                // 6. Send email userPermissionWithProjects, projectData

                foreach (var user in userPermissionWithProjects)
                {
                    var projectPermission = projectData.Find(x => x.Keys.FirstOrDefault() == user.ProjectID);
                    if (projectPermission == null) continue;

                    var projectStrataData = (List<Dictionary<string, object>>)projectPermission[user.ProjectID];

                    // Start building the table
                    var tableHtml = "<table border='1' cellpadding='5' cellspacing='0'><thead><tr><th>Block</th><th>Unit</th><th>Man-Day/Sample</</th><th>M/S (QM + Recon)</th></tr></thead><tbody>";

                    // Iterate over each strata
                    foreach (var strata in projectStrataData)
                    {
                        foreach (var block in strata.Values.FirstOrDefault() as List<Dictionary<string, object>>)
                        {
                            // Extract block name and unit data
                            var blockName = block.Keys.First();
                            var unitData = block[blockName] as List<object>;

                            foreach (var unit in unitData)
                            {
                                // Convert the unit object into a dictionary to extract unit properties
                                var unitDict = unit as dynamic;
                                tableHtml += $"<tr><td style='text-align: left;'>{unitDict.blockName}</td><td style='text-align: left;'>{unitDict.unitName}</td><td style='text-align: left;'>{unitDict.msQR}</td><td style='text-align: left;'>{unitDict.ms}</td></tr>";
                            }
                        }
                    }

                    // Close the table
                    tableHtml += "</tbody></table>";

                    var emailRequest = new SendEmailRequest
                    {
                        To = new List<string> { user.Email },
                        //To = new List<string>() { "hieu.ngtr03@gmail.com" },
                        Subject = "test",
                        EmailContent = $@"Dear {user.Name} (QAQC Admin role),<br><br>
                                          
                                          The following projects productivity is below their target<br><br>
                                          
                                          Project Name: {user.ProjectName}<br><br>

                                          {tableHtml}<br><br>

                                          Please login to <a href ='https://iddv2.wohhup.com/dashboard/qaqc/sub-qm'>QAQC Dashboard</a> to view detailed activities",
                    };

                    var resultSendMail = await _emailService.SendEmailTemplate(emailRequest);
                }

                return Ok(projectData);
            }
            catch (Exception ce)
            {
                return Ok(new
                {
                    message = ce.Message
                });
            }
        }


        //QM DATA FROM WORKER APP
        /// <summary>
        /// Get QM worker app
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (05.02.2023)
        /// QM Productivity
        public async Task<ServiceResponse> QaQcGetQmFromWorkerApp(string siteId, string blockName)
        {
            //Static for dashboard
            List<EntStatic> listStatic = new List<EntStatic>();
            List<string> list_units_inspected_noworker = new List<string>();

            //get productivity range
            List<ProductivityRane> list_productivity_range = new List<ProductivityRane>();
            List<UnitInfo> list_unit_info = new List<UnitInfo>();

            try
            {
                //get list timesheet and activities
                using var connectionWorker = _workerDapper.CreateConnection();

                // get list worker time
                string queryAllocation = "SELECT CONCAT('BLK', BLOCK, '_', UNIT) AS BLKUNITNAME, BLOCK AS BLOCKNAME, UNIT AS UNITNAME, NO_OF_SAMPLE, LEVEL AS LEVEL_NAME, DESCRIPTION, WORKER_NAME AS \"nameWorker\", HOURS AS TOTALHOURS, date, SUPERVISOR AS NAMESUPERVISOR " +
                    "FROM WORK_TIME_DETAIL WHERE PROJECT_ID = @SITEID AND UNIT IS NOT NULL ORDER BY BLKUNITNAME;";
                List<UnitResponseSQL> list_allocation = (await connectionWorker.QueryAsync<UnitResponseSQL>(queryAllocation, new
                {
                    SiteId = siteId
                })).ToList();

                if (blockName.ToLower() != "All Block".ToLower() && blockName.ToLower() != "overall")
                {
                    blockName = blockName.Replace("BLK ", "");
                    list_allocation = list_allocation.Where(x => !string.IsNullOrEmpty(x.blockName) && !string.IsNullOrEmpty(blockName) && x.blockName?.ToLower().TrimStart('0')?.Trim() == blockName.ToLower()?.TrimStart('0')?.Trim()).ToList();
                }

                var lstBlockUnitName = list_allocation?.GroupBy(x => x.blkunitname).Select(x => x.Key).ToList();

                // convert list to map
                var mapU = new Dictionary<string, List<UnitResponseSQL>>();
                mapU = list_allocation?.GroupBy(x => x?.blkunitname).ToDictionary(g => g?.Key, g => g?.ToList());
                foreach (string unit in lstBlockUnitName)
                {
                    UnitInfo ui = new UnitInfo();
                    ui.Unit = unit;
                    ui.noOfSample = 0;
                    var allocations = list_allocation?.Where(x => x.blkunitname.ToLower().Contains(unit.ToLower())).ToList();
                    if (allocations.Count > 0)
                    {
                        ui.unitName = allocations[0].unitName;
                        ui.blockName = allocations[0].blockName;
                        ui.noOfSample = allocations[0].noOfSample;
                        ui.dateTime = allocations.MaxBy(d => d.date)!.date;

                        //qm work
                        double qmwork_hours = allocations.Where(e =>
                                                                e.description.ToLower().Contains("BCA Inspection".ToLower()) ||
                                                                e.description.ToLower().Contains("Buffing/ Polishing".ToLower()) ||
                                                                e.description.ToLower().Contains("General Cleaning".ToLower()) ||
                                                                e.description.ToLower().Contains("March up".ToLower()) ||
                                                                e.description.ToLower().Contains("March Touch up".ToLower()) ||
                                                                e.description.ToLower().Contains("Marble Touch up".ToLower()) ||
                                                                e.description.ToLower().Contains("Putty (Localised)".ToLower()) ||
                                                                e.description.ToLower().Contains("Sanding and Touchup Work".ToLower()) ||
                                                                e.description.ToLower().Contains("Sanding".ToLower()) ||
                                                                e.description.ToLower().Contains("Silicon".ToLower())).Select(x => x.totalHours).Sum();

                        ui.qmwork = Math.Round(qmwork_hours, 2);

                        //recon work
                        double rework_hours = allocations.Where(e =>
                                                                    e.description.ToLower().Contains("Final cleaning".ToLower()) ||
                                                                    e.description.ToLower().Contains("Reconditioning".ToLower())
                                                                ).Select(x => x.totalHours).Sum();

                        ui.reconwork = Math.Round(rework_hours, 1);

                        //backcharge work
                        double bc_hours = allocations.Where(e =>
                                                            e.description.ToLower().Contains("Pointing".ToLower()) ||
                                                            e.description.ToLower().Contains("Silicon (".ToLower())
                                                        ).Select(x => x.totalHours).Sum();
                        ui.bcwork = Math.Round(bc_hours, 1);

                        //water proofing
                        double waterPoofing = allocations.Where(e =>
                                                            e.description.ToLower().Contains("Water Proofing".ToLower())
                                                        ).Select(x => x.totalHours).Sum();
                        ui.waterProofing = Math.Round(bc_hours, 1);

                        //other work
                        double other_hours = allocations.Where(e =>
                            e.description == "Other Work"
                        ).Select(x => x.totalHours).Sum();
                        ui.otherwork = Math.Round(other_hours, 1);

                        //other %
                        ui.ms = 0;
                        ui.msQR = 0;
                        ui.total = Math.Round(qmwork_hours + rework_hours + bc_hours + other_hours, 1);

                        if (ui.noOfSample > 0)
                        {
                            double ms = (double)(ui.total / ui.noOfSample / 11);
                            ui.ms = Math.Round(ms, 1);

                            double msQR = (double)((ui.qmwork + ui.reconwork) / ui.noOfSample / 11);
                            ui.msQR = Math.Round(msQR, 1);
                        }

                        mapU[ui.Unit].Sort((x, y) => x.date.CompareTo(y.date));

                        var listUnitDetail = mapU[ui.Unit].Select(
                            e =>
                            {
                                var duration = Math.Round(e.totalHours, 1);
                                return new UnitDetail(e.description, duration, e.date,
                                    e.nameWorker, e.nameSupervisor);
                            }
                        ).ToList();
                        ui.listUnitDetail = listUnitDetail;
                        list_unit_info.Add(ui);
                    }
                    else
                    {
                        list_units_inspected_noworker.Add(unit);
                    }
                }

                list_productivity_range.Add(new ProductivityRane("<0.79", list_unit_info.Count(x => x.msQR < 0.8)));
                list_productivity_range.Add(new ProductivityRane("0.80-0.89", list_unit_info.Count(x => x.msQR >= 0.8 && x.msQR < 0.9)));
                list_productivity_range.Add(new ProductivityRane("0.90-0.99", list_unit_info.Count(x => x.msQR >= 0.9 && x.msQR < 1)));
                list_productivity_range.Add(new ProductivityRane(">1", list_unit_info.Count(x => x.msQR >= 1)));

                //avrage manday-sample for QR (by average of unit)
                int no_of_samples = (int)list_unit_info.Select(x => x.noOfSample).Sum();
                double no_of_manday = list_unit_info.Select(x => x.total).Sum() / 11;

                double AverageMsQR = no_of_samples > 0 ? Math.Round((double)no_of_manday / no_of_samples, 1) : 0;
                listStatic.Add(new EntStatic("man-day/sample", AverageMsQR.ToString()));

                // no of unit
                listStatic.Add(new EntStatic("units (QM work started)", list_unit_info.Count().ToString()));

                // no of unit
                listStatic.Add(new EntStatic("samples", no_of_samples.ToString()));

                // no of man-day
                listStatic.Add(new EntStatic("man-days", Math.Round(no_of_manday).ToString()));
            }
            catch (Exception ce)
            {
                return Ok(new
                {
                    listStatic = listStatic,
                    list_productivity_range = list_productivity_range,
                    list_unit_info = list_unit_info
                }, message: ce.Message);
            }

            return Ok(new
            {
                listStatic = listStatic,
                list_productivity_range = list_productivity_range,
                list_unit_info = list_unit_info
            });
        }

        //QM DATA FROM JOT FORM DATA
        public async Task<ServiceResponse> QaQcGetQmFromJotFormData(string project_code)
        {
            //get list timesheet and activities
            using var connectionWorker = _workerDapper.CreateConnection();
            var parameters = new { project_code = project_code };

            var query_JotData =
               "SELECT qm.\"Unit\",qm.\"createdAt\" ,qm.\"WorkType\",qm.\"Worker\",qm.\"Activity\",qm.\"Foreman\",qm.\"Hours\",cu.no_of_sample " +
               "FROM qaqc_qm qm " +
               "LEFT JOIN common_units cu ON cu.unit_id = qm.unit_id " +
               "WHERE qm.project_code = @project_code " +
               "AND qm.\"Unit\" NOT LIKE '%T%' " +
               "AND qm.\"Unit\" <> '' " +
               "ORDER BY qm.\"Unit\" ";

            List<EntQaQcQmJot> listRecordFromJot = (await connectionWorker.QueryAsync<EntQaQcQmJot>(query_JotData, parameters)).ToList();

            var query_ListGroupByUnit =
               "SELECT qm.\"Unit\", cu.no_of_sample, " +
               "SUM(CASE WHEN qm.\"WorkType\" = 'QM Work' then qm.\"Hours\" else 0 END) qmwork, " +
               "SUM(CASE WHEN qm.\"WorkType\" = 'Reconditioning (Final)' then qm.\"Hours\" else 0 END) reconwork, " +
               "SUM(CASE WHEN qm.\"WorkType\" = 'Work to Backcharge' then qm.\"Hours\" else 0 END) bcwork, " +
               "SUM(CASE WHEN qm.\"WorkType\" = 'Other Work' then qm.\"Hours\" else 0 END) otherwork " +
               "FROM qaqc_qm qm " +
               "LEFT JOIN common_units cu ON cu.unit_id = qm.unit_id " +
               "WHERE qm.project_code = @project_code " +
               "AND cu.no_of_sample >0 " +
               "AND qm.\"Unit\" NOT LIKE '%T%' " +
               "AND qm.\"Unit\" <> '' " +
               "GROUP BY qm.\"Unit\" , cu.no_of_sample " +
               "ORDER BY qm.\"Unit\" ";

            List<EntQaQcQmJotGroupByUnit> list_group_by_unit = new List<EntQaQcQmJotGroupByUnit>();
            list_group_by_unit = (await connectionWorker.QueryAsync<EntQaQcQmJotGroupByUnit>(query_ListGroupByUnit, parameters)).ToList();

            // hard code for handy project
            if (project_code == "HAN")
            {
                list_group_by_unit = HardCodeProject("HAN");
            }

            //GET DATA FOR PRODUCTIVITY CHART
            List<object> list_p_range = new List<object>()
            {
                new { name = "<0.75", count = list_group_by_unit.Count(c => c.ms < 0.75) },
                new { name = "0.75-1.0", count = list_group_by_unit.Count(c => c.ms >= 0.75 && c.ms<1) },
                new { name = "1.0-1.25", count = list_group_by_unit.Count(c => c.ms >= 1 && c.ms<1.25) },
                new { name = "1.25-1.5", count = list_group_by_unit.Count(c => c.ms >= 1.25 && c.ms<1.5) },
                new { name = ">=1.5", count = list_group_by_unit.Count(c => c.ms >= 1.5)  }
            };

            //GET STATIC INFO

            var static_info = new List<object>();
            var total_hrs = list_group_by_unit.Sum(e => e.total);
            var total_sample = list_group_by_unit.Sum(e => e.no_of_sample);

            double productivity = 0;
            if (total_sample > 0)
            {
                double p = Convert.ToDouble(total_hrs) / Convert.ToDouble(total_sample) / 11;
                productivity = Math.Round(p, 2);
            }

            static_info.Add(new { key = "man-day/sample", value = productivity });
            static_info.Add(new { key = "units", value = list_group_by_unit.Count.ToString("N0") });
            static_info.Add(new { key = "samples", value = total_sample.ToString("N0") });
            static_info.Add(new { key = "man-days", value = (total_hrs / 11).ToString("N0") });
            static_info.Add(new { key = "average cost per unit", value = $"{Math.Round(((double)(102 * (double)(total_hrs / 11)) / list_group_by_unit.Count), 2)}$" });

            return Ok(new
            {

                list_group_by_unit = list_group_by_unit,
                listStatic = static_info,
                list_productivity_range = list_p_range,
                list_unit_info = listRecordFromJot
            });
        }

        /// <summary>
        /// Get list inspected for worker app
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        private async Task<List<string>> GetListInspected(string siteId, string typeBlock)
        {
            using var connectionDigi = _digiDapper.CreateConnection();
            List<string> listUnitBcaInspected = new List<string>();

            try
            {
                string listUnitsQuery = "SELECT unit.\"name\" FROM \"module_unit_block\" unit " +
                                        "INNER JOIN \"module_step\" step ON unit.\"id\"=step.\"module_id\" " +
                                        "WHERE unit.\"site_id\"=@SiteId AND unit.\"is_deleted\"=false " +
                                        "AND step.\"step_id\"=@StepId AND step.\"status\"=99 " +
                                        "ORDER BY unit.\"name\";";

                if (typeBlock.ToLower() == "Office".ToLower() || typeBlock.ToLower() == "Retail".ToLower() || typeBlock.ToLower() == "All Block".ToLower())
                {
                    listUnitsQuery = "SELECT unit.\"name\" FROM \"module_unit_block\" unit " +
                                        "INNER JOIN \"module_step\" step ON unit.\"id\"=step.\"module_id\" " +
                                        "WHERE unit.\"site_id\"=@SiteId AND unit.\"is_deleted\"=false " +
                                        "ORDER BY unit.\"name\";";

                    var parameters = new { SiteId = siteId };

                    listUnitBcaInspected = (await connectionDigi.QueryAsync<string>(listUnitsQuery, parameters)).ToList();
                }
                else
                {
                    string formTemplate = "QAQC-Unit-BCA-Inspection";

                    string stepQuery = $"SELECT \"step\".\"id\" FROM \"step\" " +
                        $"INNER JOIN \"form_template\" ON \"form_template\".\"id\" = \"step\".\"form_id\" " +
                        $"WHERE \"form_template\".\"name\" = '{formTemplate}' AND \"type\" = 'QaQc-Unit';";

                    var step = await connectionDigi.QueryFirstOrDefaultAsync(stepQuery);

                    var parameters = new { SiteId = siteId, StepId = step.id };

                    listUnitBcaInspected = (await connectionDigi.QueryAsync<string>(listUnitsQuery, parameters)).ToList();
                }


            }
            catch (Exception ce)
            {
                throw ce;
            }

            return listUnitBcaInspected;
        }

        /// <summary>
        /// hard code for handy project
        /// </summary>
        /// <param name="projectCode"></param>
        /// <returns></returns>
        public List<EntQaQcQmJotGroupByUnit> HardCodeProject(string projectCode)
        {
            List<EntQaQcQmJotGroupByUnit> result = new List<EntQaQcQmJotGroupByUnit>()
            {
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-03-01",
                qmwork =  32,
                reconwork =  36,
                bcwork =  5,
                otherwork =  0,
                Hours =  73,
                Noofday =  7.3,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-03-02",
                qmwork =  32,
                reconwork =  22,
                bcwork =  5,
                otherwork =  0,
                Hours =  59,
                Noofday =  5.9,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-03-03",
                qmwork =  52,
                reconwork =  25,
                bcwork =  5,
                otherwork =  0,
                Hours =  82,
                Noofday =  8.2,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-03-04",
                qmwork =  33,
                reconwork =  28,
                bcwork =  5,
                otherwork =  0,
                Hours =  66,
                Noofday =  6.6,
                no_of_sample =  6
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-03-05",
                qmwork =  30,
                reconwork =  26,
                bcwork =  5,
                otherwork =  0,
                Hours =  61,
                Noofday =  6.1,
                no_of_sample =  6
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-03-06",
                qmwork =  34,
                reconwork =  32,
                bcwork =  5,
                otherwork =  0,
                Hours =  71,
                Noofday =  7.1,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-03-07",
                qmwork =  36,
                reconwork =  36,
                bcwork =  5,
                otherwork =  0,
                Hours =  77,
                Noofday =  7.7,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-03-08",
                qmwork =  48,
                reconwork =  24,
                bcwork =  6,
                otherwork =  0,
                Hours =  78,
                Noofday =  7.8,
                no_of_sample =  9
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-03-09",
                qmwork =  46,
                reconwork =  26,
                bcwork =  6,
                otherwork =  0,
                Hours =  78,
                Noofday =  7.8,
                no_of_sample =  9
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-03-10",
                qmwork =  40,
                reconwork =  28,
                bcwork =  6,
                otherwork =  0,
                Hours =  74,
                Noofday =  7.4,
                no_of_sample =  8
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-03-11",
                qmwork =  38,
                reconwork =  36,
                bcwork =  5,
                otherwork =  0,
                Hours =  79,
                Noofday =  7.9,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-03-12",
                qmwork =  48,
                reconwork =  38,
                bcwork =  6,
                otherwork =  0,
                Hours =  92,
                Noofday =  9.2,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-03-13",
                qmwork =  48,
                reconwork =  28,
                bcwork =  6,
                otherwork =  0,
                Hours =  82,
                Noofday =  8.2,
                no_of_sample =  9
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-03-14",
                qmwork =  33,
                reconwork =  26,
                bcwork =  5,
                otherwork =  0,
                Hours =  64,
                Noofday =  6.4,
                no_of_sample =  5
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-04-01",
                qmwork =  33,
                reconwork =  22,
                bcwork =  4,
                otherwork =  0,
                Hours =  59,
                Noofday =  5.9,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-04-02",
                qmwork =  38,
                reconwork =  24,
                bcwork =  5,
                otherwork =  0,
                Hours =  67,
                Noofday =  6.7,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-04-03",
                qmwork =  32,
                reconwork =  28,
                bcwork =  5,
                otherwork =  0,
                Hours =  65,
                Noofday =  6.5,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-04-04",
                qmwork =  49,
                reconwork =  22,
                bcwork =  5,
                otherwork =  0,
                Hours =  76,
                Noofday =  7.6,
                no_of_sample =  6
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-04-05",
                qmwork =  46,
                reconwork =  22,
                bcwork =  5,
                otherwork =  0,
                Hours =  73,
                Noofday =  7.3,
                no_of_sample =  6
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-04-06",
                qmwork =  33,
                reconwork =  30,
                bcwork =  5,
                otherwork =  0,
                Hours =  68,
                Noofday =  6.8,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-04-07",
                qmwork =  28,
                reconwork =  28,
                bcwork =  5,
                otherwork =  0,
                Hours =  61,
                Noofday =  6.1,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-04-08",
                qmwork =  57,
                reconwork =  28,
                bcwork =  5,
                otherwork =  0,
                Hours =  90,
                Noofday =  9.09,
                no_of_sample =  9
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-04-09",
                qmwork =  50,
                reconwork =  28,
                bcwork =  6,
                otherwork =  0,
                Hours =  84,
                Noofday =  8.48,
                no_of_sample =  9
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-04-10",
                qmwork =  36,
                reconwork =  35,
                bcwork =  6,
                otherwork =  0,
                Hours =  77,
                Noofday =  7.7,
                no_of_sample =  8
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-04-11",
                qmwork =  33,
                reconwork =  26,
                bcwork =  6,
                otherwork =  0,
                Hours =  65,
                Noofday =  6.5,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-04-12",
                qmwork =  28,
                reconwork =  28,
                bcwork =  5,
                otherwork =  0,
                Hours =  61,
                Noofday =  6.1,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-04-13",
                qmwork =  50,
                reconwork =  26,
                bcwork =  6,
                otherwork =  0,
                Hours =  82,
                Noofday =  8.2,
                no_of_sample =  9
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-04-14",
                qmwork =  40,
                reconwork =  25,
                bcwork =  6,
                otherwork =  0,
                Hours =  71,
                Noofday =  7.1,
                no_of_sample =  5
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-05-01",
                qmwork =  30,
                reconwork =  22,
                bcwork =  5,
                otherwork =  0,
                Hours =  57,
                Noofday =  5.7,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-05-02",
                qmwork =  30,
                reconwork =  24,
                bcwork =  5,
                otherwork =  0,
                Hours =  59,
                Noofday =  5.9,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-05-03",
                qmwork =  32,
                reconwork =  22,
                bcwork =  5,
                otherwork =  0,
                Hours =  59,
                Noofday =  5.9,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-05-04",
                qmwork =  47,
                reconwork =  24,
                bcwork =  5,
                otherwork =  0,
                Hours =  76,
                Noofday =  7.6,
                no_of_sample =  6
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-05-05",
                qmwork =  44,
                reconwork =  29,
                bcwork =  5,
                otherwork =  0,
                Hours =  78,
                Noofday =  7.8,
                no_of_sample =  6
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-05-06",
                qmwork =  32,
                reconwork =  26,
                bcwork =  5,
                otherwork =  0,
                Hours =  63,
                Noofday =  6.3,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-05-07",
                qmwork =  33,
                reconwork =  28,
                bcwork =  5,
                otherwork =  0,
                Hours =  66,
                Noofday =  6.6,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-05-08",
                qmwork =  50,
                reconwork =  24,
                bcwork =  5,
                otherwork =  0,
                Hours =  79,
                Noofday =  7.9,
                no_of_sample =  9
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-05-09",
                qmwork =  48,
                reconwork =  28,
                bcwork =  6,
                otherwork =  0,
                Hours =  82,
                Noofday =  8.2,
                no_of_sample =  9
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-05-10",
                qmwork =  38,
                reconwork =  28,
                bcwork =  6,
                otherwork =  0,
                Hours =  72,
                Noofday =  7.2,
                no_of_sample =  8
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-05-11",
                qmwork =  48,
                reconwork =  20,
                bcwork =  6,
                otherwork =  0,
                Hours =  74,
                Noofday =  7.4,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-05-12",
                qmwork =  38,
                reconwork =  22,
                bcwork =  5,
                otherwork =  0,
                Hours =  65,
                Noofday =  6.5,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-05-13",
                qmwork =  60,
                reconwork =  30,
                bcwork =  6,
                otherwork =  0,
                Hours =  96,
                Noofday =  9.6,
                no_of_sample =  9
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-05-14",
                qmwork =  36,
                reconwork =  25,
                bcwork =  6,
                otherwork =  0,
                Hours =  67,
                Noofday =  6.7,
                no_of_sample =  5
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-06-01",
                qmwork =  33,
                reconwork =  40,
                bcwork =  5,
                otherwork =  0,
                Hours =  78,
                Noofday =  7.8,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-06-02",
                qmwork =  28,
                reconwork =  30,
                bcwork =  5,
                otherwork =  0,
                Hours =  63,
                Noofday =  6.3,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-06-03",
                qmwork =  28,
                reconwork =  30,
                bcwork =  5,
                otherwork =  0,
                Hours =  63,
                Noofday =  6.3,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-06-04",
                qmwork =  40,
                reconwork =  22,
                bcwork =  5,
                otherwork =  0,
                Hours =  67,
                Noofday =  6.7,
                no_of_sample =  6
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-06-05",
                qmwork =  40,
                reconwork =  29,
                bcwork =  5,
                otherwork =  0,
                Hours =  74,
                Noofday =  7.4,
                no_of_sample =  6
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-06-06",
                qmwork =  48,
                reconwork =  32,
                bcwork =  5,
                otherwork =  0,
                Hours =  85,
                Noofday =  8.5,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-06-07",
                qmwork =  46,
                reconwork =  32,
                bcwork =  5,
                otherwork =  0,
                Hours =  83,
                Noofday =  8.3,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-06-08",
                qmwork =  48,
                reconwork =  30,
                bcwork =  5,
                otherwork =  0,
                Hours =  83,
                Noofday =  8.3,
                no_of_sample =  9
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-06-09",
                qmwork =  48,
                reconwork =  28,
                bcwork =  6,
                otherwork =  0,
                Hours =  82,
                Noofday =  8.2,
                no_of_sample =  9
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-06-10",
                qmwork =  36,
                reconwork =  30,
                bcwork =  6,
                otherwork =  0,
                Hours =  72,
                Noofday =  7.2,
                no_of_sample =  8
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-06-11",
                qmwork =  40,
                reconwork =  33,
                bcwork =  6,
                otherwork =  0,
                Hours =  79,
                Noofday =  7.9,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-06-12",
                qmwork =  48,
                reconwork =  32,
                bcwork =  5,
                otherwork =  0,
                Hours =  85,
                Noofday =  8.5,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-06-13",
                qmwork =  34,
                reconwork =  26,
                bcwork =  6,
                otherwork =  0,
                Hours =  66,
                Noofday =  6.6,
                no_of_sample =  9
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-06-14",
                qmwork =  33,
                reconwork =  28,
                bcwork =  6,
                otherwork =  0,
                Hours =  67,
                Noofday =  6.7,
                no_of_sample =  6
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-07-01",
                qmwork =  48,
                reconwork =  34,
                bcwork =  5,
                otherwork =  0,
                Hours =  87,
                Noofday =  8.7,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-07-02",
                qmwork =  48,
                reconwork =  28,
                bcwork =  4,
                otherwork =  0,
                Hours =  80,
                Noofday =  8,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-07-03",
                qmwork =  46,
                reconwork =  18,
                bcwork =  5,
                otherwork =  0,
                Hours =  69,
                Noofday =  6.9,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-07-04",
                qmwork =  48,
                reconwork =  55,
                bcwork =  5,
                otherwork =  0,
                Hours =  108,
                Noofday =  10.8,
                no_of_sample =  6
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-07-05",
                qmwork =  48,
                reconwork =  34,
                bcwork =  4,
                otherwork =  0,
                Hours =  86,
                Noofday =  8.6,
                no_of_sample =  6
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-07-06",
                qmwork =  45,
                reconwork =  22,
                bcwork =  5,
                otherwork =  0,
                Hours =  72,
                Noofday =  7.2,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-07-07",
                qmwork =  48,
                reconwork =  28,
                bcwork =  5,
                otherwork =  0,
                Hours =  81,
                Noofday =  8.1,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-07-08",
                qmwork =  44,
                reconwork =  26,
                bcwork =  4,
                otherwork =  0,
                Hours =  74,
                Noofday =  7.4,
                no_of_sample =  9
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-07-09",
                qmwork =  42,
                reconwork =  28,
                bcwork =  5,
                otherwork =  0,
                Hours =  75,
                Noofday =  7.5,
                no_of_sample =  9
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-07-10",
                qmwork =  38,
                reconwork =  34,
                bcwork =  6,
                otherwork =  0,
                Hours =  78,
                Noofday =  7.8,
                no_of_sample =  8
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-07-11",
                qmwork =  32,
                reconwork =  18,
                bcwork =  6,
                otherwork =  0,
                Hours =  56,
                Noofday =  5.6,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-07-12",
                qmwork =  56,
                reconwork =  20,
                bcwork =  6,
                otherwork =  0,
                Hours =  82,
                Noofday =  8.2,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-07-13",
                qmwork =  40,
                reconwork =  28,
                bcwork =  5,
                otherwork =  0,
                Hours =  73,
                Noofday =  7.3,
                no_of_sample =  9
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-07-14",
                qmwork =  30,
                reconwork =  18,
                bcwork =  6,
                otherwork =  0,
                Hours =  54,
                Noofday =  5.4,
                no_of_sample =  6
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-08-01",
                qmwork =  30,
                reconwork =  28,
                bcwork =  6,
                otherwork =  0,
                Hours =  64,
                Noofday =  6.4,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-08-02",
                qmwork =  48,
                reconwork =  40,
                bcwork =  5,
                otherwork =  0,
                Hours =  93,
                Noofday =  9.3,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-08-03",
                qmwork =  44,
                reconwork =  34,
                bcwork =  5,
                otherwork =  0,
                Hours =  83,
                Noofday =  8.3,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-08-04",
                qmwork =  53,
                reconwork =  30,
                bcwork =  4,
                otherwork =  0,
                Hours =  87,
                Noofday =  8.7,
                no_of_sample =  6
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-08-05",
                qmwork =  48,
                reconwork =  30,
                bcwork =  5,
                otherwork =  0,
                Hours =  83,
                Noofday =  8.3,
                no_of_sample =  6
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-08-06",
                qmwork =  32,
                reconwork =  18,
                bcwork =  5,
                otherwork =  0,
                Hours =  55,
                Noofday =  5.5,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-08-07",
                qmwork =  44,
                reconwork =  38,
                bcwork =  4,
                otherwork =  0,
                Hours =  86,
                Noofday =  8.6,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-08-08",
                qmwork =  28,
                reconwork =  26,
                bcwork =  5,
                otherwork =  0,
                Hours =  59,
                Noofday =  5.9,
                no_of_sample =  9
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-08-09",
                qmwork =  48,
                reconwork =  24,
                bcwork =  5,
                otherwork =  0,
                Hours =  77,
                Noofday =  7.7,
                no_of_sample =  9
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-08-10",
                qmwork =  40,
                reconwork =  40,
                bcwork =  6,
                otherwork =  0,
                Hours =  86,
                Noofday =  8.6,
                no_of_sample =  8
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-08-11",
                qmwork =  32,
                reconwork =  30,
                bcwork =  6,
                otherwork =  0,
                Hours =  68,
                Noofday =  6.8,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-08-12",
                qmwork =  55,
                reconwork =  29,
                bcwork =  6,
                otherwork =  0,
                Hours =  90,
                Noofday =  9,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-08-13",
                qmwork =  56,
                reconwork =  28,
                bcwork =  5,
                otherwork =  0,
                Hours =  89,
                Noofday =  8.9,
                no_of_sample =  9
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-08-14",
                qmwork =  32,
                reconwork =  28,
                bcwork =  6,
                otherwork =  0,
                Hours =  66,
                Noofday =  6.6,
                no_of_sample =  6
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-09-01",
                qmwork =  30,
                reconwork =  34,
                bcwork =  6,
                otherwork =  0,
                Hours =  70,
                Noofday =  7,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-09-02",
                qmwork =  44,
                reconwork =  28,
                bcwork =  5,
                otherwork =  0,
                Hours =  77,
                Noofday =  7.7,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-09-03",
                qmwork =  73,
                reconwork =  20,
                bcwork =  4,
                otherwork =  0,
                Hours =  97,
                Noofday =  9.7,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-09-04",
                qmwork =  32,
                reconwork =  17,
                bcwork =  5,
                otherwork =  0,
                Hours =  54,
                Noofday =  5.4,
                no_of_sample =  6
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-09-05",
                qmwork =  46,
                reconwork =  25,
                bcwork =  5,
                otherwork =  0,
                Hours =  76,
                Noofday =  7.6,
                no_of_sample =  6
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-09-06",
                qmwork =  32,
                reconwork =  20,
                bcwork =  4,
                otherwork =  0,
                Hours =  56,
                Noofday =  5.6,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-09-07",
                qmwork =  44,
                reconwork =  42,
                bcwork =  5,
                otherwork =  0,
                Hours =  91,
                Noofday =  9.1,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-09-08",
                qmwork =  50,
                reconwork =  30,
                bcwork =  5,
                otherwork =  0,
                Hours =  85,
                Noofday =  8.5,
                no_of_sample =  9
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-09-09",
                qmwork =  52,
                reconwork =  32,
                bcwork =  4,
                otherwork =  0,
                Hours =  88,
                Noofday =  8.8,
                no_of_sample =  9
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-09-10",
                qmwork =  42,
                reconwork =  36,
                bcwork =  5,
                otherwork =  0,
                Hours =  83,
                Noofday =  8.3,
                no_of_sample =  8
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-09-11",
                qmwork =  46,
                reconwork =  33,
                bcwork =  5,
                otherwork =  0,
                Hours =  84,
                Noofday =  8.4,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-09-12",
                qmwork =  56,
                reconwork =  24,
                bcwork =  6,
                otherwork =  0,
                Hours =  86,
                Noofday =  8.6,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-09-13",
                qmwork =  54,
                reconwork =  32,
                bcwork =  6,
                otherwork =  0,
                Hours =  92,
                Noofday =  9.2,
                no_of_sample =  9
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-09-14",
                qmwork =  40,
                reconwork =  30,
                bcwork =  6,
                otherwork =  0,
                Hours =  76,
                Noofday =  7.6,
                no_of_sample =  6
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-10-01",
                qmwork =  30,
                reconwork =  18,
                bcwork =  5,
                otherwork =  0,
                Hours =  53,
                Noofday =  5.3,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-10-02",
                qmwork =  40,
                reconwork =  40,
                bcwork =  6,
                otherwork =  0,
                Hours =  86,
                Noofday =  8.6,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-10-03",
                qmwork =  52,
                reconwork =  42,
                bcwork =  6,
                otherwork =  0,
                Hours =  100,
                Noofday =  10,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-10-04",
                qmwork =  28,
                reconwork =  27,
                bcwork =  5,
                otherwork =  0,
                Hours =  60,
                Noofday =  6,
                no_of_sample =  6
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-10-05",
                qmwork =  51,
                reconwork =  22,
                bcwork =  5,
                otherwork =  0,
                Hours =  78,
                Noofday =  7.8,
                no_of_sample =  6
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-10-06",
                qmwork =  44,
                reconwork =  24,
                bcwork =  4,
                otherwork =  0,
                Hours =  72,
                Noofday =  7.2,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-10-07",
                qmwork =  42,
                reconwork =  22,
                bcwork =  5,
                otherwork =  0,
                Hours =  69,
                Noofday =  6.9,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-10-08",
                qmwork =  55,
                reconwork =  30,
                bcwork =  4,
                otherwork =  0,
                Hours =  89,
                Noofday =  8.9,
                no_of_sample =  9
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-10-09",
                qmwork =  50,
                reconwork =  26,
                bcwork =  5,
                otherwork =  0,
                Hours =  81,
                Noofday =  8.1,
                no_of_sample =  9
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-10-10",
                qmwork =  44,
                reconwork =  50,
                bcwork =  5,
                otherwork =  0,
                Hours =  99,
                Noofday =  9.9,
                no_of_sample =  8
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-10-11",
                qmwork =  56,
                reconwork =  42,
                bcwork =  5,
                otherwork =  0,
                Hours =  103,
                Noofday =  10.3,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-10-12",
                qmwork =  55,
                reconwork =  42,
                bcwork =  6,
                otherwork =  0,
                Hours =  103,
                Noofday =  10.3,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-10-13",
                qmwork =  48,
                reconwork =  30,
                bcwork =  6,
                otherwork =  0,
                Hours =  84,
                Noofday =  8.4,
                no_of_sample =  9
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-10-14",
                qmwork =  38,
                reconwork =  26,
                bcwork =  6,
                otherwork =  0,
                Hours =  70,
                Noofday =  7,
                no_of_sample =  6
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-11-01",
                qmwork =  53,
                reconwork =  45,
                bcwork =  5,
                otherwork =  0,
                Hours =  103,
                Noofday =  10.3,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-11-02",
                qmwork =  48,
                reconwork =  44,
                bcwork =  6,
                otherwork =  0,
                Hours =  98,
                Noofday =  9.8,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-11-03",
                qmwork =  50,
                reconwork =  45,
                bcwork =  6,
                otherwork =  0,
                Hours =  101,
                Noofday =  10.1,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-11-04",
                qmwork =  39,
                reconwork =  26,
                bcwork =  5,
                otherwork =  0,
                Hours =  70,
                Noofday =  7,
                no_of_sample =  6
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-11-05",
                qmwork =  38,
                reconwork =  28,
                bcwork =  5,
                otherwork =  0,
                Hours =  71,
                Noofday =  7.1,
                no_of_sample =  6
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-11-06",
                qmwork =  44,
                reconwork =  49,
                bcwork =  4,
                otherwork =  0,
                Hours =  97,
                Noofday =  9.7,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-11-07",
                qmwork =  46,
                reconwork =  40,
                bcwork =  5,
                otherwork =  0,
                Hours =  91,
                Noofday =  9.1,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-11-08",
                qmwork =  48,
                reconwork =  32,
                bcwork =  5,
                otherwork =  0,
                Hours =  85,
                Noofday =  8.5,
                no_of_sample =  9
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-11-09",
                qmwork =  52,
                reconwork =  26,
                bcwork =  4,
                otherwork =  0,
                Hours =  82,
                Noofday =  8.2,
                no_of_sample =  9
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-11-10",
                qmwork =  46,
                reconwork =  52,
                bcwork =  5,
                otherwork =  0,
                Hours =  103,
                Noofday =  10.3,
                no_of_sample =  8
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-11-11",
                qmwork =  54,
                reconwork =  40,
                bcwork =  5,
                otherwork =  0,
                Hours =  99,
                Noofday =  9.9,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-11-12",
                qmwork =  44,
                reconwork =  50,
                bcwork =  6,
                otherwork =  0,
                Hours =  100,
                Noofday =  10,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-11-13",
                qmwork =  50,
                reconwork =  26,
                bcwork =  4,
                otherwork =  0,
                Hours =  80,
                Noofday =  8,
                no_of_sample =  9
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-11-14",
                qmwork =  38,
                reconwork =  30,
                bcwork =  6,
                otherwork =  0,
                Hours =  74,
                Noofday =  7.4,
                no_of_sample =  6
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-12-01",
                qmwork =  48,
                reconwork =  30,
                bcwork =  5,
                otherwork =  0,
                Hours =  83,
                Noofday =  8.3,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-12-02",
                qmwork =  46,
                reconwork =  40,
                bcwork =  6,
                otherwork =  0,
                Hours =  92,
                Noofday =  9.2,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-12-03",
                qmwork =  48,
                reconwork =  54,
                bcwork =  6,
                otherwork =  0,
                Hours =  108,
                Noofday =  10.8,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-12-04",
                qmwork =  30,
                reconwork =  32,
                bcwork =  4,
                otherwork =  0,
                Hours =  66,
                Noofday =  6.6,
                no_of_sample =  6
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-12-05",
                qmwork =  32,
                reconwork =  33,
                bcwork =  5,
                otherwork =  0,
                Hours =  70,
                Noofday =  7,
                no_of_sample =  6
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-12-06",
                qmwork =  50,
                reconwork =  44,
                bcwork =  5,
                otherwork =  0,
                Hours =  99,
                Noofday =  9.9,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-12-07",
                qmwork =  52,
                reconwork =  46,
                bcwork =  5,
                otherwork =  0,
                Hours =  103,
                Noofday =  10.3,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-12-08",
                qmwork =  56,
                reconwork =  26,
                bcwork =  4,
                otherwork =  0,
                Hours =  86,
                Noofday =  8.6,
                no_of_sample =  9
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-12-09",
                qmwork =  52,
                reconwork =  24,
                bcwork =  5,
                otherwork =  0,
                Hours =  81,
                Noofday =  8.1,
                no_of_sample =  9
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-12-10",
                qmwork =  48,
                reconwork =  36,
                bcwork =  5,
                otherwork =  0,
                Hours =  89,
                Noofday =  8.9,
                no_of_sample =  8
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-12-11",
                qmwork =  55,
                reconwork =  46,
                bcwork =  4,
                otherwork =  0,
                Hours =  105,
                Noofday =  10.5,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-12-12",
                qmwork =  46,
                reconwork =  40,
                bcwork =  6,
                otherwork =  0,
                Hours =  92,
                Noofday =  9.2,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-12-13",
                qmwork =  58,
                reconwork =  28,
                bcwork =  6,
                otherwork =  0,
                Hours =  92,
                Noofday =  9.2,
                no_of_sample =  9
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK1-12-14",
                qmwork =  33,
                reconwork =  34,
                bcwork =  4,
                otherwork =  0,
                Hours =  71,
                Noofday =  7.1,
                no_of_sample =  6
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-04-15",
                qmwork =  43,
                reconwork =  28,
                bcwork =  5,
                otherwork =  0,
                Hours =  76,
                Noofday =  7.6,
                no_of_sample =  12
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-04-16",
                qmwork =  50,
                reconwork =  38,
                bcwork =  6,
                otherwork =  0,
                Hours =  94,
                Noofday =  9.4,
                no_of_sample =  8
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-04-17",
                qmwork =  48,
                reconwork =  42,
                bcwork =  6,
                otherwork =  0,
                Hours =  96,
                Noofday =  9.6,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-04-18",
                qmwork =  52,
                reconwork =  28,
                bcwork =  5,
                otherwork =  0,
                Hours =  85,
                Noofday =  8.5,
                no_of_sample =  9
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-04-19",
                qmwork =  48,
                reconwork =  40,
                bcwork =  5,
                otherwork =  0,
                Hours =  93,
                Noofday =  9.3,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-04-20",
                qmwork =  28,
                reconwork =  35,
                bcwork =  5,
                otherwork =  0,
                Hours =  68,
                Noofday =  6.8,
                no_of_sample =  6
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-04-21",
                qmwork =  56,
                reconwork =  22,
                bcwork =  5,
                otherwork =  0,
                Hours =  83,
                Noofday =  8.3,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-04-22",
                qmwork =  46,
                reconwork =  44,
                bcwork =  5,
                otherwork =  0,
                Hours =  95,
                Noofday =  9.5,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-05-15",
                qmwork =  44,
                reconwork =  30,
                bcwork =  4,
                otherwork =  0,
                Hours =  78,
                Noofday =  7.8,
                no_of_sample =  12
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-05-16",
                qmwork =  46,
                reconwork =  40,
                bcwork =  5,
                otherwork =  0,
                Hours =  91,
                Noofday =  9.1,
                no_of_sample =  8
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-05-17",
                qmwork =  48,
                reconwork =  43,
                bcwork =  5,
                otherwork =  0,
                Hours =  96,
                Noofday =  9.6,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-05-18",
                qmwork =  48,
                reconwork =  30,
                bcwork =  6,
                otherwork =  0,
                Hours =  84,
                Noofday =  8.4,
                no_of_sample =  9
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-05-19",
                qmwork =  44,
                reconwork =  40,
                bcwork =  6,
                otherwork =  0,
                Hours =  90,
                Noofday =  9,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-05-20",
                qmwork =  28,
                reconwork =  33,
                bcwork =  6,
                otherwork =  0,
                Hours =  67,
                Noofday =  6.7,
                no_of_sample =  6
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-05-21",
                qmwork =  46,
                reconwork =  44,
                bcwork =  5,
                otherwork =  0,
                Hours =  95,
                Noofday =  9.5,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-05-22",
                qmwork =  48,
                reconwork =  43,
                bcwork =  4,
                otherwork =  0,
                Hours =  95,
                Noofday =  9.5,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-06-15",
                qmwork =  48,
                reconwork =  32,
                bcwork =  6,
                otherwork =  0,
                Hours =  86,
                Noofday =  8.6,
                no_of_sample =  12
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-06-16",
                qmwork =  48,
                reconwork =  44,
                bcwork =  5,
                otherwork =  0,
                Hours =  97,
                Noofday =  9.7,
                no_of_sample =  8
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-06-17",
                qmwork =  53,
                reconwork =  44,
                bcwork =  5,
                otherwork =  0,
                Hours =  102,
                Noofday =  10.2,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-06-18",
                qmwork =  46,
                reconwork =  30,
                bcwork =  4,
                otherwork =  0,
                Hours =  80,
                Noofday =  8,
                no_of_sample =  9
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-06-19",
                qmwork =  38,
                reconwork =  48,
                bcwork =  5,
                otherwork =  0,
                Hours =  91,
                Noofday =  9.1,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-06-20",
                qmwork =  26,
                reconwork =  32,
                bcwork =  5,
                otherwork =  0,
                Hours =  63,
                Noofday =  6.3,
                no_of_sample =  6
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-06-21",
                qmwork =  44,
                reconwork =  40,
                bcwork =  5,
                otherwork =  0,
                Hours =  89,
                Noofday =  8.9,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-06-22",
                qmwork =  40,
                reconwork =  40,
                bcwork =  4,
                otherwork =  0,
                Hours =  84,
                Noofday =  8.4,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-07-15",
                qmwork =  44,
                reconwork =  30,
                bcwork =  5,
                otherwork =  0,
                Hours =  79,
                Noofday =  7.9,
                no_of_sample =  12
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-07-16",
                qmwork =  42,
                reconwork =  24,
                bcwork =  6,
                otherwork =  0,
                Hours =  72,
                Noofday =  7.2,
                no_of_sample =  8
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-07-17",
                qmwork =  40,
                reconwork =  22,
                bcwork =  6,
                otherwork =  0,
                Hours =  68,
                Noofday =  6.8,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-07-18",
                qmwork =  44,
                reconwork =  32,
                bcwork =  6,
                otherwork =  0,
                Hours =  82,
                Noofday =  8.2,
                no_of_sample =  9
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-07-19",
                qmwork =  44,
                reconwork =  18,
                bcwork =  4,
                otherwork =  0,
                Hours =  66,
                Noofday =  6.6,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-07-20",
                qmwork =  28,
                reconwork =  32,
                bcwork =  6,
                otherwork =  0,
                Hours =  66,
                Noofday =  6.6,
                no_of_sample =  6
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-07-21",
                qmwork =  42,
                reconwork =  27,
                bcwork =  6,
                otherwork =  0,
                Hours =  75,
                Noofday =  7.5,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-07-22",
                qmwork =  42,
                reconwork =  22,
                bcwork =  5,
                otherwork =  0,
                Hours =  69,
                Noofday =  6.9,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-08-15",
                qmwork =  48,
                reconwork =  32,
                bcwork =  4,
                otherwork =  0,
                Hours =  84,
                Noofday =  8.4,
                no_of_sample =  12
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-08-16",
                qmwork =  32,
                reconwork =  18,
                bcwork =  5,
                otherwork =  0,
                Hours =  55,
                Noofday =  5.5,
                no_of_sample =  8
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-08-17",
                qmwork =  30,
                reconwork =  22,
                bcwork =  5,
                otherwork =  0,
                Hours =  57,
                Noofday =  5.7,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-08-18",
                qmwork =  49,
                reconwork =  36,
                bcwork =  4,
                otherwork =  0,
                Hours =  89,
                Noofday =  8.9,
                no_of_sample =  9
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-08-19",
                qmwork =  28,
                reconwork =  24,
                bcwork =  5,
                otherwork =  0,
                Hours =  57,
                Noofday =  5.7,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-08-20",
                qmwork =  38,
                reconwork =  22,
                bcwork =  5,
                otherwork =  0,
                Hours =  65,
                Noofday =  6.5,
                no_of_sample =  6
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-08-21",
                qmwork =  32,
                reconwork =  24,
                bcwork =  5,
                otherwork =  0,
                Hours =  61,
                Noofday =  6.1,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-08-22",
                qmwork =  46,
                reconwork =  36,
                bcwork =  6,
                otherwork =  0,
                Hours =  88,
                Noofday =  8.8,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-09-15",
                qmwork =  46,
                reconwork =  28,
                bcwork =  4,
                otherwork =  0,
                Hours =  78,
                Noofday =  7.8,
                no_of_sample =  12
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-09-16",
                qmwork =  46,
                reconwork =  36,
                bcwork =  6,
                otherwork =  0,
                Hours =  88,
                Noofday =  8.8,
                no_of_sample =  8
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-09-17",
                qmwork =  48,
                reconwork =  32,
                bcwork =  5,
                otherwork =  0,
                Hours =  85,
                Noofday =  8.5,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-09-18",
                qmwork =  50,
                reconwork =  30,
                bcwork =  4,
                otherwork =  0,
                Hours =  84,
                Noofday =  8.4,
                no_of_sample =  9
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-09-19",
                qmwork =  38,
                reconwork =  24,
                bcwork =  6,
                otherwork =  0,
                Hours =  68,
                Noofday =  6.8,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-09-20",
                qmwork =  32,
                reconwork =  28,
                bcwork =  5,
                otherwork =  0,
                Hours =  65,
                Noofday =  6.5,
                no_of_sample =  6
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-09-21",
                qmwork =  26,
                reconwork =  22,
                bcwork =  5,
                otherwork =  0,
                Hours =  53,
                Noofday =  5.3,
                no_of_sample =  7
                },
                new EntQaQcQmJotGroupByUnit()
                {
                Unit =  "BLK2-09-22",
                qmwork =  32,
                reconwork =  18,
                bcwork =  4,
                otherwork =  0,
                Hours =  54,
                Noofday =  5.4,
                no_of_sample =  7
                }
            };


            return result;
        }
    }
}