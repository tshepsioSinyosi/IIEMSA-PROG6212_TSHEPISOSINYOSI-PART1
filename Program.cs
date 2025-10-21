using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using ContractMonthlyClaimsSystem.Models;
using ContractClaimSystem.Data;

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

            // 2. Register DbContext (MySQL)
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(
                    connectionString,
                    ServerVersion.AutoDetect(connectionString),
                    mySqlOptions => mySqlOptions.EnableRetryOnFailure()
                ));

            // 3. Identity configuration
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

            // 4. Other services
            builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // 5. Seed initial roles and data
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();

                try
                {
                    await SeedData.InitializeAsync(services);
                    await RoleSeeder.SeedRolesAsync(services);
                    logger.LogInformation("Database seeded successfully.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred seeding the DB.");
                }
            }

            // 6. Middleware
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
    }
}
