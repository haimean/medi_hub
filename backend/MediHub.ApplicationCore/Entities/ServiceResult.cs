using MISA.ApplicationCore.Enums;
using System.Collections.Generic;

namespace MISA.ApplicationCore.Entities
{
    /// <summary>
    /// Class lưu trữ dữ liệu cho các kết quản trả về của api
    /// </summary>
    /// CreatedBy: PQ Huy (05.07.2021)
    public class ServiceResult
    {
        /// <summary>
        /// Object chứa dữ liệu data
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// Message thông báo trạng thái
        /// </summary>
        public string Messenger { get; set; }

        /// <summary>
        /// Define MISA code
        /// </summary>
        public MISAEnum MISACode { get; set; }

        /// <summary>
        /// Message thông báo trạng thái
        /// </summary>
        public List<string> ImportMsg { get; set; }

        /// <summary>
        /// Tổng bản ghi
        /// </summary>
        public int? Total { get; set; }
    }
}
