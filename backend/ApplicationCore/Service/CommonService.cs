using MediHub.Web.ApplicationCore.Interfaces;
using MediHub.Web.Data.Repository;
using MediHub.Web.HttpConfig;

namespace MediHub.Web.ApplicationCore.Service
{
    public class CommonService : HttpConfig.Service, ICommonService
    {
        private readonly IConfiguration _configuration;
        private readonly IRepository _repository;

        public CommonService(IConfiguration configuration, IRepository repository)
        {
            _configuration = configuration;
            _repository = repository;
        }

        /// <summary>
        /// Uploads documents to the server.
        /// </summary>
        /// <param name="files">List of files to upload.</param>
        /// <returns>Service response with uploaded file keys.</returns>
        public async Task<ServiceResponse> UploadDocs(List<IFormFile> files)
        {
            var response = new ServiceResponse();

            if (files == null || files.Count == 0)
            {
                return BadRequest(message: "No files uploaded.");
            }

            var uploadedFiles = new List<string>();

            foreach (var file in files)
            {
                // var key = Guid.NewGuid().ToString(); // Tạo key giống như S3
                var filePath = Path.Combine("Uploads", $"{file.FileName}");

                // {{ edit_1 }}: Check if the directory exists, if not, create it
                var directoryPath = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                uploadedFiles.Add(filePath); // Lưu key để trả về
            }

            return Ok(uploadedFiles);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public async Task<ServiceResponse> UploadDoc(string key, IFormFile file)
        {
            var response = new ServiceResponse();

            var uuid = Guid.NewGuid().ToString(); // Tạo key giống như S3
            // string finnalKey = $"{key}-{uuid}-{file.FileName}";
            var filePath = Path.Combine("Uploads", file.FileName);

            // {{ edit_1 }}: Check if the directory exists, if not, create it
            var directoryPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(file.FileName);
        }

        /// <summary>
        /// Retrieves documents from the server based on provided keys.
        /// </summary>
        /// <param name="keys">List of keys for the documents to retrieve.</param>
        /// <returns>Service response with document byte arrays.</returns>
        public async Task<ServiceResponse> GetDocs(List<string> keys)
        {
            var documents = new List<byte[]>();

            if (keys == null || keys.Count == 0)
            {
                return BadRequest(message: "No keys provided.");
            }

            foreach (var key in keys)
            {
                var filePath = Path.Combine("Uploads", key); // Đường dẫn đến tệp

                if (System.IO.File.Exists(filePath))
                {
                    var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                    documents.Add(fileBytes); // Thêm tệp vào danh sách
                }
                else
                {
                    return BadRequest(message: $"Document with key {key} not found.");
                }
            }

            return Ok(documents);
        }

        /// <summary>
        /// Retrieves documents from the server based on provided keys.
        /// </summary>
        /// <param name="keys">List of keys for the documents to retrieve.</param>
        /// <returns>Service response with document byte arrays.</returns>
        public async Task<ServiceResponse> GetDoc(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return BadRequest(message: "No keys provided.");
            }

            var filePath = Path.Combine("Uploads", key); // Đường dẫn đến tệp

            if (System.IO.File.Exists(filePath))
            {
                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                var fileName = Path.GetFileName(filePath);

                // Đọc file và trả về
                return OkFile(fileBytes, "application/octet-stream", fileName);
            }
            else
            {
                return BadRequest(message: $"Document with key {key} not found.");
            }
        }
    }
}