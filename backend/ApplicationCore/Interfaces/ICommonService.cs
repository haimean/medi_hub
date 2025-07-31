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
        Task<ServiceResponse> UploadDocs(string urlTemp, List<IFormFile> files);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        Task<ServiceResponse> UploadDoc(string key, string urlTemp, IFormFile files);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        Task<ServiceResponse> GetDocs(List<string> keys);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        Task<ServiceResponse> GetDoc(string keys);
    }
}
