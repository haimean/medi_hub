using MISA.ApplicationCore.Entities;
using System.Collections.Generic;

namespace MISA.ApplicationCore.Interfaces.Repository
{
    /// <summary>
    /// Interface repository danh mục khách hàng 
    /// </summary>
    /// CreatedBy: PQ Huy (05.07.2021)
    public interface IEmployeeRepository : IBaseRepository<Employee>
    {
        /// <summary>
        /// Lấy mã nhân viên mới nhất trong database
        /// </summary>
        /// <returns>Trả về nhân viên</returns>
        /// CreatedBy: PQ Huy (07.07.2021)
        string GetMaxEmployeeCode();

        /// <summary>
        /// Kiểm tra mã nhân viên đã tồn trong hệ thống
        /// </summary>
        /// <param name="code">Mã nhân viên</param>
        /// <returns>Trả về true/false khi có mã nhân viên</returns>
        /// CreatedBy: ntquan(11/05/2021)
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
        IEnumerable<Employee> GetEmployeePaging(int pageIndex, int pageSize);

        /// <summary>
        /// Lọc dữ liệu theo các tiêu chí
        /// </summary>
        /// <param name="pageIndex">Chỉ mục của trang hiện tại</param>
        /// <param name="pageSize">Tổng số bản ghi trên một trang</param>
        /// <param name="employeeFilter">Value truyền vào cần lọc</param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (08.07.2021)
        IEnumerable<Employee> GetEmployeeFilter(int pageIndex, int pageSize, string employeeFilter);

        /// <summary>
        /// Lấy ra tất cả dữ liệu lọc theo các tiêu chí
        /// <param name="employeeFilter">Value truyền vào cần lọc</param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (08.07.2021)
        IEnumerable<Employee> GetEmployeeFilterAll(string employeeFilter);

        /// <summary>
        /// Đếm tổng bản ghi khi filter
        /// </summary>
        /// <param name="employeeFilter">Value truyền vào cần lọc</param>
        /// <returns>Trả về tổng số bản ghi lọc được</returns>
        /// CreatedBy: PQ Huy (09.07.2021)
        int GetTotalFilter(string employeeFilter);
    }
}
