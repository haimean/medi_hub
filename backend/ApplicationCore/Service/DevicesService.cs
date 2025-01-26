using MediHub.Web.ApplicationCore.Auth.CurrentUser;
using MediHub.Web.ApplicationCore.Interfaces;
using MediHub.Web.Data.Repository;
using MediHub.Web.HttpConfig;
using MediHub.Web.Models;

namespace MediHub.Web.ApplicationCore.Service
{
    public class DevicesService : HttpConfig.Service, IDevicesService
    {
        private readonly IRepository _repository;
        private readonly ICurrentUser _currentUser;

        public DevicesService(IRepository repository, ICurrentUser currentUser)
        {
            _repository = repository;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="devices"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (28.11.2024)
        public async Task<ServiceResponse> Create(List<DeviceEntity> devices)
        {
            try
            {
                foreach (var device in devices)
                {
                    device.IsDeleted = false;
                    device.CreatedBy = _currentUser.GetEmail();
                    device.UpdatedBy = _currentUser.GetEmail();

                    await _repository.AddAsync(device);
                }

                await _repository.SaveChangeAsync();
            }
            catch (Exception ce)
            {
                return BadRequest(ce.Message);
            }

            return Ok(devices);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (28.11.2024)
        public async Task<ServiceResponse> Get()
        {
            var reuslt = new List<DeviceEntity>();

            try
            {
                reuslt = (await _repository.FindAllAsync<DeviceEntity>()).Where(c => c.IsDeleted != true).ToList();
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
        /// <returns></returns>
        /// CreatedBy: PQ Huy (28.11.2024)
        public async Task<ServiceResponse> Get(Guid id)
        {
            var reuslt = new DeviceEntity();

            try
            {
                reuslt = (await _repository.FindAsync<DeviceEntity>(id));
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
            var result = new List<DeviceEntity>();

            try
            {
                result = (await _repository.FindAllAsync<DeviceEntity>())
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
        /// <param name="devices"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (28.11.2024)
        public async Task<ServiceResponse> Update(List<DeviceEntity> devices)
        {
            var reuslt = new List<DeviceEntity>();

            try
            {
                foreach (var device in devices)
                {
                    await _repository.UpdateAsync(device);
                }
            }
            catch (Exception ce)
            {
                return Ok(devices, message: ce.Message);
            }

            await _repository.SaveChangeAsync();
            return Ok(devices);
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
                var result = new List<Guid>();

                foreach (var id in ids)
                {
                    var findCode = (await _repository.FindAsync<DeviceEntity>(id));

                    if (findCode != null)
                    {
                        await _repository.DeleteAsync<DeviceEntity>(id);
                        result.Add(id);
                    }
                }

                await _repository.SaveChangeAsync();

                return Ok(result);
            }
            catch (Exception ce)
            {
                return BadRequest(ce.Message);
            }
        }
    }
}
