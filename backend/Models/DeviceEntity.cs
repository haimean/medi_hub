using MediHub.Web.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediHub.Web.Models
{
    [Table("devices")]
    public class DeviceEntity : IBaseEntity, ISoftDelete
    {
        [Column("device_code")]
        public string DeviceCode { get; set; } // Mã thiết bị

        [Column("device_name")]
        public string DeviceName { get; set; } // Tên thiết bị

        [Column("manufacturer_country")]
        public string ManufacturerCountry { get; set; } // Nước sản xuất

        [Column("manufacturer_name")]
        public string ManufacturerName { get; set; } // Tên hãng

        [Column("manufacturing_year")]
        public int ManufacturingYear { get; set; } // Năm sản xuất

        [Column("serial_number")]
        public string SerialNumber { get; set; } // Số seri

        [Column("function_name")]
        public string FunctionName { get; set; } // Tên chức năng

        [Column("installation_contract")]
        public string InstallationContract { get; set; } // Hợp đồng lắp đặt

        [Column("machine_status")]
        public string MachineStatus { get; set; } // Tình trạng máy

        [Column("import_source")]
        public string ImportSource { get; set; } // Nguồn nhập

        [Column("usage_date")]
        public DateTime UsageDate { get; set; } // Ngày sử dụng

        [Column("lab_usage")]
        public string LabUsage { get; set; } // Lab sử dụng

        [Column("manager_engineer_info")]
        public string ManagerEngineerInfo { get; set; } // Thông tin người quản lý và kỹ sư (dưới dạng JSON)

        [Column("maintenance_log")]
        public string MaintenanceLog { get; set; } // Nhật ký bảo dưỡng (danh sách ngày bảo dưỡng và các tệp pdf, ảnh)

        [Column("maintenance_report")]
        public string MaintenanceReport { get; set; } // Biên bản bảo trì

        [Column("internal_maintenance_check")]
        public string InternalMaintenanceCheck { get; set; } // Nội kiểm tra bảo trì

        [Column("maintenance_schedule")]
        public string MaintenanceSchedule { get; set; } // Lịch bảo dưỡng (tương tự nhật ký bảo dưỡng)

        [Column("notes")]
        public string Notes { get; set; } // Ghi chú

        [Column("is_deleted")]
        public bool IsDeleted { get; set; } // Trạng thái xóa
    }
}