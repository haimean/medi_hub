using MediHub.Web.HttpConfig;

namespace MediHub.Web.ApplicationCore.Interfaces
{
    public interface ICommonService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        Task<ServiceResponse> UploadDocs(Dictionary<string, IFormFile> files);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        Task<ServiceResponse> GetDocs(List<string> keys);
    }
}
