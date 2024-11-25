namespace DashboardApi.Dtos.Safety.Response
{
    public class WIRASRAFRDetail
    {
        public string ProjectName { get; set; }
        public double WIR { get; set; }
        public double WIRAverage { get; set; }
        public double SumWIR { get; set; }

        public double WSHPA { get; set; }
        public double SHARP { get; set; }

        public double ASR { get; set; }
        public double ASRAverage { get; set; }
        public double SumASR { get; set; }

        public double AFR { get; set; }
        public double SumAFR { get; set; }
        public double AFRAverage { get; set; }
    }
}
