using System.ComponentModel.DataAnnotations;

namespace Services.RequestsResponses.Chat
{
    public class ChatRequest
    {
        [Required]
        [StringLength(500, ErrorMessage = "Message must be less than 500 characters")]
        public string Message { get; set; } = string.Empty;
    }
}