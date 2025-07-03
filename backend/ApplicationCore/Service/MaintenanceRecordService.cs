using MediHub.Web.ApplicationCore.Auth.CurrentUser;
using MediHub.Web.ApplicationCore.Interfaces;
using MediHub.Web.Aws;
using MediHub.Web.Data.Repository;
using MediHub.Web.DatabaseContext.AppDbcontext;
using MediHub.Web.DatabaseContext.DapperDbContext;
using MediHub.Web.HttpConfig;
using MediHub.Web.Models;

namespace MediHub.Web.ApplicationCore.Service
{
    public class MaintenanceRecordService : HttpConfig.Service, IMaintenanceRecordService
    {
        private readonly IRepository _repository;
        private readonly ICurrentUser _currentUser;
        private readonly MediHubDapperContext _mediHubContext;

        public MaintenanceRecordService(IRepository repository, ICurrentUser currentUser, MediHubDapperContext mediHubContext)
        {
            _repository = repository;
            _currentUser = currentUser;
            _mediHubContext = mediHubContext;
        }

        #region Common
        /// <summary>
        /// Create
        /// </summary>
        /// <param name="devices"></param>
        /// <returns></returns>
        /// CreatedBy: HieuNM
        public async Task<ServiceResponse> Create(List<MaintenanceRecordEntity> maintenanceRecords)
        {
            try
            {

                foreach (var maintenance in maintenanceRecords)
                {
                    maintenance.IsDeleted = false;
                    maintenance.CreatedBy = _currentUser.GetEmail();
                    maintenance.UpdatedBy = _currentUser.GetEmail();

                    await _repository.AddAsync(maintenance);
                    
                }

                await _repository.SaveChangeAsync();
            }
            catch (Exception ce)
            {
                return BadRequest(ce.Message);
            }

            return Ok(maintenanceRecords);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// CreatedBy: HieuNM
        public async Task<ServiceResponse> Get()
        {
            var reuslt = new List<MaintenanceRecordEntity>();

            try
            {
                reuslt = (await _repository.FindAllAsync<MaintenanceRecordEntity>()).Where(c => c.IsDeleted != true).ToList();
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
        /// CreatedBy: HieuNM
        public async Task<ServiceResponse> Get(Guid id)
        {
            var reuslt = new MaintenanceRecordEntity();

            try
            {
                reuslt = (await _repository.FindAsync<MaintenanceRecordEntity>(id));
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
        /// CreatedBy: HieuNM
        public async Task<ServiceResponse> Get(List<Guid> ids)
        {
            var result = new List<MaintenanceRecordEntity>();

            try
            {
                result = (await _repository.FindAllAsync<MaintenanceRecordEntity>())
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
        /// CreatedBy: HieuNM
        public async Task<ServiceResponse> Update(List<MaintenanceRecordEntity> maintenanceRecords)
        {
            var reuslt = new List<MaintenanceRecordEntity>();

            try
            {
                foreach (var maintenance in maintenanceRecords)
                {
                    await _repository.UpdateAsync(maintenance);
                }
            }
            catch (Exception ce)
            {
                return Ok(maintenanceRecords, message: ce.Message);
            }

            await _repository.SaveChangeAsync();
            return Ok(maintenanceRecords);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        /// CreatedBy: HieuNM
        public async Task<ServiceResponse> Delete(List<Guid> ids)
        {
            try
            {
                var result = new List<Guid>();

                foreach (var id in ids)
                {
                    var findCode = (await _repository.FindAsync<MaintenanceRecordEntity>(id));

                    if (findCode != null)
                    {
                        await _repository.DeleteAsync<MaintenanceRecordEntity>(id);
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
#endregion