using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using ContractMonthlyClaimsSystem.Models;
using ContractClaimSystem.Data; // Import the Data namespace for SeedData

namespace ContractClaimSystem
{
    public class Program
    {
        public static async Task Main(string[] args) // *** Changed to async Task ***
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Get the connection string from configuration
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                                   ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            // Add services to the container.

            // 2. Register the ApplicationDbContext using the MySQL provider (Pomelo)
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(
                    connectionString,
                    // Auto-detect the MySQL server version (e.g., MySQL 8.0, MariaDB 10.5)
                    ServerVersion.AutoDetect(connectionString),
                    // Optional: Configure Pomelo specific options
                    mySqlOptions =>
                    {
                        mySqlOptions.EnableRetryOnFailure();
                        // Add any other specific MySQL options here if needed
                    }
                ));

            // 3. Register ASP.NET Core Identity Services
            builder.Services.AddDefaultIdentity<User>(options =>
            {
                // Set identity options here (e.g., password complexity)
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
            })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            // 4. Add MVC (Controllers and Views)
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // 5. Run SeedData Logic (MUST run after app.Build())
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();
                try
                {
                    await SeedData.InitializeAsync(services);
                    logger.LogInformation("Database roles and initial users seeded successfully.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred seeding the DB.");
                }
            }
            // End SeedData Logic

            // Configure the HTTP request pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles(); // Enables serving CSS, JavaScript, and images
            app.UseRouting();

            // Authentication MUST come before Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Map the default MVC route to Account/Login
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}");

            await app.RunAsync(); // *** Changed to await app.RunAsync() ***
        }
    }
}
