using MediHub.Web.ApplicationCore.Auth.CurrentUser;
using MediHub.Web.ApplicationCore.Interfaces;
using MediHub.Web.Data.Repository;
using MediHub.Web.HttpConfig;
using MediHub.Web.Models;

namespace MediHub.Web.ApplicationCore.Service
{
    public class DepartmentsService : HttpConfig.Service, IDepartmentsService
    {
        private readonly IRepository _repository;
        private readonly ICurrentUser _currentUser;

        public DepartmentsService(IRepository repository, ICurrentUser currentUser)
        {
            _repository = repository;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="departments"></param>
        /// <returns></returns>
        public async Task<ServiceResponse> Create(List<DepartmentEntity> departments)
        {
            try
            {
                foreach (var department in departments)
                {
                    department.IsDeleted = false; // Đánh dấu phòng ban là chưa xóa
                    department.CreatedBy = _currentUser.GetEmail(); // Ghi lại người tạo
                    department.UpdatedBy = _currentUser.GetEmail(); // Ghi lại người cập nhật

                    await _repository.AddAsync(department);
                }

                await _repository.SaveChangeAsync();
            }
            catch (Exception ce)
            {
                return BadRequest(ce.Message);
            }

            return Ok(departments);
        }

        /// <summary>
        /// Get all departments
        /// </summary>
        /// <returns></returns>
        public async Task<ServiceResponse> Get()
        {
            var result = new List<DepartmentEntity>();

            try
            {
                result = (await _repository.FindAllAsync<DepartmentEntity>())
                         .Where(c => c.IsDeleted != true)
                         .ToList();
            }
            catch (Exception ce)
            {
                return BadRequest(ce.Message);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get departments by ids
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task<ServiceResponse> Get(List<Guid> ids)
        {
            var result = new List<DepartmentEntity>();

            try
            {
                result = (await _repository.FindAllAsync<DepartmentEntity>())
                         .Where(c => !c.IsDeleted && ids.Contains(c.Id))
                         .ToList();
            }
            catch (Exception ce)
            {
                return BadRequest(ce.Message);
            }

            return Ok(result);
        }

        /// <summary>
        /// Update departments
        /// </summary>
        /// <param name="departments"></param>
        /// <returns></returns>
        public async Task<ServiceResponse> Update(List<DepartmentEntity> departments)
        {
            try
            {
                foreach (var department in departments)
                {
                    department.UpdatedBy = _currentUser.GetEmail(); // Ghi lại người cập nhật
                    await _repository.UpdateAsync(department);
                }

                await _repository.SaveChangeAsync();
                return Ok(departments);
            }
            catch (Exception ce)
            {
                return BadRequest(ce.Message);
            }
        }

        /// <summary>
        /// Delete departments
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task<ServiceResponse> Delete(List<Guid> ids)
        {
            try
            {
                foreach (var id in ids)
                {
                    await _repository.DeleteAsync<DepartmentEntity>(id);
                }

                await _repository.SaveChangeAsync();
                return Ok(ids);
            }
            catch (Exception ce)
            {
                return BadRequest(ce.Message);
            }
        }
    }
}