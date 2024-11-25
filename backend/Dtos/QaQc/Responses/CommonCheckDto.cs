namespace DashboardApi.Dtos.QaQc.Responses;

public class CommonCheckDto
{
    public class UnwindTrade
    {
        public String name { get; set; }
        public int total_no { get; set; }
        public int total_yes { get; set; }

        public double percent
        {
            get
            {
                double res = 0;
                if (total_no + total_yes != 0)
                {
                    res = 1000 * total_no / (total_no + total_yes);
                }

                return Math.Round(100 - (res / 100), 1);
            }
        }
    }
}