using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.RequestsResponses.Post;
using VLivingAPI.Authorization;
using Repositories.Constants;

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
        [AllowAnonymous] // Anyone can browse posts
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
        [AllowAnonymous] // Anyone can view specific post
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

        // POST api/Post
        [Produces("application/json", "application/xml")]
        [HttpPost]
        [BusinessAuthorize(BusinessRole.PostManagement)] // Users who can create posts
        public async Task<IActionResult> Post([FromBody] PostRequest.CreatePost request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid input data",
                        errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }

                var postId = await _postService.Create(request);
                if (postId <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Failed to create post. Please check your input data."
                    });
                }

                return CreatedAtAction(nameof(Get), new { id = postId }, new
                {
                    success = true,
                    message = "Post created successfully",
                    data = new { PostId = postId }
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error occurred while creating the post"
                });
            }
        }

        // PUT api/Post/5
        [Produces("application/json", "application/xml")]
        [HttpPut("{id}")]
        [BusinessAuthorize(BusinessRole.PostManagement)] // Users who can edit posts
        public async Task<IActionResult> Put(int id, [FromBody] PostRequest.PostUpdateRequest request)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { success = false, message = "Invalid post ID" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid input data",
                        errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }

                var result = await _postService.Update(request, id);
                if (!result)
                {
                    return NotFound(new { success = false, message = "Post not found" });
                }

                return Ok(new
                {
                    success = true,
                    message = "Post updated successfully"
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error occurred while updating the post"
                });
            }
        }

        // DELETE api/Post/5
        [Produces("application/json", "application/xml")]
        [HttpDelete("{id}")]
        [BusinessAuthorize(BusinessRole.PostManagement)] // Users who can delete posts
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { success = false, message = "Invalid post ID" });
                }

                // First check if post exists
                var post = await _postService.GetPostById(id, new List<string>());
                if (post == null)
                {
                    return NotFound(new { success = false, message = "Post not found" });
                }

                var result = await _postService.Delete(id);
                if (!result)
                {
                    return BadRequest(new { success = false, message = "Failed to delete post" });
                }

                return Ok(new
                {
                    success = true,
                    message = "Post and related amenities deleted successfully"
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error occurred while deleting the post"
                });
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
