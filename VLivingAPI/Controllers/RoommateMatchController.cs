using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Services.Interfaces;
using Services.RequestsResponses.RoommateMatch;
using Repositories.Constants;

namespace VLivingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // RoommateMatch là thông tin cá nhân, cần đăng nhập
    public class RoommateMatchController : ControllerBase
    {
        private readonly IRoommateMatchService _roommateMatchService;

        public RoommateMatchController(IRoommateMatchService roommateMatchService)
        {
            _roommateMatchService = roommateMatchService;
        }

        /// <summary>
        /// Get all roommate matches with pagination
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllRoommateMatches([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                    return BadRequest(new { success = false, message = "Page and PageSize must be greater than 0" });

                var result = await _roommateMatchService.GetAllRoommateMatchesPaginatedAsync(page, pageSize);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get roommate match by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoommateMatch(int id)
        {
            try
            {
                var roommateMatch = await _roommateMatchService.GetRoommateMatchByIdAsync(id);
                if (roommateMatch == null)
                    return NotFound(new { success = false, message = "Roommate match not found" });

                return Ok(new { success = true, data = roommateMatch });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Create new roommate match
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateRoommateMatch([FromBody] CreateRoommateMatchRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                var createdMatch = await _roommateMatchService.CreateRoommateMatchAsync(request);
                return CreatedAtAction(nameof(GetRoommateMatch), new { id = createdMatch.MatchId }, 
                    new { success = true, data = createdMatch });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error creating roommate match: {ex.Message}" });
            }
        }

        /// <summary>
        /// Update existing roommate match
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoommateMatch(int id, [FromBody] UpdateRoommateMatchRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                var updatedMatch = await _roommateMatchService.UpdateRoommateMatchAsync(id, request);
                return Ok(new { success = true, data = updatedMatch, message = "Roommate match updated successfully" });
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
                return StatusCode(500, new { success = false, message = $"Error updating roommate match: {ex.Message}" });
            }
        }

        /// <summary>
        /// Delete roommate match
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoommateMatch(int id)
        {
            try
            {
                var result = await _roommateMatchService.DeleteRoommateMatchAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = "Roommate match not found" });

                return Ok(new { success = true, message = "Roommate match deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error deleting roommate match: {ex.Message}" });
            }
        }
    }
}
