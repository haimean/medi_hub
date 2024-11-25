using MediHub.ApplicationCore.Entities;
using System;
using System.Collections.Generic;

namespace MediHub.ApplicationCore.Interfaces.Service
{
    public interface IBaseService<Generic>
    {
        /// <summary>
        /// Lấy toàn bộ danh sách
        /// </summary>
        /// <returns> Trả về danh sách </returns>
        /// CreatedBy: PQ Huy (25.11.2024)
        IEnumerable<Generic> Get();

        /// <summary>
        /// Lấy thông tin theo mã
        /// </summary>
        /// <param name="id"> Mã bản ghi</param>
        /// <returns>Trả về bản ghi tương ứng</returns>
        /// CreatedBy: PQ Huy (25.11.2024)
        IEnumerable<Generic> GetById(Guid id);

        /// <summary>
        /// Thêm mới bản ghi
        /// </summary>
        /// <param name="data">Dữ liệu bản ghi</param>
        /// <returns>Trả về số bản ghi được thêm</returns>
        /// CreatedBy: PQ Huy (25.11.2024)
        ServiceResult Insert(Generic data);

        /// <summary>
        /// Sửa thông tin bản ghi
        /// </summary>
        /// <param name="id">Mã bản ghi</param>
        /// <param name="customer">Dữ liệu bản ghi cần cập nhật</param>
        /// <returns>Trả về trạng thái cập nhật dữ liệu</returns>
        /// CreatedBy: PQ Huy (25.11.2024)
        ServiceResult Update(Guid id, Generic data);

        /// <summary>
        /// Xóa thông tin theo khóa chính
        /// </summary>
        /// <param name="id">Mã bản ghi</param>
        /// <returns>Trả về thạng thái cập nhật danh sách bản ghi</returns>
        /// CreatedBy: PQ Huy (25.11.2024)
        ServiceResult DeleteById(Guid id);

        /// <summary>
        /// Nập khẩu dữ liệu
        /// </summary>
        /// <param name="CacheKey">key cache</param>
        /// <returns>Trả về số bản ghi nhập khẩu thành công</returns>
        /// CreatedBy: PQ Huy (25.11.2024)
        ServiceResult MutilpleInsert(string CacheKey);
    }
}
