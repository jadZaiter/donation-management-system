using DonationManagementSystem.Application.Common.Interfaces;
using DonationManagementSystem.Application.Payments.Models;
using DonationManagementSystem.Domain.Entities;

namespace DonationManagementSystem.Application.Payments
{
    public class PaymentWorkflow
    {
        private readonly IUnitOfWork _uow;
        private readonly INotificationService _notificationService;

        public PaymentWorkflow(IUnitOfWork uow, INotificationService notificationService)
        {
            _uow = uow;
            _notificationService = notificationService;
        }

        public async Task<int> StartBankTransferAsync(CreatePaymentRequest request)
        {
            if (request.Amount <= 0)
                throw new ArgumentException("Amount must be greater than 0.");

            var donationCase = await _uow.DonationCases.GetByIdAsync(request.CaseId);
            if (donationCase == null)
                throw new InvalidOperationException("Donation case not found.");

            var payment = new Payment
            {
                DonationCaseId = request.CaseId,
                UserId = request.UserId,
                Amount = request.Amount,
                Method = PaymentMethod.BankTransfer,
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _uow.Payments.AddAsync(payment);
            await _uow.SaveChangesAsync();

            return payment.Id;
        }

        public async Task UploadProofAsync(UploadProofRequest request)
        {
            var payment = await _uow.Payments.GetByIdAsync(request.PaymentId);
            if (payment == null)
                throw new InvalidOperationException("Payment not found.");

            payment.ProofPath = request.ProofPath;
            payment.Status = PaymentStatus.ProofUploaded;

            _uow.Payments.Update(payment);
            await _uow.SaveChangesAsync();

            var donationCase = await _uow.DonationCases.GetByIdAsync(payment.DonationCaseId);

            await _notificationService.CreateForAdminsAsync(
                title: "Payment Proof Uploaded",
                message: $"Payment proof uploaded for case: {donationCase?.Title}",
                link: $"/DonationCases/Details/{donationCase?.Id}",
                type: NotificationType.ProofUploaded
            );
        }

        public async Task ApproveAsync(ReviewPaymentRequest request)
        {
            var payment = await _uow.Payments.GetByIdAsync(request.PaymentId);
            if (payment == null)
                throw new InvalidOperationException("Payment not found.");

            payment.Status = PaymentStatus.Approved;
            payment.ReviewedAt = DateTime.UtcNow;
            payment.ReviewedByUserId = request.AdminId;
            payment.AdminNote = request.Note;

            _uow.Payments.Update(payment);
            await _uow.SaveChangesAsync();

            var donationCase = await _uow.DonationCases.GetByIdAsync(payment.DonationCaseId);

            await _notificationService.CreateForUserAsync(
                userId: payment.UserId,
                title: "Payment Approved",
                message: $"Your payment of {payment.Amount:C} has been approved!",
                link: $"/DonationCases/Details/{payment.DonationCaseId}",
                type: NotificationType.PaymentApproved
            );
        }

        public async Task RejectAsync(ReviewPaymentRequest request)
        {
            var payment = await _uow.Payments.GetByIdAsync(request.PaymentId);
            if (payment == null)
                throw new InvalidOperationException("Payment not found.");

            payment.Status = PaymentStatus.Rejected;
            payment.ReviewedAt = DateTime.UtcNow;
            payment.ReviewedByUserId = request.AdminId;
            payment.AdminNote = request.Note;

            _uow.Payments.Update(payment);
            await _uow.SaveChangesAsync();

            await _notificationService.CreateForUserAsync(
                userId: payment.UserId,
                title: "Payment Rejected",
                message: $"Your payment of {payment.Amount:C} was rejected. Reason: {request.Note}",
                link: $"/DonationCases/Details/{payment.DonationCaseId}",
                type: NotificationType.PaymentRejected
            );
        }
    }
}