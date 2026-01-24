using DonationManagementSystem.Application.Common.Interfaces;
using DonationManagementSystem.Application.DonationCases;
using DonationManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DonationManagementSystem.Infrastructure.Services
{
    public class DonationCaseService : IDonationCaseService
    {
        private readonly IUnitOfWork _uow;

        public DonationCaseService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<DonationCase?> GetByIdAsync(int id)
        {
            return await _uow.DonationCases.Query()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task ApproveAsync(int caseId, string adminUserId, string? note)
        {
            var donationCase = await _uow.DonationCases.Query()
                .FirstOrDefaultAsync(c => c.Id == caseId);

            if (donationCase == null)
                throw new InvalidOperationException("Case not found.");

            donationCase.Status = CaseStatus.Approved;
            donationCase.ReviewedAt = DateTime.UtcNow;
            donationCase.ReviewedByUserId = adminUserId;
            donationCase.AdminNote = note;

            _uow.DonationCases.Update(donationCase);
            await _uow.SaveChangesAsync();
        }

        public async Task RejectAsync(int caseId, string adminUserId, string? note)
        {
            var donationCase = await _uow.DonationCases.Query()
                .FirstOrDefaultAsync(c => c.Id == caseId);

            if (donationCase == null)
                throw new InvalidOperationException("Case not found.");

            donationCase.Status = CaseStatus.Rejected;
            donationCase.ReviewedAt = DateTime.UtcNow;
            donationCase.ReviewedByUserId = adminUserId;
            donationCase.AdminNote = note;

            _uow.DonationCases.Update(donationCase);
            await _uow.SaveChangesAsync();
        }

        // keep it if your interface still has it, but now it uses UoW
        public async Task SaveAsync()
        {
            await _uow.SaveChangesAsync();
        }
    }
}
