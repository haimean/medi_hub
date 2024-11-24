using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.ApplicationCore.Enums
{
    /// <summary>
    /// MISA Code xách định mã trạng thái API trả về
    /// </summary>
    /// CreatedBy: PQ Huy (05.07.2021)
    public enum MISAEnum
    {
        /// <summary>
        /// Dữ liệu hợp lệ
        /// </summary>
        IsValid = 100,

        /// <summary>
        /// Dữ liệu chưa hợp lệ
        /// </summary>
        NotValid = 900,

        /// <summary>
        /// Dữ liệu thành công
        /// </summary>
        Success = 200
    }

    /// <summary>
    /// Xác định trạng thái của object
    /// </summary>
    public enum EntityState
    {
        AddNew = 1,
        Update = 2,
        Delete = 3
    }
}
