using Aliyun.OSS;
using Aliyun.OSS.Common;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

namespace EventManagement.Utilities.Storage.AlibabaCloud
{
    public class ObjectStorageServices : IStorageServices
    {
        private readonly IConfiguration _configuration;

        private readonly OssClient _ossClient;

        public ObjectStorageServices(IConfiguration configuration)
        {
            _configuration = configuration;
            var accessKeyId = _configuration["AlibabaCloud:AccessKeyId"];
            var accessKeySecret = _configuration["AlibabaCloud:AccessKeySecret"];
            var endpoint = _configuration["AlibabaCloud:Endpoint"];
            _ossClient = new OssClient(endpoint, accessKeyId, accessKeySecret);
        }


        public static string SanitizeFileName(string fileName)
        {
            // Use a regex to remove invalid characters
            return Regex.Replace(fileName, @"[^a-zA-Z0-9_\-\.]", string.Empty);
        }

        public Task<List<string>> UploadFiles(List<string> imagePaths, string folderName, long organizationId)
        {
            try
            {
                List<string> images = new List<string>();
                foreach (var imagePath in imagePaths)
                {
                    var objectUrl = UploadFile(imagePath, folderName, organizationId).Result;
                    images.Add(objectUrl);
                }
                return Task.FromResult(images);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// uploads file to alibabab cloud
        /// </summary>
        /// <param name="imagePath"></param>
        /// <param name="folderName"></param>
        /// <param name="organizationId"></param>
        /// <param name="bookingId"></param>
        /// <returns></returns>
        /// <exception cref="ServiceException"></exception>
        public Task<string> UploadFile(string imagePath, string folderName, long organizationId, long? bookingId = null)
        {
            try
            {
                var bucketName = _configuration["AlibabaCloud:BucketName"];

                // Create a bucket if not exists. 
                if (!_ossClient.DoesBucketExist(bucketName))
                {
                    _ossClient.CreateBucket(bucketName);
                }

                // Extract extension and filename from the imagePath
                string extension = Path.GetExtension(imagePath);
                string fileName = Path.GetFileNameWithoutExtension(imagePath);

                // Sanitize the file name
                fileName = SanitizeFileName(fileName);

                // If extension is null or empty, default to PNG
                if (string.IsNullOrEmpty(extension))
                    extension = ".png"; // default extension

                string objectKey = string.Empty;
                if (bookingId.HasValue)
                    objectKey = $"Organizations/{organizationId}/{folderName}/Bookings/{bookingId}/{fileName}_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
                else
                    objectKey = $"Organizations/{organizationId}/{folderName}/{fileName}_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";

                var result = _ossClient.PutObject(bucketName, objectKey, imagePath);

                // If the upload was successful, return the full path of the uploaded file
                if (result.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    var objectUrl = $"https://{bucketName}.{_configuration["AlibabaCloud:Endpoint"]}/{objectKey}";
                    return Task.FromResult(objectUrl);
                }
                else
                {
                    throw new ServiceException("File upload failed.");
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
