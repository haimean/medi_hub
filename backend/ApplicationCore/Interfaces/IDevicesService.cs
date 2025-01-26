using MediHub.Web.HttpConfig;
using MediHub.Web.Models;

namespace MediHub.Web.ApplicationCore.Interfaces
{
    public interface IDevicesService
    {
        /// <summary>
        /// Create by list
        /// </summary>
        /// <param name="devices"></param>
        /// <returns></returns>
        Task<ServiceResponse> Create(List<DeviceEntity> devices);

        /// <summary>
        /// Get all
        /// </summary>
        /// <returns></returns>
        Task<ServiceResponse> Get();

        /// <summary>
        /// Get by id
        /// </summary>
        /// <returns></returns>
        Task<ServiceResponse> Get(Guid id);

        /// <summary>
        /// Get by list id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<ServiceResponse> Get(List<Guid> id);

        /// <summary>
        /// Update
        /// </summary>
        /// <param name="devices"></param>
        /// <returns></returns>
        Task<ServiceResponse> Update(List<DeviceEntity> devices);

        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task<ServiceResponse> Delete(List<Guid> ids);
    }
}
