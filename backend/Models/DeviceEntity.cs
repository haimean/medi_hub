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
        public int ManufacturerName { get; set; } // Tên hãng

        [Column("manufacturing_year")]
        public int ManufacturingYear { get; set; } // Năm sản xuất

        [Column("serial_number")]
        public string SerialNumber { get; set; } // Số seri

        [Column("machine_status")]
        public int MachineStatus { get; set; } // Tình trạng máy

        [Column("import_source")]
        public string ImportSource { get; set; } // Nguồn nhập

        [Column("function_name")]
        public string FunctionName { get; set; } // Tên chức năng

        [Column("installation_contract")]
        public string InstallationContract { get; set; } // Hợp đồng - pháp lý

        [Column("usage_date")]
        public DateTime UsageDate
        {
            get => _usageDate;
            set => _usageDate = value.ToUniversalTime();
        }
        private DateTime _usageDate; // Ngày sử dụng

        public DateTime? ExpirationDate
        {
            get => _expirationDate;
            set => _expirationDate = value?.ToUniversalTime();
        }
        private DateTime? _expirationDate; // Ngày hết hạn sử dụng

        [Column("lab_usage")]
        public string LabUsage { get; set; } // Lab sử dụng

        [Column("manager_info")]
        public string ManagerInfo { get; set; } // Thông tin người quản lý

        [Column("manager_phonenumber")]
        public string ManagerPhoneNumber { get; set; } // Sđt người quản lý

        [Column("engineer_info")]
        public string EngineerInfo { get; set; } // Thông tin kỹ sư 

        [Column("engineer_phonenumber")]
        public string EngineerPhoneNumber { get; set; } // Sđt kỹ sư

        [Column("device_usage_instructions")]
        public string DeviceUsageInstructions { get; set; } // HDSD Thiết bị

        [Column("appraisal_file")]
        public string AppraisalFile { get; set; } // hồ sơ thẩm định

        [Column("device_status")]
        public int DeviceStatus { get; set; } // tình trạng Sử dụng
        #endregion

        #region Lịch sử hoạt động - tình trạng
        [Column("maintenance_date")]
        public DateTime? MaintenanceDate
        {
            get => _maintenanceDate;
            set => _maintenanceDate = value?.ToUniversalTime();
        }
        private DateTime? _maintenanceDate; // Ngày bảo dưỡng 

        [Column("maintenance_next_date")]
        public DateTime? MaintenanceNextDate
        {
            get => _maintenanceNextDate;
            set => _maintenanceNextDate = value?.ToUniversalTime();
        }
        private DateTime? _maintenanceNextDate; // Ngày bảo dưỡng kế tiếp

        [Column("maintenance_schedule")]
        public int MaintenanceSchedule { get; set; } // lịch bảo dưỡng định kì

        [Column("calibration_date")]
        public DateTime? CalibrationDate
        {
            get => _calibrationDate;
            set => _calibrationDate = value?.ToUniversalTime();
        }
        private DateTime? _calibrationDate; // Ngày hiệu chuẩn 

        [Column("calibration_next_date")]
        public DateTime? CalibrationNextDate
        {
            get => _calibrationNextDate;
            set => _calibrationNextDate = value?.ToUniversalTime();
        }
        private DateTime? _calibrationNextDate; // Ngày hiệu chuẩn kế tiếp

        [Column("replace_date")]
        public DateTime? ReplaceDate
        {
            get => _replaceDate;
            set => _replaceDate = value?.ToUniversalTime();
        }
        private DateTime? _replaceDate; // Ngày thay thế thiết bị
        [Column("replace_next_date")]
        public DateTime? ReplaceNextDate
        {
            get => _replaceNextDate;
            set => _replaceNextDate = value?.ToUniversalTime();
        }
        private DateTime? _replaceNextDate; // Ngày thay thế thiết bị kế tiếp
        #endregion

        [Column("notes")]
        public string Notes { get; set; } // Ghi chú
    }
}