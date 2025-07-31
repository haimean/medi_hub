using Dapper;
using MediHub.Web.ApplicationCore.Auth.CurrentUser;
using MediHub.Web.ApplicationCore.Interfaces;
using MediHub.Web.Data.Repository;
using MediHub.Web.DatabaseContext.AppDbcontext;
using MediHub.Web.DatabaseContext.DapperDbContext;
using MediHub.Web.Dtos.Common;
using MediHub.Web.HttpConfig;
using MediHub.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharpCompress.Common;

namespace MediHub.Web.ApplicationCore.Service
{
    public class DevicesService : HttpConfig.Service, IDevicesService
    {
        private readonly IRepository _repository;
        private readonly ICurrentUser _currentUser;
        private readonly MediHubDapperContext _mediHubContext;
        private readonly MediHubContext _hubContext;

        public DevicesService(IRepository repository, ICurrentUser currentUser, MediHubDapperContext mediHubContext, MediHubContext hubContext)
        {
            _repository = repository;
            _currentUser = currentUser;
            _mediHubContext = mediHubContext;
            _hubContext = hubContext;
        }

        #region Common
        /// <summary>
        /// Create
        /// </summary>
        /// <param name="devices"></param>
        /// <returns></returns>
        /// CreatedBy: HieuNM
        public async Task<ServiceResponse> Create(UpdateDeviceRequest deviceRequest)
        {
            try
            {
                await _repository.AddAsync(deviceRequest.DeviceEntity);
                foreach (var maintenance in deviceRequest.MaintenanceRecordEntity)
                {
                    maintenance.DeviceID = deviceRequest.DeviceEntity.Id;
                    maintenance.CreatedBy = _currentUser.GetEmail();
                    maintenance.UpdatedBy = _currentUser.GetEmail();
                    await _repository.AddAsync(maintenance);

                    var uploadedFiles = new List<string>();
                    var filePath = string.Empty;
                    if (maintenance.MaintenanceDate != null)
                    {
                        filePath = Path.Combine("Uploads", maintenance.MaintaindDate.Value.Year.ToString()
                        , $"{maintenance.Name}" + Guid.NewGuid());
                    }
                    else
                    {
                        filePath = Path.Combine("Uploads", $"{maintenance.Name}" + Guid.NewGuid());
                    }

                    maintenance.FileLinks = filePath;
                    await _repository.AddAsync(maintenance);

                }

                await _repository.SaveChangeAsync();
            }
            catch (Exception ce)
            {
                return BadRequest(ce.Message);
            }

            return Ok(deviceRequest);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// CreatedBy: HieuNM
        public async Task<ServiceResponse> Get()
        {
            var result = new List<SelectDeviceRequest>();

            try
            {
                result = await _repository.GetQueryable<DeviceEntity>()
                    .Where(device => !device.IsDeleted)
                    .GroupJoin(
                        _repository.GetQueryable<MaintenanceRecordEntity>(),
                        device => device.Id,
                        maintenance => maintenance.DeviceID,
                        (device, maintenances) => new { device, maintenances }
                    )
                    .Select(x => new SelectDeviceRequest
                    {
                        DeviceEntity = x.device,
                        MaintenanceRecordEntity = x.maintenances,
                    }
                    )
                    .ToListAsync();
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
        /// <returns></returns>
        /// CreatedBy: HieuNM
        public async Task<ServiceResponse> Get(Guid id)
        {
            var result = new SelectDeviceRequest();

            try
            {
                result = await _repository.GetQueryable<DeviceEntity>()
                    .Where(c => !c.IsDeleted && (id == c.Id))
                    .GroupJoin(
                        _repository.GetQueryable<MaintenanceRecordEntity>(),
                        device => device.Id,
                        maintenance => maintenance.DeviceID,
                        (device, maintenances) => new { device, maintenances }
                    )
                    .Select(x => new SelectDeviceRequest
                    {
                        DeviceEntity = x.device,
                        MaintenanceRecordEntity = x.maintenances,
                    }
                    )
                    .FirstOrDefaultAsync();
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
        /// <param name="id"></param>
        /// <returns></returns>
        /// CreatedBy: HieuNM
        public async Task<ServiceResponse> Get(List<Guid> ids)
        {
            var result = new List<SelectDeviceRequest>();

            try
            {
                result = await _repository.GetQueryable<DeviceEntity>()
                    .Where(c => !c.IsDeleted && ids.Contains(c.Id))
                    .GroupJoin(
                        _repository.GetQueryable<MaintenanceRecordEntity>(),
                        device => device.Id,
                        maintenance => maintenance.DeviceID,
                        (device, maintenances) => new { device, maintenances }
                    )
                    .Select(x => new SelectDeviceRequest
                    {
                        DeviceEntity = x.device,
                        MaintenanceRecordEntity = x.maintenances,
                    }
                    )
                    .ToListAsync();
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
        public async Task<ServiceResponse> Update(UpdateDeviceRequest devicesRequest)
        {
            try
            {
                await _repository.UpdateAsync(devicesRequest.DeviceEntity);

                IEnumerable<MaintenanceRecordEntity> data = await _repository
                    .FindAllAsync<MaintenanceRecordEntity>(record => record.DeviceID == devicesRequest.DeviceEntity.Id);

                var inputIds = new HashSet<Guid>();

                foreach (var item in devicesRequest.MaintenanceRecordEntity)
                {
                    if (item.Id != Guid.Empty) { inputIds.Add(item.Id); }
                    else
                    {
                        var uploadedFiles = new List<string>();
                        await _repository.AddAsync(item);
                    }
                }

                foreach (MaintenanceRecordEntity maintenance in data)
                {
                    if (!inputIds.Contains(maintenance.Id))
                    {
                        if (File.Exists(maintenance.FileLinks))
                        {
                            File.Delete(maintenance.FileLinks);
                        }
                        await _repository.DeleteAsync(maintenance);
                    }
                }
            }
            catch (Exception ce)
            {
                return Ok(devicesRequest, message: ce.Message);
            }

            await _repository.SaveChangeAsync();
            return Ok(devicesRequest);
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
                    var findCode = (await _repository.FindAsync<DeviceEntity>(id));

                    if (findCode != null)
                    {
                        await _repository.DeleteAsync<DeviceEntity>(id);
                        result.Add(id);

                        var findMaintainCode = await _repository.FindAllAsync<MaintenanceRecordEntity>(record => record.DeviceID == id);
                        foreach (var item in findMaintainCode)
                        {
                            if (File.Exists(item.FileLinks))
                            {
                                File.Delete(item.FileLinks);
                            }
                            await _repository.DeleteAsync<MaintenanceRecordEntity>(item.Id);
                        }
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
        #endregion

        #region Analytics
        /// <summary>
        /// Create
        /// </summary>
        /// <param name="devices"></param>
        /// <returns></returns>
        /// CreatedBy: PQ Huy (28.11.2024)
        public async Task<ServiceResponse> GetManufacturerBranch()
        {
            string query = "SELECT DISTINCT MANUFACTURER_NAME AS NAME FROM DEVICES WHERE MANUFACTURER_NAME IS NOT NULL AND IS_DELETED IS NOT TRUE; SELECT DISTINCT FUNCTION_NAME AS NAME FROM DEVICES WHERE FUNCTION_NAME IS NOT NULL AND IS_DELETED IS NOT TRUE;";
            var result = new Dictionary<string, List<string>>()
            {
                {"manufacturer", new List<string>() },
                {"function", new List<string>() },
            };

            try
            {
                using (var connect = _mediHubContext.CreateConnection())
                {
                    connect.Open();

                    using (var reQuery = await connect.QueryMultipleAsync(query))
                    {
                        result["manufacturer"] = (await reQuery.ReadAsync<string>()).ToList();
                        result["function"] = (await reQuery.ReadAsync<string>()).ToList();
                    }

                    connect.Close();
                    connect.Dispose();
                }
            }
            catch (Exception ce)
            {
                return BadRequest(ce.Message);
            }

            return Ok(result);
        }

        public async Task<ServiceResponse> GetDeviceByManufacturerName(int manufacturerName)
        {
            var devicesEntities = new List<DeviceEntity>();

            try
            {
                devicesEntities = await _hubContext.DeviceEntity.Where(record => record.ManufacturerName == manufacturerName).ToListAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok(devicesEntities);
        }

        public async Task<ServiceResponse> GetDeviceByCalibrationNextDate()
        {
            var devicesEntities = new List<DeviceEntity>();
            try
            {
                devicesEntities = await _hubContext.DeviceEntity
                    .Where(record => (record.CalibrationNextDate != null && (record.CalibrationNextDate.Value - DateTime.Now).TotalDays < 15))
                    .ToListAsync();
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
            return Ok(devicesEntities);
        }
        public async Task<ServiceResponse> GetDeviceByMaintenanceNextDate()
        {
            var devicesEntities = new List<DeviceEntity>();
            try
            {
                devicesEntities = await _hubContext.DeviceEntity
                    .Where(record => (record.MaintenanceNextDate != null && (record.MaintenanceNextDate.Value - DateTime.Now).TotalDays < 15))
                    .ToListAsync();
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
            return Ok(devicesEntities);
        }
        #endregion
    }
}
