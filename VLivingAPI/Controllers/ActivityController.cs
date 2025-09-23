using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Services.Interfaces;
using Services.RequestsResponses.Activity;
using Repositories.Constants;

namespace VLivingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivityController : ControllerBase
    {
        private readonly IActivityService _activityService;

        public ActivityController(IActivityService activityService)
        {
            _activityService = activityService;
        }

        /// <summary>
        /// Get all activities with pagination
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllActivities([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                    return BadRequest(new { success = false, message = "Page and PageSize must be greater than 0" });

                var result = await _activityService.GetAllActivitiesPaginatedAsync(page, pageSize);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get activity by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetActivity(int id)
        {
            try
            {
                var activity = await _activityService.GetActivityByIdAsync(id);
                if (activity == null)
                    return NotFound(new { success = false, message = "Activity not found" });

                return Ok(new { success = true, data = activity });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Create new activity
        /// </summary>
        [HttpPost]
        [Authorize] // Cần đăng nhập để tạo activity
        public async Task<IActionResult> CreateActivity([FromBody] CreateActivityRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                var createdActivity = await _activityService.CreateActivityAsync(request);
                return CreatedAtAction(nameof(GetActivity), new { id = createdActivity.ActivityId }, 
                    new { success = true, data = createdActivity });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error creating activity: {ex.Message}" });
            }
        }

        /// <summary>
        /// Update existing activity
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateActivity(int id, [FromBody] UpdateActivityRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                var updatedActivity = await _activityService.UpdateActivityAsync(id, request);
                return Ok(new { success = true, data = updatedActivity });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error updating activity: {ex.Message}" });
            }
        }

        /// <summary>
        /// Delete activity by ID
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteActivity(int id)
        {
            try
            {
                var deleted = await _activityService.DeleteActivityAsync(id);
                if (!deleted)
                    return NotFound(new { success = false, message = "Activity not found" });

                return Ok(new { success = true, message = "Activity deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error deleting activity: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get activities by creator ID with pagination
        /// </summary>
        [HttpGet("creator/{creatorId}")]
        public async Task<IActionResult> GetActivitiesByCreator(int creatorId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                    return BadRequest(new { success = false, message = "Page and PageSize must be greater than 0" });

                var result = await _activityService.GetActivitiesByCreatorAsync(creatorId, page, pageSize);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get activities by location ID with pagination
        /// </summary>
        [HttpGet("location/{locationId}")]
        public async Task<IActionResult> GetActivitiesByLocation(int locationId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                    return BadRequest(new { success = false, message = "Page and PageSize must be greater than 0" });

                var result = await _activityService.GetActivitiesByLocationAsync(locationId, page, pageSize);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get activities by status with pagination
        /// </summary>
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetActivitiesByStatus(string status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                    return BadRequest(new { success = false, message = "Page and PageSize must be greater than 0" });

                var result = await _activityService.GetActivitiesByStatusAsync(status, page, pageSize);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Search activities with filters and pagination
        /// </summary>
        [HttpPost("search")]
        public async Task<IActionResult> SearchActivities([FromBody] ActivitySearchRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid search parameters", errors = ModelState });

                if (request.Page < 1 || request.PageSize < 1)
                    return BadRequest(new { success = false, message = "Page and PageSize must be greater than 0" });

                var result = await _activityService.SearchActivitiesAsync(request);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error searching activities: {ex.Message}" });
            }
        }

        /// <summary>
        /// Update activity status
        /// </summary>
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateActivityStatus(int id, [FromBody] UpdateActivityStatusRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                var updated = await _activityService.UpdateActivityStatusAsync(id, request);
                if (!updated)
                    return NotFound(new { success = false, message = "Activity not found or status update failed" });

                return Ok(new { success = true, message = "Activity status updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error updating activity status: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get upcoming activities with pagination
        /// </summary>
        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcomingActivities([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                    return BadRequest(new { success = false, message = "Page and PageSize must be greater than 0" });

                var result = await _activityService.GetUpcomingActivitiesAsync(page, pageSize);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get past activities with pagination
        /// </summary>
        [HttpGet("past")]
        public async Task<IActionResult> GetPastActivities([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                    return BadRequest(new { success = false, message = "Page and PageSize must be greater than 0" });

                var result = await _activityService.GetPastActivitiesAsync(page, pageSize);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }
    }
}
