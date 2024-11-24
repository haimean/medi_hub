using Microsoft.AspNetCore.Mvc;
using MISA.ApplicationCore.Entities;
using MISA.ApplicationCore.Enums;
using MISA.ApplicationCore.Interfaces.Service;
using MISA.CukCuk.Web.Controllers;
using System;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MISA.AMIS.Web.Controllers
{
    /// <summary>
    /// Api Danh mục khác hàng
    /// CreatedBy: PQ Huy (05.07.2021)
    /// </summary>
    [Route("v1/[controller]")]
    [ApiController]
    public class EmployeesController : BaseEntityController<Employee>
    {
        #region DECLARE
        IEmployeeService _employeeService;
        #endregion

        #region Contructor
        public EmployeesController(IEmployeeService employeeService) : base(employeeService)
        {
            _employeeService = employeeService;
        }
        #endregion

        #region Method

        // GET api/<EmployeesController>/1
        /// <summary>
        /// Lấy mã nhân viên mới nhất
        /// </summary>
        /// <returns>Trả về mã nhân viên mới</returns>
        /// CreatedBy: PQ Huy (07.07.2021)
        [HttpGet("NewEmployeeCode")]
        public IActionResult GetNewEmployeeCode()
        {
            try
            {
                var newCode = _employeeService.GetNewEmployeeCode();

                // return a new code
                return Ok(newCode);
            } catch(Exception ce)
            {
                //trả về dữ liệu
                return BadRequest(ce);
            }
            
        }

        // GET api/<EmployeesController>/2
        /// <summary>
        /// Kiểm tra mã nhân viên tồn tại trong hệ thống
        /// </summary>
        /// <param name="code">Mã nhân viên</param>
        /// <returns>Trả về true/false khi có mã nhân viên</returns>
        /// CreatedBy: PQ Huy (07.07.2021)
        [HttpGet("CheckEmployeeCodeExist")]
        public IActionResult CheckEmployeeCodeExist(string code)
        {
            try
            {
                var isExist = _employeeService.CheckEmployeeCodeExits(code);

                // return value
                return Ok(isExist);
            }
            catch (Exception ce)
            {
                //trả về dữ liệu
                return BadRequest(ce);
            }
            
        }

        // GET api/<EmployeesController>/3
        /// <summary>
        /// Lấy dữ liệu bản ghi theo id
        /// </summary>
        /// <returns>Trả về bản ghi với id tương ứng</returns>
        /// CreatedBy: PQ Huy (05.07.2021)
        [HttpGet("Email")]
        public IActionResult GetEmployeeByEmail(string email)
        {
            try
            {
                // gọi function lấy dữ liệu
                var data = _employeeService.GetByEmail(email);

                //trả về dữ liệu
                return Ok(data);
            }
            catch (Exception ce)
            {
                //trả về dữ liệu
                return BadRequest(ce);
            }
            
        }

        // GET api/<EmployeesController>/4
        /// <summary>
        /// Lọc ra danh sách nhân viên
        /// </summary>
        /// <param name="pageIndex">Index trang hiện tại</param>
        /// <param name="pageSize">Tổng số bản ghi trên một trang</param>
        /// <param name="employeeFilter">value filter</param>
        /// <returns>Trả về nhân viên filer</returns>
        /// CreatedBy: PQ Huy (08.07.2021)
        [HttpGet("EmployeeFilter")]
        public IActionResult GetEmployeeFilter(int pageIndex, int pageSize, string employeeFilter)
        {
            try
            {
                // Get service
                var resultFilter = _employeeService.GetEmployeeFilter(pageIndex, pageSize, employeeFilter);

                // Return value
                return Ok(resultFilter);
            }
            catch (Exception ce)
            { 
                //trả về dữ liệu
                return BadRequest(ce);
            }
            
        }

        // GET api/<EmployeesController>/5
        /// <summary>
        /// Lấy ra tất cả dánh sách nhân viên đã lọc
        /// </summary>
        /// <param name="employeeFilter">value filter</param>
        /// <returns>Trả về tất cả dữ liệu bản ghi filter</returns>
        /// CreatedBy: PQ Huy (08.07.2021)
        [HttpGet("EmployeeFilterAll")]
        public IActionResult GetEmployeeFilterAll(string employeeFilter)
        {
            try
            {
                // Get service
                var resultFilter = _employeeService.GetEmployeeFilterAll(employeeFilter);

                // Return value
                return Ok(resultFilter);
            }
            catch (Exception ce)
            {
                //trả về dữ liệu
                return BadRequest(ce);
            }
            
        }

        // GET api/<EmployeesController>/6
        /// <summary>
        /// Phân trang dữ liệu
        /// </summary>
        /// <param name="pageIndex">Index của trang hiện tại</param>
        /// <param name="pageSize">Số bản ghi trên một trang</param>
        /// <returns>Lấy ra bản ghi theo paging</returns>
        /// CreatedBy: PQ Huy (08.07.2021)
        [HttpGet("EmployeePaging")]
        public IActionResult GetEmployeePaging(int pageIndex, int pageSize)
        {
            try
            {
                // Get service
                var resultPaging = _employeeService.GetEmployeePaging(pageIndex, pageSize);

                // Return value
                return Ok(resultPaging);
            }
            catch (Exception ce)
            {
                //trả về dữ liệu
                return BadRequest(ce);
            }
            
        }

        // GET api/<EmployeesController>/7
        /// <summary>
        /// Tổng số bản ghi
        /// </summary>
        /// <returns>Trả về tổng số bản ghi hiện tại</returns>
        /// CreatedBy: PQ Huy (08.07.2021)
        [HttpGet("EmployeeTotal")]
        public IActionResult GetEmployeeTotal( )
        {
            try
            {
                // Get service
                var total = _employeeService.GetEmployeeTotal();

                // Return value
                return Ok(total);
            }
            catch (Exception ce)
            {
                //trả về dữ liệu
                return BadRequest(ce);
            }
        }
        #endregion
    }
}
