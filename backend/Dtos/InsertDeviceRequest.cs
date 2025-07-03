using MediHub.Web.Models;

namespace MediHub.Web.Dtos.Common
{
    public class InsertDeviceRequest
    {
        public List<DeviceEntity> DeviceEntity { get; set; }
        public List<MaintenanceRecordEntity> MaintenanceRecordEntity { get; set; }

    }
}
