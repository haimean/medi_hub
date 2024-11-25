namespace DashboardApi.Dtos.Safety.Response
{
    public class IncidentNumbersByMonth
    {
        public string Name { get; set; }
        public double WH { get; set; }
        public double WHJV { get; set; }
        public double WIR { get; set; }
        public double JVWIR { get; set; }
        public double Total_WIR { get; set; }
        public double Total_JVWIR { get; set; }
        public double Nation_Average { get; set; }
    }
    public class TotalIncidentNumbersByMonthReport
    {
        public double Total_WIR { get; set; }
        public double Total_JVWIR { get; set; }
        public double Nation_Average { get; set; }
        public List<IncidentNumbersByMonth> IncidentNumbersByMonths { get; set; }
    }
}
