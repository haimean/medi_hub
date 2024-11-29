using MediHub.Web.ApplicationCore.Interfaces;
using MediHub.Web.Auth.CurrentUser;
using MediHub.Web.Auth.PermisionChecker;
using MediHub.Web.Data.Repository;
using MediHub.Web.HttpConfig;
using MediHub.Web.Models;

namespace MediHub.Web.ApplicationCore.Service
{
    public class UsersService : HttpConfig.Service, IUsersService
    {
        private readonly IRepository _repository;
        private readonly IPermissionChecker _permissionChecker;
        private readonly ICurrentUser _currentUser;

        public UsersService(IRepository repository, IPermissionChecker permissionChecker, ICurrentUser currentUser)
        {
            _repository = repository;
            _permissionChecker = permissionChecker;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (28.11.2024)
        public async Task<ServiceResponse> Create(List<UserEntity> users)
        {
            try
            {
                foreach (var user in users)
                {
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
        /// 
        /// </summary>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (28.11.2024)
        public async Task<ServiceResponse> Get()
        {
            var reuslt = new List<UserEntity>();

            try
            {
                reuslt = (await _repository.FindAllAsync<UserEntity>()).Where(c => c.IsDeleted != true).ToList();
            }
            catch (Exception ce)
            {
                return BadRequest(ce.Message);
            }

            return Ok(reuslt);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (28.11.2024)
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
        /// 
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (28.11.2024)
        public async Task<ServiceResponse> Update(List<UserEntity> users)
        {
            var reuslt = new List<UserEntity>();

            try
            {
                foreach (var user in users)
                {
                    await _repository.UpdateAsync(user);
                }
            }
            catch (Exception ce)
            {
                return Ok(users, message: ce.Message);
            }

            await _repository.SaveChangeAsync();
            return Ok(users);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (28.11.2024)
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
