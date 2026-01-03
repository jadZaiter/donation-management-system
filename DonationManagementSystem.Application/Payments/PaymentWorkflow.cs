using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DonationManagementSystem.Application.Payments.Models;
using DonationManagementSystem.Domain.Entities;

namespace DonationManagementSystem.Application.Payments
{
    public class PaymentWorkflow
    {
        private readonly IPaymentService _payments;

        public PaymentWorkflow(IPaymentService payments)
        {
            _payments = payments;
        }

        public async Task<int> StartBankTransferAsync(CreatePaymentRequest req)
        {
            if (req.Amount <= 0) throw new ArgumentException("Amount must be greater than 0.");
            var target = await _payments.GetTargetAmountAsync(req.CaseId);
            var collected = await _payments.GetCollectedAmountAsync(req.CaseId);

            if (collected >= target)
                throw new InvalidOperationException("This case has already reached its target and no longer accepts donations.");

            // (optional later) add domain rules: case open, not completed, etc.
            return await _payments.CreatePaymentAsync(req.CaseId, req.UserId, req.Amount);
        }

        public async Task UploadProofAsync(UploadProofRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.ProofPath))
                throw new ArgumentException("ProofPath is required.");

            await _payments.UploadProofAsync(req.PaymentId, req.UserId, req.ProofPath);
        }

        public async Task ApproveAsync(ReviewPaymentRequest req)
        {
            await _payments.ApproveAsync(req.PaymentId, req.AdminId, req.Note);
        }

        public async Task RejectAsync(ReviewPaymentRequest req)
        {
            await _payments.RejectAsync(req.PaymentId, req.AdminId, req.Note);
        }
        public Task<List<Payment>> GetMyPaymentsAsync(string userId)
        {
            return _payments.GetMyPaymentsAsync(userId);
        }
        public Task<List<Payment>> GetPendingReviewAsync()
        {
            return _payments.GetPendingReviewAsync();
        }

    }
}