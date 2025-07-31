using MediHub.Web.Dtos.Common;
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
        Task<ServiceResponse> Create(UpdateDeviceRequest deviceRequest);

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
        Task<ServiceResponse> Update(UpdateDeviceRequest devicesRequest);

        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task<ServiceResponse> Delete(List<Guid> ids);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<ServiceResponse> GetManufacturerBranch();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mamufaceturer"></param>
        /// <returns></returns>
        Task<ServiceResponse> GetDeviceByManufacturerName(int mamufaceturer);

        Task<ServiceResponse> GetDeviceByCalibrationNextDate();

        Task<ServiceResponse> GetDeviceByMaintenanceNextDate();
    }
}
