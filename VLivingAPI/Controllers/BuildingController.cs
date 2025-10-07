using Microsoft.AspNetCore.Mvc;
using VLivingAPI.Authorization;
using Services;
using Services.RequestsResponses;
using Services.RequestsResponses.Building;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace VLivingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BuildingController : ControllerBase
    {
        private readonly IBuildingService _buildingService;
        private readonly ILogger<BuildingController> _logger;

        public BuildingController(IBuildingService buildingService, ILogger<BuildingController> logger)
        {
            _buildingService = buildingService;
            _logger = logger;
        }

        /// <summary>
        /// Get all buildings with pagination, sorting, and dynamic search (Public access)
        /// </summary>
        /// <param name="queryParams">Query parameters (Page, PageSize, SearchField, Search, SortBy, IsDescending, Select)</param>
        /// <returns>Paginated list of buildings</returns>
        /// <response code="200">Returns the paginated list of buildings</response>
        /// <response code="500">Internal server error</response>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll([FromQuery] BuildingQuery queryParams)
        {
            try
            {
                var result = await _buildingService.GetAllAsync(queryParams);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting buildings");
                return StatusCode(500, new { message = "An error occurred while retrieving buildings", error = ex.Message });
            }
        }

        /// <summary>
        /// Get a building by ID (Public access)
        /// </summary>
        /// <param name="id">Building ID</param>
        /// <param name="select">Optional: Comma-separated list of fields to select</param>
        /// <returns>Building details</returns>
        /// <response code="200">Returns the building</response>
        /// <response code="404">Building not found</response>
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

                var result = await _buildingService.GetByIdAsync(id, selectedFields);
                
                if (result == null)
                {
                    return NotFound(new { message = $"Building with ID {id} not found" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting building with ID {BuildingId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the building", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new building (Admin/Manager only)
        /// </summary>
        /// <param name="request">Building data</param>
        /// <returns>Created building ID</returns>
        /// <response code="201">Building created successfully</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="401">Unauthorized - User not authenticated</response>
        /// <response code="403">Forbidden - User doesn't have required permissions</response>
        /// <response code="500">Internal server error</response>
        [HttpPost]
        [BusinessAuthorize(BusinessRole.MasterDataManagement)]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] BuildingRequest.BuildingCreate request)
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

                if (string.IsNullOrWhiteSpace(request.BlockCode))
                {
                    return BadRequest(new { message = "BlockCode is required" });
                }

                var buildingId = await _buildingService.CreateAsync(request);

                if (buildingId > 0)
                {
                    return CreatedAtAction(
                        nameof(GetById),
                        new { id = buildingId },
                        new { id = buildingId, message = "Building created successfully" }
                    );
                }

                return StatusCode(500, new { message = "Failed to create building" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating building");
                return StatusCode(500, new { message = "An error occurred while creating the building", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing building (Admin/Manager only)
        /// </summary>
        /// <param name="id">Building ID</param>
        /// <param name="request">Building update data</param>
        /// <returns>Update result</returns>
        /// <response code="200">Building updated successfully</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="401">Unauthorized - User not authenticated</response>
        /// <response code="403">Forbidden - User doesn't have required permissions</response>
        /// <response code="404">Building not found</response>
        /// <response code="500">Internal server error</response>
        [HttpPut("{id}")]
        [BusinessAuthorize(BusinessRole.MasterDataManagement)]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] BuildingRequest.BuildingUpdate request)
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

                if (string.IsNullOrWhiteSpace(request.BlockCode))
                {
                    return BadRequest(new { message = "BlockCode is required" });
                }

                var result = await _buildingService.UpdateAsync(request, id);

                if (result)
                {
                    return Ok(new { message = "Building updated successfully" });
                }

                return NotFound(new { message = $"Building with ID {id} not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating building with ID {BuildingId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the building", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a building (Admin/Manager only)
        /// </summary>
        /// <param name="id">Building ID</param>
        /// <returns>Delete result</returns>
        /// <response code="200">Building deleted successfully</response>
        /// <response code="401">Unauthorized - User not authenticated</response>
        /// <response code="403">Forbidden - User doesn't have required permissions</response>
        /// <response code="404">Building not found</response>
        /// <response code="500">Internal server error</response>
        [HttpDelete("{id}")]
        [BusinessAuthorize(BusinessRole.MasterDataManagement)]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _buildingService.DeleteAsync(id);

                if (result)
                {
                    return Ok(new { message = "Building deleted successfully" });
                }

                return NotFound(new { message = $"Building with ID {id} not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting building with ID {BuildingId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the building", error = ex.Message });
            }
        }
    }
}
