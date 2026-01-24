using DonationManagementSystem.Domain.Entities;

namespace DonationManagementSystem.Web.ViewModels.Dashboard
{
    public class AdminDashboardVm
    {
        public int PendingCasesCount { get; set; }
        public int ApprovedCasesCount { get; set; }
        public int RejectedCasesCount { get; set; }

        public int PendingPaymentsCount { get; set; } // ProofUploaded usually
        public int ApprovedPaymentsCount { get; set; }
        public int RejectedPaymentsCount { get; set; }

        public decimal TotalApprovedDonations { get; set; }

        public List<DonationCase> LatestPendingCases { get; set; } = new();
        public List<Payment> LatestPendingPayments { get; set; } = new();
    }
}
