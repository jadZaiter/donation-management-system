using DonationManagementSystem.Application.Common.Interfaces;
using DonationManagementSystem.Application.Common.Interfaces;
using DonationManagementSystem.Domain.Entities;
using DonationManagementSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }

        public IRepository<DonationCase> DonationCases { get; }
        public IRepository<Payment> Payments { get; }
        public IRepository<Comment> Comments { get; }
        public IRepository<Donation> Donations { get; }    

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }
}