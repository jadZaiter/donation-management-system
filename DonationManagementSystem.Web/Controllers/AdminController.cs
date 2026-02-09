using DonationManagementSystem.Application.Common.Interfaces;
using DonationManagementSystem.Application.DonationCases;
using DonationManagementSystem.Application.Payments;
using DonationManagementSystem.Application.Payments.Models;
using DonationManagementSystem.Domain.Entities;
using DonationManagementSystem.Infrastructure.Data;
using DonationManagementSystem.Infrastructure.Services; // ✅ ADD THIS
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;


namespace DonationManagementSystem.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly PaymentWorkflow _paymentWorkflow;
        private readonly DonationCaseWorkflow _caseWorkflow;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly INotificationService _notificationService;
        private readonly PaymentService _paymentService; // ✅ ADD THIS

        public AdminController(
            ApplicationDbContext db,
            PaymentWorkflow paymentWorkflow,
            DonationCaseWorkflow caseWorkflow,
            UserManager<IdentityUser> userManager,
            INotificationService notificationService,
            PaymentService paymentService) // ✅ ADD THIS
        {
            _db = db;
            _paymentWorkflow = paymentWorkflow;
            _caseWorkflow = caseWorkflow;
            _userManager = userManager;
            _notificationService = notificationService;
            _paymentService = paymentService; // ✅ ADD THIS
        }

        // ✅ List pending cases
        [HttpGet("PendingCases")]
        public async Task<IActionResult> PendingCases()
        {
            var cases = await _db.DonationCases
                .AsNoTracking()
                .Where(c => c.Status == CaseStatus.Pending)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(cases);
        }

        // ✅ Approve case - GET (show form)
        [HttpGet("Approve/{caseId}")]
        public async Task<IActionResult> Approve(int caseId)
        {
            var donationCase = await _db.DonationCases
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == caseId);

            if (donationCase == null)
                return NotFound();

            ViewBag.CaseId = caseId;
            ViewBag.CaseTitle = donationCase.Title;
            return View();
        }

        // ✅ Approve case - POST (execute approval)
        [HttpPost("ApproveConfirm")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveConfirm(int caseId, string? note)
        {
            var adminId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(adminId))
                return Unauthorized();

            try
            {
                await _caseWorkflow.ApproveAsync(caseId, adminId, note);
                TempData["Message"] = "Case approved successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(PendingCases));
        }

        // ✅ Reject case - GET (show form)
        [HttpGet("Reject/{caseId}")]
        public async Task<IActionResult> Reject(int caseId)
        {
            var donationCase = await _db.DonationCases
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == caseId);

            if (donationCase == null)
                return NotFound();

            ViewBag.CaseId = caseId;
            ViewBag.CaseTitle = donationCase.Title;
            return View();
        }

        // ✅ Reject case - POST (execute rejection)
        [HttpPost("RejectConfirm")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectConfirm(int caseId, string? note)
        {
            var adminId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(adminId))
                return Unauthorized();

            try
            {
                await _caseWorkflow.RejectAsync(caseId, adminId, note);
                TempData["Message"] = "Case rejected successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(PendingCases));
        }

        // ✅ Pending payments review - UPDATED
        [HttpGet("PendingPayments")]
        public async Task<IActionResult> PendingPayments()
        {
                var list = await _paymentService.GetPendingReviewAsync(); // ✅ CHANGED
            return View(list);
        }

        // ✅ Approve payment
        [HttpPost("ApprovePayment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApprovePayment(int paymentId, string? note)
        {
            var adminId = _userManager.GetUserId(User) ?? "admin";

            try
            {
                await _paymentWorkflow.ApproveAsync(new ReviewPaymentRequest
                {
                    PaymentId = paymentId,
                    AdminId = adminId,
                    Note = note
                });
                TempData["Message"] = "Payment approved successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(PendingPayments));
        }

        // ✅ Reject payment
        [HttpPost("RejectPayment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectPayment(int paymentId, string? note)
        {
            var adminId = _userManager.GetUserId(User) ?? "admin";

            try
            {
                await _paymentWorkflow.RejectAsync(new ReviewPaymentRequest
                {
                    PaymentId = paymentId,
                    AdminId = adminId,
                    Note = note
                });
                TempData["Message"] = "Payment rejected successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(PendingPayments));
        }

        // ✅ Reviewed cases
        [HttpGet("ReviewedCases")]
        public async Task<IActionResult> ReviewedCases()
        {
            var cases = await _db.DonationCases
                .AsNoTracking()
                .Where(c => c.Status == CaseStatus.Approved || c.Status == CaseStatus.Rejected)
                .OrderByDescending(c => c.ReviewedAt)
                .ToListAsync();

            return View(cases);
        }
    }
}
