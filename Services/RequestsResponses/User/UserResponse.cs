namespace Services.RequestsResponses.User
{
    public class UserResponse
    {
        // Flat structure for direct use
        public int UserID { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Role { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ProfilePictureURL { get; set; }
        public string? Bio { get; set; }
        public bool IsEmailVerified { get; set; }
        public DateTime CreatedAt { get; set; }

        // Nested classes for field selection
        public class UserGetAll
        {
            public int UserId { get; set; }
            public required string Username { get; set; }
            public required string Email { get; set; }
            public required string Role { get; set; }
            public string? FullName { get; set; }
            public bool IsEmailVerified { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public class UserDetail
        {
            public int UserId { get; set; }
            public required string Username { get; set; }
            public required string Email { get; set; }
            public required string Role { get; set; }
            public string? FullName { get; set; }
            public string? PhoneNumber { get; set; }
            public string? ProfilePictureURL { get; set; }
            public string? Bio { get; set; }
            public bool IsEmailVerified { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}

