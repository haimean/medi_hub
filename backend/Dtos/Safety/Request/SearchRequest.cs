

using DashboardApi.Dtos.Safety.Response;

namespace DashboardApi.Dtos.Safety.Request
{
    public class SearchRequest
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set;}
        public string ProjectName { get; set; }
        public int? Month { get; set; }
        public List<ProjectResponse> ListProjects { get; set; }
    }
}
