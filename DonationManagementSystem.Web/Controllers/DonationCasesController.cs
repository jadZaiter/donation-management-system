using DonationManagementSystem.Application.Payments;
using DonationManagementSystem.Application.Payments.Models;
using DonationManagementSystem.Domain.Entities;
using DonationManagementSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DonationManagementSystem.Web.Controllers
{
    public class DonationCasesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly PaymentWorkflow _paymentWorkflow;

        public DonationCasesController(
            ApplicationDbContext db,
            UserManager<IdentityUser> userManager,
            IWebHostEnvironment env,
            PaymentWorkflow paymentWorkflow)
        {
            _db = db;
            _userManager = userManager;
            _env = env;
            _paymentWorkflow = paymentWorkflow;
        }

        //  Submit Case Page
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        //  Submit Case (Pending)
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

            //  Save image (optional)
            string? imagePath = null;

            if (imageFile != null && imageFile.Length > 0)
            {
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

            var userId = _userManager.GetUserId(User)!;

            var donationCase = new DonationCase
            {
                Title = title,
                Description = description,
                TargetAmount = targetAmount,
                Status = CaseStatus.Pending,
                CreatedByUserId = userId,
                ImagePath = imagePath
            };

            _db.DonationCases.Add(donationCase);
            await _db.SaveChangesAsync();

            TempData["Message"] = "Case submitted successfully. Waiting for admin approval.";
            return RedirectToAction(nameof(Create));
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

        //  Donate => create Payment (Pending) via Application workflow
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Donate(int caseId, decimal amount)
        {
            if (amount <= 0)
            {
                TempData["Error"] = "Please enter a valid amount.";
                return RedirectToAction(nameof(Details), new { id = caseId });
            }

            var userId = _userManager.GetUserId(User)!;

            try
            {
                var paymentId = await _paymentWorkflow.StartBankTransferAsync(new CreatePaymentRequest
                {
                    CaseId = caseId,
                    Amount = amount,
                    UserId = userId
                });

                TempData["Message"] = "Payment created. Please upload proof to complete verification.";
                return RedirectToAction(nameof(UploadProof), new { paymentId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id = caseId });
            }
        }

        //  Comments (still direct DB for now)
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int caseId, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                TempData["Error"] = "Comment cannot be empty.";
                return RedirectToAction(nameof(Details), new { id = caseId });
            }

            var donationCase = await _db.DonationCases
                .FirstOrDefaultAsync(c => c.Id == caseId && c.Status == CaseStatus.Approved);

            if (donationCase == null)
                return NotFound();

            var userId = _userManager.GetUserId(User)!;

            _db.Comments.Add(new Comment
            {
                DonationCaseId = caseId,
                UserId = userId,
                Text = text.Trim(),
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            TempData["Message"] = "Comment added.";
            return RedirectToAction(nameof(Details), new { id = caseId });
        }

        [Authorize]
        public async Task<IActionResult> MyCases()
        {
            var userId = _userManager.GetUserId(User)!;

            var cases = await _db.DonationCases
                .Where(c => c.CreatedByUserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(cases);
        }

        //  Upload proof (GET)
        [Authorize]
        public IActionResult UploadProof(int paymentId)
        {
            ViewBag.PaymentId = paymentId;
            return View();
        }

        // Upload proof (POST) => Application workflow
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadProof(int paymentId, IFormFile proofFile)
        {
            if (proofFile == null || proofFile.Length == 0)
            {
                TempData["Error"] = "Please choose a proof file.";
                return RedirectToAction(nameof(UploadProof), new { paymentId });
            }

            if (!proofFile.ContentType.StartsWith("image/"))
            {
                TempData["Error"] = "Proof must be an image (jpg/png).";
                return RedirectToAction(nameof(UploadProof), new { paymentId });
            }

            var userId = _userManager.GetUserId(User)!;

            var folder = Path.Combine(_env.WebRootPath, "uploads", "proofs");
            Directory.CreateDirectory(folder);

            var ext = Path.GetExtension(proofFile.FileName);
            var fileName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await proofFile.CopyToAsync(stream);
            }

            var proofPath = $"/uploads/proofs/{fileName}";

            await _paymentWorkflow.UploadProofAsync(new UploadProofRequest
            {
                PaymentId = paymentId,
                UserId = userId,
                ProofPath = proofPath
            });

            TempData["Message"] = "Proof uploaded. Waiting for admin verification.";
            return RedirectToAction(nameof(MyPayments));
        }

        //  MyPayments (via Application workflow)
        [Authorize]
        public async Task<IActionResult> MyPayments()
        {
            var userId = _userManager.GetUserId(User)!;
            var payments = await _paymentWorkflow.GetMyPaymentsAsync(userId);
            return View(payments);
        }
    }
}
