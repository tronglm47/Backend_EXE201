using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using VLivingAPI.Authorization;
using Repositories.Constants;

namespace VLivingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly ICloudStorageService _cloudStorageService;
        private readonly ILogger<ImageController> _logger;

        public ImageController(ICloudStorageService cloudStorageService, ILogger<ImageController> logger)
        {
            _cloudStorageService = cloudStorageService;
            _logger = logger;
        }

        /// <summary>
        /// Upload single image for posts
        /// </summary>
        /// <param name="file">Image file to upload</param>
        /// <param name="folder">Optional folder path (default: posts)</param>
        /// <returns>Image URL</returns>
        [HttpPost("upload")]
        [BusinessAuthorize(BusinessRole.PostManagement)]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadImage(
            [FromForm] IFormFile file, 
            [FromForm] string folder = "posts")
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "No file was uploaded"
                    });
                }

                var imageUrl = await _cloudStorageService.UploadImageAsync(file, folder);

                return Ok(new
                {
                    success = true,
                    message = "Image uploaded successfully",
                    data = new { imageUrl }
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error occurred while uploading the image"
                });
            }
        }

        /// <summary>
        /// Upload multiple images for posts
        /// </summary>
        /// <param name="files">Array of image files to upload</param>
        /// <param name="folder">Optional folder path (default: posts)</param>
        /// <returns>Array of image URLs</returns>
        [HttpPost("upload-multiple")]
        [BusinessAuthorize(BusinessRole.PostManagement)]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadMultipleImages(
            [FromForm] List<IFormFile> files, 
            [FromForm] string folder = "posts")
        {
            try
            {
                if (files == null || !files.Any())
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "No files were uploaded"
                    });
                }

                // Limit number of files (max 10)
                if (files.Count > 10)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Maximum 10 files allowed per upload"
                    });
                }

                var uploadTasks = files.Select(file => _cloudStorageService.UploadImageAsync(file, folder));
                var imageUrls = await Task.WhenAll(uploadTasks);

                return Ok(new
                {
                    success = true,
                    message = $"Successfully uploaded {imageUrls.Length} images",
                    data = new { imageUrls }
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading multiple images");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error occurred while uploading images"
                });
            }
        }

        /// <summary>
        /// Delete image by URL
        /// </summary>
        /// <param name="imageUrl">URL of the image to delete</param>
        /// <returns>Success status</returns>
        [HttpDelete]
        [BusinessAuthorize(BusinessRole.PostManagement)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteImage([FromQuery] string imageUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(imageUrl))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Image URL is required"
                    });
                }

                var result = await _cloudStorageService.DeleteImageAsync(imageUrl);

                if (result)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Image deleted successfully"
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Failed to delete image"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image: {ImageUrl}", imageUrl);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error occurred while deleting the image"
                });
            }
        }

        /// <summary>
        /// Delete multiple images by URLs
        /// </summary>
        /// <param name="imageUrls">Array of image URLs to delete</param>
        /// <returns>Success status</returns>
        [HttpDelete("multiple")]
        [BusinessAuthorize(BusinessRole.PostManagement)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteMultipleImages([FromBody] List<string> imageUrls)
        {
            try
            {
                if (imageUrls == null || !imageUrls.Any())
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Image URLs are required"
                    });
                }

                var result = await _cloudStorageService.DeleteImagesAsync(imageUrls);

                if (result)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"Successfully deleted {imageUrls.Count} images"
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Some images failed to delete"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting multiple images");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error occurred while deleting images"
                });
            }
        }

        /// <summary>
        /// Health check for image service
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                success = true,
                message = "Image service is healthy",
                timestamp = DateTime.UtcNow
            });
        }
    }
}