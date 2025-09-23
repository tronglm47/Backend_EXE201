using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Services.Interfaces;
using Services.RequestsResponses.Location;
using Repositories.Constants;

namespace VLivingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly ILocationService _locationService;

        public LocationController(ILocationService locationService)
        {
            _locationService = locationService;
        }

        /// <summary>
        /// Get all locations with pagination
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        /// <returns>Paginated list of locations</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllLocations([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                    return BadRequest(new { success = false, message = "Page and PageSize must be greater than 0" });

                var result = await _locationService.GetAllLocationsPaginatedAsync(page, pageSize);
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
        /// Get location by ID with detailed information
        /// </summary>
        /// <param name="id">Location ID</param>
        /// <returns>Location details</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLocation(int id)
        {
            try
            {
                var location = await _locationService.GetLocationDetailsByIdAsync(id);
                if (location == null)
                    return NotFound(new { success = false, message = "Location not found" });

                return Ok(new { success = true, data = location });
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
        /// Create new location
        /// </summary>
        /// <param name="request">Location creation request</param>
        /// <returns>Created location</returns>
        [HttpPost]
        [Authorize(Roles = UserRoleConstants.AdminRole)] // Chỉ admin mới được tạo location
        public async Task<IActionResult> CreateLocation([FromBody] CreateLocationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                var createdLocation = await _locationService.CreateLocationAsync(request);
                return CreatedAtAction(nameof(GetLocation), new { id = createdLocation.LocationId }, 
                    new { success = true, data = createdLocation });
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
                return StatusCode(500, new { success = false, message = $"Error creating location: {ex.Message}" });
            }
        }

        /// <summary>
        /// Update existing location
        /// </summary>
        /// <param name="id">Location ID</param>
        /// <param name="request">Location update request</param>
        /// <returns>Updated location</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = UserRoleConstants.AdminRole)] // Chỉ admin mới được cập nhật location
        public async Task<IActionResult> UpdateLocation(int id, [FromBody] UpdateLocationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                var updatedLocation = await _locationService.UpdateLocationAsync(id, request);
                return Ok(new { success = true, data = updatedLocation });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error updating location: {ex.Message}" });
            }
        }

        /// <summary>
        /// Delete location
        /// </summary>
        /// <param name="id">Location ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = UserRoleConstants.AdminRole)] // Chỉ admin mới được xóa location
        public async Task<IActionResult> DeleteLocation(int id)
        {
            try
            {
                var deleted = await _locationService.DeleteLocationAsync(id);
                if (!deleted)
                    return NotFound(new { success = false, message = "Location not found" });

                return Ok(new { success = true, message = "Location deleted successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error deleting location: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get root locations (locations without parent)
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        /// <returns>Paginated list of root locations</returns>
        [HttpGet("root")]
        public async Task<IActionResult> GetRootLocations([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                    return BadRequest(new { success = false, message = "Page and PageSize must be greater than 0" });

                var result = await _locationService.GetRootLocationsAsync(page, pageSize);
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
        /// Get child locations of a parent location
        /// </summary>
        /// <param name="parentId">Parent location ID</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        /// <returns>Paginated list of child locations</returns>
        [HttpGet("parent/{parentId}/children")]
        public async Task<IActionResult> GetChildLocations(int parentId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                    return BadRequest(new { success = false, message = "Page and PageSize must be greater than 0" });

                var result = await _locationService.GetChildLocationsByParentAsync(parentId, page, pageSize);
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
        /// Get complete hierarchy of a location (location and all its descendants)
        /// </summary>
        /// <param name="id">Location ID</param>
        /// <returns>Complete location hierarchy</returns>
        [HttpGet("{id}/hierarchy")]
        public async Task<IActionResult> GetLocationHierarchy(int id)
        {
            try
            {
                var hierarchy = await _locationService.GetLocationHierarchyAsync(id);
                return Ok(new { success = true, data = hierarchy });
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
        /// Get path from location to root (breadcrumb trail)
        /// </summary>
        /// <param name="id">Location ID</param>
        /// <returns>Path from root to location</returns>
        [HttpGet("{id}/path")]
        public async Task<IActionResult> GetLocationPath(int id)
        {
            try
            {
                var path = await _locationService.GetLocationPathToRootAsync(id);
                return Ok(new { success = true, data = path });
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
        /// Search locations with advanced filters
        /// </summary>
        /// <param name="request">Search criteria</param>
        /// <returns>Filtered and paginated locations</returns>
        [HttpPost("search")]
        public async Task<IActionResult> SearchLocations([FromBody] LocationSearchRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { success = false, message = "Search request is required" });

                var result = await _locationService.SearchLocationsAsync(request);
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
        /// Search locations by name
        /// </summary>
        /// <param name="name">Location name to search</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        /// <returns>Locations matching the name</returns>
        [HttpGet("search")]
        public async Task<IActionResult> SearchLocationsByName([FromQuery] string name, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return BadRequest(new { success = false, message = "Search name is required" });

                if (page < 1 || pageSize < 1)
                    return BadRequest(new { success = false, message = "Page and PageSize must be greater than 0" });

                var result = await _locationService.GetLocationsByNameAsync(name, page, pageSize);
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
        /// Check if location can be deleted (has no dependencies)
        /// </summary>
        /// <param name="id">Location ID</param>
        /// <returns>Whether location can be deleted</returns>
        [HttpGet("{id}/can-delete")]
        public async Task<IActionResult> CanDeleteLocation(int id)
        {
            try
            {
                var canDelete = await _locationService.CanDeleteLocationAsync(id);
                var hasChildren = await _locationService.HasChildLocationsAsync(id);
                var hasProperties = await _locationService.HasPropertiesAsync(id);
                var hasActivities = await _locationService.HasActivitiesAsync(id);

                return Ok(new 
                { 
                    success = true, 
                    data = new 
                    {
                        canDelete = canDelete,
                        hasChildren = hasChildren,
                        hasProperties = hasProperties,
                        hasActivities = hasActivities,
                        reasons = new List<string>()
                            .Concat(hasChildren ? new[] { "Has child locations" } : new string[0])
                            .Concat(hasProperties ? new[] { "Has properties" } : new string[0])
                            .Concat(hasActivities ? new[] { "Has activities" } : new string[0])
                            .ToList()
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }
    }
}
