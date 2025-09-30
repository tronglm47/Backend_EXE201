using Google.Cloud.Storage.V1;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Services
{
    public interface ICloudStorageService
    {
        Task<string> UploadImageAsync(IFormFile file, string folderPath = "posts");
        Task<bool> DeleteImageAsync(string imageUrl);
        Task<bool> DeleteImagesAsync(IEnumerable<string> imageUrls);
    }

    public class CloudStorageService : ICloudStorageService
    {
        private readonly StorageClient _storageClient;
        private readonly string _bucketName;
        private readonly ILogger<CloudStorageService> _logger;

        public CloudStorageService(IConfiguration configuration, ILogger<CloudStorageService> logger)
        {
            _logger = logger;
            _bucketName = configuration["GoogleCloudStorage:BucketName"] ?? "vliving-post-image";

            try
            {
                var useEnvironmentCredentials = configuration.GetValue<bool>("GoogleCloudStorage:UseEnvironmentCredentials");
                
                if (useEnvironmentCredentials)
                {
                    // Use environment variables for authentication (recommended for production)
                    _storageClient = StorageClient.Create();
                    _logger.LogInformation("Using environment credentials for Google Cloud Storage");
                }
                else
                {
                    // Use JSON file for local development
                    var credentialPath = configuration["GoogleCloudStorage:CredentialPath"];
                    if (!string.IsNullOrEmpty(credentialPath) && File.Exists(credentialPath))
                    {
                        var credential = GoogleCredential.FromFile(credentialPath);
                        _storageClient = StorageClient.Create(credential);
                        _logger.LogInformation("Using file credentials for Google Cloud Storage: {CredentialPath}", credentialPath);
                    }
                    else
                    {
                        // Fallback to default credentials
                        _storageClient = StorageClient.Create();
                        _logger.LogInformation("Using default credentials for Google Cloud Storage");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Google Cloud Storage client");
                throw;
            }
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folderPath = "posts")
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is null or empty");
            }

            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
            {
                throw new ArgumentException($"File type {file.ContentType} is not supported. Allowed types: {string.Join(", ", allowedTypes)}");
            }

            // Validate file size (max 5MB)
            const long maxFileSize = 5 * 1024 * 1024; // 5MB
            if (file.Length > maxFileSize)
            {
                throw new ArgumentException($"File size {file.Length} bytes exceeds maximum allowed size of {maxFileSize} bytes");
            }

            try
            {
                // Generate unique filename
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var objectName = $"{folderPath}/{uniqueFileName}";

                // Upload to Google Cloud Storage
                using var stream = file.OpenReadStream();
                var storageObject = await _storageClient.UploadObjectAsync(
                    _bucketName,
                    objectName,
                    file.ContentType,
                    stream);

                // Return public URL
                var publicUrl = $"https://storage.googleapis.com/{_bucketName}/{objectName}";
                
                _logger.LogInformation("Successfully uploaded file {FileName} to {ObjectName}", file.FileName, objectName);
                return publicUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload file {FileName} to Google Cloud Storage", file.FileName);
                throw new InvalidOperationException($"Failed to upload file: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return true; // Consider empty URL as successfully "deleted"
            }

            try
            {
                // Extract object name from URL
                // URL format: https://storage.googleapis.com/bucket-name/folder/filename.ext
                var objectName = ExtractObjectNameFromUrl(imageUrl);
                if (string.IsNullOrEmpty(objectName))
                {
                    _logger.LogWarning("Could not extract object name from URL: {ImageUrl}", imageUrl);
                    return false;
                }

                await _storageClient.DeleteObjectAsync(_bucketName, objectName);
                _logger.LogInformation("Successfully deleted object {ObjectName} from bucket {BucketName}", objectName, _bucketName);
                return true;
            }
            catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Object not found when trying to delete {ImageUrl}", imageUrl);
                return true; // Consider not found as successfully deleted
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete image from Google Cloud Storage: {ImageUrl}", imageUrl);
                return false;
            }
        }

        public async Task<bool> DeleteImagesAsync(IEnumerable<string> imageUrls)
        {
            if (imageUrls == null || !imageUrls.Any())
            {
                return true;
            }

            var results = await Task.WhenAll(
                imageUrls.Select(url => DeleteImageAsync(url))
            );

            return results.All(result => result);
        }

        private string ExtractObjectNameFromUrl(string imageUrl)
        {
            try
            {
                // URL format: https://storage.googleapis.com/bucket-name/folder/filename.ext
                var uri = new Uri(imageUrl);
                var path = uri.AbsolutePath;
                
                // Remove leading slash and bucket name
                var bucketPrefix = $"/{_bucketName}/";
                if (path.StartsWith(bucketPrefix))
                {
                    return path.Substring(bucketPrefix.Length);
                }

                // Alternative: if URL doesn't contain bucket name, assume it's after first slash
                return path.TrimStart('/');
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse image URL: {ImageUrl}", imageUrl);
                return string.Empty;
            }
        }
    }
}