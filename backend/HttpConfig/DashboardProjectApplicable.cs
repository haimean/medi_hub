namespace DashboardApi.HttpConfig
{
    public class DashboardProjectApplicable
    {
        public string ProjectID { get; set; }
        public string ProjectName { get; set; }
        public Applicable Applicable { get; set; }
    }

    public class Applicable
    {
        public bool qm { get; set; }
    }
    public class UserProject
    {
        public string ProjectID { get; set; }
        public string ProjectName { get; set; }
        public string Email { get; set; }
        public string ShortName { get; set; }
        public string Name { get; set; }
    }
}
