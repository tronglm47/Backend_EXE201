using System.ComponentModel.DataAnnotations;

namespace Services.RequestsResponses.Notification
{
    public class UpdateNotificationRequest
    {
        [StringLength(50)]
        public string? Type { get; set; }

        [StringLength(500)]
        public string? Content { get; set; }

        public int? RelatedId { get; set; }

        public bool? IsRead { get; set; }
    }
}