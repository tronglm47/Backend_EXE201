using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.RequestsResponses.Post;
using Services.RequestsResponses.PropertyType;
using VLivingAPI.Repositories.Data.Models;
using Microsoft.AspNetCore.Authorization;
using VLivingAPI.Authorization;
using Repositories.Constants;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace VLivingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PropertyTypeController : ControllerBase
    {
        private readonly IPropertyTypeService _Service;
        private readonly ILogger<PropertyTypeController> _logger;
        public PropertyTypeController(IPropertyTypeService Service, ILogger<PropertyTypeController> logger)
        {
            _Service = Service;
            _logger = logger;
        }
        // GET: api/<PropertyTypeController>
        [Produces("application/json", "application/xml")]
        [HttpGet]
        [AllowAnonymous] // Anyone can view property types for reference
        public async Task<IActionResult> Get([FromQuery] PropertyTypeQueryParameters queryParams)
        {
            try
            {
                var result = await _Service.GetAllAsync(queryParams);
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

        // GET api/<PropertyTypeController>/5
        [Produces("application/json", "application/xml")]
        [HttpGet("{id}")]
        [AllowAnonymous] // Anyone can view specific property type for reference
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

                var result = await _Service.GetById(id, selectedFields);
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

        // POST api/<PropertyTypeController>
        [Consumes("application/json", "application/xml")]
        [Produces("application/json", "application/xml")]
        [HttpPost]
        [BusinessAuthorize(BusinessRole.MasterDataManagement)] // Only Admin/Manager can create
        public async Task<IActionResult> Post([FromBody] PropertyTypeRequest.CreateRequest request)
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

                var propertityId = await _Service.Create(request);
                if (propertityId == 0)
                {
                    return BadRequest(new { success = false, message = "Failed to create propertity type" });
                }

                return CreatedAtAction(nameof(Get), new { id = propertityId }, new
                {
                    success = true,
                    message = "Propertity type created successfully",
                    propertityId = propertityId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating propertity type");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        // PUT api/<PropertyTypeController>/5
        [Consumes("application/json", "application/xml")]
        [Produces("application/json", "application/xml")]
        [HttpPut("{id}")]
        [BusinessAuthorize(BusinessRole.MasterDataManagement)] // Only Admin/Manager can update
        public async Task<IActionResult> Put(int id, [FromBody] PropertyTypeRequest.UpdateRequest request)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { success = false, message = "Invalid propertity type ID" });
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

                // Ensure the ID matches
                var result = await _Service.Update(request, id);
                if (!result)
                {
                    return NotFound(new { success = false, message = "Propertity type not found or failed to update" });
                }

                return Ok(new { success = true, message = "Propertity type updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating propertity type with ID {CategoryId}", id);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        // DELETE api/<PropertyTypeController>/5
        [Produces("application/json", "application/xml")]
        [HttpDelete("{id}")]
        [BusinessAuthorize(BusinessRole.MasterDataManagement)] // Only Admin/Manager can delete
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { success = false, message = "Invalid propertity type ID" });
                }

                // First check if category exists
                var propertyType = await _Service.GetById(id, new List<string>());
                if (propertyType == null)
                {
                    return NotFound(new { success = false, message = "Propertity type not found" });
                }

                // Delete using ID
                var result = await _Service.Delete(id);

                if (!result)
                {
                    return BadRequest(new { success = false, message = "Failed to delete propertity type" });
                }

                return Ok(new { success = true, message = "Propertity type deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting propertity type with ID {CategoryId}", id);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }
    }
}
