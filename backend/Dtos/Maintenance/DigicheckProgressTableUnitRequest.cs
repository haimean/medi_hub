namespace DashboardApi.Dtos.Maintenance
{
    public class DigicheckProgressTableUnitRequest
    {
        public string Block { get; set; } = "";
        public string BlockId { get; set; }
        public string Level { get; set; } = "";
        public string LevelId { get; set; }
        public string StartDateCasting { get; set; }
        public string EndDateCasting { get; set; }
        public string TotalCompleteCasting { get; set; }
        public string TotalCasting { get; set; }
        public string TotalCompleteWater { get; set; }
        public string TotalWater { get; set; }
        public string TotalCompletePlastered { get; set; }
        public string TotalPlastered { get; set; }

        public string StartDateFitout { get; set; }
        public string EndDateFitout { get; set; }
        public string TotalCompleteFitout { get; set; }
        public string TotalFitout { get; set; }

        public string StartDateDelivered { get; set; }
        public string EndDateDelivered { get; set; }
        public string TotalCompleteDelivered { get; set; }
        public string TotalDelivered { get; set; }

        public string StartDateInstalled { get; set; }
        public string EndDateInstalled { get; set; }
        public string TotalCompleteInstalled { get; set; }
        public string TotalInstalled { get; set; }

        public string MaxTotalModule { get; set; }
        public string TotalPPVC { get; set; }

    }
}
