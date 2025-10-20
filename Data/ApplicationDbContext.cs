using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ContractMonthlyClaimsSystem.Models
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

            // Optional: Rename tables if you want more readable names in the DB
            builder.Entity<Claim>().ToTable("Claims");
            builder.Entity<SupportingDocument>().ToTable("SupportingDocuments");
            builder.Entity<User>().ToTable("Users");

            // Relationship: Claim has many SupportingDocuments
            builder.Entity<Claim>()
                .HasMany(c => c.SupportingDocuments)
                .WithOne(d => d.Claim)
                .HasForeignKey(d => d.ClaimId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship: User (Lecturer) has many Claims
            builder.Entity<User>()
                .HasMany(u => u.Claims)
                .WithOne(c => c.Lecturer)
                .HasForeignKey(c => c.LecturerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
