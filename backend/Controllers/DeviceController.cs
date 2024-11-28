using MediHub.Web.ApplicationCore.Interfaces;
using MediHub.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace MediHub.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        public async Task<IActionResult> Create([FromBody] List<DeviceEntity> devices)
        {
            var response = await _devicesService.Create(devices);
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
        public async Task<IActionResult> Update([FromBody] List<DeviceEntity> devices)
        {
            var response = await _devicesService.Update(devices);
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
    }
}