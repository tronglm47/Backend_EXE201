using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Services.Interfaces;
using Services.RequestsResponses.Ad;

namespace VLivingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdController : ControllerBase
    {
        private readonly IAdService _adService;

        public AdController(IAdService adService)
        {
            _adService = adService;
        }

        /// <summary>
        /// Get all ads with pagination
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllAds([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                    return BadRequest(new { success = false, message = "Page and PageSize must be greater than 0" });

                var result = await _adService.GetAllAdsPaginatedAsync(page, pageSize);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get ad by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAd(int id)
        {
            try
            {
                var ad = await _adService.GetAdByIdAsync(id);
                if (ad == null)
                    return NotFound(new { success = false, message = "Ad not found" });

                return Ok(new { success = true, data = ad });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Create new ad
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateAd([FromBody] CreateAdRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                var createdAd = await _adService.CreateAdAsync(request);
                return CreatedAtAction(nameof(GetAd), new { id = createdAd.AdId }, 
                    new { success = true, data = createdAd });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error creating ad: {ex.Message}" });
            }
        }

        /// <summary>
        /// Update existing ad
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateAd(int id, [FromBody] UpdateAdRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                var updatedAd = await _adService.UpdateAdAsync(id, request);
                return Ok(new { success = true, data = updatedAd, message = "Ad updated successfully" });
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
                return StatusCode(500, new { success = false, message = $"Error updating ad: {ex.Message}" });
            }
        }

        /// <summary>
        /// Delete ad by ID
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteAd(int id)
        {
            try
            {
                var result = await _adService.DeleteAdAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = "Ad not found" });

                return Ok(new { success = true, message = "Ad deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error deleting ad: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get ads by user ID with pagination
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetAdsByUser(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                    return BadRequest(new { success = false, message = "Page and PageSize must be greater than 0" });

                var result = await _adService.GetAdsByUserAsync(userId, page, pageSize);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get ads by request ID with pagination
        /// </summary>
        [HttpGet("request/{requestId}")]
        public async Task<IActionResult> GetAdsByRequest(int requestId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                    return BadRequest(new { success = false, message = "Page and PageSize must be greater than 0" });

                var result = await _adService.GetAdsByRequestAsync(requestId, page, pageSize);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get ads by status with pagination
        /// </summary>
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetAdsByStatus(string status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                    return BadRequest(new { success = false, message = "Page and PageSize must be greater than 0" });

                var result = await _adService.GetAdsByStatusAsync(status, page, pageSize);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Search ads with filters and pagination
        /// </summary>
        [HttpPost("search")]
        public async Task<IActionResult> SearchAds([FromBody] AdSearchRequest request)
        {
            try
            {
                if (request.Page < 1 || request.PageSize < 1)
                    return BadRequest(new { success = false, message = "Page and PageSize must be greater than 0" });

                var result = await _adService.SearchAdsAsync(request);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Update ad status
        /// </summary>
        [HttpPatch("{id}/status")]
        [Authorize]
        public async Task<IActionResult> UpdateAdStatus(int id, [FromBody] UpdateAdStatusRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                var result = await _adService.UpdateAdStatusAsync(id, request);
                if (!result)
                    return NotFound(new { success = false, message = "Ad not found or status update failed" });

                return Ok(new { success = true, message = "Ad status updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error updating ad status: {ex.Message}" });
            }
        }

        /// <summary>
        /// Increment ad views (tracking endpoint)
        /// </summary>
        [HttpPost("{id}/view")]
        public async Task<IActionResult> IncrementAdViews(int id)
        {
            try
            {
                var result = await _adService.IncrementAdViewsAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = "Ad not found" });

                return Ok(new { success = true, message = "Ad view recorded" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error recording ad view: {ex.Message}" });
            }
        }

        /// <summary>
        /// Increment ad clicks (tracking endpoint)
        /// </summary>
        [HttpPost("{id}/click")]
        public async Task<IActionResult> IncrementAdClicks(int id)
        {
            try
            {
                var result = await _adService.IncrementAdClicksAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = "Ad not found" });

                return Ok(new { success = true, message = "Ad click recorded" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error recording ad click: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get active ads with pagination
        /// </summary>
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveAds([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                    return BadRequest(new { success = false, message = "Page and PageSize must be greater than 0" });

                var result = await _adService.GetActiveAdsAsync(page, pageSize);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get expired ads with pagination
        /// </summary>
        [HttpGet("expired")]
        public async Task<IActionResult> GetExpiredAds([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                    return BadRequest(new { success = false, message = "Page and PageSize must be greater than 0" });

                var result = await _adService.GetExpiredAdsAsync(page, pageSize);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }
    }
}
