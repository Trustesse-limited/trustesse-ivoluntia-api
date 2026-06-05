using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Data.Repositories.Interfaces;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.Enums;

namespace Trustesse.Ivoluntia.Data.Repositories.Implementation
{
    public class VolunteerRepository : IVolunteerRepository
    {
        private readonly iVoluntiaDataContext _context;
        private readonly RoleManager<Role> _roleManager;

        public VolunteerRepository(iVoluntiaDataContext context, RoleManager<Role> roleManager)
        {
            _context = context;
            _roleManager = roleManager;
        }


        public IQueryable<User> GetVolunteers(string foundationId, bool? isActive = null)
        {
            var volunteerRoleName = UserRolesEnum.Volunteer.ToString();

            var volunteerRoleIds = _context.Roles
                                           .Where(r => r.Name == volunteerRoleName)
                                           .Select(r => r.Id);

            var volunteerUserIds = _context.Set<IdentityUserRole<string>>()
                                           .Where(ur => volunteerRoleIds.Contains(ur.RoleId))
                                           .Select(ur => ur.UserId);

            var query = _context.Users.Where(u => volunteerUserIds.Contains(u.Id) && u.FoundationId == foundationId);

            if (isActive.HasValue)
            {
                query = query.Where(u => u.IsActive == isActive.Value);
            }

            return query;
        }
    }
}
