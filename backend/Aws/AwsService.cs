
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using DashboardApi.HttpConfig;
using Microsoft.Extensions.Options;
using QAQCApi.Aws.Dtos;
using QAQCApi.Data.Repository;
using SafetyApi.Application.Aws.Dtos;
using System.Net;


namespace QAQCApi.Aws
{
    public class AwsService : Service, IAwsService
    {
        private readonly ILogger<AwsService> _logger;
        private readonly IRepository _repository;
        private readonly IOptions<AwsOptions> _options;
        private string _awsAccessKey;
        private string _awsSecretAccessKey;

        public AwsService(IRepository repository, ILogger<AwsService> logger, IOptions<AwsOptions> options)
        {
            _repository = repository;
            _logger = logger;
            _options = options;

            _awsAccessKey = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AWS_ACCESS_ID")) ? Environment.GetEnvironmentVariable("AWS_ACCESS_ID") : _options.Value.Key;
            _awsSecretAccessKey = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AWS_ACCESS_KEY")) ? Environment.GetEnvironmentVariable("AWS_ACCESS_KEY") : _options.Value.Secret;
        }

        public async Task<ServiceResponse> GetPublicUrlByS3Key(GetS3SignedUrlRequest request)
        {
            //var 
            var s3Client = new AmazonS3Client(_awsAccessKey, _awsSecretAccessKey, Amazon.RegionEndpoint.APSoutheast1);
            GetPreSignedUrlRequest s3Request = new GetPreSignedUrlRequest
            {
                BucketName = _options.Value.BucketInvoice,
                Key = request.Key,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddHours(request.Hours)
            };

            var rs = s3Client.GetPreSignedURL(s3Request);

            return Ok(rs);
        }

        public async Task<ServiceResponse> UploadFileToS3(UploadFileRequest request)
        {
            var file = request.File;
            using (var client = new AmazonS3Client(_awsAccessKey, _awsSecretAccessKey, Amazon.RegionEndpoint.APSoutheast1))
            {
                using (var newMemoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(newMemoryStream);

                    var uploadRequest = new TransferUtilityUploadRequest
                    {
                        InputStream = newMemoryStream,
                        Key = request.Key,
                        BucketName = request.BucketName,
                    };

                    var fileTransferUtility = new TransferUtility(client);
                    await fileTransferUtility.UploadAsync(uploadRequest);
                }
            }

            return Ok(file.FileName);
        }

        public async Task<ServiceResponse> UploadFilesToS3(UploadFilesRequest request)
        {

            // var dos = await _repository.FindAllAsync<DoEntity>();
            var fileList = new Dictionary<Guid, string>();
            //using (var client = new AmazonS3Client(_options.Value.AWS_KEY, _options.Value.AWS_SECRET, Amazon.RegionEndpoint.APSoutheast1))
            //{
            //    foreach (var file in request.Files.Files)
            //    {
            //        var doFileName = Path.GetFileNameWithoutExtension(file.FileName);


            //        using (var newMemoryStream = new MemoryStream())
            //        {
            //            await file.CopyToAsync(newMemoryStream);

            //            var uploadRequest = new TransferUtilityUploadRequest
            //            {
            //                InputStream = newMemoryStream,
            //                Key = file.FileName.ToLower(),
            //                BucketName = request.BucketName,
            //            };

            //            var fileTransferUtility = new TransferUtility(client);
            //            await fileTransferUtility.UploadAsync(uploadRequest);
            //        }

            //        fileList.Add(doData.Id, file.FileName);
            //    }
            //}


            return Ok(fileList);


        }

        public async Task<byte[]> DownloadExcelTemplateFromS3(string bucketName)
        {
            MemoryStream ms = null;

            GetObjectRequest getObjectRequest = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = "excel_template.xlsx"
            };

            using (var client = new AmazonS3Client(_awsAccessKey, _awsSecretAccessKey, Amazon.RegionEndpoint.APSoutheast1))
            {
                using (var response = await client.GetObjectAsync(getObjectRequest))

                {
                    if (response.HttpStatusCode == HttpStatusCode.OK)
                    {
                        using (ms = new MemoryStream())
                        {
                            await response.ResponseStream.CopyToAsync(ms);
                        }
                    }
                }
            }

            return ms.ToArray();
        }

        public async Task<byte[]> DownloadFileFromS3(string bucketName, string key)
        {
            MemoryStream ms = null;

            GetObjectRequest getObjectRequest = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            using (var client = new AmazonS3Client(_awsAccessKey, _awsSecretAccessKey, Amazon.RegionEndpoint.APSoutheast1))
            {
                using (var response = await client.GetObjectAsync(getObjectRequest))

                {
                    if (response.HttpStatusCode == HttpStatusCode.OK)
                    {
                        using (ms = new MemoryStream())
                        {
                            await response.ResponseStream.CopyToAsync(ms);
                        }
                    }
                }
            }

            return ms.ToArray();
        }

        public async Task<byte[]> DownloadExcelFormTemplateFromS3(string bucketName)
        {
            MemoryStream ms = null;

            GetObjectRequest getObjectRequest = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = "excel_form_template.xlsx"
            };

            using (var client = new AmazonS3Client(_awsAccessKey, _awsSecretAccessKey, Amazon.RegionEndpoint.APSoutheast1))
            {
                using (var response = await client.GetObjectAsync(getObjectRequest))
                {
                    if (response.HttpStatusCode == HttpStatusCode.OK)
                    {
                        using (ms = new MemoryStream())
                        {
                            await response.ResponseStream.CopyToAsync(ms);
                        }
                    }
                }
            }

            return ms.ToArray();
        }

        public async Task<ServiceResponse> GetJsonDataByS3Key(GetS3JsonDataRequest request)
        {
            var s3Client = new AmazonS3Client(_awsAccessKey, _awsSecretAccessKey, Amazon.RegionEndpoint.APSoutheast1);
            GetObjectRequest getObjectRequest = new GetObjectRequest();
            getObjectRequest.BucketName = "store-mapped-textract-output";
            getObjectRequest.Key = request.Key;
            GetObjectResponse response = await s3Client.GetObjectAsync(getObjectRequest);
            StreamReader reader = new StreamReader(response.ResponseStream);
            string content = reader.ReadToEnd();
            //dynamic json = JsonConvert.DeserializeObject(content);
            reader.Close();

            return Ok(content);
        }
    }
}