namespace DashboardApi.Dtos.Safety.Response
{
    public class SummaryOfPerformanceScore
    {
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public double WellAboveExpectation { get; set; }
        public double JustAboveExpectation { get; set; }
        public double MeetExpectation { get; set; }
        public double BelowExpectation { get; set; }
    }
    public class SummaryOfPerformanceScoreV2
    {
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }

        private double _pcAuditScore;
        public double PCAuditScore
        {
            get => Math.Round(_pcAuditScore, 1);
            set => _pcAuditScore = value;
        }

        public string PCAuditScoreName { get; set; }
        public string PCAuditScoreShortName { get; set; }

        private double _houseKeepingAuditScore;
        public double HouseKeepingAuditScore
        {
            get => Math.Round(_houseKeepingAuditScore, 1);
            set => _houseKeepingAuditScore = value;
        }

        public string HouseKeepingAuditScoreName { get; set; }
        public string HouseKeepingAuditScoreShortName { get; set; }

        public bool IsPCAuditGreaterThanHouseKeeping { get; set; }
    }

}
