using Microsoft.AspNetCore.Mvc;
using Services;
using Services.RequestsResponses;
using Services.RequestsResponses.Utility;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace VLivingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UtilityController : ControllerBase
    {
        private readonly IUtilityService _utilityService;
        private readonly ILogger<UtilityController> _logger;
        public UtilityController(IUtilityService utilityService, ILogger<UtilityController> logger)
        {
            _utilityService = utilityService;
            _logger = logger;
        }
        /// <summary>
        /// Get all utilities with pagination, sorting, and dynamic search
        /// </summary>
        /// <param name="queryParams">Query parameters (Page, PageSize, SearchField, Search, SortBy, IsDescending, Select)</param>
        /// <returns>Paginated list of utilities</returns>
        /// <response code="200">Returns the paginated list of utilities</response>
        /// <response code="500">Internal server error</response>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll([FromQuery] UtilityQuery queryParams)
        {
            try
            {
                var result = await _utilityService.GetAllAsync(queryParams);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting utilities");
                return StatusCode(500, new { message = "An error occurred while retrieving utilities", error = ex.Message });
            }
        }

        /// <summary>
        /// Get a utility by ID
        /// </summary>
        /// <param name="id">Utility ID</param>
        /// <param name="select">Optional: Comma-separated list of fields to select</param>
        /// <returns>Utility details</returns>
        /// <response code="200">Returns the utility</response>
        /// <response code="404">Utility not found</response>
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

                var result = await _utilityService.GetByIdAsync(id, selectedFields);
                
                if (result == null)
                {
                    return NotFound(new { message = $"Utility with ID {id} not found" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting utility with ID {UtilityId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the utility", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new utility
        /// </summary>
        /// <param name="request">Utility creation data</param>
        /// <returns>Created utility ID</returns>
        /// <response code="201">Utility created successfully</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="500">Internal server error</response>
        [HttpPost]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] UtilityRequest.UtilityCreate request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { message = "Request body cannot be null" });
                }

                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return BadRequest(new { message = "Name is required" });
                }

                var utilityId = await _utilityService.CreateAsync(request);

                if (utilityId > 0)
                {
                    return CreatedAtAction(
                        nameof(GetById),
                        new { id = utilityId },
                        new { id = utilityId, message = "Utility created successfully" }
                    );
                }

                return StatusCode(500, new { message = "Failed to create utility" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating utility");
                return StatusCode(500, new { message = "An error occurred while creating the utility", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing utility
        /// </summary>
        /// <param name="id">Utility ID</param>
        /// <param name="request">Utility update data</param>
        /// <returns>Update result</returns>
        /// <response code="200">Utility updated successfully</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="404">Utility not found</response>
        /// <response code="500">Internal server error</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] UtilityRequest.UtilityUpdate request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { message = "Request body cannot be null" });
                }

                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return BadRequest(new { message = "Name is required" });
                }

                var result = await _utilityService.UpdateAsync(request, id);

                if (result)
                {
                    return Ok(new { message = "Utility updated successfully" });
                }

                return NotFound(new { message = $"Utility with ID {id} not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating utility with ID {UtilityId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the utility", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a utility
        /// </summary>
        /// <param name="id">Utility ID</param>
        /// <returns>Delete result</returns>
        /// <response code="200">Utility deleted successfully</response>
        /// <response code="404">Utility not found</response>
        /// <response code="500">Internal server error</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _utilityService.DeleteAsync(id);

                if (result)
                {
                    return Ok(new { message = "Utility deleted successfully" });
                }

                return NotFound(new { message = $"Utility with ID {id} not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting utility with ID {UtilityId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the utility", error = ex.Message });
            }
        }
    }
}
