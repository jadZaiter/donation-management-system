using DonationManagementSystem.Application.Common.Interfaces;
using DonationManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DonationManagementSystem.Infrastructure.Services
{
    public class PaymentService
    {
        private readonly IUnitOfWork _uow;

        public PaymentService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // ✅ Get pending payments for admin review
        public async Task<List<Payment>> GetPendingReviewAsync()
        {
            return await _uow.Payments.Query()
                .AsNoTracking()
                .Where(p => p.Status == PaymentStatus.ProofUploaded)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        // ✅ Get user's payments
        public async Task<List<Payment>> GetMyPaymentsAsync(string userId)
        {
            return await _uow.Payments.Query()
                .AsNoTracking()
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
    }
}
