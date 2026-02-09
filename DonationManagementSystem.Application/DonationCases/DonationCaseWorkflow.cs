using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DonationManagementSystem.Application.Common.Interfaces;
using DonationManagementSystem.Domain.Entities;

namespace DonationManagementSystem.Application.DonationCases
{
    public class DonationCaseWorkflow
    {
        private readonly IUnitOfWork _uow;
        private readonly INotificationService _notificationService;

        public DonationCaseWorkflow(IUnitOfWork uow, INotificationService notificationService)
        {
            _uow = uow;
            _notificationService = notificationService;
        }

        // ✅ When case is submitted
        public async Task SubmitAsync(DonationCase donationCase)
        {
            donationCase.Status = CaseStatus.Pending;
            donationCase.CreatedAt = DateTime.UtcNow;

            await _uow.DonationCases.AddAsync(donationCase);
            await _uow.SaveChangesAsync();

            // ✅ Notify admins
            await _notificationService.CreateForAdminsAsync(
                title: "New Case Submitted",
                message: $"New donation case submitted: {donationCase.Title}",
                link: $"/DonationCases/Details/{donationCase.Id}",
                type: NotificationType.CaseSubmitted
            );
        }

        // ✅ When case is approved
        public async Task ApproveAsync(int caseId, string adminUserId, string? note)
        {
            var donationCase = await _uow.DonationCases.GetByIdAsync(caseId);

            if (donationCase == null)
                throw new InvalidOperationException("Case not found.");

            donationCase.Status = CaseStatus.Approved;
            donationCase.ReviewedAt = DateTime.UtcNow;
            donationCase.ReviewedByUserId = adminUserId;
            donationCase.AdminNote = note;

            _uow.DonationCases.Update(donationCase);
            await _uow.SaveChangesAsync();

            // ✅ Notify user who submitted
            await _notificationService.CreateForUserAsync(
                userId: donationCase.CreatedByUserId,
                title: "Case Approved",
                message: $"Your case '{donationCase.Title}' has been approved!",
                link: $"/DonationCases/Details/{donationCase.Id}", // ✅ This is correct
                type: NotificationType.CaseApproved
            );
        }

        // ✅ When case is rejected
        public async Task RejectAsync(int caseId, string adminUserId, string? note)
        {
            var donationCase = await _uow.DonationCases.GetByIdAsync(caseId);

            if (donationCase == null)
                throw new InvalidOperationException("Case not found.");

            donationCase.Status = CaseStatus.Rejected;
            donationCase.ReviewedAt = DateTime.UtcNow;
            donationCase.ReviewedByUserId = adminUserId;
            donationCase.AdminNote = note;

            _uow.DonationCases.Update(donationCase);
            await _uow.SaveChangesAsync();

            // ✅ Notify user who submitted
            await _notificationService.CreateForUserAsync(
                userId: donationCase.CreatedByUserId,
                title: "Case Rejected",
                message: $"Your case '{donationCase.Title}' was not approved. Reason: {note}",
                link: $"/DonationCases/Details/{donationCase.Id}", // ✅ This is correct
                type: NotificationType.CaseRejected
            );
        }
    }
}
