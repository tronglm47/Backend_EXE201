using System.ComponentModel.DataAnnotations;

namespace Services.RequestsResponses.User
{
    public class UserRequest
    {
        public class AdminUpdateUserRequest
        {
            [Required(ErrorMessage = "Username is required")]
            [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
            public string Username { get; set; } = string.Empty;

            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Invalid email format")]
            [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Role is required")]
            [StringLength(20, ErrorMessage = "Role cannot exceed 20 characters")]
            public string Role { get; set; } = string.Empty;

            [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
            public string? FullName { get; set; }

            [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
            [Phone(ErrorMessage = "Invalid phone number format")]
            public string? PhoneNumber { get; set; }

            [StringLength(255, ErrorMessage = "Profile picture URL cannot exceed 255 characters")]
            [Url(ErrorMessage = "Invalid URL format")]
            public string? ProfilePictureUrl { get; set; }

            [StringLength(500, ErrorMessage = "Bio cannot exceed 500 characters")]
            public string? Bio { get; set; }
        }

        public class UpdateLocationSharingRequest
        {
            [Required(ErrorMessage = "IsLocationSharingEnabled is required")]
            public bool IsLocationSharingEnabled { get; set; }
        }
    }
}

