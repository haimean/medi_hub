using MediHub.Web.ApplicationCore.Interfaces;
using MediHub.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace MediHub.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUsersService _usersService;

        public UsersController(IUsersService usersService)
        {
            _usersService = usersService;
        }

        /// <summary>
        /// Create new devices
        /// </summary>
        /// <param name="devices"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] List<UserEntity> devices)
        {
            var response = await _usersService.Create(devices);
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Get all devices
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var response = await _usersService.Get();
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
            var response = await _usersService.Get(ids);
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Update devices
        /// </summary>
        /// <param name="devices"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] List<UserEntity> devices)
        {
            var response = await _usersService.Update(devices);
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
            var response = await _usersService.Delete(ids);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}