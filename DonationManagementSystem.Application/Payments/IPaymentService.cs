using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DonationManagementSystem.Domain.Entities;

namespace DonationManagementSystem.Application.Payments
{
    public interface IPaymentService
    {
        Task<int> CreatePaymentAsync(int caseId, string userId, decimal amount);
        Task UploadProofAsync(int paymentId, string userId, string proofPath);
        Task<List<Payment>> GetMyPaymentsAsync(string userId);

        Task<List<Payment>> GetPendingReviewAsync(); // proof uploaded
        Task ApproveAsync(int paymentId, string adminUserId, string? note);
        Task RejectAsync(int paymentId, string adminUserId, string? note);

        Task<decimal> GetCollectedAmountAsync(int caseId);
        Task<decimal> GetTargetAmountAsync(int caseId);

    }
}
