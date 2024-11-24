using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.ApplicationCore.Entities
{
    /// <summary>
    /// Thông tin nhân viên
    /// </summary>
    /// CreatedBy: PQ Huy (06.07.2021)
    public class Department : BaseEntity
    {
        /// <summary>
        /// Khóa chính
        /// </summary>
        [PrimaryKey]
        [DisplayName("Khóa chính đơn vị nhân viên")]
        public Guid DepartmentId { get; set; }

        /// <summary>
        /// Tên đơn vị
        /// </summary>
        [DisplayName("Tên đơn vị")]
        public string DepartmentName { get; set; }

        /// <summary>
        /// Mô tả bộ phận
        /// </summary>
        [DisplayName("Mô tả")]
        public string Description { get; set; }

        /// <summary>
        /// Người tạo
        /// </summary>
        [DisplayName("Người tạo")]
        #pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        public string CreatedBy { get; set; }
        #pragma warning restore CS0108 // Member hides inherited member; missing new keyword

        /// <summary>
        /// Ngày tạo
        /// </summary>
        [DisplayName("Ngày tạo")]
        #pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        public DateTime CreatedDate { get; set; }
        #pragma warning restore CS0108 // Member hides inherited member; missing new keyword

        /// <summary>
        ///   Người sửa
        /// </summary>
        [DisplayName("Người sửa")]
        public string ModifiedBy { get; set; }

        /// <summary>
        /// Ngày sửa
        /// </summary>
        [DisplayName("Ngày sửa")]
        public DateTime ModifiedDate { get; set; }

    }
}
