using MISA.ApplicationCore.Enums;
using System;
using System.Collections.Generic;

namespace MISA.ApplicationCore.Entities
{
    /// <summary>
    /// Kiểm tra các bản ghi cần required
    /// </summary>
    /// CreatedBy: PQ Huy (05.07.2021)
    [AttributeUsage(AttributeTargets.Property)]
    public class Required : Attribute
    {

    }

    /// <summary>
    /// Kiểm tra các bản ghi cần check trùng dữ liệu
    /// </summary>
    /// CreatedBy: PQ Huy (05.07.2021)
    [AttributeUsage(AttributeTargets.Property)]
    public class CheckDuplicate : Attribute
    {

    }

    /// <summary>
    /// Kiểm tra các bản ghi có khóa chính
    /// </summary>
    /// CreatedBy: PQ Huy (05.07.2021)
    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKey : Attribute
    {

    }

    /// <summary>
    /// Thêm các dữ liệu cho nghiệp vụ
    /// </summary>
    /// CreatedBy: PQ Huy (05.07.2021)
    public class BaseEntity
    {
        public EntityState EntityState { get; set; } = EntityState.AddNew;
        
        public DateTime? CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? ModifedDate { get; set; }

        public string ModifedBy { get; set; }

        public List<string> MsgImport { get; set; }

        public ServiceResult ImportResult { get; set; }

    }
}
