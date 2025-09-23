using System.ComponentModel.DataAnnotations;

namespace Services.RequestsResponses.Notification
{
    public class CreateNotificationRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Content { get; set; } = string.Empty;

        public int? RelatedId { get; set; }

        public bool? IsRead { get; set; } = false;
    }
}