using MongoDB.Driver;

namespace DashboardApi.Dtos.DashboardWorker.Response
{
    public class UnitResponseSQL
    {
        public string? blkunitname { get; set; }
        public string? levelName { get; set; }
        public Guid? WABlockID { get; set; }
        public Guid? BLKID { get; set; }
        public string? projectId { get; set; }
        public string? projectName { get; set; }

        public int? noOfSample { get; set; }
        public string description { get; set; }

        public string startTime { get; set; }
        public string endTime { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }

        public string nameWorker { get; set; }

        public int workerId { get; set; }

        public string blockName { get; set; }

        public string unitName { get; set; }

        public string nameSupervisor { get; set; }
        public double productivityTarget { get; set; }
        public string strataName { get; set; }

        public double totalHours { get; set; }

        /*public double totalHours
        {
            get
            {
                var startTimeSpan = startTime.TryParseTimeHourMinute();
                var endTimeSpan = endTime.TryParseTimeHourMinute();

                var start = DateTimeUtils.DumpTodayDateTime(startTimeSpan.Hours, startTimeSpan.Minutes, 0);
                var end = DateTimeUtils.DumpTodayDateTime(endTimeSpan.Hours, endTimeSpan.Minutes, 0);

                if (startDate == "tomorrow")
                {
                    start = DateTimeUtils.DumpTomorrowDateTime(endTimeSpan.Hours, endTimeSpan.Minutes, 0);
                }

                if (start > end || endDate == "tomorrow")
                {
                    end = DateTimeUtils.DumpTomorrowDateTime(endTimeSpan.Hours, endTimeSpan.Minutes, 0);
                }

                var lunchStart = DateTimeUtils.DumpTodayDateTime(12, 0, 0);
                var lunchEnd = DateTimeUtils.DumpTodayDateTime(13, 40, 0);

                if (start < lunchStart && end >= lunchEnd)
                {
                    return (end - start).TotalHours - (lunchEnd - lunchStart).TotalHours;
                }


                return (end - start).TotalHours;
            }
        }*/


        public DateTime date { get; set; }
    }
}