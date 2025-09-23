using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Services.Interfaces;
using Services.RequestsResponses.Post;
using Repositories.Constants;

namespace VLivingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostController(IPostService postService)
        {
            _postService = postService;
        }

        /// <summary>
        /// Get all posts with pagination
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        /// <returns>Paginated list of posts</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                    return BadRequest(new { success = false, message = "Page and PageSize must be greater than 0" });

                var result = await _postService.GetAllPostsPaginatedAsync(page, pageSize);
                return Ok(new { success = true, data = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get post by ID with detailed information
        /// </summary>
        /// <param name="id">Post ID</param>
        /// <returns>Post details</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPost(int id)
        {
            try
            {
                var post = await _postService.GetPostDetailsByIdAsync(id);
                if (post == null)
                    return NotFound(new { success = false, message = "Post not found" });

                return Ok(new { success = true, data = post });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Create new post
        /// </summary>
        /// <param name="request">Post creation request</param>
        /// <returns>Created post</returns>
        [HttpPost]
        [Authorize] // Cần đăng nhập để tạo post
        public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                var createdPost = await _postService.CreatePostAsync(request);
                return CreatedAtAction(nameof(GetPost), new { id = createdPost.PostId }, 
                    new { success = true, data = createdPost });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error creating post: {ex.Message}" });
            }
        }

        /// <summary>
        /// Update existing post
        /// </summary>
        /// <param name="id">Post ID</param>
        /// <param name="request">Post update request</param>
        /// <returns>Updated post</returns>
        [HttpPut("{id}")]
        [Authorize] // Cần đăng nhập để cập nhật post
        public async Task<IActionResult> UpdatePost(int id, [FromBody] UpdatePostRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                var updatedPost = await _postService.UpdatePostAsync(id, request);
                return Ok(new { success = true, data = updatedPost });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error updating post: {ex.Message}" });
            }
        }

        /// <summary>
        /// Delete post
        /// </summary>
        /// <param name="id">Post ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        [Authorize] // Cần đăng nhập để xóa post
        public async Task<IActionResult> DeletePost(int id)
        {
            try
            {
                var deleted = await _postService.DeletePostAsync(id);
                if (!deleted)
                    return NotFound(new { success = false, message = "Post not found" });

                return Ok(new { success = true, message = "Post deleted successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error deleting post: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get posts by user ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        /// <returns>Paginated list of user's posts</returns>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetPostsByUser(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                    return BadRequest(new { success = false, message = "Page and PageSize must be greater than 0" });

                var result = await _postService.GetPostsByUserAsync(userId, page, pageSize);
                return Ok(new { success = true, data = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get posts by property ID
        /// </summary>
        /// <param name="propertyId">Property ID</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        /// <returns>Paginated list of property's posts</returns>
        [HttpGet("property/{propertyId}")]
        public async Task<IActionResult> GetPostsByProperty(int propertyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                    return BadRequest(new { success = false, message = "Page and PageSize must be greater than 0" });

                var result = await _postService.GetPostsByPropertyAsync(propertyId, page, pageSize);
                return Ok(new { success = true, data = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get posts by type
        /// </summary>
        /// <param name="type">Post type</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        /// <returns>Paginated list of posts of specified type</returns>
        [HttpGet("type/{type}")]
        public async Task<IActionResult> GetPostsByType(string type, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                    return BadRequest(new { success = false, message = "Page and PageSize must be greater than 0" });

                var result = await _postService.GetPostsByTypeAsync(type, page, pageSize);
                return Ok(new { success = true, data = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get popular posts (sorted by views)
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        /// <returns>Paginated list of popular posts</returns>
        [HttpGet("popular")]
        public async Task<IActionResult> GetPopularPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                    return BadRequest(new { success = false, message = "Page and PageSize must be greater than 0" });

                var result = await _postService.GetPopularPostsAsync(page, pageSize);
                return Ok(new { success = true, data = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get recent posts (sorted by creation date)
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        /// <returns>Paginated list of recent posts</returns>
        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                    return BadRequest(new { success = false, message = "Page and PageSize must be greater than 0" });

                var result = await _postService.GetRecentPostsAsync(page, pageSize);
                return Ok(new { success = true, data = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Increment post views
        /// </summary>
        /// <param name="id">Post ID</param>
        /// <returns>Success status</returns>
        [HttpPost("{id}/view")]
        public async Task<IActionResult> IncrementPostViews(int id)
        {
            try
            {
                var result = await _postService.IncrementPostViewsAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = "Post not found" });

                return Ok(new { success = true, message = "Post views incremented successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error incrementing post views: {ex.Message}" });
            }
        }

        /// <summary>
        /// Search posts with advanced filters
        /// </summary>
        /// <param name="request">Search criteria</param>
        /// <returns>Filtered and paginated posts</returns>
        [HttpPost("search")]
        public async Task<IActionResult> SearchPosts([FromBody] PostSearchRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { success = false, message = "Search request is required" });

                var result = await _postService.SearchPostsAsync(request);
                return Ok(new { success = true, data = result });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get post count by user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Number of posts by the user</returns>
        [HttpGet("user/{userId}/count")]
        public async Task<IActionResult> GetPostCountByUser(int userId)
        {
            try
            {
                var count = await _postService.GetPostCountByUserAsync(userId);
                return Ok(new { success = true, data = new { userId = userId, postCount = count } });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get post count by property
        /// </summary>
        /// <param name="propertyId">Property ID</param>
        /// <returns>Number of posts for the property</returns>
        [HttpGet("property/{propertyId}/count")]
        public async Task<IActionResult> GetPostCountByProperty(int propertyId)
        {
            try
            {
                var count = await _postService.GetPostCountByPropertyAsync(propertyId);
                return Ok(new { success = true, data = new { propertyId = propertyId, postCount = count } });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get total views by user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Total views across all user's posts</returns>
        [HttpGet("user/{userId}/total-views")]
        public async Task<IActionResult> GetTotalViewsByUser(int userId)
        {
            try
            {
                var totalViews = await _postService.GetTotalViewsByUserAsync(userId);
                return Ok(new { success = true, data = new { userId = userId, totalViews = totalViews } });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }
    }
}
