using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Repositories.Models
{
    public class ChatUsage
    {
        [Key]
        public int Id { get; set; }

        public int? UserId { get; set; } // Null for anonymous users

        [StringLength(45)]
        public string? IpAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime RequestTime { get; set; } = DateTime.UtcNow;

        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Response { get; set; }

        public int TokensUsed { get; set; } = 0;

        public bool IsSuccess { get; set; } = true;

        [StringLength(500)]
        public string? ErrorMessage { get; set; }

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}