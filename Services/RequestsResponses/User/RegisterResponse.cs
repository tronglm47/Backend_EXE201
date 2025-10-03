namespace Services.RequestsResponses.User
{
    public class RegisterResponse
    {
        public int UserId { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Role { get; set; }
        public string? FullName { get; set; }
        public string Message { get; set; } = "User registered successfully. Please check your email to verify your account.";
        public bool RequiresEmailVerification { get; set; } = true;
    }
}
