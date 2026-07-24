using System.ComponentModel.DataAnnotations;

namespace Trustesse.Ivoluntia.Domain.Entities
{
    public class UserSecurityValidationAttempt : BaseEntity
    {
        [Required]
        public string UserId { get; set; }
        public int AttemptCount { get; set; }
        public DateTime LastAttemptDate { get; set; }
        public DateTime? LockedUntil { get; set; }
    }
}
