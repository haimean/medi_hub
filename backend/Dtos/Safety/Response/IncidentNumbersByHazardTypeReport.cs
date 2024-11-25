namespace DashboardApi.Dtos.Safety.Response
{
    public class IncidentNumbersByHazardTypeReport
    {
        public string NameOfHazardType { get; set; }
        public int CurrentYear { get; set; }
        public int BeforeYear { get; set; }
    }

    public class IncidentNumbersByInjuredPartReport
    {
        public string NameOfInjuredPart { get; set; }
        public int NumberOfInjuredPart { get; set; }
        public int Type { get; set; }
    }

}
