using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using ContractMonthlyClaimsSystem.Models; // *** IMPORTANT: Update this to your correct Models namespace ***

namespace ContractClaimSystem
{
    public class Program
    {
        public static void Main(string[] args)
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
                options.SignIn.RequireConfirmedAccount = false; // Set to true if email confirmation is required
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
            })
                .AddRoles<IdentityRole>() // *** Essential for Lecturer/Coordinator roles ***
                .AddEntityFrameworkStores<ApplicationDbContext>();

            // 4. Add MVC (Controllers and Views)
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles(); // Enables serving CSS, JavaScript, and images
            app.UseRouting();

            // *** Authentication MUST come before Authorization ***
            app.UseAuthentication();
            app.UseAuthorization();

            // Map the default MVC route to Account/Login
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}");

            app.Run();
        }
    }
}