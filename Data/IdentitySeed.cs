using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace ReactNativeSI.Data
{
    public static class IdentitySeed
    {
        public static async Task EnsureSeededAsync(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            var roles = configuration.GetSection("Seed:Roles").Get<string[]>() ?? Array.Empty<string>();
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            var adminEmail = configuration["Seed:Admin:Email"];
            var adminPassword = configuration["Seed:Admin:Password"];
            var adminRoles = configuration.GetSection("Seed:Admin:Roles").Get<string[]>() ?? Array.Empty<string>();

            if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
            {
                var adminUser = await userManager.FindByEmailAsync(adminEmail);
                if (adminUser == null)
                {
                    adminUser = new IdentityUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true
                    };
                    var createResult = await userManager.CreateAsync(adminUser, adminPassword);
                    if (!createResult.Succeeded)
                    {
                        var errors = string.Join(", ", createResult.Errors.Select(e => $"{e.Code}:{e.Description}"));
                        throw new InvalidOperationException($"Failed to create admin user: {errors}");
                    }
                }

                foreach (var roleName in adminRoles)
                {
                    if (!await userManager.IsInRoleAsync(adminUser, roleName))
                    {
                        await userManager.AddToRoleAsync(adminUser, roleName);
                    }
                }
            }
        }
    }
}


