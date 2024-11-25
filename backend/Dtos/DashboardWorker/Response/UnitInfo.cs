namespace DashboardApi.Dtos.DashboardWorker.Response
{
    public class UnitInfo
    {
        public string? Unit { get; set; }

        public int? noOfSample { get; set; }

        public double TotalHours { get; set; }
        public double qmwork { get; set; }
        public double reconwork { get; set; }
        public double bcwork { get; set; }
        public double waterProofing { get; set; }
        public double otherwork { get; set; }
        public double total { get; set; }
        public double ms { get; set; }
        public double msQR { get; set; }
        public DateTime dateTime { get; set; }
        public string? blockName { get; set; }
        public string? unitName { get; set; }

        public List<UnitDetail> listUnitDetail { get; set; }
    }
}