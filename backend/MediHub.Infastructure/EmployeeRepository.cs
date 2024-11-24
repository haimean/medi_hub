using Dapper;
using Microsoft.Extensions.Configuration;
using MISA.ApplicationCore.Entities;
using MISA.ApplicationCore.Interfaces.Repository;
using MISA.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MISA.Infastructure
{
    /// <summary>
    /// Class employee repository
    /// </summary>
    /// CreatedBy: PQ Huy (05.07.2021)
    public class EmployeeRepository : BaseRepository<Employee>, IEmployeeRepository
    {
        #region DECLARE

        #endregion

        #region Contructor
        public EmployeeRepository(IConfiguration configuration) : base(configuration)
        {
            
        }

        #endregion

        #region Method

        /// <summary>
        /// Hàm lấy ra code lớn nhất
        /// </summary>
        /// <returns>Trả về code mới nhất của nhân viên</returns>
        /// CreatedBy: PQ Huy(05.07.2021)
        public string GetMaxEmployeeCode()
        {
            using (var transaction = _dbConnection.BeginTransaction())
            {
                // Set store
                var storeName = $"Proc_GetMaxEmployeeCode";
                try
                {
                    // Query new code
                    var maxCode = _dbConnection.QueryFirstOrDefault<string>("Proc_GetMaxEmployeeCode", commandType: CommandType.StoredProcedure, transaction: transaction);

                    //Commit lại transaction
                    transaction.Commit();

                    // Return a max code
                    return maxCode;
                }
                catch (Exception)
                {
                    // roll back lại transaction
                    transaction.Rollback();

                    throw;
                }
            }
            
        }

        /// <summary>
        /// Hàm kiểm tra mã code tồn tại không
        /// </summary>
        /// <param name="code">code</param>
        /// <returns>Trả về true/false</returns>
        /// CreatedBy: PQ Huy(05.07.2021)
        public bool CheckEmployeeCodeExits(string code)
        {
            using (var transaction = _dbConnection.BeginTransaction())
            {
                // create store name
                var storeName = $"Proc_CheckEmployeeCodeExist";

                // add paramer
                DynamicParameters dynamicParameters = new DynamicParameters();
                dynamicParameters.Add($"@EmployeeCode", code);

                try
                {
                    // query database
                    var res = _dbConnection.QueryFirstOrDefault<bool>(storeName, param: dynamicParameters, commandType: CommandType.StoredProcedure, transaction: transaction);

                    //Commit lại transaction
                    transaction.Commit();

                    // return boolen
                    return res;
                }
                catch (Exception)
                {
                    // roll back lại transaction
                    transaction.Rollback();

                    throw;
                }
            }
        }

        /// <summary>
        /// Hàm lấy ra nhân viên theo email
        /// </summary>
        /// <param name="email">email</param>
        /// <returns>Trả về bản ghi theo email</returns>
        /// CreatedBy: PQ Huy(05.07.2021)
        public IEnumerable<Employee> GetByEmail(string email)
        {
            using (var transaction = _dbConnection.BeginTransaction())
            {
                //khởi tạo các commandText
                var parameterId = new DynamicParameters();
                parameterId.Add("Email", email);

                try
                {
                    var data = _dbConnection.Query<Employee>($"Proc_GetEmployeeByEmail", parameterId, commandType: CommandType.StoredProcedure, transaction: transaction);

                    //Commit lại transaction
                    transaction.Commit();

                    //Trả về dữ liệu
                    return data;
                }
                catch (Exception)
                {
                    // roll back lại transaction
                    transaction.Rollback();

                    throw;
                }
            }
            
        }

        /// <summary>
        /// Hàm lấy ra tổng bản ghi
        /// </summary>
        /// <returns>Trả về tổng số bản ghi hiện tại</returns>
        /// CreatedBy: PQ Huy(05.07.2021)
        public int GetEmployeeTotal()
        {
            using (var transaction = _dbConnection.BeginTransaction())
            {
                // Store name
                var storeName = $"Proc_GetEmployeeTotal";

                try
                {
                    // Query database
                    var resTotal = _dbConnection.QueryFirstOrDefault<int>(storeName, commandType: CommandType.StoredProcedure, transaction: transaction);

                    //Commit lại transaction
                    transaction.Commit();

                    // Return value
                    return resTotal;
                }
                catch (Exception)
                {
                    // roll back lại transaction
                    transaction.Rollback();

                    throw;
                }
            }
            
        }

        /// <summary>
        /// Hàm lấy ra bản ghi theo paging
        /// </summary>
        /// <param name="pageIndex">index page</param>
        /// <param name="pageSize">size page</param>
        /// CreatedBy: PQ Huy(05.07.2021)
        /// <returns>Lấy ra bản ghi theo paging</returns>
        public IEnumerable<Employee> GetEmployeePaging(int pageIndex, int pageSize)
        {
            using (var transaction = _dbConnection.BeginTransaction())
            {
                // Store name
                var storeName = $"Proc_GetEmployeePaging";

                // Add paramer
                DynamicParameters dynamicParameters = new DynamicParameters();
                dynamicParameters.Add($"@pageIndex", pageIndex);
                dynamicParameters.Add($"@pageSize", pageSize);

                try
                {
                    // Query database
                    var resultValue = _dbConnection.Query<Employee>(storeName, param: dynamicParameters, commandType: CommandType.StoredProcedure, transaction: transaction);

                    //Commit lại transaction
                    transaction.Commit();

                    // Return value
                    return resultValue;
                }
                catch (Exception)
                {
                    // roll back lại transaction
                    transaction.Rollback();

                    throw;
                }
            }
            
        }

        /// <summary>
        /// Hàm lọc ra nhân viên theo paging
        /// </summary>
        /// <param name="pageIndex">index page</param>
        /// <param name="pageSize">size page</param>
        /// <param name="employeeFilter">value filter</param>
        /// <returns>Lấy ra bản ghi theo filter</returns>
        /// CreatedBy: PQ Huy(05.07.2021)
        public IEnumerable<Employee> GetEmployeeFilter(int pageIndex, int pageSize, string employeeFilter)
        {
            using (var transaction = _dbConnection.BeginTransaction())
            {
                // Store name
                var storeName = $"Proc_GetEmployeeFilter";

                // Add paramer
                DynamicParameters dynamicParameters = new DynamicParameters();
                dynamicParameters.Add($"@pageIndex", pageIndex);
                dynamicParameters.Add($"@pageSize", pageSize);
                dynamicParameters.Add($"@employeeFilter", employeeFilter);

                try
                {
                    // Query database
                    var resultFilter = _dbConnection.Query<Employee>(storeName, param: dynamicParameters, commandType: CommandType.StoredProcedure, transaction: transaction);

                    //Commit lại transaction
                    transaction.Commit();

                    // Return value
                    return resultFilter;
                }
                catch (Exception)
                {
                    // roll back lại transaction
                    transaction.Rollback();

                    throw;
                }
            }
            
        }

        /// <summary>
        /// Hàm lấy ra tổng bản ghi filter
        /// </summary>
        /// <param name="employeeFilter">value filter</param>
        /// <returns>Lấy ra tất cả bản ghi khi filter</returns>
        /// CreatedBy: PQ Huy(05.07.2021)
        public IEnumerable<Employee> GetEmployeeFilterAll(string employeeFilter)
        {
            using (var transaction = _dbConnection.BeginTransaction())
            {
                // Store name
                var storeName = $"Proc_GetEmployeeFilterAll";

                // Add paramer
                DynamicParameters dynamicParameters = new DynamicParameters();
                dynamicParameters.Add($"@employeeFilter", employeeFilter);

                try
                {
                    // Query database
                    var resultFilter = _dbConnection.Query<Employee>(storeName, param: dynamicParameters, commandType: CommandType.StoredProcedure, transaction: transaction);

                    //Commit lại transaction
                    transaction.Commit();

                    // Return value
                    return resultFilter;
                }
                catch (Exception)
                {
                    // roll back lại transaction
                    transaction.Rollback();

                    throw;
                }
            }
            
            
        }

        /// <summary>
        /// Hàm lọc ra tổng nhân viên theo điều kiện
        /// </summary>
        /// <param name="employeeFilter">giá tị lọc</param>
        /// <returns>Lấy ra tổng bản ghi filter</returns>
        /// CreatedBy: PQ Huy(05.07.2021)
        public int GetTotalFilter(string employeeFilter)
        {
            using (var transaction = _dbConnection.BeginTransaction())
            {
                // Store name
                var storeName = $"Proc_EmployeeCountFilter";

                // Add paramer
                DynamicParameters dynamicParameters = new DynamicParameters();
                dynamicParameters.Add($"@employeeFilter", employeeFilter);

                try
                {
                    // Query database
                    var resTotal = _dbConnection.QueryFirstOrDefault<int>(storeName, param: dynamicParameters, commandType: CommandType.StoredProcedure, transaction: transaction);

                    //Commit lại transaction
                    transaction.Commit();

                    // Return value
                    return resTotal;
                }
                catch (Exception)
                {
                    // roll back lại transaction
                    transaction.Rollback();

                    throw;
                }
            }
        }

        #endregion
    }
}
