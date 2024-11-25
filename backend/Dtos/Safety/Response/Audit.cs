using System.ComponentModel.DataAnnotations.Schema;

namespace DashboardApi.Dtos.Safety.Response
{
    public class Audit
    {
        public DateTime DateCreated { get; set; }
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public double AreaPassed { get; set; }
        public double AreaSampled { get; set; }
        public double AuditScore { get; set; }
        public string AuditRating { get; set; }
        public double AuditScorePC { get; set; }
        public double AuditFindingsDeduction { get; set; }
        public double FinalScore { get; set; }
        public string FinalRating { get; set; }
        public double Access { get; set; }
        public double FallingHeight { get; set; }
        public double OverheadHazard { get; set; }
        public double CraneEquipment { get; set; }
        public double Excavation { get; set; }
        public double Fire { get; set; }
        public double Scaffold { get; set; }
        public double Equipment { get; set; }
        public double Electrical { get; set; }
        public double Security { get; set; }
        public double Slip { get; set; }
        public double HealthHazard { get; set; }
        public double PublicSafety { get; set; }
        public double VehicularSafety { get; set; }
        public double Others { get; set; }
        public double Total { get; set; }
        public double AuditScoreRating { get; set; }
        public double AuditFindingsDeductionRating { get; set; }
        public bool IsDeleted { get; set; }
    }
}
