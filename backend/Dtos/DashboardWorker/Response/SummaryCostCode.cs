namespace DashboardApi.Dtos.DashboardWorker.Response
{
    public class SummaryCostCode
    {
        public string endCurrentMonth { get; set; }
        public string endPrevMonth { get; set; }
        public string startCurrentMonth { get; set; }
        public string startPrevMonth { get; set; }
        public string block { get; set; }
        public List<CostData> costCodeDatasById { get; set; } = new List<CostData>();
    }

    public class Block
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class totalRework
    {
        public float rework_score { get; set; }
        public float rework { get; set; }
    }
    public class CostData
    {
        public DateOnly? date { get; set; }
        public string costCode { get; set; }
        public string block { get; set; }
        public string description { get; set; }
        public double totalAmount { get; set; }
        public double totalHours { get; set; }
        public double totalManMonth { get; set; }
        public AmountData currentMonthData { get; set; }
        public AmountData previousMonthData { get; set; }
        public string trade { get; set; }
        public List<CostDataByBlockUnit> costDataByBlockUnits { get; set; }

    }

    public class CostDataByBlockUnit
    {
        public double totalAmount { get; set; }
        public double totalHours { get; set; }
        public string block { get; set; }
        public string unit { get; set; }
        public int noOfSample { get; set; }
    }

    public class AmountData
    {
        public double amount { get; set; }
        public double hourNumberWithRate { get; set; }
        public double points { get; set; }


        public static AmountData Sum(List<AmountData> list)
        {
            return new AmountData()
            {
                amount = list.Sum(x => x.amount),
                hourNumberWithRate = list.Sum(x => x.hourNumberWithRate),
                points = list.Sum(x => x.points),
            };
        }
    }
}
