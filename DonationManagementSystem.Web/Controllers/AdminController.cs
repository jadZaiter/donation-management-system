using DonationManagementSystem.Application.DonationCases;
using DonationManagementSystem.Application.DonationCases.Models;
using DonationManagementSystem.Application.Payments;
using DonationManagementSystem.Application.Payments.Models;
using DonationManagementSystem.Domain.Entities;
using DonationManagementSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;


namespace DonationManagementSystem.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly PaymentWorkflow _paymentWorkflow;
        private readonly DonationCaseWorkflow _caseWorkflow;
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(
            ApplicationDbContext db,
            PaymentWorkflow paymentWorkflow,
            DonationCaseWorkflow caseWorkflow,
            UserManager<IdentityUser> userManager)
        {
            _db = db;
            _paymentWorkflow = paymentWorkflow;
            _caseWorkflow = caseWorkflow;
            _userManager = userManager;
        }

        // ✅ List pending cases (listing only; OK to stay in Web for now)
        public async Task<IActionResult> PendingCases()
        {
            var cases = await _db.DonationCases
                .Where(c => c.Status == CaseStatus.Pending)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(cases);
        }

        // ✅ Approve case (moved to Application)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, string? note)
        {
            var adminId = _userManager.GetUserId(User) ?? "admin";


            await _caseWorkflow.ApproveAsync(new ReviewDonationCaseRequest
            {
                CaseId = id,
                AdminId = adminId,
                Note = note
            });

            return RedirectToAction(nameof(PendingCases));
        }

        // ✅ Reject case (moved to Application)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string? note)
        {
            var adminId = _userManager.GetUserId(User) ?? "admin";


            await _caseWorkflow.RejectAsync(new ReviewDonationCaseRequest
            {
                CaseId = id,
                AdminId = adminId,
                Note = note
            });

            return RedirectToAction(nameof(PendingCases));
        }

        // ✅ Payments review (ProofUploaded)
        public async Task<IActionResult> PendingPayments()
        {
            var list = await _paymentWorkflow.GetPendingReviewAsync();
            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApprovePayment(int paymentId, string? note)
        {
            var adminId = _userManager.GetUserId(User) ?? "admin";


            await _paymentWorkflow.ApproveAsync(new ReviewPaymentRequest
            {
                PaymentId = paymentId,
                AdminId = adminId,
                Note = note
            });

            return RedirectToAction(nameof(PendingPayments));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectPayment(int paymentId, string? note)
        {
            var adminId = _userManager.GetUserId(User) ?? "admin";


            await _paymentWorkflow.RejectAsync(new ReviewPaymentRequest
            {
                PaymentId = paymentId,
                AdminId = adminId,
                Note = note
            });

            return RedirectToAction(nameof(PendingPayments));
        }
        public async Task<IActionResult> ReviewedCases()
        {
            var cases = await _db.DonationCases
                .Where(c => c.Status == CaseStatus.Approved || c.Status == CaseStatus.Rejected)
                .OrderByDescending(c => c.ReviewedAt)
                .ToListAsync();

            return View(cases);
        }

    }
}
