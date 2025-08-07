using Microsoft.AspNetCore.Identity;

namespace Trustesse.Ivoluntia.Domain.Entities
{
    public class Role : IdentityRole
    {
        public bool AllowedForFoundation { get; set; }
    }
}
