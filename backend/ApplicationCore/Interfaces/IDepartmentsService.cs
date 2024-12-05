using MediHub.Web.HttpConfig;
using MediHub.Web.Models;

namespace MediHub.Web.ApplicationCore.Interfaces
{
    public interface IDepartmentsService
    {
        /// <summary>
        /// Create by list
        /// </summary>
        /// <param name="departments"></param>
        /// <returns></returns>
        Task<ServiceResponse> Create(List<DepartmentEntity> departments);

        /// <summary>
        /// Get all
        /// </summary>
        /// <returns></returns>
        Task<ServiceResponse> Get();

        /// <summary>
        /// Get by list id
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task<ServiceResponse> Get(List<Guid> ids);

        /// <summary>
        /// Update
        /// </summary>
        /// <param name="departments"></param>
        /// <returns></returns>
        Task<ServiceResponse> Update(List<DepartmentEntity> departments);

        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task<ServiceResponse> Delete(List<Guid> ids);
    }
}