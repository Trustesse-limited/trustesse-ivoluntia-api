using System.ComponentModel.DataAnnotations;

namespace Trustesse.Ivoluntia.Domain.Entities
{
    public class Otp : BaseEntity
    {
        [MaxLength(100)]
        public string UserId { get; set; }
        [MaxLength(10)]
        public string OtpCode { get; set; }
        public bool IsUsed { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        [MaxLength(255)]
        public string Purpose { get; set; }
        [MaxLength(100)]
        public string Channel { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Purpose { get; set; }
        public string Channel { get; set; } = "email"; // or "sms"
    }
}
