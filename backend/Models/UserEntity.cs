using MediHub.Web.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediHub.Web.Models
{
    [Table("users")]
    public class UserEntity : IBaseEntity, ISoftDelete
    {
        #region Thông tin chung
        [Column("username")]
        public string Username { get; set; } // Tên đăng nhập

        [Column("password_hash")]
        public string PasswordHash { get; set; } // Mật khẩu (đã mã hóa)

        [Column("email")]
        public string Email { get; set; } // Email liên lạc

        [Column("phone_number")]
        public string PhoneNumber { get; set; } // Số điện thoại

        [Column("full_name")]
        public string FullName { get; set; } // Họ tên đầy đủ

        [Column("profile_picture")]
        public string ProfilePicture { get; set; } // Ảnh đại diện

        [Column("role")]
        public List<string> Role { get; set; } // Vai trò người dùng (Admin, User, Manager, v.v.)
        #endregion

        #region Trạng thái hoạt động
        [Column("is_active")]
        public bool IsActive { get; set; } // Trạng thái kích hoạt tài khoản

        [Column("last_login")]
        public DateTime? LastLogin { get; set; } // Lần đăng nhập gần nhất

        [Column("last_logout")]
        public DateTime? LastLogout { get; set; } // Lần đăng xuất gần nhất

        [Column("token_expiration")]
        public DateTime? TokenExpiration { get; set; } // Thời gian hết hạn của token

        [Column("is_token_valid")]
        public bool IsTokenValid { get; set; } // Trạng thái hợp lệ của token
        #endregion

        #region Phân quyền phòng ban

        [Column("department_ids")]
        public List<Guid> DepartmentIds { get; set; } // Danh sách ID phòng ban mà người dùng có quyền truy cập

        #endregion

        #region Metadata
        [Column("is_deleted")]
        public bool IsDeleted { get; set; } // Trạng thái xóa
        #endregion
    }
}