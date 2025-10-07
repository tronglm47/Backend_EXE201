using Microsoft.AspNetCore.Mvc;
using Services;
using Services.RequestsResponses;
using Services.RequestsResponses.PostUtility;
using System.Collections.Generic;
using VLivingAPI.Authorization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace VLivingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostUtilityController : ControllerBase
    {
        private readonly IPostUtilityService _postUtilityService;
        private readonly ILogger<PostUtilityController> _logger;
        public PostUtilityController(IPostUtilityService postUtilityService, ILogger<PostUtilityController> logger)
        {
            _postUtilityService = postUtilityService;
            _logger = logger;
        }
        /// <summary>
        /// Get all post utilities with pagination, sorting, and dynamic search
        /// </summary>
        /// <param name="queryParams">Query parameters (Page, PageSize, SearchField, Search, SortBy, IsDescending, Select)</param>
        /// <returns>Paginated list of post utilities</returns>
        /// <response code="200">Returns the paginated list of post utilities</response>
        /// <response code="500">Internal server error</response>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll([FromQuery] PostUtilityQuery queryParams)
        {
            try
            {
                var result = await _postUtilityService.GetAllAsync(queryParams);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting post utilities");
                return StatusCode(500, new { message = "An error occurred while retrieving post utilities", error = ex.Message });
            }
        }

        /// <summary>
        /// Get a post utility by ID
        /// </summary>
        /// <param name="id">Post Utility ID (PostId)</param>
        /// <param name="select">Optional: Comma-separated list of fields to select</param>
        /// <returns>Post Utility details</returns>
        /// <response code="200">Returns the post utility</response>
        /// <response code="404">Post Utility not found</response>
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

                var result = await _postUtilityService.GetByIdAsync(id, selectedFields);
                
                if (result == null)
                {
                    return NotFound(new { message = $"Post Utility with ID {id} not found" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting post utility with ID {PostUtilityId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the post utility", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new post utility
        /// </summary>
        /// <param name="request">Post Utility creation data</param>
        /// <returns>Created post utility ID</returns>
        /// <response code="201">Post Utility created successfully</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="401">Unauthorized - User not authenticated</response>
        /// <response code="403">Forbidden - User doesn't have required permissions</response>
        /// <response code="500">Internal server error</response>
        [HttpPost]
        [BusinessAuthorize(BusinessRole.PostManagement)]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] PostUtilityRequest.PostUtilityCreate request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { message = "Request body cannot be null" });
                }

                if (request.PostId <= 0 || request.UtilityId <= 0)
                {
                    return BadRequest(new { message = "PostId and UtilityId are required and must be positive" });
                }

                var postUtilityId = await _postUtilityService.CreateAsync(request);

                if (postUtilityId > 0)
                {
                    return CreatedAtAction(
                        nameof(GetById),
                        new { id = postUtilityId },
                        new { id = postUtilityId, message = "Post Utility created successfully" }
                    );
                }

                return StatusCode(500, new { message = "Failed to create post utility" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post utility");
                return StatusCode(500, new { message = "An error occurred while creating the post utility", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing post utility
        /// </summary>
        /// <param name="id">Post Utility ID (PostId)</param>
        /// <param name="request">Post Utility update data</param>
        /// <returns>Update result</returns>
        /// <response code="200">Post Utility updated successfully</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="401">Unauthorized - User not authenticated</response>
        /// <response code="403">Forbidden - User doesn't have required permissions</response>
        /// <response code="404">Post Utility not found</response>
        /// <response code="500">Internal server error</response>
        [HttpPut("{id}")]
        [BusinessAuthorize(BusinessRole.PostManagement)]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] PostUtilityRequest.PostUtilityUpdate request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { message = "Request body cannot be null" });
                }

                var result = await _postUtilityService.UpdateAsync(request, id);

                if (result)
                {
                    return Ok(new { message = "Post Utility updated successfully" });
                }

                return NotFound(new { message = $"Post Utility with ID {id} not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating post utility with ID {PostUtilityId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the post utility", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a post utility
        /// </summary>
        /// <param name="id">Post Utility ID (PostId)</param>
        /// <returns>Delete result</returns>
        /// <response code="200">Post Utility deleted successfully</response>
        /// <response code="401">Unauthorized - User not authenticated</response>
        /// <response code="403">Forbidden - User doesn't have required permissions</response>
        /// <response code="404">Post Utility not found</response>
        /// <response code="500">Internal server error</response>
        [HttpDelete("{id}")]
        [BusinessAuthorize(BusinessRole.PostManagement)]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _postUtilityService.DeleteAsync(id);

                if (result)
                {
                    return Ok(new { message = "Post Utility deleted successfully" });
                }

                return NotFound(new { message = $"Post Utility with ID {id} not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting post utility with ID {PostUtilityId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the post utility", error = ex.Message });
            }
        }
    }
}
