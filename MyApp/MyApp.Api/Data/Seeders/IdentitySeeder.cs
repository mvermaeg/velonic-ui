using Microsoft.AspNetCore.Identity;
using MyApp.Api.Constants;
using MyApp.Api.Models.Identity;

namespace MyApp.Api.Data.Seeders
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles =
            {
                AppRoles.SuperAdmin,
                AppRoles.Admin,
                AppRoles.SalesManager,
                AppRoles.SalesExecutive,
                AppRoles.Support,
                AppRoles.Accounts,
                AppRoles.VendorManager
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var adminEmail = "admin@velonic.com";
            var adminPassword = "123456";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    FullName = "Super Admin",
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, AppRoles.SuperAdmin);
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(adminUser, AppRoles.SuperAdmin))
                {
                    await userManager.AddToRoleAsync(adminUser, AppRoles.SuperAdmin);
                }
            }
        }
    }
}