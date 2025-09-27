using System.ComponentModel.DataAnnotations;

namespace VLivingAPI.RequestsResponses.User
{
    public class ForgotPasswordRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public required string Email { get; set; }
    }
}