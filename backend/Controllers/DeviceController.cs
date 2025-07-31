using Azure;
using MediHub.Web.ApplicationCore.Interfaces;
using MediHub.Web.Dtos.Common;
using MediHub.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace MediHub.Web.Controllers
{
    [ApiController]
    [Route("v1/devices")]
    public class DevicesController : ControllerBase
    {
        private readonly IDevicesService _devicesService;

        public DevicesController(IDevicesService devicesService)
        {
            _devicesService = devicesService;
        }

        /// <summary>
        /// Create new devices
        /// </summary>
        /// <param name="devices"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UpdateDeviceRequest devicesRequest)
        {
            var response = await _devicesService.Create(devicesRequest);
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Get all devices
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var response = await _devicesService.Get();
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Get devices by id
        /// </summary>
        /// <returns></returns>
        [HttpGet("id")]
        public async Task<IActionResult> Get(Guid id)
        {
            var response = await _devicesService.Get(id);
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Get devices by IDs
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpGet("ids")]
        public async Task<IActionResult> GetByIds([FromQuery] List<Guid> ids)
        {
            var response = await _devicesService.Get(ids);
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Update devices
        /// </summary>
        /// <param name="devices"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateDeviceRequest devicesRequest)
        {
            var response = await _devicesService.Update(devicesRequest);
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Delete devices by IDs
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] List<Guid> ids)
        {
            var response = await _devicesService.Delete(ids);
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Get all manufacture and branch
        /// </summary>
        /// <returns></returns>
        [HttpGet("manufacturer-branch")]
        public async Task<IActionResult> GetManufacturerBranch()
        {
            var response = await _devicesService.GetManufacturerBranch();
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("manufactureName")]
        public async Task<IActionResult> GetDeviceByManufacturerName(int manufactureName)
        {
            var response = await _devicesService.GetDeviceByManufacturerName(manufactureName);
            return StatusCode((int) response.StatusCode, response);
        }

        [HttpGet("calibrationNextDate")]
        public async Task<IActionResult> GetDeviceByCalibrationNextDate()
        {
            var response = await _devicesService.GetDeviceByCalibrationNextDate();
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("maintenanceNextDate")]
        public async Task<IActionResult> GetDeviceByMaintenanceNextDate()
        {
            var response = await _devicesService.GetDeviceByMaintenanceNextDate();
            return StatusCode(response.StatusCode, response);
        }
    }
}