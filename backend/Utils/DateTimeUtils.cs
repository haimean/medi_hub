using System.Globalization;

//using WorkerApp.Application.Dtos.Enums;
//using WorkerApp.Models;

namespace DashboardApi.Utils
{
    public enum DayTypeEnum
    {
        NormalDay = 1,
        Sunday = 2,
        Holiday = 3
    }

    public static class DateTimeUtils
    {

        private static DateTime StartLunch = DumpTodayDateTime(11, 40, 0);
        private static DateTime EndLunch = DumpTodayDateTime(13, 0, 0);
        private static DateTime StartTea = DumpTodayDateTime(16, 0, 0);
        private static DateTime EndTea = DumpTodayDateTime(16, 20, 0);
        private static DateTime StartNightLunch = DumpTomorrowDateTime(00, 00, 0);
        private static DateTime EndNightLunch = DumpTomorrowDateTime(00, 40, 0);

        public static DateTimeRange LunchRange = new(StartLunch, EndLunch);
        public static DateTimeRange TeaRange = new(StartTea, EndTea);
        public static DateTimeRange NightLunchRange = new(StartNightLunch, EndNightLunch);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="range"></param>
        /// <param name="type">1 lunch , 2  tea , 3 night</param>
        /// <returns></returns>
        public static DateTimeRange IntersectWithBreakTime(this DateTimeRange range, int type)
        {
            if (type == 1)
            {
                return range.GetIntersection(LunchRange);
            }

            if (type == 2)
            {
                return range.GetIntersection(TeaRange);
            }

            return range.GetIntersection(NightLunchRange);
        }


        /// <summary>
        /// parse date string
        /// </summary>
        /// <param name="dateString">YYYY-MM-DD</param>
        /// <returns></returns>
        public static DateOnly ParseDateOnlyByString(this string dateString, string format = "yyyy-MM-dd")
        {
            dateString = dateString.Replace("/", "-");
            DateOnly.TryParseExact(dateString, format, out var rs);
            return rs;
        }


        public static DateTime ParseDateTimeByString(this string dateString, string format = "yyyy-MM-dd HH:mm")
        {
            dateString = dateString.Replace("/", "-");
            DateTime.TryParseExact(dateString, format, null, DateTimeStyles.None, out var rs);
            return rs;
        }

        public static DateTime DumpTodayDateTime(int hour, int minute, int second)
        {
            DateTime dt = new DateTime(2020, 1, 5, hour, minute, second);

            return dt;
        }

        public static DateTime DumpTomorrowDateTime(int hour, int minute, int second)
        {
            DateTime dt = new DateTime(2020, 1, 6, hour, minute, second);

            return dt;
        }
        //test

        public static TimeSpan TryParseTimeHourMinute(this string time)
        {
            var h = int.Parse(time.Split(':')[0]);
            var m = int.Parse(time.Split(':')[1]);
            var ts = new TimeSpan(h, m, 0);
            return ts;

        }


        public static DayTypeEnum GetDayTypeEnum(this DateOnly dateOnly, List<DateTime> holidays = null)
        {
            if (holidays != null && holidays.Any(x => x.Date.DayOfYear == dateOnly.DayOfYear && x.Date.Year == dateOnly.Year))
            {
                return DayTypeEnum.Holiday;
            }

            if (dateOnly.IsSunday()) return DayTypeEnum.Sunday;

            return DayTypeEnum.NormalDay;
        }

        public static bool IsSunday(this DateOnly dateOnly)
        {
            return dateOnly.DayOfWeek == DayOfWeek.Sunday;
        }

        public static bool IsSunday(this DateTime dateOnly)
        {
            return dateOnly.DayOfWeek == DayOfWeek.Sunday;
        }

        public static bool IsSaturday(this DateOnly dateOnly)
        {
            return dateOnly.DayOfWeek == DayOfWeek.Saturday;
        }

        public static DateTime ToTomorrow(this DateTime date)
        {
            return date.AddDays(1);
        }

        public static DateOnly ToDateOnly(this DateTime date)
        {
            return new DateOnly(date.Year, date.Month, date.Day);
        }

        public static DateTime ToDate(this DateOnly date)
        {
            return new DateTime(date.Year, date.Month, date.Day);
        }
    }
}
