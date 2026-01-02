using DonationManagementSystem.Infrastructure.Data;
using DonationManagementSystem.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DonationManagementSystem.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AdminController(ApplicationDbContext db)
        {
            _db = db;
        }

        // List pending cases
        public async Task<IActionResult> PendingCases()
        {
            var cases = await _db.DonationCases
                .Where(c => c.Status == CaseStatus.Pending)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(cases);
        }

        // Approve
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var donationCase = await _db.DonationCases.FindAsync(id);
            if (donationCase == null) return NotFound();

            donationCase.Status = CaseStatus.Approved;
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(PendingCases));
        }

        // Reject
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var donationCase = await _db.DonationCases.FindAsync(id);
            if (donationCase == null) return NotFound();

            donationCase.Status = CaseStatus.Rejected;
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(PendingCases));
        }
    }
}