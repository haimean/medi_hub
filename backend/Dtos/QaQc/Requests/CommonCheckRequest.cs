namespace DashboardApi.Dtos.QaQc.Requests;

public class CommonCheckRequest
{
    public List<string> project { get; set; }
    public List<string> projectId { get; set; }
    public string discipline { get; set; }
    public string trade { get; set; }
    public string lteDate { get; set; }
    public string gteDate { get; set; }
    public string tradeSelected { get; set; }
}