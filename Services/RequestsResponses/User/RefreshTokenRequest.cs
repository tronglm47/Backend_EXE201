using System.ComponentModel.DataAnnotations;

namespace Services.RequestsResponses.User
{
    public class RefreshTokenRequest
    {
        [Required(ErrorMessage = "Token is required")]
        public required string Token { get; set; }
    }
}
