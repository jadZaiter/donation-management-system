using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DonationManagementSystem.Application.DonationCases.Models;
using DonationManagementSystem.Domain.Entities;



namespace DonationManagementSystem.Application.DonationCases
{
    public class DonationCaseWorkflow
    {
        private readonly IDonationCaseService _cases;

        public DonationCaseWorkflow(IDonationCaseService cases)
        {
            _cases = cases;
        }

        public async Task ApproveAsync(ReviewDonationCaseRequest req)
        {
            var donationCase = await _cases.GetByIdAsync(req.CaseId);
            if (donationCase == null)
                throw new InvalidOperationException("Case not found.");

            if (donationCase.Status != CaseStatus.Pending)
                throw new InvalidOperationException("Only pending cases can be approved.");

            donationCase.Status = CaseStatus.Approved;
            donationCase.ReviewedByUserId = req.AdminId;
            donationCase.ReviewedAt = DateTime.UtcNow;
            donationCase.AdminNote = req.Note;

            await _cases.SaveAsync();
        }

        public async Task RejectAsync(ReviewDonationCaseRequest req)
        {
            var donationCase = await _cases.GetByIdAsync(req.CaseId);
            if (donationCase == null)
                throw new InvalidOperationException("Case not found.");

            donationCase.Status = CaseStatus.Rejected;
            donationCase.ReviewedByUserId = req.AdminId;
            donationCase.ReviewedAt = DateTime.UtcNow;
            donationCase.AdminNote = req.Note;

            await _cases.SaveAsync();
        }
    }
}
