using Microsoft.AspNetCore.Identity;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.Enums;

namespace Trustesse.Ivoluntia.Data.DataContext
{
    public static class Seeder
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

        public static async Task SeedFoundationAdminAsync(UserManager<User> userManager, iVoluntiaDataContext context)
        {
            const string foundationAdminEmail = "foundationadmin@ivoluntia.com";
            const string foundationAdminPassword = "Foundation#123";

            var existingUser = await userManager.FindByEmailAsync(foundationAdminEmail);

            if (existingUser == null)
            {
                var foundation = context.Foundations.First();
                var user = new User
                {
                    UserName = foundationAdminEmail,
                    Email = foundationAdminEmail,
                    EmailConfirmed = true,
                    FirstName = "Foundation",
                    LastName = "Admin",
                    IsActive = true,
                    FoundationId = foundation.Id
                };

                var result = await userManager.CreateAsync(user, foundationAdminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, UserRolesEnum.FoundationAdmin.ToString());
                }
                else
                {
                    throw new Exception("Failed to create FoundationAdmin user");
                }
            }
        }

        public static async Task SeedFoundationAsync(iVoluntiaDataContext context)
        {
            if (!context.FoundationCategories.Any())
            {
                var category = new FoundationCategory
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Non-Profit Organization",
                    Description = "General non-profit organization",
                    DateCreated = DateTime.UtcNow
                };
                context.FoundationCategories.Add(category);
                await context.SaveChangesAsync();
            }

            if (!context.Foundations.Any())
            {
                var category = context.FoundationCategories.First();
                var foundation = new Foundation
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "iVoluntia Foundation",
                    CategoryId = category.Id,
                    Mission = "Connecting volunteers with meaningful opportunities",
                    Email = "contact@ivoluntia.com",
                    YearEstablished = DateTime.UtcNow,
                    IsActive = true,
                    HasAgreedToDisclaimer = true,
                    DateCreated = DateTime.UtcNow
                };
                context.Foundations.Add(foundation);
                await context.SaveChangesAsync();
            }
        }

        public static async Task SeedSkillsAsync(iVoluntiaDataContext context)
        {
            if (!context.Skills.Any())
            {
                var skills = new[]
                {
                    new Skill { Id = Guid.NewGuid().ToString(), Name = "Teaching", Description = "Education and tutoring", DateCreated = DateTime.UtcNow },
                    new Skill { Id = Guid.NewGuid().ToString(), Name = "Healthcare", Description = "Medical and health services", DateCreated = DateTime.UtcNow },
                    new Skill { Id = Guid.NewGuid().ToString(), Name = "Construction", Description = "Building and repair work", DateCreated = DateTime.UtcNow },
                    new Skill { Id = Guid.NewGuid().ToString(), Name = "IT Support", Description = "Technology and computer skills", DateCreated = DateTime.UtcNow },
                    new Skill { Id = Guid.NewGuid().ToString(), Name = "Event Planning", Description = "Organizing and coordinating events", DateCreated = DateTime.UtcNow }
                };
                context.Skills.AddRange(skills);
                await context.SaveChangesAsync();
            }
        }

        public static async Task SeedProgramAsync(iVoluntiaDataContext context)
        {
            if (!context.Programs.Any())
            {
                var foundation = context.Foundations.First();

                if (!context.Countries.Any())
                {
                    var country = new Country { Id = Guid.NewGuid(), CountryName = "Nigeria" };
                    context.Countries.Add(country);
                    await context.SaveChangesAsync();
                }

                var countryId = context.Countries.First().Id;

                if (!context.States.Any())
                {
                    var state = new State { Id = Guid.NewGuid(), StateName = "Lagos", CountryId = countryId };
                    context.States.Add(state);
                    await context.SaveChangesAsync();
                }

                var stateId = context.States.First().Id;

                var location = new Location
                {
                    Id = Guid.NewGuid().ToString(),
                    CountryId = countryId,
                    StateId = stateId,
                    City = "Ikeja",
                    Address = "123 Volunteer Street",
                    DateCreated = DateTime.UtcNow
                };
                context.Locations.Add(location);
                await context.SaveChangesAsync();

                var program = new Program
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = "Community Outreach Program",
                    Description = "Help local communities through volunteer work",
                    StartDate = DateTime.UtcNow.AddDays(7),
                    EndDate = DateTime.UtcNow.AddMonths(3),
                    LocationId = location.Id,
                    FoundationId = foundation.Id,
                    IsActive = true,
                    Status = (int)ProgramStatus.Active,
                    HasDonation = false,
                    DateCreated = DateTime.UtcNow
                };
                context.Programs.Add(program);
                await context.SaveChangesAsync();

                var skills = context.Skills.Take(3).ToList();
                foreach (var skill in skills)
                {
                    context.ProgramSkills.Add(new ProgramSkill
                    {
                        ProgramId = program.Id,
                        SkillId = skill.Id,
                        DateCreated = DateTime.UtcNow
                    });
                }

                var goals = new[]
                {
                    new ProgramGoal { Id = Guid.NewGuid().ToString(), ProgramId = program.Id, Goal = "Reach 100 volunteers", IsAchieved = false, DateCreated = DateTime.UtcNow },
                    new ProgramGoal { Id = Guid.NewGuid().ToString(), ProgramId = program.Id, Goal = "Complete 50 community projects", IsAchieved = false, DateCreated = DateTime.UtcNow }
                };
                context.ProgramGoals.AddRange(goals);
                await context.SaveChangesAsync();
            }
        }
    }
}
