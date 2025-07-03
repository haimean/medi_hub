using MediHub.Web.ApplicationCore.Auth.CurrentUser;
using MediHub.Web.ApplicationCore.Interfaces;
using MediHub.Web.Data.Repository;
using MediHub.Web.HttpConfig;
using MediHub.Web.Models;

namespace MediHub.Web.ApplicationCore.Service
{
    public class UsersService : HttpConfig.Service, IUsersService
    {
        private readonly IRepository _repository;
        private readonly ICurrentUser _currentUser;

        public UsersService(IRepository repository, ICurrentUser currentUser)
        {
            _repository = repository;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        /// CreatedBy: HieuNM
        public async Task<ServiceResponse> Create(List<UserEntity> users)
        {
            try
            {
                foreach (var user in users)
                {
                    // Băm mật khẩu trước khi lưu
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

                    user.IsDeleted = false;
                    user.CreatedBy = _currentUser.GetEmail();
                    user.UpdatedBy = _currentUser.GetEmail();

                    await _repository.AddAsync(user);
                }

                await _repository.SaveChangeAsync();
            }
            catch (Exception ce)
            {
                return BadRequest(ce.Message);
            }

            return Ok(users);
        }

        /// <summary>
        /// Get all users
        /// </summary>
        /// <returns></returns>
        /// CreatedBy: HieuNM
        public async Task<ServiceResponse> Get()
        {
            var result = new List<UserEntity>();

            try
            {
                result = (await _repository.FindAllAsync<UserEntity>())
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
        /// Get users by ids
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        /// CreatedBy: HieuNM
        public async Task<ServiceResponse> Get(List<Guid> ids)
        {
            var result = new List<UserEntity>();

            try
            {
                result = (await _repository.FindAllAsync<UserEntity>())
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
        /// Update users
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        /// CreatedBy: HieuNM
        public async Task<ServiceResponse> Update(List<UserEntity> users)
        {
            try
            {
                foreach (var user in users)
                {
                    if (!string.IsNullOrWhiteSpace(user.PasswordHash))
                    {
                        // Băm mật khẩu nếu mật khẩu được cập nhật
                        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
                    }

                    await _repository.UpdateAsync(user);
                }

                await _repository.SaveChangeAsync();
                return Ok(users);
            }
            catch (Exception ce)
            {
                return Ok(users, message: ce.Message);
            }
        }

        /// <summary>
        /// Delete users
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        /// CreatedBy: HieuNM
        public async Task<ServiceResponse> Delete(List<Guid> ids)
        {
            try
            {
                foreach (var id in ids)
                {
                    await _repository.DeleteAsync<UserEntity>(id);
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
