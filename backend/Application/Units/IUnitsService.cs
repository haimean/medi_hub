using DashboardApi.Application.Project.Blocks;

namespace DashboardApi.Application.Project
{
    public interface IUnitsService
    {
        /// <summary>
        /// get units by id project
        /// </summary>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (01.06.2023)
        Task<List<UnitskResponse>> GetUnits(string id);
    }
}
