using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.RequestsResponses.Amenity;
using Services.RequestsResponses.PropertyForm;
using VLivingAPI.Repositories.Data.Models;
using Microsoft.AspNetCore.Authorization;
using VLivingAPI.Authorization;
using Repositories.Constants;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace VLivingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AmenityController : ControllerBase
    {
        private readonly IAmenityService _service;
        private readonly ILogger _logger;
        public AmenityController(IAmenityService Service, ILogger logger)
        {
            _service = Service;
            _logger = logger;
        }
        // GET: api/<AmenityController>
        [Produces("application/json", "application/xml")]
        [HttpGet]
        [AllowAnonymous] // Anyone can view amenities for reference
        public async Task<IActionResult> Get([FromQuery] AmenityQueryParameters queryParams)
        {
            try
            {
                var result = await _service.GetAllAsync(queryParams);
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
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        // GET api/<AmenityController>/5
        [Produces("application/json", "application/xml")]
        [HttpGet("{id}")]
        [AllowAnonymous] // Anyone can view specific amenity for reference
        public async Task<IActionResult> Get(int id, [FromQuery] string? select = null)
        {
            try
            {
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
                    return NotFound(new { success = false, message = "Post not found" });
                }

                return Ok(new { success = true, data = result });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        // POST api/<AmenityController>
        [Consumes("application/json", "application/xml")]
        [Produces("application/json", "application/xml")]
        [HttpPost]
        [BusinessAuthorize(BusinessRole.MasterDataManagement)] // Only Admin/Manager can create
        public async Task<IActionResult> Post([FromBody] AmenityRequest.CreateAmenity request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Validation failed",
                        errors = ModelState.Where(x => x.Value != null && x.Value.Errors.Any())
                                         .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                                         .ToList()
                    });
                }

                var amenityId = await _service.Create(request);
                if (amenityId == 0)
                {
                    return BadRequest(new { success = false, message = "Failed to create amenity" });
                }

                return CreatedAtAction(nameof(Get), new { id = amenityId }, new
                {
                    success = true,
                    message = "Amenity created successfully",
                    amenityId = amenityId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating amenity");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        // PUT api/<AmenityController>/5
        [Consumes("application/json", "application/xml")]
        [Produces("application/json", "application/xml")]
        [HttpPut("{id}")]
        [BusinessAuthorize(BusinessRole.MasterDataManagement)] // Only Admin/Manager can update
        public async Task<IActionResult> Put(int id, [FromBody] AmenityRequest.UpdateAmenity request)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { success = false, message = "Invalid amenity ID" });
                }

                if (!ModelState.IsValid)
                {
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
                    return NotFound(new { success = false, message = "amenity not found or failed to update" });
                }

                return Ok(new { success = true, message = "amenity updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating amenity with ID {AmenityId}", id);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        // DELETE api/<AmenityController>/5
        [Produces("application/json", "application/xml")]
        [HttpDelete("{id}")]
        [BusinessAuthorize(BusinessRole.MasterDataManagement)] // Only Admin/Manager can delete
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { success = false, message = "Invalid amenity ID" });
                }

                // First check if category exists
                var propertyType = await _service.GetById(id, new List<string>());
                if (propertyType == null)
                {
                    return NotFound(new { success = false, message = "Amenity not found" });
                }

                // Delete using ID
                var result = await _service.Delete(id);

                if (!result)
                {
                    return BadRequest(new { success = false, message = "Failed to delete amenity" });
                }

                return Ok(new { success = true, message = "Amenity deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting amenity with ID {AmenityId}", id);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }
    }
}
