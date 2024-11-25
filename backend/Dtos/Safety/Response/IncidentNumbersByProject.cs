namespace DashboardApi.Dtos.Safety.Response
{
    public class IncidentNumbersByProject
    {
        public string ProjectName { get; set; }
        public int ReportableAccident { get; set; }
        public double ManDayLost { get; set; }
    }
}
