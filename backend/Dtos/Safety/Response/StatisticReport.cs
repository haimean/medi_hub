namespace DashboardApi.Dtos.Safety.Response
{
    public class StatisticReport
    {
        public double WhStrength { get; set; }
        public double SubconStrength { get; set; }
        public double ClientRPStrength { get; set; }
        public double ClientRPWorker { get; set; }
        public double WhManhoursWorker { get; set; }

        public double WorkplaceInjuryRate { get; set; }
        public double WorkplaceInjuryRateJV { get; set; }

        public double SubconManhoursWorked { get; set; }

        public double AccidentSeverityRate { get; set; }
        public double AccidentSeverityRateJV { get; set; }

        public int NumberOfFatality { get; set; }
        public int NumberOfReportableAccidents { get; set; }
        public int NumberOfDangerousOccurrence { get; set; }
        public int NumberOfLostTimeInjury { get; set; }
        public int NumberOfLightDutyCase { get; set; }
        public int NumberOfNearMiss { get; set; }
    }


}
