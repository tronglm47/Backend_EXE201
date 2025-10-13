using Microsoft.AspNetCore.Mvc;
using VLivingAPI.Authorization;
using Services;
using Services.RequestsResponses;
using Services.RequestsResponses.User;

namespace VLivingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Get all users with pagination, sorting, and dynamic search (Admin only)
        /// </summary>
        /// <param name="queryParams">Query parameters (Page, PageSize, SearchField, Search, SortBy, IsDescending, Select)</param>
        /// <returns>Paginated list of users (without password)</returns>
        /// <response code="200">Returns the paginated list of users</response>
        /// <response code="401">Unauthorized - User not authenticated</response>
        /// <response code="403">Forbidden - User doesn't have admin permissions</response>
        /// <response code="500">Internal server error</response>
        [HttpGet]
        [BusinessAuthorize(BusinessRole.FullSystemAccess)]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll([FromQuery] UserQueryParameters queryParams)
        {
            try
            {
                var result = await _userService.GetAllAsync(queryParams);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                return StatusCode(500, new { message = "An error occurred while retrieving users", error = ex.Message });
            }
        }

        /// <summary>
        /// Get a user by ID (Admin only)
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="select">Optional: Comma-separated list of fields to select</param>
        /// <returns>User details (without password)</returns>
        /// <response code="200">Returns the user</response>
        /// <response code="401">Unauthorized - User not authenticated</response>
        /// <response code="403">Forbidden - User doesn't have admin permissions</response>
        /// <response code="404">User not found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("{id}")]
        [BusinessAuthorize(BusinessRole.FullSystemAccess)]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

                var result = await _userService.GetByIdAsync(id, selectedFields);
                
                if (result == null)
                {
                    return NotFound(new { message = $"User with ID {id} not found" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user with ID {UserId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the user", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing user (Admin only)
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="request">User update data</param>
        /// <returns>Update result</returns>
        /// <response code="200">User updated successfully</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="401">Unauthorized - User not authenticated</response>
        /// <response code="403">Forbidden - User doesn't have admin permissions</response>
        /// <response code="404">User not found</response>
        /// <response code="409">Conflict - Username or email already exists</response>
        /// <response code="500">Internal server error</response>
        [HttpPut("{id}")]
        [BusinessAuthorize(BusinessRole.FullSystemAccess)]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] UserRequest.AdminUpdateUserRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { message = "Request body cannot be null" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Invalid request data", errors = ModelState });
                }

                var result = await _userService.UpdateAsync(request, id);

                if (result)
                {
                    return Ok(new { message = "User updated successfully" });
                }

                // Check if it's a conflict (username/email already exists) or not found
                return StatusCode(409, new { message = "Username or email already exists, or user not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user with ID {UserId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the user", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a user (Admin only)
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>Delete result</returns>
        /// <response code="200">User deleted successfully</response>
        /// <response code="401">Unauthorized - User not authenticated</response>
        /// <response code="403">Forbidden - User doesn't have admin permissions</response>
        /// <response code="404">User not found</response>
        /// <response code="500">Internal server error</response>
        [HttpDelete("{id}")]
        [BusinessAuthorize(BusinessRole.FullSystemAccess)]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _userService.DeleteAsync(id);

                if (result)
                {
                    return Ok(new { message = "User deleted successfully" });
                }

                return NotFound(new { message = $"User with ID {id} not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with ID {UserId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the user", error = ex.Message });
            }
        }
    }
}
