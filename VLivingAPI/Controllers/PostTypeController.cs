using Microsoft.AspNetCore.Mvc;
using Services;
using Services.RequestsResponses.Amenity;
using Services.RequestsResponses.PostType;
using Microsoft.AspNetCore.Authorization;
using VLivingAPI.Authorization;
using Repositories.Constants;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace VLivingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostTypeController : ControllerBase
    {
        private readonly IPostTypeService _service;
        private readonly ILogger _logger;
        public PostTypeController(IPostTypeService service, ILogger logger)
        {
            _service = service;
            _logger = logger;
        }
        // GET: api/<PostTypeController>
        [Produces("application/json", "application/xml")]
        [HttpGet]
        [AllowAnonymous] // Anyone can view post types for reference
        public async Task<IActionResult> Get([FromQuery] PostTypeQueryParameters queryParams)
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


        // GET api/<PostTypeController>/5
        [Produces("application/json", "application/xml")]
        [HttpGet("{id}")]
        [AllowAnonymous] // Anyone can view specific post type for reference
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
                    return NotFound(new { success = false, message = "PostType not found" });
                }

                return Ok(new { success = true, data = result });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }


        // POST api/<PostTypeController>
        [Consumes("application/json", "application/xml")]
        [Produces("application/json", "application/xml")]
        [HttpPost]
        [BusinessAuthorize(BusinessRole.MasterDataManagement)] // Only Admin/Manager can create
        public async Task<IActionResult> Post([FromBody] PostTypeRequest.PostTypeCreate request)
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

                var postTypeId = await _service.Create(request);
                if (postTypeId == 0)
                {
                    return BadRequest(new { success = false, message = "Failed to create post type" });
                }

                return CreatedAtAction(nameof(Get), new { id = postTypeId }, new
                {
                    success = true,
                    message = "Post Type created successfully",
                    postTypeId = postTypeId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post type");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }


        // PUT api/<PostTypeController>/5
        [Consumes("application/json", "application/xml")]
        [Produces("application/json", "application/xml")]
        [HttpPut("{id}")]
        [BusinessAuthorize(BusinessRole.MasterDataManagement)] // Only Admin/Manager can update
        public async Task<IActionResult> Put(int id, [FromBody] PostTypeRequest.PostTypeUpdate request)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { success = false, message = "Invalid postType ID" });
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
                    return NotFound(new { success = false, message = "Post type not found or failed to update" });
                }

                return Ok(new { success = true, message = "Post type updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating post type with ID {AmenityId}", id);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }


        // DELETE api/<PostTypeController>/5
        [Produces("application/json", "application/xml")]
        [HttpDelete("{id}")]
        [BusinessAuthorize(BusinessRole.MasterDataManagement)] // Only Admin/Manager can delete
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { success = false, message = "Invalid post type ID" });
                }

                // First check if category exists
                var postType = await _service.GetById(id, new List<string>());
                if (postType == null)
                {
                    return NotFound(new { success = false, message = "Post type not found" });
                }

                // Delete using ID
                var result = await _service.Delete(id);

                if (!result)
                {
                    return BadRequest(new { success = false, message = "Failed to delete post type" });
                }

                return Ok(new { success = true, message = "Post type deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting post type with ID {AmenityId}", id);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }
    }
}
