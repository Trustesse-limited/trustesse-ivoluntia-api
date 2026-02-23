using Microsoft.AspNetCore.Identity;
using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Domain.Entities;

namespace Trustesse.Ivoluntia.API.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static async Task SeedDefaultDataAsync(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var context = scope.ServiceProvider.GetRequiredService<iVoluntiaDataContext>();

            await Seeder.SeedRolesAsync(roleManager);
            await Seeder.SeedSuperAdminUserAsync(userManager, roleManager);
            await Seeder.SeedFoundationAsync(context);
            await Seeder.SeedFoundationAdminAsync(userManager, context);
            await Seeder.SeedSkillsAsync(context);
            await Seeder.SeedProgramAsync(context);
        }
        public static void ConfigureHsts(this WebApplicationBuilder builder)
        {
            builder.Services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(365);
            });
        }
    }
}
