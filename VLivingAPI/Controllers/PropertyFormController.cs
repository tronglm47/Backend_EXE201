using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Services;
using Services.RequestsResponses.PropertyForm;
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
    public class PropertyFormController : ControllerBase
    {
        private readonly IPropertyFormService _Service;
        private readonly ILogger<PropertyFormController> _logger;
        public PropertyFormController(IPropertyFormService Service, ILogger<PropertyFormController> logger)
        {
            _Service = Service;
            _logger = logger;
        }

        // GET: api/<PropertyFormController>
        [Produces("application/json", "application/xml")]
        [HttpGet]
        [AllowAnonymous] // Anyone can view property forms for reference
        public async Task<IActionResult> Get([FromQuery] PropertyFormQueryParameters queryParams)
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

        // GET api/<PropertyFormController>/5
        [Produces("application/json", "application/xml")]
        [HttpGet("{id}")]
        [AllowAnonymous] // Anyone can view specific property form for reference
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

        // POST api/<PropertyFormController>
        [Consumes("application/json", "application/xml")]
        [Produces("application/json", "application/xml")]
        [HttpPost]
        [BusinessAuthorize(BusinessRole.MasterDataManagement)] // Only Admin/Manager can create
        public async Task<IActionResult> Post([FromBody] PropertyFormRequest.CreatePropertyForm request)
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

                var propertityFormId = await _Service.Create(request);
                if (propertityFormId == 0)
                {
                    return BadRequest(new { success = false, message = "Failed to create propertity form" });
                }

                return CreatedAtAction(nameof(Get), new { id = propertityFormId }, new
                {
                    success = true,
                    message = "Propertity form created successfully",
                    propertityFormId = propertityFormId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating propertity form");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        // PUT api/<PropertyFormController>/5
        [Consumes("application/json", "application/xml")]
        [Produces("application/json", "application/xml")]
        [HttpPut("{id}")]
        [BusinessAuthorize(BusinessRole.MasterDataManagement)] // Only Admin/Manager can update
        public async Task<IActionResult> Put(int id, [FromBody] PropertyFormRequest.UpdatePropertyForm request)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { success = false, message = "Invalid propertity form ID" });
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
                    return NotFound(new { success = false, message = "Propertity form not found or failed to update" });
                }

                return Ok(new { success = true, message = "Propertity form updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating propertity form with ID {PropertityId}", id);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        // DELETE api/<PropertyFormController>/5
        [Produces("application/json", "application/xml")]
        [HttpDelete("{id}")]
        [BusinessAuthorize(BusinessRole.MasterDataManagement)] // Only Admin/Manager can delete
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { success = false, message = "Invalid propertity form ID" });
                }

                // First check if category exists
                var propertyForm = await _Service.GetById(id, new List<string>());
                if (propertyForm == null)
                {
                    return NotFound(new { success = false, message = "Propertity form not found" });
                }

                // Delete using ID
                var result = await _Service.Delete(id);

                if (!result)
                {
                    return BadRequest(new { success = false, message = "Failed to delete propertity form" });
                }

                return Ok(new { success = true, message = "Propertity form deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting propertity form with ID {PropertityFormId}", id);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }
    }
}
