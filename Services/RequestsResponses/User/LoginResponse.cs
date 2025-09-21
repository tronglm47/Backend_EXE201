namespace VLivingAPI.RequestsResponses.User
{
    public class LoginResponse
    {
        public required string Token { get; set; }
        // Có thể thêm fields khác như RefreshToken nếu cần sau này
    }
}
