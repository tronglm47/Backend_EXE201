using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Services.Interfaces;
using Services.RequestsResponses.RoommatePreference;
using Repositories.Constants;

namespace VLivingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // RoommatePreference là thông tin cá nhân, cần đăng nhập
    public class RoommatePreferenceController : ControllerBase
    {
        private readonly IRoommatePreferenceService _roommatePreferenceService;

        public RoommatePreferenceController(IRoommatePreferenceService roommatePreferenceService)
        {
            _roommatePreferenceService = roommatePreferenceService;
        }

        /// <summary>
        /// Get all roommate preferences with pagination
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllRoommatePreferences([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                    return BadRequest(new { success = false, message = "Page and PageSize must be greater than 0" });

                var result = await _roommatePreferenceService.GetAllRoommatePreferencesPaginatedAsync(page, pageSize);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get roommate preference by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoommatePreference(int id)
        {
            try
            {
                var preference = await _roommatePreferenceService.GetRoommatePreferenceByIdAsync(id);
                if (preference == null)
                    return NotFound(new { success = false, message = "Roommate preference not found" });

                return Ok(new { success = true, data = preference });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Create new roommate preference
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateRoommatePreference([FromBody] CreateRoommatePreferenceRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                var createdPreference = await _roommatePreferenceService.CreateRoommatePreferenceAsync(request);
                return CreatedAtAction(nameof(GetRoommatePreference), new { id = createdPreference.PreferenceId }, 
                    new { success = true, data = createdPreference });
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
                return StatusCode(500, new { success = false, message = $"Error creating roommate preference: {ex.Message}" });
            }
        }

        /// <summary>
        /// Update roommate preference
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoommatePreference(int id, [FromBody] UpdateRoommatePreferenceRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                var updatedPreference = await _roommatePreferenceService.UpdateRoommatePreferenceAsync(id, request);
                return Ok(new { success = true, data = updatedPreference });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error updating roommate preference: {ex.Message}" });
            }
        }

        /// <summary>
        /// Delete roommate preference
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoommatePreference(int id)
        {
            try
            {
                var result = await _roommatePreferenceService.DeleteRoommatePreferenceAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = "Roommate preference not found" });

                return Ok(new { success = true, message = "Roommate preference deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error deleting roommate preference: {ex.Message}" });
            }
        }
    }
}
