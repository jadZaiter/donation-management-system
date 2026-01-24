using DonationManagementSystem.Application.Payments;
using DonationManagementSystem.Application.Payments.Models;
using DonationManagementSystem.Domain.Entities;
using DonationManagementSystem.Infrastructure.Data;
using DonationManagementSystem.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Microsoft.AspNetCore.SignalR;
using DonationManagementSystem.Web.Hubs;

namespace DonationManagementSystem.Web.Controllers
{
    public class DonationCasesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly PaymentWorkflow _paymentWorkflow;

        private readonly IHubContext<AdminNotificationHub> _hub;

        public DonationCasesController(
            ApplicationDbContext db,
            UserManager<IdentityUser> userManager,
            IWebHostEnvironment env,
            PaymentWorkflow paymentWorkflow,
            IHubContext<AdminNotificationHub> hub)
        {
            _db = db;
            _userManager = userManager;
            _env = env;
            _paymentWorkflow = paymentWorkflow;
            _hub = hub;
        }

        // ✅ Submit Case Page
        [Authorize]
        public IActionResult Create()
        {
            return View(new DonationCaseCreateVm());
        }

        // ✅ Submit Case (Pending)
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DonationCaseCreateVm vm)
        {
            // 1️⃣ DataAnnotations validation (client + server)
            if (!ModelState.IsValid)
                return View(vm);

            // 2️⃣ Extra server-only image validation
            if (vm.ImageFile != null)
            {
                if (!vm.ImageFile.ContentType.StartsWith("image/"))
                {
                    ModelState.AddModelError(nameof(vm.ImageFile), "Please upload an image file only.");
                    return View(vm);
                }

                if (vm.ImageFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError(nameof(vm.ImageFile), "Image must be 5MB or less.");
                    return View(vm);
                }
            }

            // 3️⃣ Save image (optional)
            string? imagePath = null;

            if (vm.ImageFile != null && vm.ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "cases");
                Directory.CreateDirectory(uploadsFolder);

                var extension = Path.GetExtension(vm.ImageFile.FileName);
                var fileName = $"{Guid.NewGuid()}{extension}";
                var fullPath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await vm.ImageFile.CopyToAsync(stream);
                }

                imagePath = $"/uploads/cases/{fileName}";
            }

            // 4️⃣ Create entity
            var userId = _userManager.GetUserId(User)!;

            var donationCase = new DonationCase
            {
                Title = vm.Title,
                Description = vm.Description,
                TargetAmount = vm.TargetAmount,
                Status = CaseStatus.Pending,
                CreatedByUserId = userId,
                ImagePath = imagePath
            };

            // 5️⃣ Logging (user intent)
            Log.Information(
                "Donation case submitted. Title: {Title}, Target: {Target}, UserId: {UserId}",
                vm.Title, vm.TargetAmount, userId);

            // 6️⃣ Save
            _db.DonationCases.Add(donationCase);
            await _db.SaveChangesAsync();

            TempData["Message"] = "Case submitted successfully. Waiting for admin approval.";
            return RedirectToAction(nameof(Create));
        }

        // ✅ Details (Approved only)
        public async Task<IActionResult> Details(int id)
        {
            var item = await _db.DonationCases
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && c.Status == CaseStatus.Approved);

            if (item == null) return NotFound();

            // Aggregates in SQL (fast)
            var collected = await _db.Donations
                .AsNoTracking()
                .Where(d => d.DonationCaseId == id)
                .SumAsync(d => (decimal?)d.Amount) ?? 0m;

            var donorsCount = await _db.Donations
                .AsNoTracking()
                .Where(d => d.DonationCaseId == id)
                .Select(d => d.UserId)
                .Distinct()
                .CountAsync();

            // Comments only (don’t load donations)
            item.Comments = await _db.Comments
                .AsNoTracking()
                .Where(c => c.DonationCaseId == id)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            ViewBag.Collected = collected;
            ViewBag.DonorsCount = donorsCount;

            return View(item);
        }


        // ✅ Donate => create Payment (Pending) via Application workflow (ViewModel validation)
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Donate(DonateVm vm)
        {
            // 1️⃣ DataAnnotations validation
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please enter a valid amount.";
                return RedirectToAction(nameof(Details), new { id = vm.CaseId });
            }

            var userId = _userManager.GetUserId(User)!;

            try
            {
                var paymentId = await _paymentWorkflow.StartBankTransferAsync(
                    new CreatePaymentRequest
                    {
                        CaseId = vm.CaseId,
                        UserId = userId,
                        Amount = vm.Amount
                    });

                TempData["Message"] = "Payment created. Please upload proof to complete verification.";
                return RedirectToAction(nameof(UploadProof), new { paymentId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id = vm.CaseId });
            }
        }

        // ✅ Comments (still direct DB for now)
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

        // ✅ Upload proof (GET)
        [Authorize]
        public IActionResult UploadProof(int paymentId)
        {
            ViewBag.PaymentId = paymentId;
            return View();
        }

        // ✅ Upload proof (POST) => Application workflow
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

            if (proofFile.Length > 5 * 1024 * 1024)
            {
                TempData["Error"] = "Proof image must be 5MB or less.";
                return RedirectToAction(nameof(UploadProof), new { paymentId });
            }

            var userId = _userManager.GetUserId(User)!;

            // ✅ Save file
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

            // ✅ Save proof in DB via Application workflow
            await _paymentWorkflow.UploadProofAsync(new UploadProofRequest
            {
                PaymentId = paymentId,
                UserId = userId,
                ProofPath = proofPath
            });

            // ✅ STEP 5 FIX: Notify admins in real-time
            await _hub.Clients.Group("Admins").SendAsync("PaymentProofUploaded", new
            {
                PaymentId = paymentId,
                CaseId = (int?)null,         // optional later
                UserId = userId,
                ProofPath = proofPath,
                Time = DateTime.UtcNow
            });

            TempData["Message"] = "Proof uploaded. Waiting for admin verification.";
            return RedirectToAction(nameof(MyPayments));
        }

        // ✅ MyPayments (via Application workflow)
        [Authorize]
        public async Task<IActionResult> MyPayments()
        {
            var userId = _userManager.GetUserId(User)!;
            var payments = await _paymentWorkflow.GetMyPaymentsAsync(userId);
            return View(payments);
        }
    }
}
