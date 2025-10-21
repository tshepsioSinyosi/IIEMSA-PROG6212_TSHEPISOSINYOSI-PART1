using System;
using System.Linq;
using System.Threading.Tasks;
using ContractMonthlyClaimsSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging; // Added ILogger dependency

namespace ContractClaimSystem.Data
{
    // The User model is assumed to be named 'User' based on your provided file.
    public static class SeedData
    {
        // New structure to allow access to services like ILogger
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                // Get the Logger instance
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

                // 1. Ensure Database is created and migrations applied
                await context.Database.MigrateAsync();

                // 2. Seed Roles
                await SeedRolesAsync(roleManager, logger);

                // 3. Seed Users and Ensure Correct Role Assignment

                // --- COORDINATORS ---
                await EnsureUserAndRoleAssignedAsync(
                    userManager, logger,
                    "Coordinator",
                    "coord1@claims.com",
                    "Coord@123",
                    "Alice Coordinator"
                );

                await EnsureUserAndRoleAssignedAsync(
                    userManager, logger,
                    "Coordinator",
                    "coord2@claims.com",
                    "Coord@123",
                    "Bob Coordinator"
                );


                // --- LECTURERS ---
                await EnsureUserAndRoleAssignedAsync(
                    userManager, logger,
                    "Lecturer",
                    "lecturer1@claims.com",
                    "Lect@123",
                    "Carol Lecturer"
                );

                await EnsureUserAndRoleAssignedAsync(
                    userManager, logger,
                    "Lecturer",
                    "lecturer2@claims.com",
                    "Lect@123",
                    "David Lecturer"
                );


                // --- MANAGERS ---
                await EnsureUserAndRoleAssignedAsync(
                    userManager, logger,
                    "Manager",
                    "manager@claims.com",
                    "Manager@123",
                    "Claims Manager"
                );
            }
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
        {
            // Define the roles required for the application, including Manager
            string[] roleNames = { "Coordinator", "Lecturer", "Manager" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    // Create the role if it doesn't exist
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                    logger.LogInformation($"Role '{roleName}' created.");
                }
            }
        }

        // Helper function to create the user OR ensure an existing user has the correct role (and ONLY that role)
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
                // User does not exist, create the user
                user = new User
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FullName = fullName,
                    Role = roleName // Set the custom User model Role property
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
                // User exists, now check and enforce the correct role assignment
                var currentRoles = await userManager.GetRolesAsync(user);

                // If the user is missing the required role OR has extra roles (e.g., Coordinator is also Lecturer)
                if (!currentRoles.Contains(roleName) || currentRoles.Count() > 1)
                {
                    // 1. Remove all existing roles to clean up old data
                    var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
                    if (!removeResult.Succeeded)
                    {
                        logger.LogError($"Failed to remove existing roles for user '{email}'.");
                        return;
                    }

                    // 2. Add only the correct role
                    var addResult = await userManager.AddToRoleAsync(user, roleName);
                    if (addResult.Succeeded)
                    {
                        logger.LogInformation($"User '{email}' role enforced: now only in '{roleName}'.");
                    }
                    else
                    {
                        logger.LogError($"Failed to add role '{roleName}' to user '{email}'.");
                    }
                }
            }
        }
    }
}
