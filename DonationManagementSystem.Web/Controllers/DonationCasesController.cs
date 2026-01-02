using DonationManagementSystem.Domain.Entities;
using DonationManagementSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;


namespace DonationManagementSystem.Web.Controllers
{
    public class DonationCasesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public DonationCasesController(ApplicationDbContext db, UserManager<IdentityUser> userManager, IWebHostEnvironment env)
        {
            _db = db;
            _userManager = userManager;
            _env = env;
        }

        // ✅ Submit Case Page
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // ✅ Submit Case (Pending)
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string title, string description, decimal targetAmount, IFormFile? imageFile)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description) || targetAmount <= 0)
            {
                ModelState.AddModelError("", "Please fill all fields correctly.");
                return View();
            }

            // ✅ Save image (optional)
            string? imagePath = null;

            if (imageFile != null && imageFile.Length > 0)
            {
                // (Optional) basic validation: only images + max 5MB
                if (!imageFile.ContentType.StartsWith("image/"))
                {
                    ModelState.AddModelError("", "Please upload an image file only.");
                    return View();
                }

                if (imageFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("", "Image must be 5MB or less.");
                    return View();
                }

                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "cases");
                Directory.CreateDirectory(uploadsFolder);

                var extension = Path.GetExtension(imageFile.FileName);
                var fileName = $"{Guid.NewGuid()}{extension}";
                var fullPath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                imagePath = $"/uploads/cases/{fileName}";
            }

            var userId = _userManager.GetUserId(User);

            var donationCase = new DonationCase
            {
                Title = title,
                Description = description,
                TargetAmount = targetAmount,
                Status = CaseStatus.Pending,
                CreatedByUserId = userId!,
                ImagePath = imagePath
            };

            _db.DonationCases.Add(donationCase);
            await _db.SaveChangesAsync();

            TempData["Message"] = "Case submitted successfully. Waiting for admin approval.";
            return RedirectToAction("Create");
        }


        // ✅ Details (Approved only)
        public async Task<IActionResult> Details(int id)
        {
            var item = await _db.DonationCases
                .Include(c => c.Donations)
                .Include(c => c.Comments)
                   
                .FirstOrDefaultAsync(c => c.Id == id && c.Status == CaseStatus.Approved);

            if (item == null) return NotFound();

            return View(item);
        }


        // ✅ Donate
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Donate(int caseId, decimal amount)
        {
            if (amount <= 0)
            {
                TempData["Error"] = "Please enter a valid amount.";
                return RedirectToAction("Details", new { id = caseId });
            }

            var donationCase = await _db.DonationCases
                .FirstOrDefaultAsync(c => c.Id == caseId && c.Status == CaseStatus.Approved);

            if (donationCase == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);

            _db.Donations.Add(new Donation
            {
                DonationCaseId = caseId,
                UserId = userId!,
                Amount = amount,
                DonatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            TempData["Message"] = "Thank you for your donation!";
            return RedirectToAction("Details", new { id = caseId });
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int caseId, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                TempData["Error"] = "Comment cannot be empty.";
                return RedirectToAction("Details", new { id = caseId });
            }

            var donationCase = await _db.DonationCases
                .FirstOrDefaultAsync(c => c.Id == caseId && c.Status == CaseStatus.Approved);

            if (donationCase == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);

            _db.Comments.Add(new Comment
            {
                DonationCaseId = caseId,
                UserId = userId!,
                Text = text.Trim(),
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            TempData["Message"] = "Comment added.";
            return RedirectToAction("Details", new { id = caseId });
        }
        [Authorize]
        public async Task<IActionResult> MyCases()
        {
            var userId = _userManager.GetUserId(User);

            var cases = await _db.DonationCases
                .Where(c => c.CreatedByUserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(cases);
        }


    }
}
