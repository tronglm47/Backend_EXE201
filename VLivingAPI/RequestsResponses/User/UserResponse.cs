namespace VLivingAPI.RequestsResponses.User
{
    public class UserResponse
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string ProfilePictureURL { get; set; }
        public string Bio { get; set; }
        // Thêm các fields khác từ table Users nếu cần, trừ PasswordHash và các sensitive khác
    }
}
