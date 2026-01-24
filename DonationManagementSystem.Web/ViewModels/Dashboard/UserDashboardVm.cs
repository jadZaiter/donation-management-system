using DonationManagementSystem.Domain.Entities;

namespace DonationManagementSystem.Web.ViewModels.Dashboard
{
    public class UserDashboardVm
    {
        public int MyCasesCount { get; set; }
        public int MyPaymentsCount { get; set; }
        public decimal TotalDonatedApproved { get; set; }

        public int PendingPaymentsCount { get; set; }
        public int ApprovedPaymentsCount { get; set; }
        public int RejectedPaymentsCount { get; set; }

        public List<DonationCase> LatestCases { get; set; } = new();
        public List<Payment> LatestPayments { get; set; } = new();
    }
}
