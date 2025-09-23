using System.ComponentModel.DataAnnotations;

namespace Services.RequestsResponses.Notification
{
    public class UpdateNotificationStatusRequest
    {
        [Required]
        public bool IsRead { get; set; }
    }
}