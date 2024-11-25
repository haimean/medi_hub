namespace DashboardApi.Dtos.Safety.Response
{
    public class LegalNCRegister
    {
        public DateTime DateCreated { get; set; }
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string Authority { get; set; }
        public string Type { get; set; }
        public string Reason { get; set; }
        public double FineAmount { get; set; }
        public double NEARodent { get; set; }
        public double NEAWdph { get; set; }
        public DateTime? SWOStartDate { get; set; }
        public DateTime? SWOLifedDate { get; set; }
        public string Remark { get; set; }
    }

    public class LegalNCRegisterTypeResponse
    {
        public double DemeritPoint { get; set; }
        public double SWO { get; set; }
        public double Fine { get; set; }
        public double NNC { get; set; }
        public double RestrictionOfWorkHours { get; set; }
    }

    public class NEA
    {
        public string Month { get; set; }
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public double TotalFineAmount { get; set; }
    }
}
