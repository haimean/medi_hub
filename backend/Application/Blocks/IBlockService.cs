using DashboardApi.Application.Project.Blocks;

namespace DashboardApi.Application.Project
{
    public interface IBlockService
    {
        /// <summary>
        /// get all block
        /// </summary>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (01.06.2023)
        Task<List<BlockResponse>> GetBlock();

        /// <summary>
        /// get block by id project
        /// </summary>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (01.06.2023)
        Task<List<BlockResponse>> GetBlock(string id);

        /// <summary>
        /// get all unit
        /// </summary>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (01.06.2023)
        Task<List<UnitResponse>> GetUnit();
    }
}
