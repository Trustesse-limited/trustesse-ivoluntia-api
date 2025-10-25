using Microsoft.AspNetCore.Identity;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.Enums;

namespace Trustesse.Ivoluntia.Data.DataContext
{
    public static class RoleSeeder
    {
        public static async Task SeedRolesAsync(RoleManager<Role> roleManager)
        {
            var roles = new[]
            {
            new Role
            {
                Name = UserRolesEnum.Volunteer.ToString(),
                NormalizedName = UserRolesEnum.Volunteer.ToString().ToUpper(),
                AllowedForFoundation = false
            },
            new Role
            {
                Name = UserRolesEnum.FoundationAdmin.ToString(),
                NormalizedName = UserRolesEnum.FoundationAdmin.ToString().ToUpper(),
                AllowedForFoundation = true
            },
            new Role
            {
                Name = UserRolesEnum.SuperAdmin.ToString(),
                NormalizedName = UserRolesEnum.SuperAdmin.ToString().ToUpper(),
                AllowedForFoundation = false
            }
        };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role.Name))
                {
                    await roleManager.CreateAsync(role);
                }
            }
        }


        public static async Task SeedSuperAdminUserAsync(UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            const string superAdminEmail = "admin@ivoluntia.com";
            const string superAdminPassword = "Admin#123";

            var existingUser = await userManager.FindByEmailAsync(superAdminEmail);

            if (existingUser == null)
            {
                var user = new User
                {
                    UserName = superAdminEmail,
                    Email = superAdminEmail,
                    EmailConfirmed = true,
                    FirstName = "Super",
                    LastName = "Admin",
                    IsActive = true
                };

                var result = await userManager.CreateAsync(user, superAdminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, UserRolesEnum.SuperAdmin.ToString());
                }
                else
                {
                    throw new Exception("Failed to create SuperAdmin user");
                }
            }
        }
    }

}
