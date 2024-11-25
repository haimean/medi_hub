using System.ComponentModel.DataAnnotations.Schema;

namespace DashboardApi.Dtos.Safety.Response
{
    public class AccumulationEntity
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public double CumulativeMHWorked { get; set; }
        public double CumulativeReportableAccidents { get; set; }
        public double CumulativeLTI { get; set; }
        public double CumulativeLDC { get; set; }
        public double CumulativeIACase { get; set; }
        public double CumulatvieMTC { get; set; }
        public double CumulativeFAC { get; set; }
        public double CumulativeOD { get; set; }
        public double CumulativeLostDaysFromReportableAccidents { get; set; }
        public double CumulativeEI { get; set; }
        public double CumulativeDo { get; set; }
        public double CumulativePUD { get; set; }
        public double CumulativeNM { get; set; }
        public double CumulativeAuthoriyFines { get; set; }
        public bool IsDeleted { get; set; }
    }
}
