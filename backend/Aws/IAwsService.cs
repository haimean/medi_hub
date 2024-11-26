using MediHub.Web.Application.Aws.Dtos;
using MediHub.Web.Aws.Dtos;
using MediHub.Web.HttpConfig;

namespace MediHub.Web.Aws
{
    public interface IAwsService
    {
        Task<ServiceResponse> GetPublicUrlByS3Key(GetS3SignedUrlRequest request);
        Task<ServiceResponse> UploadFileToS3(UploadFileRequest request);
        Task<ServiceResponse> UploadFilesToS3(UploadFilesRequest request);
        Task<byte[]> DownloadExcelTemplateFromS3(string bucketName);
        Task<byte[]> DownloadFileFromS3(string bucketName, string key);
        Task<byte[]> DownloadExcelFormTemplateFromS3(string bucketName);
        Task<ServiceResponse> GetJsonDataByS3Key(GetS3JsonDataRequest request);

    }
}