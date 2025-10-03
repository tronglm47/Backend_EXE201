using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.RequestsResponses.Amenity;
using Services.RequestsResponses.Location;
using VLivingAPI.Authorization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace VLivingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly ILogger<LocationController> _logger;
        private readonly ILocationService _service;
        public LocationController(ILocationService service, ILogger<LocationController> logger)
        {
            _service = service;
            _logger = logger;
        }
        // GET: api/<LocationController>
        [Produces("application/json", "application/xml")]
        [HttpGet]
        [AllowAnonymous] // Anyone can view locations for reference
        public async Task<IActionResult> Get([FromQuery] LocationQueryParameters queryParams)
        {
            try
            {
                _logger.LogInformation("Location list requested with parameters: Page={Page}, PageSize={PageSize}, Search={Search}", 
                    queryParams.Page, queryParams.PageSize, queryParams.Search ?? "None");
                
                var result = await _service.GetAllAsync(queryParams);
                
                _logger.LogInformation("Location list returned {Count} items out of {Total} total", 
                    result.Items.Count, result.TotalItems);
                
                return Ok(new
                {
                    success = true,
                    data = result.Items,
                    pagination = new
                    {
                        currentPage = result.CurrentPage,
                        totalPages = result.TotalPages,
                        totalItems = result.TotalItems,
                        pageSize = queryParams.PageSize
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving location list with parameters: Page={Page}, PageSize={PageSize}", 
                    queryParams.Page, queryParams.PageSize);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        // GET api/<LocationController>/5
        [Produces("application/json", "application/xml")]
        [HttpGet("{id}")]
        [AllowAnonymous] // Anyone can view specific location for reference
        public async Task<IActionResult> Get(int id, [FromQuery] string? select = null)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid location ID {Id} requested", id);
                    return BadRequest(new { success = false, message = "Invalid location ID" });
                }

                _logger.LogInformation("Location detail requested for ID: {Id}, Selected fields: {Select}", 
                    id, select ?? "All");

                var selectedFields = new List<string>();
                if (!string.IsNullOrWhiteSpace(select))
                {
                    selectedFields = select.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                          .Select(x => x.Trim().ToLowerInvariant())
                                          .ToList();
                }

                var result = await _service.GetById(id, selectedFields);
                if (result == null)
                {
                    _logger.LogWarning("Location not found for ID: {Id}", id);
                    return NotFound(new { success = false, message = "Location not found" });
                }

                _logger.LogInformation("Location detail returned for ID: {Id}", id);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving location detail for ID: {Id}", id);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        // POST api/<LocationController>
        [Consumes("application/json", "application/xml")]
        [Produces("application/json", "application/xml")]
        [HttpPost]
        [BusinessAuthorize(BusinessRole.MasterDataManagement)] // Only Admin/Manager can create
        public async Task<IActionResult> Post([FromBody] LocationRequest.LocationCreate request)
        {
            try
            {
                // Additional user validation
                var currentUser = User.Identity?.Name;
                if (string.IsNullOrEmpty(currentUser))
                {
                    _logger.LogWarning("Unauthorized location creation attempt from IP: {IP}", HttpContext.Connection.RemoteIpAddress);
                    return Unauthorized(new { success = false, message = "Authentication required" });
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Location creation validation failed for user: {User}", currentUser);
                    return BadRequest(new
                    {
                        success = false,
                        message = "Validation failed",
                        errors = ModelState.Where(x => x.Value != null && x.Value.Errors.Any())
                                         .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                                         .ToList()
                    });
                }

                var locationId = await _service.Create(request);
                if (locationId == 0)
                {
                    _logger.LogWarning("Failed to create location for user: {User}", currentUser);
                    return BadRequest(new { success = false, message = "Failed to create location" });
                }

                _logger.LogInformation("Location created successfully by user: {User}, LocationId: {LocationId}", currentUser, locationId);
                return CreatedAtAction(nameof(Get), new { id = locationId }, new
                {
                    success = true,
                    message = "Location created successfully",
                    locationId = locationId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating location for user: {User}", User.Identity?.Name);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        // PUT api/<LocationController>/5
        [Consumes("application/json", "application/xml")]
        [Produces("application/json", "application/xml")]
        [HttpPut("{id}")]
        [BusinessAuthorize(BusinessRole.MasterDataManagement)] // Only Admin/Manager can update
        public async Task<IActionResult> Put(int id, [FromBody] LocationRequest.LocationUpdate request)
        {
            try
            {
                // Additional user validation
                var currentUser = User.Identity?.Name;
                if (string.IsNullOrEmpty(currentUser))
                {
                    _logger.LogWarning("Unauthorized location update attempt from IP: {IP}", HttpContext.Connection.RemoteIpAddress);
                    return Unauthorized(new { success = false, message = "Authentication required" });
                }

                if (id <= 0)
                {
                    _logger.LogWarning("Invalid location ID {Id} for update by user: {User}", id, currentUser);
                    return BadRequest(new { success = false, message = "Invalid location ID" });
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Location update validation failed for user: {User}, LocationId: {Id}", currentUser, id);
                    return BadRequest(new
                    {
                        success = false,
                        message = "Validation failed",
                        errors = ModelState.Where(x => x.Value != null && x.Value.Errors.Any())
                                          .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                                          .ToList()
                    });
                }

                var result = await _service.Update(request, id);
                if (!result)
                {
                    _logger.LogWarning("Location not found or failed to update. LocationId: {Id}, User: {User}", id, currentUser);
                    return NotFound(new { success = false, message = "Location not found or failed to update" });
                }

                _logger.LogInformation("Location updated successfully by user: {User}, LocationId: {Id}", currentUser, id);
                return Ok(new { success = true, message = "Location updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating location with ID {LocationId} by user: {User}", id, User.Identity?.Name);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        // DELETE api/<LocationController>/5
        [Produces("application/json", "application/xml")]
        [HttpDelete("{id}")]
        [BusinessAuthorize(BusinessRole.MasterDataManagement)] // Only Admin/Manager can delete
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Additional user validation
                var currentUser = User.Identity?.Name;
                if (string.IsNullOrEmpty(currentUser))
                {
                    _logger.LogWarning("Unauthorized location deletion attempt from IP: {IP}", HttpContext.Connection.RemoteIpAddress);
                    return Unauthorized(new { success = false, message = "Authentication required" });
                }

                if (id <= 0)
                {
                    _logger.LogWarning("Invalid location ID {Id} for deletion by user: {User}", id, currentUser);
                    return BadRequest(new { success = false, message = "Invalid location ID" });
                }

                // First check if location exists
                var location = await _service.GetById(id, new List<string>());
                if (location == null)
                {
                    _logger.LogWarning("Location not found for deletion. LocationId: {Id}, User: {User}", id, currentUser);
                    return NotFound(new { success = false, message = "Location not found" });
                }

                // Delete using ID
                var result = await _service.Delete(id);

                if (!result)
                {
                    _logger.LogWarning("Failed to delete location. LocationId: {Id}, User: {User}", id, currentUser);
                    return BadRequest(new { success = false, message = "Failed to delete location" });
                }

                _logger.LogInformation("Location deleted successfully by user: {User}, LocationId: {Id}", currentUser, id);
                return Ok(new { success = true, message = "Location deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Location with ID {LocationId} by user: {User}", id, User.Identity?.Name);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }
    }
}
