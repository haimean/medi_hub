namespace DashboardApi.Dtos.Safety.Response
{
    public class MOMNotice
    {
        public string ProjectName { get; set; }
        public double Amount { get; set; }
    }

    public class NEAOffencesByMonth
    {
        public string ProjectName { get; set; }
        public double Mosquito { get; set; }
        public double Rodent { get; set; }
        public double Vector { get; set; }
        public double WDPH { get; set; }
        public double Noise { get; set; }
    }

    public class MOMReport
    {
        public List<MOMNotice> MOMNotices { get; set; }
        public List<NEAOffencesByMonth> NEAOffencesByMonths { get; set; }
        public MOMProjectReport MOMProjectReport { get; set; }
    }

    public class NEAOffencesByProject
    {
        public string ProjectName { get; set; }
        public double Amount { get; set; }
    }
    public class MOMProjectReport
    {
        public List<string> Projects { get; set; }
        public List<Dictionary<String, object>> MOMNotices { get; set; }

    }
}
