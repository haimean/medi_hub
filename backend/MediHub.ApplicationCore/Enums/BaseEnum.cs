namespace MediHub.ApplicationCore.Enums
{
    public enum BaseEnum
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
