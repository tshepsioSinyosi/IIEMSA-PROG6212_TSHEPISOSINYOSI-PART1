using ContractClaimSystem.Data;
using ContractClaimSystem.Models;
using ContractClaimSystem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ContractClaimSystem
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Get connection string
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                                   ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddScoped<ClaimService>();
            builder.Services.AddScoped<LecturerService>();
            //builder.Services.AddScoped<HRService>();
            //builder.Services.AddScoped<EventService>();
            //builder.Services.AddScoped<UserService>();


            // 2. Register DbContext
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 10,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                })
            );


            // 3. Configure Identity
            builder.Services.AddDefaultIdentity<User>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // 4. Apply migrations & seed database automatically
            await ApplyMigrationsAndSeedAsync(app);

            // 5. Middleware
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}");

            await app.RunAsync();
        }

        private static async Task ApplyMigrationsAndSeedAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<Program>>();

            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();

                // Apply any pending migrations
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    logger.LogInformation("Applying migrations...");
                    await context.Database.MigrateAsync();
                    logger.LogInformation("Migrations applied successfully.");
                }
                else
                {
                    logger.LogInformation("No pending migrations.");
                }

                // Seed roles
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                string[] roles = { "Admin", "Coordinator", "Lecturer", "Manager", "HR" };
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        var roleResult = await roleManager.CreateAsync(new IdentityRole(role));
                        if (roleResult.Succeeded)
                            logger.LogInformation($"Role '{role}' created successfully.");
                        else
                            logger.LogError($"Failed to create role '{role}': {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                    }
                    else
                    {
                        logger.LogInformation($"Role '{role}' already exists.");
                    }
                }

                // Seed default users
                var userManager = services.GetRequiredService<UserManager<User>>();

                var defaultUsers = new List<(string Email, string Password, string Role, string FullName)>
                {
                    ("admin@admin.com", "Admin@123", "Admin", "System Administrator"),
                    ("manager@claims.com", "Manager@123", "Manager", "Claims Manager"),
                    ("hr@claims.com", "Hr@123", "HR", "HR User"),
                    ("coord1@claims.com", "Coord@1234", "Coordinator", "Alice Coordinator"),
                    ("coord2@claims.com", "Coord@12345", "Coordinator", "Bob Coordinator")
                };

                foreach (var userInfo in defaultUsers)
                {
                    var user = await userManager.FindByEmailAsync(userInfo.Email);
                    if (user == null)
                    {
                        user = new User
                        {
                            UserName = userInfo.Email,
                            Email = userInfo.Email,
                            EmailConfirmed = true,
                            FullName = userInfo.FullName
                        };

                        var createResult = await userManager.CreateAsync(user, userInfo.Password);
                        if (createResult.Succeeded)
                        {
                            var roleResult = await userManager.AddToRoleAsync(user, userInfo.Role);
                            if (roleResult.Succeeded)
                                logger.LogInformation($"User '{userInfo.Email}' created and assigned role '{userInfo.Role}'.");
                            else
                                logger.LogError($"Failed to assign role '{userInfo.Role}' to '{userInfo.Email}': {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                        }
                        else
                        {
                            logger.LogError($"Failed to create user '{userInfo.Email}': {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                        }
                    }
                    else
                    {
                        // Reset password
                        try
                        {
                            var token = await userManager.GeneratePasswordResetTokenAsync(user);
                            var resetResult = await userManager.ResetPasswordAsync(user, token, userInfo.Password);
                            if (resetResult.Succeeded)
                                logger.LogInformation($"Password reset for existing user '{userInfo.Email}' succeeded.");
                            else
                                logger.LogError($"Failed to reset password for '{userInfo.Email}': {string.Join(", ", resetResult.Errors.Select(e => e.Description))}");
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, $"Exception while resetting password for user '{userInfo.Email}'.");
                        }

                        // Ensure user has the correct role
                        var currentRoles = await userManager.GetRolesAsync(user);
                        if (!currentRoles.Contains(userInfo.Role))
                        {
                            await userManager.RemoveFromRolesAsync(user, currentRoles);
                            var roleResult = await userManager.AddToRoleAsync(user, userInfo.Role);
                            if (roleResult.Succeeded)
                                logger.LogInformation($"User '{userInfo.Email}' role updated to '{userInfo.Role}'.");
                            else
                                logger.LogError($"Failed to update role for '{userInfo.Email}': {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                        }
                        else
                        {
                            logger.LogInformation($"User '{userInfo.Email}' already has role '{userInfo.Role}'.");
                        }
                    }
                }

                logger.LogInformation("Database seeding completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while applying migrations or seeding the database.");
            }
        }
    }
}
