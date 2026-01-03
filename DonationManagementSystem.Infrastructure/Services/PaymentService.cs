using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DonationManagementSystem.Application.Payments;
using DonationManagementSystem.Domain.Entities;
using DonationManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DonationManagementSystem.Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _db;

        public PaymentService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<int> CreatePaymentAsync(int caseId, string userId, decimal amount)
        {
            if (amount <= 0) throw new ArgumentException("Amount must be > 0");

            var c = await _db.DonationCases.FirstOrDefaultAsync(x => x.Id == caseId && x.Status == CaseStatus.Approved);
            if (c == null) throw new InvalidOperationException("Case not found or not approved.");

            var payment = new Payment
            {
                DonationCaseId = caseId,
                UserId = userId,
                Amount = amount,
                Status = PaymentStatus.Pending,
                Method = PaymentMethod.BankTransfer,
                CreatedAt = DateTime.UtcNow
            };

            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();
            return payment.Id;
        }

        public async Task UploadProofAsync(int paymentId, string userId, string proofPath)
        {
            var payment = await _db.Payments.FirstOrDefaultAsync(p => p.Id == paymentId && p.UserId == userId);
            if (payment == null) throw new InvalidOperationException("Payment not found.");

            payment.ProofPath = proofPath;
            payment.Status = PaymentStatus.ProofUploaded;

            await _db.SaveChangesAsync();
        }

        public async Task<List<Payment>> GetMyPaymentsAsync(string userId)
        {
            return await _db.Payments
                .Include(p => p.DonationCase)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Payment>> GetPendingReviewAsync()
        {
            return await _db.Payments
                .Include(p => p.DonationCase)
                .Where(p => p.Status == PaymentStatus.ProofUploaded)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task ApproveAsync(int paymentId, string adminUserId, string? note)
        {
            var payment = await _db.Payments.Include(p => p.DonationCase).FirstOrDefaultAsync(p => p.Id == paymentId);
            if (payment == null) throw new InvalidOperationException("Payment not found.");
            if (payment.Status != PaymentStatus.ProofUploaded) throw new InvalidOperationException("Payment is not ready for review.");

            payment.Status = PaymentStatus.Approved;
            payment.ReviewedAt = DateTime.UtcNow;
            payment.ReviewedByUserId = adminUserId;
            payment.AdminNote = note;

            //  Create real donation ONLY after approval
            _db.Donations.Add(new Donation
            {
                DonationCaseId = payment.DonationCaseId,
                UserId = payment.UserId,
                Amount = payment.Amount,
                DonatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
        }

        public async Task RejectAsync(int paymentId, string adminUserId, string? note)
        {
            var payment = await _db.Payments.FirstOrDefaultAsync(p => p.Id == paymentId);
            if (payment == null) throw new InvalidOperationException("Payment not found.");
            if (payment.Status != PaymentStatus.ProofUploaded) throw new InvalidOperationException("Payment is not ready for review.");

            payment.Status = PaymentStatus.Rejected;
            payment.ReviewedAt = DateTime.UtcNow;
            payment.ReviewedByUserId = adminUserId;
            payment.AdminNote = note;

            await _db.SaveChangesAsync();
        }
        public async Task<decimal> GetCollectedAmountAsync(int caseId)
        {
            return await _db.Donations
                .Where(d => d.DonationCaseId == caseId)
                .SumAsync(d => d.Amount);
        }

        public async Task<decimal> GetTargetAmountAsync(int caseId)
        {
            var c = await _db.DonationCases
                .Where(x => x.Id == caseId)
                .Select(x => x.TargetAmount)
                .FirstOrDefaultAsync();

            return c;
        }

    }
}
