using System.Security.Claims;
using KinderHub.Application.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace KinderHub.Web.Data;

// Creates the login database, the 3 roles and 3 demo accounts.
public static class IdentitySeeder
{
    public static async Task SeedIdentityAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;

        // 1. Make sure the login database (app.db) exists
        var db = sp.GetRequiredService<ApplicationDbContext>();
        if ((await db.Database.GetPendingMigrationsAsync()).Any())
            await db.Database.MigrateAsync();
        else
            await db.Database.EnsureCreatedAsync();

        // 2. Roles
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // 3. Demo users (one for each role)
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        await EnsureUserAsync(userManager, "admin@kinderhub.kz", "Admin123!", "Aliya Nurova", Roles.Admin);
        await EnsureUserAsync(userManager, "teacher@kinderhub.kz", "Teacher123!", "Dana Akhmetova", Roles.Teacher);
        await EnsureUserAsync(userManager, "parent@kinderhub.kz", "Parent123!", "Serik Nurlanov", Roles.Parent);
    }

    private static async Task EnsureUserAsync(UserManager<ApplicationUser> userManager,
        string email, string password, string fullName, string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    $"Could not create {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            await userManager.AddClaimAsync(user, new Claim(KinderHubClaims.FullName, fullName));
        }

        if (!await userManager.IsInRoleAsync(user, role))
            await userManager.AddToRoleAsync(user, role);
    }
}
