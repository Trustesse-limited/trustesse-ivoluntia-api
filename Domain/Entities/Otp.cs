using Trustesse.Ivoluntia.Domain.Enums;

namespace Trustesse.Ivoluntia.Domain.Entities
{
    public class Otp
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;
        public bool IsUsed { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public OtpPurpose Purpose { get; set; }
        public string Channel { get; set; } = "email"; // or "sms"
    }
}
