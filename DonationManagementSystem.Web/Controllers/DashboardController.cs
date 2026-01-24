using DonationManagementSystem.Domain.Entities;
using DonationManagementSystem.Infrastructure.Data;
using DonationManagementSystem.Web.ViewModels.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DonationManagementSystem.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public DashboardController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // ✅ USER DASHBOARD  /Dashboard
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;

            var myCasesQuery = _db.DonationCases
                .AsNoTracking()
                .Where(c => c.CreatedByUserId == userId);

            var myPaymentsQuery = _db.Payments
                .AsNoTracking()
                .Include(p => p.DonationCase)
                .Where(p => p.UserId == userId);

            var vm = new UserDashboardVm
            {
                MyCasesCount = await myCasesQuery.CountAsync(),
                MyPaymentsCount = await myPaymentsQuery.CountAsync(),

                TotalDonatedApproved = await myPaymentsQuery
                    .Where(p => p.Status == PaymentStatus.Approved)
                    .SumAsync(p => (decimal?)p.Amount) ?? 0m,

                PendingPaymentsCount = await myPaymentsQuery.CountAsync(p =>
                    p.Status == PaymentStatus.Pending || p.Status == PaymentStatus.ProofUploaded),

                ApprovedPaymentsCount = await myPaymentsQuery.CountAsync(p => p.Status == PaymentStatus.Approved),
                RejectedPaymentsCount = await myPaymentsQuery.CountAsync(p => p.Status == PaymentStatus.Rejected),

                LatestCases = await myCasesQuery
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(5)
                    .ToListAsync(),

                LatestPayments = await myPaymentsQuery
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(5)
                    .ToListAsync()
            };

            return View(vm);
        }

        // ✅ ADMIN DASHBOARD  /Dashboard/Admin
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Admin()
        {
            var casesQuery = _db.DonationCases.AsNoTracking();
            var paymentsQuery = _db.Payments
                .AsNoTracking()
                .Include(p => p.DonationCase);

            var vm = new AdminDashboardVm
            {
                PendingCasesCount = await casesQuery.CountAsync(c => c.Status == CaseStatus.Pending),
                ApprovedCasesCount = await casesQuery.CountAsync(c => c.Status == CaseStatus.Approved),
                RejectedCasesCount = await casesQuery.CountAsync(c => c.Status == CaseStatus.Rejected),

                PendingPaymentsCount = await paymentsQuery.CountAsync(p => p.Status == PaymentStatus.ProofUploaded),
                ApprovedPaymentsCount = await paymentsQuery.CountAsync(p => p.Status == PaymentStatus.Approved),
                RejectedPaymentsCount = await paymentsQuery.CountAsync(p => p.Status == PaymentStatus.Rejected),

                TotalApprovedDonations = await paymentsQuery
                    .Where(p => p.Status == PaymentStatus.Approved)
                    .SumAsync(p => (decimal?)p.Amount) ?? 0m,

                LatestPendingCases = await casesQuery
                    .Where(c => c.Status == CaseStatus.Pending)
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(5)
                    .ToListAsync(),

                LatestPendingPayments = await paymentsQuery
                    .Where(p => p.Status == PaymentStatus.ProofUploaded)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(5)
                    .ToListAsync()
            };

            return View(vm);
        }
    }
}
