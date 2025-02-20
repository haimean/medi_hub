using MediHub.Web.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediHub.Web.Models
{
    [Table("devices")]
    public class DeviceEntity : IBaseEntity, ISoftDelete
    {
        #region Thông tin chung
        [Column("device_avatar")]
        public List<string> DeviceAvatar { get; set; } // Ảnh đại diện thiết bị

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
        public List<string> InstallationContract { get; set; } // Hợp đồng lắp đặt

        [Column("contract_duration")]
        public DateTime? ContractDuration
        {
            get => _contractDuration;
            set => _contractDuration = value?.ToUniversalTime();
        }
        private DateTime? _contractDuration; // Thời hạn hợp đồng

        [Column("machine_status")]
        public string MachineStatus { get; set; } // Tình trạng máy

        [Column("import_source")]
        public string ImportSource { get; set; } // Nguồn nhập

        [Column("usage_date")]
        public DateTime UsageDate
        {
            get => _usageDate;
            set => _usageDate = value.ToUniversalTime();
        }
        private DateTime _usageDate; // Ngày sử dụng

        [Column("lab_usage")]
        public string LabUsage { get; set; } // Lab sử dụng

        [Column("manager_info", TypeName = "jsonb")]
        public ManagerEngineerInfo? ManagerInfo { get; set; } // Thông tin người quản lý và kỹ sư (dưới dạng JSON)

        [Column("engineer_info", TypeName = "jsonb")]
        public ManagerEngineerInfo? EngineerInfo { get; set; } // Thông tin người quản lý và kỹ sư (dưới dạng JSON)

        [Column("device_usage_instructions")]
        public string DeviceUsageInstructions { get; set; } // HDSD Thiết bị

        [Column("device_troubleshooting_instructions")]
        public string DeviceTroubleshootingInstructions { get; set; } // HD sử lý sự cố thiết bị
        #endregion

        #region Lịch sử hoạt động - tình trạng
        [Column("maintenance_log", TypeName = "jsonb")]
        public List<MaintenanceRecord>? MaintenanceLog { get; set; } // Nhật ký bảo dưỡng (danh sách ngày bảo dưỡng và các tệp pdf, ảnh)

        [Column("maintenance_report", TypeName = "jsonb")]
        public List<MaintenanceRecord>? MaintenanceReport { get; set; } // Biên bản bảo trì

        [Column("internal_maintenance_check", TypeName = "jsonb")]
        public List<MaintenanceRecord>? InternalMaintenanceCheck { get; set; } // Nội kiểm tra bảo trì

        [Column("maintenance_schedule")]
        public DateTime? MaintenanceSchedule
        {
            get => _maintenanceSchedule;
            set => _maintenanceSchedule = value?.ToUniversalTime();
        }
        private DateTime? _maintenanceSchedule; // Lịch bảo dưỡng (tương tự nhật ký bảo dưỡng)

        #endregion

        [Column("notes")]
        public string Notes { get; set; } // Ghi chú

        [Column("is_deleted")]
        public bool IsDeleted { get; set; } // Trạng thái xóa

        #region Phân quyền phòng ban

        [Column("department_ids")]
        public List<Guid> DepartmentIds { get; set; } // Danh sách ID phòng ban mà người dùng có quyền truy cập

        #endregion
    }

    #region Thông tin chi tiết kỹ sư, lịch sử hoạt động
    /// <summary>
    /// Manager, engineer
    /// </summary>
    public class ManagerEngineerInfo
    {
        public string FullName { get; set; } // Họ tên
        public DateTime DateOfBirth { get; set; } // Ngày tháng năm sinh
        public string PhoneNumber { get; set; } // Số điện thoại
        public string Address { get; set; } // Địa chỉ
    }

    /// <summary>
    /// Maintenance record with date and file links
    /// </summary>
    public class MaintenanceRecord
    {
        public string MaintenanceDate { get; set; } // Ngày bảo dưỡng
        public List<string> FileLinks { get; set; } // Danh sách đường link dẫn đến các tệp đính kèm
    }
    #endregion
}