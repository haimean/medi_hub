using MediHub.Web.ApplicationCore.Interfaces;
using MediHub.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace MediHub.Web.Controllers
{
    [ApiController]
    [Route("v1/departments")]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentsService _departmentsService;

        public DepartmentsController(IDepartmentsService departmentsService)
        {
            _departmentsService = departmentsService;
        }

        /// <summary>
        /// Create new departments
        /// </summary>
        /// <param name="departments"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] List<DepartmentEntity> departments)
        {
            var response = await _departmentsService.Create(departments);
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Get all departments
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var response = await _departmentsService.Get();
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Get departments by IDs
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpGet("ids")]
        public async Task<IActionResult> GetByIds([FromQuery] List<Guid> ids)
        {
            var response = await _departmentsService.Get(ids);
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Update departments
        /// </summary>
        /// <param name="departments"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] List<DepartmentEntity> departments)
        {
            var response = await _departmentsService.Update(departments);
            return StatusCode((int)response.StatusCode, response);
        }

        /// <summary>
        /// Delete departments by IDs
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] List<Guid> ids)
        {
            var response = await _departmentsService.Delete(ids);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}