namespace DashboardApi.Application.BaseCommon
{
    public interface IBaseCommonService
    {
        /// <summary>
        /// get list pervious month
        /// </summary>
        /// <param name="numberPreviousMonth"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (06.03.2023)
        Dictionary<string, List<DateTime>> GetListMonthNeed(int numberPreviousMonth);

        /// <summary>
        /// get list pervious month
        /// </summary>
        /// <param name="numberPreviousMonth"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (06.03.2023)
        Dictionary<string, List<DateTime>> GetListWeekNeed(int numberPreviousMonth);
    }
}
