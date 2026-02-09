using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DonationManagementSystem.Domain.Entities;
using DonationManagementSystem.Application.Common.Interfaces;


namespace DonationManagementSystem.Infrastructure.Data
{
    public class ApplicationDbContext
        : IdentityDbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<DonationCase> DonationCases => Set<DonationCase>();
        public DbSet<Donation> Donations => Set<Donation>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<DonationCaseTag> DonationCaseTags => Set<DonationCaseTag>();
        public DbSet<Notification> Notifications => Set<Notification>(); // ? ADD THIS

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure DonationCaseTag composite key
            modelBuilder.Entity<DonationCaseTag>()
                .HasKey(dct => new { dct.DonationCaseId, dct.TagId });

            // Configure DonationCaseTag relationships
            modelBuilder.Entity<DonationCaseTag>()
                .HasOne(dct => dct.DonationCase)
                .WithMany(dc => dc.DonationCaseTags)
                .HasForeignKey(dct => dct.DonationCaseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DonationCaseTag>()
                .HasOne(dct => dct.Tag)
                .WithMany(t => t.DonationCaseTags)
                .HasForeignKey(dct => dct.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Category relationship
            modelBuilder.Entity<DonationCase>()
                .HasOne(dc => dc.Category)
                .WithMany(c => c.DonationCases)
                .HasForeignKey(dc => dc.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure unique constraints
            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Slug)
                .IsUnique();

            modelBuilder.Entity<Tag>()
                .HasIndex(t => t.Slug)
                .IsUnique();

            // ? Configure Notification entity
            modelBuilder.Entity<Notification>()
                .HasKey(n => n.Id);

            modelBuilder.Entity<Notification>()
                .HasIndex(n => n.UserId)
                .IsUnique(false);

            modelBuilder.Entity<Notification>()
                .HasIndex(n => new { n.UserId, n.IsRead, n.CreatedAt })
                .IsUnique(false);
        }
    }
}