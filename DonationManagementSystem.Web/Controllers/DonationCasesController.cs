using DonationManagementSystem.Domain.Entities;
using DonationManagementSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DonationManagementSystem.Web.Controllers
{
    public class DonationCasesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public DonationCasesController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // Show submit form
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // Submit case (creates Pending)
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string title, string description, decimal targetAmount)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description) || targetAmount <= 0)
            {
                ModelState.AddModelError("", "Please fill all fields correctly.");
                return View();
            }

            var userId = _userManager.GetUserId(User);

            var donationCase = new DonationCase
            {
                Title = title,
                Description = description,
                TargetAmount = targetAmount,
                Status = CaseStatus.Pending,
                CreatedByUserId = userId!
            };

            _db.DonationCases.Add(donationCase);
            await _db.SaveChangesAsync();

            TempData["Message"] = "Case submitted successfully. Waiting for admin approval.";
            return RedirectToAction("Create");
        }
    }
}
