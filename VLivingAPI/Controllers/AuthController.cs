using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using System.Security.Claims;
using VLivingAPI.RequestsResponses.User;

namespace VLivingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var token = await _authService.LoginAsync(request.Username, request.Password);
                var response = new LoginResponse { Token = token };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }
        [HttpGet("userinfo")]
        [Authorize]
        public async Task<IActionResult> GetUserInfo()
        {
            Console.WriteLine("=== GetUserInfo method called ===");
            Console.WriteLine($"User.Identity.IsAuthenticated: {User.Identity?.IsAuthenticated}");
            Console.WriteLine($"User.Identity.Name: {User.Identity?.Name}");
            Console.WriteLine($"Claims count: {User.Claims.Count()}");
            
            foreach (var claim in User.Claims)
            {
                Console.WriteLine($"Claim: {claim.Type} = {claim.Value}");
            }

            try
            {
                // Lấy userId từ JWT claims (đã set ClaimTypes.NameIdentifier trong Login)
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                Console.WriteLine($"UserIdClaim: {userIdClaim}");
                
                if (string.IsNullOrEmpty(userIdClaim)) 
                    return Unauthorized("Invalid token");

                var userId = int.Parse(userIdClaim);
                var userResponse = await _authService.GetUserInfoAsync(userId);
                return Ok(userResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetUserInfo: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("test")]
        [Authorize]
        public IActionResult TestAuth()
        {
            Console.WriteLine("=== TestAuth method called ===");
            return Ok(new { message = "Authentication successful!", user = User.Identity?.Name });
        }
    }
}
