using DonationManagementSystem.Application.Common.Interfaces;
using DonationManagementSystem.Application.Payments;
using DonationManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DonationManagementSystem.Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _uow;

        public PaymentService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<int> CreatePaymentAsync(int caseId, string userId, decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be > 0");

            var donationCase = await _uow.DonationCases.Query()
                .FirstOrDefaultAsync(x => x.Id == caseId && x.Status == CaseStatus.Approved);

            if (donationCase == null)
                throw new InvalidOperationException("Case not found or not approved.");

            var payment = new Payment
            {
                DonationCaseId = caseId,
                UserId = userId,
                Amount = amount,
                Status = PaymentStatus.Pending,
                Method = PaymentMethod.BankTransfer,
                CreatedAt = DateTime.UtcNow
            };

            await _uow.Payments.AddAsync(payment);
            await _uow.SaveChangesAsync();

            return payment.Id;
        }

        public async Task UploadProofAsync(int paymentId, string userId, string proofPath)
        {
            var payment = await _uow.Payments.Query()
                .FirstOrDefaultAsync(p => p.Id == paymentId && p.UserId == userId);

            if (payment == null)
                throw new InvalidOperationException("Payment not found.");

            payment.ProofPath = proofPath;
            payment.Status = PaymentStatus.ProofUploaded;

            _uow.Payments.Update(payment);
            await _uow.SaveChangesAsync();
        }

        public async Task<List<Payment>> GetMyPaymentsAsync(string userId)
        {
            return await _uow.Payments.Query()
                .Include(p => p.DonationCase)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Payment>> GetPendingReviewAsync()
        {
            return await _uow.Payments.Query()
                .Include(p => p.DonationCase)
                .Where(p => p.Status == PaymentStatus.ProofUploaded)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task ApproveAsync(int paymentId, string adminUserId, string? note)
        {
            var payment = await _uow.Payments.Query()
                .Include(p => p.DonationCase)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
                throw new InvalidOperationException("Payment not found.");

            if (payment.Status != PaymentStatus.ProofUploaded)
                throw new InvalidOperationException("Payment is not ready for review.");

            payment.Status = PaymentStatus.Approved;
            payment.ReviewedAt = DateTime.UtcNow;
            payment.ReviewedByUserId = adminUserId;
            payment.AdminNote = note;

            // Create real donation AFTER approval
            await _uow.Donations.AddAsync(new Donation
            {
                DonationCaseId = payment.DonationCaseId,
                UserId = payment.UserId,
                Amount = payment.Amount,
                DonatedAt = DateTime.UtcNow
            });

            _uow.Payments.Update(payment);
            await _uow.SaveChangesAsync();
        }

        public async Task RejectAsync(int paymentId, string adminUserId, string? note)
        {
            var payment = await _uow.Payments.Query()
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
                throw new InvalidOperationException("Payment not found.");

            if (payment.Status != PaymentStatus.ProofUploaded)
                throw new InvalidOperationException("Payment is not ready for review.");

            payment.Status = PaymentStatus.Rejected;
            payment.ReviewedAt = DateTime.UtcNow;
            payment.ReviewedByUserId = adminUserId;
            payment.AdminNote = note;

            _uow.Payments.Update(payment);
            await _uow.SaveChangesAsync();
        }

        public async Task<decimal> GetCollectedAmountAsync(int caseId)
        {
            return await _uow.Donations.Query()
                .Where(d => d.DonationCaseId == caseId)
                .SumAsync(d => d.Amount);
        }

        public async Task<decimal> GetTargetAmountAsync(int caseId)
        {
            return await _uow.DonationCases.Query()
                .Where(x => x.Id == caseId)
                .Select(x => x.TargetAmount)
                .FirstOrDefaultAsync();
        }
    }
}
