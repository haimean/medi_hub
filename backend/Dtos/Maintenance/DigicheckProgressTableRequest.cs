namespace DashboardApi.Dtos.Maintenance
{
    public class DigicheckProgressTableRequest
    {
        public string Name { get; set; }
        public string SiteId { get; set; }
        public string Block { get; set; } = "";
        public string BlockId { get; set; }
        public string Unit { get; set; } = "";
        public string UnitType { get; set; } = "";
        public string Level { get; set; } = "";
        public string LevelId { get; set; }
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
        public string Marking { get; set; }
        public string FlowType { get; set; }
        public string Status { get; set; }
        public string FormTemplateName { get; set; }

    }
}
