using System.ComponentModel.DataAnnotations;

namespace Trustesse.Ivoluntia.Domain.Entities
{
    public class SecurityQuestion : BaseEntity
    {
        [Required]
        [MaxLength(500)]
        public string Question { get; set; }
        public bool IsActive { get; set; } = true;

        public virtual ICollection<UserSecurityQuestion> UserSecurityQuestions { get; set; } = new List<UserSecurityQuestion>();
    }
}
