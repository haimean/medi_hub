namespace DashboardApi.Dtos.QaQc.Requests
{
    public class BaseRequest
    {
        public string? projectId { get; set; }
        public DateTime? lteDate { get; set; }
        public DateTime? gteDate { get; set; }
    }
}
