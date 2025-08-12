using Microsoft.AspNetCore.Identity;

namespace Trustesse.Ivoluntia.Domain.Entities
{
    public class Role : IdentityRole
    {
        public bool AllowedForFoundation { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public DateTime? DateUpdated { get; set; }
        public bool IsDeprecated { get; set; }
    }
}
