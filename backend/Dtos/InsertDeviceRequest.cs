using MediHub.Web.Models;

namespace MediHub.Web.Dtos.Common
{
    public class SelectDeviceRequest
    {
        public DeviceEntity DeviceEntity { get; set; }
        public IEnumerable<MaintenanceRecordEntity> MaintenanceRecordEntity { get; set; }
    }

    public class UpdateDeviceRequest
    {
        public DeviceEntity DeviceEntity { get; set; }
        public List<MaintenanceRecordEntity> MaintenanceRecordEntity { get; set; }
    }
}
