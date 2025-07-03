using MediHub.Web.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediHub.Web.Models
{
    #region
    /// <summary>
    /// Maintenance record with date and file links
    /// </summary>
    public class MaintenanceRecordEntity : IBaseEntity, ISoftDelete
    {
        [Column("maintaind_date")]
        public DateTime MaintaindDate { get; set; } // Ngày bảo trì

        [Column("maintenance_date")]
        public string MaintenanceDate { get; set; } // Ngày bảo dưỡng

        [Column("file_links")]
        public string FileLinks { get; set; } // đường link dẫn đến các tệp đính kèm

        [Column("device_id")]
        public Guid DeviceID { get; set; } // mã thiết bị bảo trì

        [Column("type_of_maintenance")]
        public int TypeOfMaintenance { get; set; } // loại bảo trì (0: Biên bản bảo trì, 1: Nội kiểm tra bảo trì, 2: Nhật ký bảo dưỡng)
    }
    #endregion
}