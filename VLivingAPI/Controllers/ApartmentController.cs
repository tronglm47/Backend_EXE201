using Microsoft.AspNetCore.Mvc;
using VLivingAPI.Authorization;
using Services;
using Services.RequestsResponses;
using Services.RequestsResponses.Apartment;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace VLivingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApartmentController : ControllerBase
    {
        private readonly IApartmentService _apartmentService;
        private readonly ILogger<ApartmentController> _logger;

        public ApartmentController(IApartmentService apartmentService, ILogger<ApartmentController> logger)
        {
            _apartmentService = apartmentService;
            _logger = logger;
        }

        /// <summary>
        /// Get all apartments with pagination, sorting, and dynamic search (Public access)
        /// </summary>
        /// <param name="queryParams">Query parameters (Page, PageSize, SearchField, Search, SortBy, IsDescending, Select)</param>
        /// <returns>Paginated list of apartments</returns>
        /// <response code="200">Returns the paginated list of apartments</response>
        /// <response code="500">Internal server error</response>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll([FromQuery] ApartmentQuery queryParams)
        {
            try
            {
                var result = await _apartmentService.GetAllAsync(queryParams);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all apartments");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while processing your request" });
            }
        }

        /// <summary>
        /// Get an apartment by ID (Public access)
        /// </summary>
        /// <param name="id">Apartment ID</param>
        /// <param name="select">Optional: Comma-separated list of fields to select</param>
        /// <returns>Apartment details</returns>
        /// <response code="200">Returns the apartment</response>
        /// <response code="404">Apartment not found</response>
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
                            .Select(f => f.Trim().ToLower())
                            .ToList();

                var result = await _apartmentService.GetByIdAsync(id, selectedFields);

                if (result == null)
                {
                    return NotFound(new { message = $"Apartment with ID {id} not found" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting apartment by ID: {ApartmentId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while processing your request" });
            }
        }

        /// <summary>
        /// Create a new apartment (Landlord/Agent only)
        /// </summary>
        /// <param name="request">Apartment data</param>
        /// <returns>Created apartment</returns>
        /// <response code="201">Apartment created successfully</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="401">Unauthorized - User not authenticated</response>
        /// <response code="403">Forbidden - User doesn't have required permissions</response>
        /// <response code="500">Internal server error</response>
        [HttpPost]
        [BusinessAuthorize(BusinessRole.PostManagement)]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] ApartmentRequest.ApartmentCreate request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var apartmentId = await _apartmentService.CreateAsync(request);

                if (apartmentId == 0)
                {
                    return BadRequest(new { message = "Failed to create apartment" });
                }

                // Get the created apartment to return
                var createdApartment = await _apartmentService.GetByIdAsync(apartmentId, new List<string>());

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = apartmentId },
                    new
                    {
                        message = "Apartment created successfully",
                        apartmentId = apartmentId,
                        data = createdApartment
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating apartment");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while processing your request" });
            }
        }

        /// <summary>
        /// Update an existing apartment (Landlord/Agent only)
        /// </summary>
        /// <param name="id">Apartment ID</param>
        /// <param name="request">Apartment update data</param>
        /// <returns>Update result</returns>
        /// <response code="200">Apartment updated successfully</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="401">Unauthorized - User not authenticated</response>
        /// <response code="403">Forbidden - User doesn't have required permissions</response>
        /// <response code="404">Apartment not found</response>
        /// <response code="500">Internal server error</response>
        [HttpPut("{id}")]
        [BusinessAuthorize(BusinessRole.PostManagement)]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] ApartmentRequest.ApartmentUpdate request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _apartmentService.UpdateAsync(request, id);

                if (!result)
                {
                    return NotFound(new { message = $"Apartment with ID {id} not found" });
                }

                var updatedApartment = await _apartmentService.GetByIdAsync(id, new List<string>());

                return Ok(new
                {
                    message = "Apartment updated successfully",
                    data = updatedApartment
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating apartment: {ApartmentId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while processing your request" });
            }
        }

        /// <summary>
        /// Delete an apartment (Landlord/Agent only)
        /// </summary>
        /// <param name="id">Apartment ID</param>
        /// <returns>Delete result</returns>
        /// <response code="200">Apartment deleted successfully</response>
        /// <response code="401">Unauthorized - User not authenticated</response>
        /// <response code="403">Forbidden - User doesn't have required permissions</response>
        /// <response code="404">Apartment not found</response>
        /// <response code="500">Internal server error</response>
        [HttpDelete("{id}")]
        [BusinessAuthorize(BusinessRole.PostManagement)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _apartmentService.DeleteAsync(id);

                if (!result)
                {
                    return NotFound(new { message = $"Apartment with ID {id} not found" });
                }

                return Ok(new { message = "Apartment deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting apartment: {ApartmentId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while processing your request" });
            }
        }
    }
}
