namespace VLivingAPI.RequestsResponses.User
{
    public class UserResponse
    {
        public int UserID { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Role { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ProfilePictureURL { get; set; }
        public string? Bio { get; set; }
    }
}
