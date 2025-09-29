using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.RequestsResponses.Post;

namespace VLivingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostController(IPostService postService)
        {
            _postService = postService;
        }

        // GET: api/Post
        [Produces("application/json", "application/xml")]
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] PostQueryParameters queryParams)
        {
            try
            {
                var result = await _postService.GetAllPostAsync(queryParams);
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

        // GET api/Post/5
        [Produces("application/json", "application/xml")]
        [HttpGet("{id}")]
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

                var result = await _postService.GetPostById(id, selectedFields);
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
        /* Example
        // GET: api/<OrderController>
        [Produces("application/json", "application/xml")]
        [HttpGet]
        [Authorize(Roles = "User,Manager,Admin")]
        public async Task<IActionResult> Get([FromQuery] OrderQueryParameters queryParams)
        {
            try
            {
                var result = await _orderService.GetAllOrderAsync(queryParams);
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
                _logger.LogError(ex, "Error getting orders");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        // GET api/<OrderController>/5
        [Produces("application/json", "application/xml")]
        [HttpGet("{id}")]
        [Authorize(Roles = "User,Manager,Admin")]
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

                var result = await _orderService.GetOrderById(id, selectedFields);
                if (result == null)
                {
                    return NotFound(new { success = false, message = "Order not found" });
                }

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order by ID {OrderId}", id);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }
        */
    }
}
