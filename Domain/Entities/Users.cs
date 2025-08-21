using Microsoft.AspNetCore.Identity;

namespace Trustesse.Ivoluntia.Domain.Entities
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Location Location { get; set; }
        public string? UserImage { get; set; }
        public string? Bio { get; set; }
        public DateTime? LastLogin { get; set; }
        public byte? Gender { get; set; }
        public bool IsActive { get; set; }
        public bool HasAgreedToTermsAndCondition { get; set; }
        public bool HasChangedDefaultPassword { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public DateTime? DateUpdated { get; set; }
        public bool IsDeprecated { get; set; }
        public string? FoundationId { get; set; }
        public Foundation? Foundation { get; set; }
        public ICollection<Skill?> Skills { get; set; } = new List<Skill>();
        public ICollection<Interest?> Interests { get; set; } = new List<Interest>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
