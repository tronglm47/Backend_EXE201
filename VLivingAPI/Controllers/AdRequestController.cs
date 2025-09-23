using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Services.Interfaces;
using Services.RequestsResponses.AdRequest;
using Repositories.Constants;

namespace VLivingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // AdRequest cần đăng nhập để quản lý
    public class AdRequestController : ControllerBase
    {
        private readonly IAdRequestService _adRequestService;

        public AdRequestController(IAdRequestService adRequestService)
        {
            _adRequestService = adRequestService;
        }

        /// <summary>
        /// Get all ad requests with pagination
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllAdRequests([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                    return BadRequest(new { success = false, message = "Page and PageSize must be greater than 0" });

                var result = await _adRequestService.GetAllAdRequestsPaginatedAsync(page, pageSize);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get ad request by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAdRequest(int id)
        {
            try
            {
                var adRequest = await _adRequestService.GetAdRequestByIdAsync(id);
                if (adRequest == null)
                    return NotFound(new { success = false, message = "Ad request not found" });

                return Ok(new { success = true, data = adRequest });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Create new ad request
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateAdRequest([FromBody] CreateAdRequestRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                var createdAdRequest = await _adRequestService.CreateAdRequestAsync(request);
                return CreatedAtAction(nameof(GetAdRequest), new { id = createdAdRequest.RequestId }, 
                    new { success = true, data = createdAdRequest });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error creating ad request: {ex.Message}" });
            }
        }

        /// <summary>
        /// Update ad request
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAdRequest(int id, [FromBody] UpdateAdRequestRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                var updatedAdRequest = await _adRequestService.UpdateAdRequestAsync(id, request);
                return Ok(new { success = true, data = updatedAdRequest });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error updating ad request: {ex.Message}" });
            }
        }

        /// <summary>
        /// Delete ad request
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdRequest(int id)
        {
            try
            {
                var result = await _adRequestService.DeleteAdRequestAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = "Ad request not found" });

                return Ok(new { success = true, message = "Ad request deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error deleting ad request: {ex.Message}" });
            }
        }
    }
}
