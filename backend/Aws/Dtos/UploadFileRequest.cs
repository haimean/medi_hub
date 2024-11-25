using Microsoft.AspNetCore.Http;
using System;

namespace SafetyApi.Application.Aws.Dtos
{
    public class UploadFileRequest
    {
        public IFormFile File { get; set; }
        public string BucketName { get; set; }
        public string Key { get; set; }
    }

    public class UploadFilesRequest
    {
        public IFormCollection Files { get; set; }
        public string BucketName { get; set; }
    }

    public class FilesRequest
    {
        //public List<Guid> DoIds { get; set; }
        public Guid Id { get; set; }
        public IFormFileCollection Files { get; set; }
    }
}
