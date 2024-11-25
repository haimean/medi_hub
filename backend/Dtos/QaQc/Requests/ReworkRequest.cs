namespace DashboardApi.Dtos.QaQc.Requests;

public class ReworkRequest
{
    public string project { get; set; }
    public string subcontractor { get; set; }
    public string lteDate { get; set; }
    public string gteDate { get; set; }
}