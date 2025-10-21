using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System.IO;

// IMPORTANT: Replace this using statement with the correct namespace where your ApplicationDbContext is located.
using ContractMonthlyClaimsSystem.Models;

namespace ContractClaimSystem.Data
{
    // This factory is required because the migration tools cannot automatically
    // determine how to construct the DbContext when it's registered via Program.cs
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // The migration tool looks for appsettings.json in the project root.
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Could not find connection string 'DefaultConnection' in appsettings.json.");
            }

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            optionsBuilder.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString),
                mysqlOptions =>
                {
                    mysqlOptions.EnableRetryOnFailure();
                }
            );

            // The DbContext must have a constructor that accepts DbContextOptions<ApplicationDbContext>
            // which should already be defined in your ApplicationDbContext class.
            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
