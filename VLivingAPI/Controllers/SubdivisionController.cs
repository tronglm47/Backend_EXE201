using Microsoft.AspNetCore.Mvc;
using Services;
using Services.RequestsResponses;
using Services.RequestsResponses.Subdivision;

namespace VLivingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubdivisionController : ControllerBase
    {
        private readonly ISubdivisionService _subdivisionService;
        private readonly ILogger<SubdivisionController> _logger;

        public SubdivisionController(ISubdivisionService subdivisionService, ILogger<SubdivisionController> logger)
        {
            _subdivisionService = subdivisionService;
            _logger = logger;
        }

        /// <summary>
        /// Get all subdivisions with pagination, sorting, and dynamic search
        /// </summary>
        /// <param name="queryParams">Query parameters (Page, PageSize, SearchField, Search, SortBy, IsDescending, Select)</param>
        /// <returns>Paginated list of subdivisions</returns>
        /// <response code="200">Returns the paginated list of subdivisions</response>
        /// <response code="500">Internal server error</response>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll([FromQuery] SubdivisionQuery queryParams)
        {
            try
            {
                var result = await _subdivisionService.GetAllAsync(queryParams);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subdivisions");
                return StatusCode(500, new { message = "An error occurred while retrieving subdivisions", error = ex.Message });
            }
        }

        /// <summary>
        /// Get a subdivision by ID
        /// </summary>
        /// <param name="id">Subdivision ID</param>
        /// <param name="select">Optional: Comma-separated list of fields to select</param>
        /// <returns>Subdivision details</returns>
        /// <response code="200">Returns the subdivision</response>
        /// <response code="404">Subdivision not found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id, [FromQuery] string? select = null)
        {
            try
            {
                var selectedFields = string.IsNullOrWhiteSpace(select)
                    ? new List<string>()
                    : select.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => x.Trim().ToLowerInvariant())
                            .ToList();

                var result = await _subdivisionService.GetByIdAsync(id, selectedFields);
                
                if (result == null)
                {
                    return NotFound(new { message = $"Subdivision with ID {id} not found" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subdivision with ID {SubdivisionId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the subdivision", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new subdivision
        /// </summary>
        /// <param name="request">Subdivision creation data</param>
        /// <returns>Created subdivision ID</returns>
        /// <response code="201">Subdivision created successfully</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="500">Internal server error</response>
        [HttpPost]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] SubdivisionRequest.SubdivisionCreate request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { message = "Request body cannot be null" });
                }

                if (string.IsNullOrWhiteSpace(request.name))
                {
                    return BadRequest(new { message = "Name is required" });
                }

                var subdivisionId = await _subdivisionService.CreateAsync(request);

                if (subdivisionId > 0)
                {
                    return CreatedAtAction(
                        nameof(GetById),
                        new { id = subdivisionId },
                        new { id = subdivisionId, message = "Subdivision created successfully" }
                    );
                }

                return StatusCode(500, new { message = "Failed to create subdivision" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating subdivision");
                return StatusCode(500, new { message = "An error occurred while creating the subdivision", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing subdivision
        /// </summary>
        /// <param name="id">Subdivision ID</param>
        /// <param name="request">Subdivision update data</param>
        /// <returns>Update result</returns>
        /// <response code="200">Subdivision updated successfully</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="404">Subdivision not found</response>
        /// <response code="500">Internal server error</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] SubdivisionRequest.SubdivisionUpdate request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { message = "Request body cannot be null" });
                }

                if (string.IsNullOrWhiteSpace(request.name))
                {
                    return BadRequest(new { message = "Name is required" });
                }

                var result = await _subdivisionService.UpdateAsync(request, id);

                if (result)
                {
                    return Ok(new { message = "Subdivision updated successfully" });
                }

                return NotFound(new { message = $"Subdivision with ID {id} not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating subdivision with ID {SubdivisionId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the subdivision", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a subdivision
        /// </summary>
        /// <param name="id">Subdivision ID</param>
        /// <returns>Delete result</returns>
        /// <response code="200">Subdivision deleted successfully</response>
        /// <response code="404">Subdivision not found</response>
        /// <response code="500">Internal server error</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _subdivisionService.DeleteAsync(id);

                if (result)
                {
                    return Ok(new { message = "Subdivision deleted successfully" });
                }

                return NotFound(new { message = $"Subdivision with ID {id} not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting subdivision with ID {SubdivisionId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the subdivision", error = ex.Message });
            }
        }
    }
}
