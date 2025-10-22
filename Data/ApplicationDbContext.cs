using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ContractClaimSystem.Models
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Claim> Claims { get; set; }
        public DbSet<SupportingDocument> SupportingDocuments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Rename tables to match existing database
            builder.Entity<User>().ToTable("Users");
            builder.Entity<IdentityRole>().ToTable("AspNetRoles");
            builder.Entity<IdentityUserRole<string>>().ToTable("AspNetUserRoles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("AspNetUserClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("AspNetUserLogins");
            builder.Entity<IdentityUserToken<string>>().ToTable("AspNetUserTokens");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("AspNetRoleClaims");

            // Claim has many SupportingDocuments
            builder.Entity<Claim>()
                .HasMany(c => c.SupportingDocuments)
                .WithOne(d => d.Claim)
                .HasForeignKey(d => d.ClaimId)
                .OnDelete(DeleteBehavior.Cascade);

            // User (Lecturer) has many Claims
            builder.Entity<User>()
                .HasMany(u => u.Claims)
                .WithOne(c => c.Lecturer)
                .HasForeignKey(c => c.LecturerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
