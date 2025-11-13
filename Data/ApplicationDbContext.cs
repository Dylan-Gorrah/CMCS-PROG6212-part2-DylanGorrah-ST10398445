using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CMS_ASSIGNMENT.Models;

namespace CMS_ASSIGNMENT.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Claim> Claims { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure relationships
            builder.Entity<Claim>()
                .HasOne(c => c.Lecturer)
                .WithMany(u => u.SubmittedClaims)
                .HasForeignKey(c => c.LecturerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Claim>()
                .HasOne(c => c.Coordinator)
                .WithMany(u => u.CoordinatedClaims)
                .HasForeignKey(c => c.CoordinatorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Claim>()
                .HasOne(c => c.ApprovedBy)
                .WithMany()
                .HasForeignKey(c => c.ApprovedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Claim>()
                .HasOne(c => c.RejectedBy)
                .WithMany()
                .HasForeignKey(c => c.RejectedById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}