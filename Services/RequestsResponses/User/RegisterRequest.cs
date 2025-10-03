using System.ComponentModel.DataAnnotations;

namespace Services.RequestsResponses.User
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email must not exceed 100 characters")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 255 characters")]
        public required string Password { get; set; }

        [StringLength(100, ErrorMessage = "Full name must not exceed 100 characters")]
        public string? FullName { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Phone number must not exceed 20 characters")]
        public string? PhoneNumber { get; set; }

        [OptionalUrl(ErrorMessage = "Invalid URL format")]
        [StringLength(255, ErrorMessage = "Profile picture URL must not exceed 255 characters")]
        public string? ProfilePictureUrl { get; set; }

        [StringLength(500, ErrorMessage = "Bio must not exceed 500 characters")]
        public string? Bio { get; set; }

        [StringLength(20, ErrorMessage = "Role must not exceed 20 characters")]
        public string? Role { get; set; } // Will default to "User" if empty
    }

    // Custom validation attribute for optional URL
    public class OptionalUrlAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return true; // Allow null or empty

            return Uri.TryCreate(value.ToString(), UriKind.Absolute, out _);
        }
    }
}
