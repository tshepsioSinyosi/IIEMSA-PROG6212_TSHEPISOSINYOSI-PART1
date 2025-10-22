using System;
using System.Linq;
using System.Threading.Tasks;
using ContractClaimSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ContractClaimSystem.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

            // Apply migrations
            await context.Database.MigrateAsync();

            // Seed roles
            await SeedRolesAsync(roleManager, logger);

            // Seed users
            await EnsureUserAndRoleAssignedAsync(userManager, logger, "Coordinator", "coord1@claims.com", "Coord@123", "Alice Coordinator");
            await EnsureUserAndRoleAssignedAsync(userManager, logger, "Coordinator", "coord2@claims.com", "Coord@123", "Bob Coordinator");
            await EnsureUserAndRoleAssignedAsync(userManager, logger, "Lecturer", "lecturer1@claims.com", "Lect@123", "Carol Lecturer");
            await EnsureUserAndRoleAssignedAsync(userManager, logger, "Lecturer", "lecturer2@claims.com", "Lect@123", "David Lecturer");
            await EnsureUserAndRoleAssignedAsync(userManager, logger, "Manager", "manager@claims.com", "Manager@123", "Claims Manager");
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
        {
            string[] roles = { "Coordinator", "Lecturer", "Manager" };
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                    logger.LogInformation($"Role '{roleName}' created.");
                }
            }
        }

        private static async Task EnsureUserAndRoleAssignedAsync(
            UserManager<User> userManager,
            ILogger logger,
            string roleName,
            string email,
            string password,
            string fullName)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new User
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FullName = fullName
                };

                var createResult = await userManager.CreateAsync(user, password);
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, roleName);
                    logger.LogInformation($"User '{email}' created and assigned role '{roleName}'.");
                }
                else
                {
                    logger.LogError($"Failed to create user '{email}': {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                var currentRoles = await userManager.GetRolesAsync(user);
                if (!currentRoles.Contains(roleName))
                {
                    await userManager.RemoveFromRolesAsync(user, currentRoles);
                    await userManager.AddToRoleAsync(user, roleName);
                    logger.LogInformation($"User '{email}' role enforced to '{roleName}'.");
                }
            }
        }
    }
}
