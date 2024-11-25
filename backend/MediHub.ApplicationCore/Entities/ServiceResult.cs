using MediHub.ApplicationCore.Enums;
using System.Collections.Generic;

namespace MediHub.ApplicationCore.Entities
{
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
        public BaseEnum BaseCode { get; set; }

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
