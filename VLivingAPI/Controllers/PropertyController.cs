using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Services.Interfaces;
using Services.RequestsResponses.Property;
using Repositories.Constants;

namespace VLivingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PropertyController : ControllerBase
    {
        private readonly IPropertyService _propertyService;

        public PropertyController(IPropertyService propertyService)
        {
            _propertyService = propertyService;
        }

        /// <summary>
        /// Get all properties with pagination
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllProperties([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                    return BadRequest(new { success = false, message = "Page and PageSize must be greater than 0" });

                var result = await _propertyService.GetAllPropertiesPaginatedAsync(page, pageSize);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get property by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProperty(int id)
        {
            try
            {
                var property = await _propertyService.GetPropertyByIdAsync(id);
                if (property == null)
                    return NotFound(new { success = false, message = "Property not found" });

                return Ok(new { success = true, data = property });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Create new property
        /// </summary>
        [HttpPost]
        [Authorize] // Cần đăng nhập để tạo property
        public async Task<IActionResult> CreateProperty([FromBody] CreatePropertyRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                var createdProperty = await _propertyService.CreatePropertyAsync(request);
                return CreatedAtAction(nameof(GetProperty), new { id = createdProperty.PropertyId }, 
                    new { success = true, data = createdProperty });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error creating property: {ex.Message}" });
            }
        }

        /// <summary>
        /// Update existing property
        /// </summary>
        [HttpPut("{id}")]
        [Authorize] // Cần đăng nhập để cập nhật property
        public async Task<IActionResult> UpdateProperty(int id, [FromBody] UpdatePropertyRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                var updatedProperty = await _propertyService.UpdatePropertyAsync(id, request);
                return Ok(new { success = true, data = updatedProperty, message = "Property updated successfully" });
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
                return StatusCode(500, new { success = false, message = $"Error updating property: {ex.Message}" });
            }
        }

        /// <summary>
        /// Delete property
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize] // Cần đăng nhập để xóa property
        public async Task<IActionResult> DeleteProperty(int id)
        {
            try
            {
                var result = await _propertyService.DeletePropertyAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = "Property not found" });

                return Ok(new { success = true, message = "Property deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error deleting property: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get properties by owner with pagination
        /// </summary>
        [HttpGet("owner/{ownerId}")]
        public async Task<IActionResult> GetPropertiesByOwner(int ownerId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                    return BadRequest(new { success = false, message = "Page and PageSize must be greater than 0" });

                var result = await _propertyService.GetPropertiesByOwnerAsync(ownerId, page, pageSize);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get properties by location with pagination
        /// </summary>
        [HttpGet("location/{locationId}")]
        public async Task<IActionResult> GetPropertiesByLocation(int locationId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                    return BadRequest(new { success = false, message = "Page and PageSize must be greater than 0" });

                var result = await _propertyService.GetPropertiesByLocationAsync(locationId, page, pageSize);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get properties by type with pagination
        /// </summary>
        [HttpGet("type/{type}")]
        public async Task<IActionResult> GetPropertiesByType(string type, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                    return BadRequest(new { success = false, message = "Page and PageSize must be greater than 0" });

                var result = await _propertyService.GetPropertiesByTypeAsync(type, page, pageSize);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Search properties with filters and pagination
        /// </summary>
        [HttpPost("search")]
        public async Task<IActionResult> SearchProperties([FromBody] PropertySearchRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                if (request.Page < 1 || request.PageSize < 1)
                    return BadRequest(new { success = false, message = "Page and PageSize must be greater than 0" });

                var result = await _propertyService.SearchPropertiesAsync(request);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Search properties with GET method (alternative)
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchPropertiesGet(
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] string? type = null,
            [FromQuery] int? locationId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                    return BadRequest(new { success = false, message = "Page and PageSize must be greater than 0" });

                var request = new PropertySearchRequest
                {
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    Type = type,
                    LocationId = locationId,
                    Page = page,
                    PageSize = pageSize
                };

                var result = await _propertyService.SearchPropertiesAsync(request);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Update property status
        /// </summary>
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdatePropertyStatus(int id, [FromBody] UpdatePropertyStatusRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                var result = await _propertyService.UpdatePropertyStatusAsync(id, request);
                if (!result)
                    return NotFound(new { success = false, message = "Property not found" });

                return Ok(new { success = true, message = "Property status updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error updating status: {ex.Message}" });
            }
        }

        /// <summary>
        /// Create property with associated post (complex operation with transaction)
        /// </summary>
        [HttpPost("with-post")]
        public async Task<IActionResult> CreatePropertyWithPost([FromBody] CreatePropertyWithPostRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                var createdProperty = await _propertyService.CreatePropertyWithPostAsync(request);
                return CreatedAtAction(nameof(GetProperty), new { id = createdProperty.PropertyId },
                    new { success = true, data = createdProperty, message = "Property and post created successfully" });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error creating property with post: {ex.Message}" });
            }
        }
    }
}
