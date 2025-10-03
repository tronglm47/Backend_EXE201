using Microsoft.AspNetCore.Mvc;
using Services;
using Services.RequestsResponses;
using Services.RequestsResponses.Post;
using Repositories.Constants;
using System.Security.Claims;

namespace VLivingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly ILogger<PostController> _logger;

        public PostController(IPostService postService, ILogger<PostController> logger)
        {
            _postService = postService;
            _logger = logger;
        }

        /// <summary>
        /// Get all posts with pagination, sorting, and dynamic search
        /// </summary>
        /// <param name="queryParams">Query parameters (Page, PageSize, SearchField, Search, SortBy, IsDescending, Select)</param>
        /// <returns>Paginated list of posts</returns>
        /// <response code="200">Returns the paginated list of posts</response>
        /// <response code="500">Internal server error</response>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll([FromQuery] PostQuery queryParams)
        {
            try
            {
                var result = await _postService.GetAllAsync(queryParams);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting posts");
                return StatusCode(500, new { message = "An error occurred while retrieving posts", error = ex.Message });
            }
        }

        /// <summary>
        /// Get a post by ID
        /// </summary>
        /// <param name="id">Post ID</param>
        /// <param name="select">Optional: Comma-separated list of fields to select</param>
        /// <returns>Post details</returns>
        /// <response code="200">Returns the post</response>
        /// <response code="404">Post not found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id, [FromQuery] string? select = null)
        {
            try
            {
                var selectedFields = string.IsNullOrWhiteSpace(select)
                    ? new List<string>()
                    : select.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => x.Trim().ToLowerInvariant())
                            .ToList();

                var result = await _postService.GetByIdAsync(id, selectedFields);
                
                if (result == null)
                {
                    return NotFound(new { message = $"Post with ID {id} not found" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting post with ID {PostId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the post", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all posts for landlord with full details (Apartment, Building, Subdivision)
        /// </summary>
        /// <param name="queryParams">Query parameters (Page, PageSize, SearchField, Search, SortBy, IsDescending, Select)</param>
        /// <returns>Paginated list of posts with complete details</returns>
        /// <response code="200">Returns the paginated list of posts with full details</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("landlord/details")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllPostsForLandLord([FromQuery] PostQuery queryParams)
        {
            try
            {
                var result = await _postService.GetAllPostsForLandLordAsync(queryParams);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting posts for landlord with details");
                return StatusCode(500, new { message = "An error occurred while retrieving posts", error = ex.Message });
            }
        }

        /// <summary>
        /// Get detailed information for a single post for landlord (ForRent or ForSale)
        /// </summary>
        /// <param name="id">Post ID</param>
        /// <returns>Post detail with full nested information</returns>
        /// <response code="200">Post details retrieved successfully</response>
        /// <response code="404">Post not found or not a landlord post</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("landlord/{id}")]
        [ProducesResponseType(typeof(PostResponse.PostDetailForLandLord), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDetailForLandLord(int id)
        {
            try
            {
                var result = await _postService.GetDetailForLandLordAsync(id);
                
                if (result == null)
                {
                    return NotFound(new { message = $"Post with ID {id} not found or not a landlord post" });
                }
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting post detail for landlord with ID {PostId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving post details", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a post for regular user (finding room)
        /// </summary>
        /// <param name="request">Post creation data with title and description</param>
        /// <returns>Created post ID</returns>
        /// <response code="201">Post created successfully</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="401">User not authenticated</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("user")]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreatePostForUser([FromBody] PostRequest.PostCreateForUser request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { message = "Request body is required" });
                }

                if (string.IsNullOrWhiteSpace(request.Title))
                {
                    return BadRequest(new { message = "Title is required" });
                }

                // Get userId from JWT token claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var postId = await _postService.CreatePostForUserAsync(request, userId);

                if (postId > 0)
                {
                    return StatusCode(StatusCodes.Status201Created, new
                    {
                        message = "Post created successfully",
                        postId = postId,
                        postType = PostTypeConstants.FindRoom
                    });
                }

                return BadRequest(new { message = "Failed to create post" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post for user");
                return StatusCode(500, new { message = "An error occurred while creating the post", error = ex.Message });
            }
        }

        /// <summary>
        /// Update a post for regular user (finding room)
        /// </summary>
        /// <param name="id">Post ID</param>
        /// <param name="request">Post update data with title and description</param>
        /// <returns>Update result</returns>
        /// <response code="200">Post updated successfully</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="401">User not authenticated</response>
        /// <response code="403">User is not the owner of the post</response>
        /// <response code="404">Post not found</response>
        /// <response code="500">Internal server error</response>
        [HttpPut("user/{id}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdatePostForUser(int id, [FromBody] PostRequest.PostUpdateForUser request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { message = "Request body is required" });
                }

                if (string.IsNullOrWhiteSpace(request.Title))
                {
                    return BadRequest(new { message = "Title is required" });
                }

                // Get userId from JWT token claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var result = await _postService.UpdatePostForUserAsync(request, id, userId);

                if (result)
                {
                    return Ok(new
                    {
                        message = "Post updated successfully",
                        postId = id
                    });
                }

                return BadRequest(new { message = "Failed to update post. Please check if the post exists and you are the owner." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating post for user");
                return StatusCode(500, new { message = "An error occurred while updating the post", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a post for landlord (rental listing with apartment details)
        /// </summary>
        /// <param name="request">Post creation data with apartment information</param>
        /// <returns>Created post ID</returns>
        /// <response code="201">Post created successfully</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="401">User not authenticated</response>
        /// <response code="404">Building not found</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("landlord")]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreatePostForLandLord([FromBody] PostRequest.PostCreateForLandLord request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { message = "Request body is required" });
                }

                if (string.IsNullOrWhiteSpace(request.Title))
                {
                    return BadRequest(new { message = "Title is required" });
                }

                if (request.Apartment == null)
                {
                    return BadRequest(new { message = "Apartment information is required for landlord post" });
                }

                if (request.Price <= 0)
                {
                    return BadRequest(new { message = "Price must be greater than 0" });
                }

                // Get userId from JWT token claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var postId = await _postService.CreatePostForLandLordAsync(request, userId);

                if (postId > 0)
                {
                    return StatusCode(StatusCodes.Status201Created, new
                    {
                        message = "Post created successfully",
                        postId = postId,
                        postType = PostTypeConstants.ForRent
                    });
                }

                return BadRequest(new { message = "Failed to create post. Please check if the building exists." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post for landlord");
                return StatusCode(500, new { message = "An error occurred while creating the post", error = ex.Message });
            }
        }

        /// <summary>
        /// Update a post for landlord (rental listing with apartment details)
        /// </summary>
        /// <param name="id">Post ID</param>
        /// <param name="request">Post update data with apartment information</param>
        /// <returns>Update result</returns>
        /// <response code="200">Post updated successfully</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="401">User not authenticated</response>
        /// <response code="403">User is not the owner of the post</response>
        /// <response code="404">Post not found</response>
        /// <response code="500">Internal server error</response>
        [HttpPut("landlord/{id}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdatePostForLandLord(int id, [FromBody] PostRequest.PostUpdateForLandLord request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { message = "Request body is required" });
                }

                if (string.IsNullOrWhiteSpace(request.Title))
                {
                    return BadRequest(new { message = "Title is required" });
                }

                if (request.Apartment == null)
                {
                    return BadRequest(new { message = "Apartment information is required for landlord post" });
                }

                if (request.Price <= 0)
                {
                    return BadRequest(new { message = "Price must be greater than 0" });
                }

                // Get userId from JWT token claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var result = await _postService.UpdatePostForLandLordAsync(request, id, userId);

                if (result)
                {
                    return Ok(new
                    {
                        message = "Post and apartment updated successfully",
                        postId = id
                    });
                }

                return BadRequest(new { message = "Failed to update post. Please check if the post exists and you are the owner." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating post for landlord");
                return StatusCode(500, new { message = "An error occurred while updating the post", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a post (soft delete)
        /// </summary>
        /// <param name="id">Post ID</param>
        /// <returns>Delete result</returns>
        /// <response code="200">Post deleted successfully</response>
        /// <response code="401">User not authenticated</response>
        /// <response code="403">User is not the owner of the post</response>
        /// <response code="404">Post not found</response>
        /// <response code="500">Internal server error</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeletePost(int id)
        {
            try
            {
                // Get userId from JWT token claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var result = await _postService.DeletePostAsync(id, userId);

                if (result)
                {
                    return Ok(new
                    {
                        message = "Post deleted successfully",
                        postId = id
                    });
                }

                return BadRequest(new { message = "Failed to delete post. Please check if the post exists and you are the owner." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting post");
                return StatusCode(500, new { message = "An error occurred while deleting the post", error = ex.Message });
            }
        }
    }
}
