using MediHub.Web.Models.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediHub.Web.Models
{
    [Table("departments")]
    public class DepartmentEntity : IBaseEntity, ISoftDelete
    {
        #region Thông tin chung
        [Column("name")]
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } // Tên phòng ban

        [Column("description")]
        public string Description { get; set; } // Mô tả phòng ban
        #endregion


        #region Metadata
        [Column("is_deleted")]
        public bool IsDeleted { get; set; } // Trạng thái xóa
        #endregion
    }
}