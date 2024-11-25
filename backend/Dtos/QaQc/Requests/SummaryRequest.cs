namespace DashboardApi.Dtos.QaQc.Requests;

public class SummaryRequest
{
    public string lteDate { get; set; }

    public string gteDate { get; set; }

    public string typeProject { get; set; }

    public string type { get; set; }

    public string block { get; set; }

    public string typeKPR { get; set; }

    /// <summary>
    /// list project need query
    /// </summary>
    public string[] listProject { get; set; }
    public string[] listProjectId { get; set; }

    public string projectID { get; set; }

    public string projectName { get; set; }

    /// <summary>
    /// List month previous get from current month
    /// </summary>
    public int numberMonthPrevious { get; set; }

    /// <summary>
    /// is overvall to get display data project
    /// </summary>
    public bool isOvervall { get; set; }

    /// <summary>
    /// type view
    /// </summary>
    public string TotalNumberDefectStatuses { get; set; }

    /// <summary>
    /// PrecasttType
    /// </summary>
    public string PrecastType { get; set; }

    /// <summary>
    /// PrecastTypeId
    /// </summary>
    public string PrecastTypeId { get; set; }

    /// <summary>
    /// FlowID
    /// </summary>
    public string[] FlowID { get; set; }

    /// <summary>
    /// IdFlowCasting
    /// </summary>
    public string IdFlowCasting { get; set; }

    /// <summary>
    /// IdFlowFitOut
    /// </summary>
    public string IdFlowFitOut { get; set; }

    /// <summary>
    /// IdFlowPrestorage
    /// </summary>
    public string IdFlowPrestorage { get; set; }

    /// <summary>
    /// IdFlowMEP
    /// </summary>
    public string IdFlowMEP { get; set; }

    /// <summary>
    /// IdFlowPrestorage
    /// </summary>
    public string IdFlowOnSite { get; set; }

    /// <summary>
    /// year query filter
    /// </summary>
    public List<DateTime> Year { get; set; }
}