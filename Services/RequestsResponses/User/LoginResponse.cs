namespace Services.RequestsResponses.User
{
    public class LoginResponse
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsEmailVerified { get; set; }
    }
}

