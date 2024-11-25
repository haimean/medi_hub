namespace DashboardApi.Dtos.Safety.Response
{
    public class Statistics
    {
        public class StatisticResponse
        {
            public DateTime Date { get; set; }
            public string ProjectId { get; set; }
            public string ProjectName { get; set; }
            public double AveDailyWHPersonnel { get; set; }
            public double AveDailyClientAndRepPersonnel { get; set; }
            public double AveDailySubconPersonnel { get; set; }
            public double TotalWHManhoursWorked { get; set; }
            public double TotalClientWorked { get; set; }
            public double TotalSCMHWorked { get; set; }
            public bool IsDeleted { get; set; }
        }

        public class StatisticDailyPersonnelResponse
        {
            public DateTime Month { get; set; }
            public string ProjectId { get; set; }
            public string ProjectName { get; set; }
            public double TotalWHPersonnel { get; set; }
            public double TotalClientAndRepPersonnel { get; set; }
            public double TotalSubconPersonnel { get; set; }
            public double TotalSum { get; set; }
            public double AvgLast12Months { get; set; }
        }
    }
}
