using System.Text.Json;

namespace DashboardApi.Dtos.Safety.Response
{
    public class IncidentRegister
    {
        public DateTime DateCreated { get; set; }
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string IncidentTime { get; set; }
        public string IncidentClassification { get; set; }
        public double MedicalLeaveDay { get; set; }
        public double LightDutyDay { get; set; }
        public string Location { get; set; }
        public string NatureOfHazard { get; set; }
        public string InjuredPart { get; set; }
        public string TypeOfInjury { get; set; }
        public string CompanyInvolved { get; set; }
        public string NameOfInjured { get; set; }
        public string BriefDescription { get; set; }
        public string MedicalLeave { get; set; }
        public DateTime? LightDutyFrom { get; set; }
        public DateTime? LightDutyTo { get; set; }
        public bool IsDeleted { get; set; }
    }
}
