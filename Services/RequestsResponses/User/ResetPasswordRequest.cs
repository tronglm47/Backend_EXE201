using System.ComponentModel.DataAnnotations;

namespace VLivingAPI.RequestsResponses.User
{
    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "Token is required")]
        public required string Token { get; set; }

        [Required(ErrorMessage = "New password is required")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 255 characters")]
        public required string NewPassword { get; set; }
    }
}