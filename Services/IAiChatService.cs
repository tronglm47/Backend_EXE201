using Services.RequestsResponses.Chat;

namespace Services
{
    public interface IAiChatService
    {
        Task<ChatResponse> ProcessMessageAsync(string message);
    }
}