using Microsoft.AspNetCore.Mvc;
using Services;
using Services.RequestsResponses.PostAmenity;
using Microsoft.AspNetCore.Authorization;
using VLivingAPI.Authorization;
using Repositories.Constants;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace VLivingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostAmenityController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IPostAmenityService _service;
        public PostAmenityController(ILogger logger, IPostAmenityService service)
        {
            _logger = logger;
            _service = service;
        }
        // GET: api/<PostAmenityController>
        [Produces("application/json", "application/xml")]
        [HttpGet]
        [Authorize(Roles = UserRoleConstants.AllUsersExceptGuest)] // All authenticated users can view
        public async Task<IActionResult> Get([FromQuery] PostAmenityQueryParameters queryParams)
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

        // GET api/<PostAmenityController>/5
        [Produces("application/json", "application/xml")]
        [HttpGet("{id}")]
        [Authorize(Roles = UserRoleConstants.AllUsersExceptGuest)] // All authenticated users can view
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
                    return NotFound(new { success = false, message = "Post amenity not found" });
                }

                return Ok(new { success = true, data = result });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        // POST api/<PostAmenityController>
        [Consumes("application/json", "application/xml")]
        [Produces("application/json", "application/xml")]
        [HttpPost]
        [BusinessAuthorize(BusinessRole.PostManagement)] // Users who can manage posts
        public async Task<IActionResult> Post([FromBody] PostAmenityRequest.CreatePostAmenity request)
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

                var postAmenityId = await _service.Create(request);
                if (postAmenityId == 0)
                {
                    return BadRequest(new { success = false, message = "Failed to create post amenity" });
                }

                return CreatedAtAction(nameof(Get), new { id = postAmenityId }, new
                {
                    success = true,
                    message = "Post amenity created successfully",
                    postAmenityId = postAmenityId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post amenity");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        // PUT api/<PostAmenityController>/5
        [Consumes("application/json", "application/xml")]
        [Produces("application/json", "application/xml")]
        [HttpPut("{id}")]
        [BusinessAuthorize(BusinessRole.PostManagement)] // Users who can manage posts
        public async Task<IActionResult> Put(int id, [FromBody] PostAmenityRequest.UpdatePostAmenity request)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { success = false, message = "Invalid postAmenity ID" });
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
                    return NotFound(new { success = false, message = "Post amenity not found or failed to update" });
                }

                return Ok(new { success = true, message = "Post amenity updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating postAmenity with ID {PostAmenity}", id);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        // DELETE api/<PostAmenityController>/5
        [Produces("application/json", "application/xml")]
        [HttpDelete("{id}")]
        [BusinessAuthorize(BusinessRole.PostManagement)] // Users who can manage posts
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { success = false, message = "Invalid post amenity ID" });
                }

                // First check if category exists
                var postAmenity = await _service.GetById(id, new List<string>());
                if (postAmenity == null)
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
