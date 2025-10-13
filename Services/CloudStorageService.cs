using Google.Cloud.Storage.V1;
using Google.Apis.Auth.OAuth2;
using Google;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Services
{
    public interface ICloudStorageService
    {
        Task<string> UploadImageAsync(IFormFile file, string entityType = "posts", int? entityId = null);
        Task<bool> DeleteImageAsync(string imageUrl);
        Task<bool> DeleteImagesAsync(IEnumerable<string> imageUrls);
        Task<List<string>> UploadMultipleImagesAsync(IEnumerable<IFormFile> files, string entityType = "posts", int? entityId = null);
    }

    public class CloudStorageService : ICloudStorageService
    {
        private readonly StorageClient? _storageClient;
        private readonly string _bucketName;
        private readonly ILogger<CloudStorageService> _logger;

        public CloudStorageService(IConfiguration configuration, ILogger<CloudStorageService> logger)
        {
            _logger = logger;
            _bucketName = configuration["GoogleCloudStorage:BucketName"] ?? "vliving-storage-bucket";

            try
            {
                var useEnvironmentCredentials = configuration.GetValue<bool>("GoogleCloudStorage:UseEnvironmentCredentials");
                
                _logger.LogInformation("Google Cloud Storage Configuration:");
                _logger.LogInformation("- BucketName: {BucketName}", _bucketName);
                _logger.LogInformation("- UseEnvironmentCredentials: {UseEnvironmentCredentials}", useEnvironmentCredentials);
                
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
                    _logger.LogInformation("- CredentialPath: {CredentialPath}", credentialPath);
                    
                    if (!string.IsNullOrEmpty(credentialPath))
                    {
                        // Check if path is relative, make it absolute
                        var fullPath = Path.IsPathRooted(credentialPath) 
                            ? credentialPath 
                            : Path.Combine(Directory.GetCurrentDirectory(), credentialPath);
                            
                        _logger.LogInformation("- Full credential path: {FullPath}", fullPath);
                        _logger.LogInformation("- File exists: {FileExists}", File.Exists(fullPath));
                        
                        if (File.Exists(fullPath))
                        {
                            var credential = GoogleCredential.FromFile(fullPath);
                            _storageClient = StorageClient.Create(credential);
                            _logger.LogInformation("Successfully initialized Google Cloud Storage with file credentials");
                        }
                        else
                        {
                            _storageClient = null;
                            _logger.LogWarning("Google Cloud Storage credential file not found at: {FullPath}", fullPath);
                        }
                    }
                    else
                    {
                        // Fallback: disable cloud storage if no credentials
                        _storageClient = null;
                        _logger.LogWarning("Google Cloud Storage CredentialPath is empty or null. Cloud storage will be disabled.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Google Cloud Storage client. Cloud storage will be disabled.");
                _storageClient = null;
            }
        }

        private async Task EnsureBucketExistsAsync()
        {
            if (_storageClient == null)
                return;

            try
            {
                // Try to get bucket info to check if it exists
                await _storageClient.GetBucketAsync(_bucketName);
                _logger.LogInformation("Bucket {BucketName} exists and is accessible", _bucketName);
            }
            catch (GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Bucket {BucketName} does not exist. Attempting to create it...", _bucketName);
                
                try
                {
                    var projectId = "valiant-index-438307-p0";
                    await _storageClient.CreateBucketAsync(projectId, _bucketName);
                    _logger.LogInformation("Successfully created bucket {BucketName}", _bucketName);
                }
                catch (Exception createEx)
                {
                    _logger.LogError(createEx, "Failed to create bucket {BucketName}. Please create it manually on Google Cloud Console.", _bucketName);
                    throw new InvalidOperationException($"Bucket {_bucketName} does not exist and could not be created automatically. Please create it manually on Google Cloud Console.", createEx);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check bucket {BucketName} status", _bucketName);
                throw;
            }
        }

        public async Task<string> UploadImageAsync(IFormFile file, string entityType = "posts", int? entityId = null)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is null or empty");
            }

            if (_storageClient == null)
            {
                _logger.LogWarning("Google Cloud Storage is not available. Image upload skipped for file: {FileName}", file.FileName);
                return string.Empty; // Return empty string instead of placeholder
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
                // Ensure bucket exists before uploading
                await EnsureBucketExistsAsync();

                // Generate organized folder structure
                var timestamp = DateTime.UtcNow.ToString("yyyy/MM/dd");
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                
                // Create folder structure: entityType/yyyy/MM/dd/[entityId]/filename
                string folderPath;
                if (entityId.HasValue)
                {
                    folderPath = $"{entityType}/{timestamp}/{entityId.Value}";
                }
                else
                {
                    folderPath = $"{entityType}/{timestamp}";
                }
                
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
                _logger.LogError(ex, "Failed to upload file {FileName} to Google Cloud Storage. Error: {ErrorMessage}", file.FileName, ex.Message);
                // Return empty string instead of throwing exception to allow post creation to continue
                return string.Empty;
            }
        }

        public async Task<List<string>> UploadMultipleImagesAsync(IEnumerable<IFormFile> files, string entityType = "posts", int? entityId = null)
        {
            if (files == null || !files.Any())
            {
                return new List<string>();
            }

            var uploadTasks = files.Select(file => UploadImageAsync(file, entityType, entityId));
            var results = await Task.WhenAll(uploadTasks);
            
            // Filter out empty strings (failed uploads)
            return results.Where(url => !string.IsNullOrEmpty(url)).ToList();
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return true; // Consider empty URL as successfully "deleted"
            }

            if (_storageClient == null)
            {
                _logger.LogWarning("Google Cloud Storage is not available. Cannot delete image: {ImageUrl}", imageUrl);
                return false;
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