using System;
using System.ComponentModel;

namespace MISA.ApplicationCore.Entities
{
    /// <summary>
    /// Thông tin nhân viên
    /// </summary>
    /// CreatedBy: PQ Huy (05.07.2021)
    public class Employee : BaseEntity
    {
        /// <summary>
        /// Khóa chính nhân viên
        /// </summary>
        [PrimaryKey]
        [DisplayName("Khóa chính nhân viên")]
        public Guid EmployeeId { get; set; }

        /// <summary>
        /// Mã nhân viên
        /// </summary>
        [CheckDuplicate]
        [Required]
        [DisplayName("Mã nhân viên")]
        public string EmployeeCode { get; set; }

        /// <summary>
        /// Họ và tên nhân viên
        /// </summary>
        [DisplayName("Họ và tên")]
        public string EmployeeName { get; set; }

        /// <summary>
        /// Ngày sinh nhân viên
        /// </summary>
        [DisplayName("Ngày sinh")]
        public DateTime? DateOfBirth { get; set; }

        /// <summary>
        /// Giới tính
        /// </summary>
        [DisplayName("Giới tính")]
        public int? Gender { get; set; }

        /// <summary>
        /// Mã phòng ban
        /// </summary>
        [DisplayName("Mã phòng ban")]
        public Guid DepartmentId { get; set; }

        /// <summary>
        /// Tên phòng ban
        /// </summary>
        [DisplayName("Tên phòng ban")]
        public string DepartmentName { get; set; }

        /// <summary>
        /// Số chứng minh nhân dân của nhân viên
        /// </summary>
        [DisplayName("Mã chứng minh nhân dân")]
        public string IdentityNumber { get; set; }

        /// <summary>
        /// Ngày cấp minh nhân dân của nhân viên
        /// </summary>
        [DisplayName("Ngày cấp chứng minh nhân dân")]
        public DateTime? IdentityDate { get; set; }

        /// <summary>
        /// Nơi cấp minh nhân dân của nhân viên
        /// </summary>
        [DisplayName("Nơi cấp chứng minh nhân dân")]
        public string IdentityPlace { get; set; }

        /// <summary>
        /// Vị trí nhân viên
        /// </summary>
        [DisplayName("Vị trí nhân viên")]
        public string EmployeePosition { get; set; }

        /// <summary>
        /// Địa chỉ nhân viên
        /// </summary>
        [DisplayName("Địa chỉ")]
        public string Address { get; set; }

        /// <summary>
        /// Mã tài khoản ngân hàng nhân viên
        /// </summary>
        [DisplayName("Số tài khoản ngân hàng")]
        public string BankAccountNumber { get; set; }

        /// <summary>
        /// Tên ngân hàng nhân viên
        /// </summary>
        [DisplayName("Tên ngân hàng")]
        public string BankName { get; set; }

        /// <summary>
        /// Tên tỉnh ngân hàng hàng nhân viên
        /// </summary>
        [DisplayName("Tên chi nhánh ngân hàng")]
        public string BankBranchName { get; set; }

        /// <summary>
        /// Tên tỉnh ngân hàng
        /// </summary>
        public string BankProvinceName { get; set; }

        /// <summary>
        ///Số điện thoại
        /// </summary>
        [DisplayName("Số điện thoại di động")]
        public string PhoneNumber { get; set; }

        /// <summary>
        ///Số điện thoại
        /// </summary>
        [DisplayName("Số điện thoại cố định")]
        public string TelephoneNumber { get; set; }

        /// <summary>
        /// Địa chỉ email
        /// </summary>
        [DisplayName("Email")]
        public string Email { get; set; }

        /// <summary>
        /// Tạo bởi
        /// </summary>
        [DisplayName("Người tạo")]
        #pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        public string CreatedBy { get; set; }
        #pragma warning restore CS0108 // Member hides inherited member; missing new keyword

        /// <summary>
        /// Ngày tạo nhân viên
        /// </summary>
        [DisplayName("Ngày tạo")]
        #pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        public DateTime CreatedDate { get; set; }
        #pragma warning restore CS0108 // Member hides inherited member; missing new keyword

        /// <summary>
        /// Thay đổi bởi
        /// </summary>
        [DisplayName("Người sửa")]
        public string ModifiedBy { get; set; }

        /// <summary>
        /// Ngày thay đổi
        /// </summary>
        [DisplayName("Ngày sửa")]
        public DateTime? ModifiedDate { get; set; }
    }
}
