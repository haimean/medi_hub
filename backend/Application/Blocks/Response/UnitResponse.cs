namespace DashboardApi.Application.Project.Blocks
{
    public class UnitResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string TypeId { get; set; }
        public string TypeName { get; set; }

        public string ProjectId { get; set; }
        public string ProjectName { get; set; }

        public string BlockId { get; set; }
        public string BlockName { get; set; }

        public string LevelId { get; set; }
        public string LevelName { get; set; }
    }
}
