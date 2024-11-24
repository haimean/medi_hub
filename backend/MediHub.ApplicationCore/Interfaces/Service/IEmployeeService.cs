using MISA.ApplicationCore.Entities;
using System.Collections.Generic;

namespace MISA.ApplicationCore.Interfaces.Service
{
    /// <summary>
    /// Interface service danh mục khách hàng 
    /// </summary>
    /// CreatedBy: PQ Huy (05.07.2021)
    public interface IEmployeeService : IBaseService<Employee>
    {
        /// <summary>
        /// Lấy mã nhân viên mới nhất
        /// </summary>
        /// <returns>Trả về mã nhân viên mới nhất</returns>
        /// CreatedBy: PQ Huy(07.07.2021)
        string GetNewEmployeeCode();

        /// <summary>
        /// Kiểm tra mã nhân viên
        /// </summary>
        /// <param name="code">mã nhân viên</param>
        /// <returns>Trả về true/false khi có mã nhân viên</returns>
        /// CreatedBy: PQ Huy (07.07.2021)
        bool CheckEmployeeCodeExits(string code);

        /// <summary>
        /// Lấy thông tin theo email
        /// </summary>
        /// <param name="email"> Mã bản ghi</param>
        /// <returns>Trả về bản ghi tương ứng</returns>
        /// CreatedBy: PQ Huy (08.07.2021)
        IEnumerable<Employee> GetByEmail(string email);

        /// <summary>
        /// Hàm trả về tổng số bản ghi
        /// </summary>
        /// <returns>Trả về tổng số bản ghi</returns>
        /// CreatedBy: PQ Huy (08.07.2021)
        int GetEmployeeTotal();

        /// <summary>
        /// Hàm trả về dữ liệu phân trang theo tham số truyền vào
        /// </summary>
        /// <param name="pageIndex">Chỉ mục của page hiện tại</param>
        /// <param name="pageSize">Số bản ghi trên một page</param>
        /// <returns>Trả về dữ liệu phân trang theo tham số truyền vào và tổng số bản ghi hiện tại</returns>
        ServiceResult GetEmployeePaging(int pageIndex, int pageSize);

        /// <summary>
        /// Lọc dữ liệu theo các tiêu chí
        /// </summary>
        /// <param name="pageIndex">Chỉ mục của trang hiện tại</param>
        /// <param name="pageSize">Tổng số bản ghi trên một trang</param>
        /// <param name="employeeFilter">Value truyền vào cần lọc</param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (08.07.2021)
        ServiceResult GetEmployeeFilter(int pageIndex, int pageSize, string employeeFilter);

        /// <summary>
        /// Lấy ra rất cả dữ liệu được lọc theo các tiêu chí
        /// </summary>
        /// <param name="employeeFilter">Value truyền vào cần lọc</param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (11.07.2021)
        ServiceResult GetEmployeeFilterAll(string employeeFilter);

        /// <summary>
        /// Đếm tổng bản ghi khi filter
        /// </summary>
        /// <param name="employeeFilter">Value truyền vào cần lọc</param>
        /// <returns>Trả về tổng số bản ghi lọc được</returns>
        /// CreatedBy: PQ Huy (09.07.2021)
        int GetTotalFilter(string employeeFilter);
    }
}
