using DonationManagementSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DonationManagementSystem.Application.Common.Interfaces
{
    public interface IUnitOfWork
    {
        IRepository<DonationCase> DonationCases { get; }
        IRepository<Donation> Donations { get; }
        IRepository<Payment> Payments { get; }
        IRepository<Comment> Comments { get; }
        IRepository<Notification> Notifications { get; } // ✅ NEW
        IRepository<Category> Categories { get; } // ✅ NEW
        IRepository<Tag> Tags { get; } // ✅ NEW

        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}