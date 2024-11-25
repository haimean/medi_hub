namespace DashboardApi.Dtos.QaQc.Requests;

public class ObservationRequest
{
    public string project { get; set; }
    public string sxf { get; set; }
    public string lteDate { get; set; }
    public string gteDate { get; set; }
}