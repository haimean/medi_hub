using Dapper;
using Microsoft.Extensions.Configuration;
using MISA.ApplicationCore.Entities;
using MISA.ApplicationCore.Enums;
using MISA.ApplicationCore.Interfaces;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace MISA.Infrastructure
{
    /// <summary>
    /// class base repository
    /// </summary>
    /// <typeparam name="Generic">generic</typeparam>
    /// CreatedBy: PQ Huy (05.07.2021)
    public class BaseRepository<Generic> : IBaseRepository<Generic> where Generic : BaseEntity
    {
        #region DECLARE
        IConfiguration _configuration;
        string _connectionString = string.Empty;
        protected IDbConnection _dbConnection = null;
        public string _tableName = string.Empty;
        #endregion

        #region Contructor
        public BaseRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("MISAAMISConnectionString");
            _dbConnection = new MySqlConnection(_connectionString);
            _dbConnection.Open();
            _tableName = typeof(Generic).Name;
        }
        #endregion

        #region Method
        /// <summary>
        /// Hàm lấy ra tất cả các bản ghi
        /// </summary>
        /// <returns>Trả về tất cả các bản ghi</returns>
        /// CreatedBy: PQ Huy (05.07.2021)
        public IEnumerable<Generic> Get()
        {
            using(var transaction = _dbConnection.BeginTransaction())
            {
                try
                {
                    //khởi tạo và thực thi các commandText
                    var resEmployees = _dbConnection.Query<Generic>($"Proc_Get{_tableName}s", commandType: CommandType.StoredProcedure, transaction: transaction);

                    // commit laji transaction
                    transaction.Commit();

                    //Trả về dữ liệu kết quả
                    return resEmployees;
                }
                catch (Exception ce)
                {
                    transaction.Rollback();
                    throw new NotImplementedException(ce.ToString());
                }
            }
        }

        /// <summary>
        /// Hàm lấy ra bản ghi theo id
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>Trả về bản ghi theo id</returns>
        /// CreatedBy: PQ Huy (05.07.2021)
        public IEnumerable<Generic> GetById(Guid id)
        {
            //khởi tạo các commandText
            var parameterId = new DynamicParameters();
            parameterId.Add($"@{_tableName}Id", id);

            using (var transaction = _dbConnection.BeginTransaction())
            {
                try
                {
                    var data = _dbConnection.Query<Generic>($"Proc_Get{_tableName}ById", parameterId, commandType: CommandType.StoredProcedure, transaction: transaction);

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
        /// Hàm lấy ra bảng ghi theo code
        /// </summary>
        /// <param name="code">code</param>
        /// <returns>Trả về bản ghi theo code</returns>
        /// CreatedBy: PQ Huy (05.07.2021)
        public IEnumerable<Generic> GetByCode(string code)
        {
            //khởi tạo các commandText
            var parameterId = new DynamicParameters();
            parameterId.Add($"@{_tableName}Code", code);
            using (var transaction = _dbConnection.BeginTransaction())
            {
                try
                {
                    var data = _dbConnection.Query<Generic>($"Proc_Get{_tableName}ByCode", parameterId, commandType: CommandType.StoredProcedure, transaction: transaction);

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
        /// Hàm thêm dữ liệu
        /// </summary>
        /// <param name="data">data</param>
        /// <returns>Trả về kết quả sau khi thêm</returns>
        /// CreatedBy: PQ Huy (05.07.2021)
        public ServiceResult Insert(Generic data)
        {
            // mapping data
            var parameter = MappingDbType(data);
            var serviceResult = new ServiceResult();
            using (var transaction = _dbConnection.BeginTransaction())
            {
                try
                {
                    //khởi tạo các commandText
                    var rowAffects = _dbConnection.Execute($"Proc_Insert{_tableName}", parameter, commandType: CommandType.StoredProcedure, transaction: transaction);
                    serviceResult.Data = rowAffects;
                    serviceResult.MISACode = MISAEnum.IsValid;
                    serviceResult.Messenger = Infastructure.Properties.Resources.AddSuccess;

                    //Commit lại transaction
                    transaction.Commit();
                }
                catch (Exception ce)
                {
                    serviceResult.MISACode = MISAEnum.NotValid;
                    serviceResult.Messenger = ce.ToString();

                    // roll back lại transaction
                    transaction.Rollback();

                    return serviceResult;
                }

                //Trả về dữ liệu số bản ghi thêm mới
                return serviceResult;
            }
            
        }

        /// <summary>
        /// Cập nhật dữ liệu
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="data">data</param>
        /// <returns>Trả về kết quả sau khi update</returns>
        /// CreatedBy: PQ Huy (05.07.2021)
        public ServiceResult Update(Guid id, Generic data)
        {
            var serviceResult = new ServiceResult();

            //khởi tạo các commandText
            var parameter = MappingDbType(data);
            using (var transaction = _dbConnection.BeginTransaction())
            {
                try
                {
                    var rowAffects = _dbConnection.Execute($"Proc_Update{_tableName}", parameter, commandType: CommandType.StoredProcedure, transaction: transaction);

                    //Trả về dữ liệu số bản ghi thêm mới
                    serviceResult.Data = rowAffects;
                    serviceResult.MISACode = MISAEnum.IsValid;
                    serviceResult.Messenger = Infastructure.Properties.Resources.EditSuccess;

                    //Commit lại transaction
                    transaction.Commit();
                }
                catch (Exception ce)
                {
                    throw new NotImplementedException(ce.ToString());

                    //Trả về dữ liệu số bản ghi thêm mới
                    #pragma warning disable CS0162 // Unreachable code detected
                    serviceResult.Data = ce;
                    #pragma warning restore CS0162 // Unreachable code detected
                    serviceResult.MISACode = MISAEnum.NotValid;
                    serviceResult.Messenger = Infastructure.Properties.Resources.EditError;

                    // roll back lại transaction
                    transaction.Rollback();
                }

                // return service result
                return serviceResult;
            }
            
        }

        /// <summary>
        /// Xóa theo id
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>Trả về kết quả sau khi xóa</returns>
        /// CreatedBy: PQ Huy (05.07.2021)
        public ServiceResult DeleteById(Guid id)
        {

            //khởi tạo các commandText
            var parameterId = new DynamicParameters();
            parameterId.Add($"@{_tableName}Id", id);
            using (var transaction = _dbConnection.BeginTransaction())
            {
                try
                {
                    var rowAffects = _dbConnection.Execute($"Proc_Delete{_tableName}ById", parameterId, commandType: CommandType.StoredProcedure, transaction: transaction);

                    //Trả về dữ liệu số bản ghi xóa
                    var serviceResult = new ServiceResult();
                    serviceResult.Data = rowAffects;

                    //Commit lại transaction
                    transaction.Commit();

                    // trả về kết quản
                    return serviceResult;
                }
                catch (Exception ce)
                {
                    //Trả về dữ liệu số bản ghi xóa
                    var serviceResult = new ServiceResult();
                    serviceResult.Data = ce;
                    serviceResult.MISACode = MISAEnum.NotValid;

                    // roll back lại transaction
                    transaction.Rollback();

                    // trả về kết quản
                    return serviceResult;
                }
            }
            
            
        }

        /// <summary>
        /// Hàm mapping data type
        /// </summary>
        /// <typeparam name="Generic"></typeparam>
        /// <param name="generic"></param>
        /// <returns>Trả về param sau khi maping</returns>
        /// CreatedBy: PQ Huy (05.07.2021)
        #pragma warning disable CS0693 // Type parameter has the same name as the type parameter from outer type
        protected DynamicParameters MappingDbType<Generic>(Generic generic)
        #pragma warning restore CS0693 // Type parameter has the same name as the type parameter from outer type
        {
            // get data type
            var properties = generic.GetType().GetProperties();
            var parameters = new DynamicParameters();

            try
            {
                // for list property
                foreach (var property in properties)
                {
                    var propertyName = property.Name;
                    var propertyValue = property.GetValue(generic);
                    var propertyType = property.PropertyType;

                    // check type of property
                    if (propertyType == typeof(Guid) || propertyType == typeof(Guid?))
                    {
                        parameters.Add($"@{propertyName}", propertyValue, DbType.String);
                    }
                    else if (propertyType == typeof(ServiceResult))
                    {
                        continue;
                    }
                    else
                    {
                        parameters.Add($"@{propertyName}", propertyValue);
                    }
                }


                // return paramer
                return parameters;
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// Hàm lấy ra entity theo property
        /// </summary>
        /// <param name="generic"></param>
        /// <param name="property"></param>
        /// <returns>Trả về bản ghi theo property</returns>
        /// CreatedBy: PQ Huy (05.07.2021)
        public IEnumerable<Generic> GetEntityByProperty(Generic generic, PropertyInfo property)
        {
            // query database
            var propertyName = property.Name;
            var propertyValue = property.GetValue(generic);
            var keyValue = generic.GetType().GetProperty($"{_tableName}Id").GetValue(generic);
            var query = "";
            using (var transaction = _dbConnection.BeginTransaction())
            {
                try
                {
                    // check state action
                    if (generic.EntityState == EntityState.AddNew)
                    {
                        // add paramer
                        DynamicParameters dynamicParameters = new DynamicParameters();
                        dynamicParameters.Add($"{propertyName}", propertyValue);

                        query = $"SELECT * FROM {_tableName} WHERE {propertyName} = '{propertyValue}'";
                    }
                    else if (generic.EntityState == EntityState.Update)
                    {
                        query = $"SELECT * FROM {_tableName} WHERE {propertyName} = '{propertyValue}' AND {_tableName}Id <> '{keyValue}'";
                    }
                    else
                    {
                        return null;
                    }

                    var entity = _dbConnection.Query<Generic>(query, commandType: CommandType.Text, transaction: transaction);

                    //Commit lại transaction
                    transaction.Commit();

                    // trả về entity
                    return entity;
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
        /// Hàm import dữ liệu
        /// </summary>
        /// <param name="data">data</param>
        /// <returns>Trả về kết quả import</returns>
        /// CreatedBy: PQ Huy (05.07.2021)
        public ServiceResult ImportData(Generic[] data)
        {
            var rowAffects = 0;
            var serviceResult = new ServiceResult();
            using (var transaction = _dbConnection.BeginTransaction())
            {
                try
                {
                    serviceResult.Data = rowAffects;
                    serviceResult.MISACode = MISAEnum.Success;

                    //Commit lại transaction
                    transaction.Commit();

                    // return value
                    return serviceResult;
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
        /// Hàm get entity theo property
        /// </summary>
        /// <param name="generic">generic</param>
        /// <param name="propertyName">property name</param>
        /// <returns>Trả về entity theo property</returns>
        /// CreatedBy: PQ Huy (05.07.2021)
        public IEnumerable<Generic> GetEntityByProperty(Generic generic, string propertyName)
        {
            var propvalue = generic.GetType().GetProperty(propertyName).GetValue(generic);
            using (var transaction = _dbConnection.BeginTransaction())
            {
                try
                {
                    // query database
                    string query = $"select * FROM {_tableName} where {propertyName} = '{propvalue}'";
                    var entitySearch = _dbConnection.Query<Generic>(query, transaction: transaction);

                    //Commit lại transaction
                    transaction.Commit();

                    // return value
                    return entitySearch;
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
        /// Hàm insert nhiều bản ghi
        /// </summary>
        /// <param name="generics">generics</param>
        /// <returns>Trả về số bản ghi được insert</returns>
        /// CreatedBy: PQ Huy (05.07.2021)
        public ServiceResult MutilpleInsert(List<Generic> generics)
        {
            ServiceResult result = new ServiceResult();
            int count = 0;

            try
            {
                foreach (Generic generic in generics)
                {
                    ServiceResult res = Insert(generic);

                    if (res.MISACode == MISAEnum.IsValid)
                    {
                        count++;
                    }
                }

                result.Data = count;

                // return data
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion
    }
}
