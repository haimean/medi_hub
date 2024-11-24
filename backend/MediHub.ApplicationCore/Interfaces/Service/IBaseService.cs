using Microsoft.AspNetCore.Http;
using MISA.ApplicationCore.Entities;
using System;
using System.Collections.Generic;
using System.Threading;

namespace MISA.ApplicationCore.Interfaces
{
    public interface IBaseService<Generic>
    {
        /// <summary>
        /// Lấy toàn bộ danh sách
        /// </summary>
        /// <returns> Trả về danh sách </returns>
        /// CreatedBy: PQ Huy (05.07.2021)
        IEnumerable<Generic> Get();

        /// <summary>
        /// Lấy thông tin theo mã
        /// </summary>
        /// <param name="id"> Mã bản ghi</param>
        /// <returns>Trả về bản ghi tương ứng</returns>
        /// CreatedBy: PQ Huy (05.07.2021)
        IEnumerable<Generic> GetById(Guid id);

        /// <summary>
        /// Thêm mới bản ghi
        /// </summary>
        /// <param name="data">Dữ liệu bản ghi</param>
        /// <returns>Trả về số bản ghi được thêm</returns>
        /// CreatedBy: PQ Huy (05.07.2021)
        ServiceResult Insert(Generic data);

        /// <summary>
        /// Sửa thông tin bản ghi
        /// </summary>
        /// <param name="id">Mã bản ghi</param>
        /// <param name="customer">Dữ liệu bản ghi cần cập nhật</param>
        /// <returns>Trả về trạng thái cập nhật dữ liệu</returns>
        /// CreatedBy: PQ Huy (05.07.2021)
        ServiceResult Update(Guid id, Generic data);

        /// <summary>
        /// Xóa thông tin theo khóa chính
        /// </summary>
        /// <param name="id">Mã bản ghi</param>
        /// <returns>Trả về thạng thái cập nhật danh sách bản ghi</returns>
        /// CreatedBy: PQ Huy (05.07.2021)
        ServiceResult DeleteById(Guid id);

        /// <summary>
        /// Xử lý dữ liệu import từ người dùng
        /// </summary>
        /// <param name="generics"> dữ liệu file</param>
        /// <returns>Trả về danh sách dữ liệu và kết quả sau khi xử lý</returns>
        /// CreatedBy: PQ Huy (05.07.2021)
        string ProcessDataImport(IFormFile formFile, CancellationToken cancellationToken);

        /// <summary>
        /// Nập khẩu dữ liệu
        /// </summary>
        /// <param name="CacheKey">key cache</param>
        /// <returns>Trả về số bản ghi nhập khẩu thành công</returns>
        /// CreatedBy: PQ Huy (05.07.2021)
        ServiceResult MutilpleInsert(string CacheKey);

    }
}
