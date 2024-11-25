using AutoMapper;
using DashboardApi.Auth.CurrentUser;
using DashboardApi.Auth.PermisionChecker;
using DashboardApi.DatabaseContext.DapperDbContext;
using DashboardApi.HttpConfig;
using System.Globalization;

namespace DashboardApi.Application.BaseCommon
{
    public class BaseCommonService : Service, IBaseCommonService
    {
        #region DECLARE
        private readonly MaintenanceDbContext _appMaintenanceDbContext;
        private readonly AppMainDapperContext _appMainDapper;
        private readonly IPermissionChecker _permissionChecker;
        public readonly IMapper _mapper;
        public readonly ICurrentUser _currentUser;
        #endregion

        #region Constructor
        public BaseCommonService() { }
        #endregion

        #region Common Method
        /// <summary>
        /// get list pervious month
        /// </summary>
        /// <param name="numberPreviousMonth"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (13.03.2023)
        public Dictionary<string, List<DateTime>> GetListMonthNeed(int numberPreviousMonth)
        {
            Dictionary<string, List<DateTime>> lstMonth = new Dictionary<string, List<DateTime>>();

            var today = DateTime.Today;
            var currentMonth = new DateTime(today.Year, today.Month, 1);

            if (numberPreviousMonth >= 0)
            {
                for (int i = 0; i <= numberPreviousMonth; i++)
                {
                    var startOfMonth = currentMonth.AddMonths(-i);

                    int daysInMonth = DateTime.DaysInMonth(year: startOfMonth.Year, month: startOfMonth.Month);
                    var lastOfMonth = new DateTime(startOfMonth.Year, startOfMonth.Month, daysInMonth);

                    List<DateTime> temp = new List<DateTime>()
                    {
                        startOfMonth,
                        lastOfMonth
                    };

                    lstMonth.Add(startOfMonth.ToString("MMM-yy", CultureInfo.InvariantCulture), temp);
                }
            }

            return lstMonth;
        }

        /// <summary>
        /// get list pervious week need
        /// </summary>
        /// <param name="numberPrevious"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (05.06.2023)
        public Dictionary<string, List<DateTime>> GetListWeekNeed(int numberPrevious)
        {
            Dictionary<string, List<DateTime>> lstMonth = new Dictionary<string, List<DateTime>>();
            List<DateTime> temp = new List<DateTime>();

            DateTime baseDate = DateTime.Now;

            // add current week
            var thisWeekStart = baseDate.AddDays(-(int)baseDate.DayOfWeek);
            var thisWeekEnd = thisWeekStart.AddDays(7).AddSeconds(-1);

            temp.Add(thisWeekStart);
            temp.Add(thisWeekEnd);
            lstMonth.Add(thisWeekStart.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture), temp);

            for (int i = 1; i <= numberPrevious; i++)
            {
                var lastWeekStart = thisWeekStart.AddDays(-7 * i);
                var lastWeekEnd = thisWeekStart.AddSeconds(-1);

                temp = new List<DateTime> { lastWeekStart, lastWeekEnd };

                lstMonth.Add(lastWeekStart.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture), temp);
            }


            return lstMonth;
        }
        #endregion
    }
}