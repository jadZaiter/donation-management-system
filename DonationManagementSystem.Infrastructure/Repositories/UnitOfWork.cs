using DonationManagementSystem.Application.Common.Interfaces;
using DonationManagementSystem.Domain.Entities;
using DonationManagementSystem.Infrastructure.Data;

namespace DonationManagementSystem.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;

            DonationCases = new Repository<DonationCase>(_db);
            Payments = new Repository<Payment>(_db);
            Comments = new Repository<Comment>(_db);
            Donations = new Repository<Donation>(_db);
            Notifications = new Repository<Notification>(_db);
            Categories = new Repository<Category>(_db);
            Tags = new Repository<Tag>(_db);
        }

        public IRepository<DonationCase> DonationCases { get; }
        public IRepository<Payment> Payments { get; }
        public IRepository<Comment> Comments { get; }
        public IRepository<Donation> Donations { get; }
        public IRepository<Notification> Notifications { get; }
        public IRepository<Category> Categories { get; }
        public IRepository<Tag> Tags { get; }

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }
}