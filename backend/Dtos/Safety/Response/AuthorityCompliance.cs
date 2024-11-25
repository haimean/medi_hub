namespace DashboardApi.Dtos.Safety.Response
{
    public class AuthorityCompliance
    {
        public string Name { get; set; }
        public double Fine { get; set; }
        public double SWO { get; set; }
        public double NNC { get; set; }
        public double Demerit { get; set; }
        public double FineOfLine { get; set; }
        public double NEARodent { get; set; }
        public double NEAWdph { get; set; }
    }
}
