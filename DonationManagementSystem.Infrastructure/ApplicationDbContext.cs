using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DonationManagementSystem.Domain.Entities;


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

        // Later:
        // public DbSet<Donation> Donations { get; set; }
    }
}
