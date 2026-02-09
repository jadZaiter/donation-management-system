using DonationManagementSystem.Application.Common.Interfaces;
using DonationManagementSystem.Application.Payments;
using DonationManagementSystem.Application.Payments.Models;
using DonationManagementSystem.Domain.Entities;
using DonationManagementSystem.Infrastructure.Data;
using DonationManagementSystem.Infrastructure.Services;
using DonationManagementSystem.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc; 
using Microsoft.EntityFrameworkCore;
using Serilog;
using Microsoft.AspNetCore.SignalR;
using DonationManagementSystem.Web.Hubs;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DonationManagementSystem.Web.Controllers
{
    [Route("DonationCases")]
    public class DonationCasesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly PaymentWorkflow _paymentWorkflow;
        private readonly IHubContext<AdminNotificationHub> _hub;
        private readonly PaymentService _paymentService;
        private readonly INotificationService _notificationService;

        public DonationCasesController(
            ApplicationDbContext db,
            UserManager<IdentityUser> userManager,
            IWebHostEnvironment env,
            PaymentWorkflow paymentWorkflow,
            IHubContext<AdminNotificationHub> hub,
            PaymentService paymentService,
            INotificationService notificationService)
        {
            _db = db;
            _userManager = userManager;
            _env = env;
            _paymentWorkflow = paymentWorkflow;
            _hub = hub;
            _paymentService = paymentService;
            _notificationService = notificationService;
        }

        // ✅ Submit Case Page - GET
        [HttpGet("Create")]
        [Authorize]
        public async Task<IActionResult> Create()
        {
            var categories = await _db.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();
            
            var tags = await _db.Tags
                .AsNoTracking()
                .OrderBy(t => t.Name)
                .ToListAsync();
            
            ViewBag.Categories = categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();
            
            ViewBag.Tags = tags;
            
            return View(new DonationCaseCreateVm());
        }

        // ✅ Submit Case - POST
        [HttpPost("Create")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DonationCaseCreateVm vm)
        {
            // 1️⃣ DataAnnotations validation (client + server)
            if (!ModelState.IsValid)
                return View(vm);

            // 2️⃣ Validate category exists
            var categoryExists = await _db.Categories.AnyAsync(c => c.Id == vm.CategoryId);
            if (!categoryExists)
            {
                ModelState.AddModelError(nameof(vm.CategoryId), "Selected category is invalid.");
                return View(vm);
            }

            // 3️⃣ Validate tags exist
            var validTagIds = await _db.Tags
                .Where(t => vm.TagIds.Contains(t.Id))
                .Select(t => t.Id)
                .ToListAsync();
            
            if (validTagIds.Count != vm.TagIds.Count)
            {
                ModelState.AddModelError(nameof(vm.TagIds), "One or more selected tags are invalid.");
                return View(vm);
            }

            // 4️⃣ Extra server-only image validation
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

            // 5️⃣ Save image (optional)
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

            // 6️⃣ Create entity with Category
            var userId = _userManager.GetUserId(User)!;

            var donationCase = new DonationCase
            {
                Title = vm.Title,
                Description = vm.Description,
                TargetAmount = vm.TargetAmount,
                CategoryId = vm.CategoryId,
                Status = CaseStatus.Pending,
                CreatedByUserId = userId,
                ImagePath = imagePath
            };

            // 7️⃣ Add tags (many-to-many)
            foreach (var tagId in vm.TagIds)
            {
                donationCase.DonationCaseTags.Add(new DonationCaseTag
                {
                    TagId = tagId
                });
            }

            // 8️⃣ Logging (user intent)
            Log.Information(
                "Donation case submitted. Title: {Title}, Target: {Target}, Category: {Category}, Tags: {Tags}, UserId: {UserId}",
                vm.Title, vm.TargetAmount, vm.CategoryId, string.Join(",", vm.TagIds), userId);

            // 9️⃣ Save case
            _db.DonationCases.Add(donationCase);
            await _db.SaveChangesAsync();

            // ✅ 🔟 CREATE NOTIFICATION FOR ADMINS
            await _notificationService.CreateForAdminsAsync(
                title: "New Case Submitted",
                message: $"New donation case submitted: {donationCase.Title}",
                link: $"/DonationCases/Details/{donationCase.Id}",
                type: NotificationType.CaseSubmitted
            );

            TempData["Message"] = "Case submitted successfully. Waiting for admin approval.";
            return RedirectToAction(nameof(Create));
        }

        // ✅ Details
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);
            
            var item = await _db.DonationCases
                .AsNoTracking()
                .Include(c => c.Comments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (item == null) 
                return NotFound();

            // ✅ Allow viewing if: case is approved OR user created it OR user is admin
            var isOwner = userId == item.CreatedByUserId;
            var isAdmin = User.IsInRole("Admin");
            var isApproved = item.Status == CaseStatus.Approved;

            if (!isApproved && !isOwner && !isAdmin)
                return NotFound();

            // ✅ ONLY count APPROVED payments
            var collected = await _db.Payments
                .AsNoTracking()
                .Where(d => d.DonationCaseId == id && d.Status == PaymentStatus.Approved)
                .SumAsync(d => (decimal?)d.Amount) ?? 0m;

            var donorsCount = await _db.Payments
                .AsNoTracking()
                .Where(d => d.DonationCaseId == id && d.Status == PaymentStatus.Approved)
                .Select(d => d.UserId)
                .Distinct()
                .CountAsync();

                ViewBag.Collected = collected;
            ViewBag.DonorsCount = donorsCount;
            ViewBag.IsOwner = isOwner;
            ViewBag.IsAdmin = isAdmin;

            return View(item);
        }

        // ✅ Donate
        [HttpPost("Donate")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Donate(DonateVm vm)
        {
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

        // ✅ Add Comment
        [HttpPost("AddComment")]
        [Authorize]
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

        // ✅ My Cases
        [HttpGet("MyCases")]
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

        // ✅ Upload Proof - GET
        [HttpGet("UploadProof/{paymentId}")]
        [Authorize]
        public IActionResult UploadProof(int paymentId)
        {
            ViewBag.PaymentId = paymentId;
            return View();
        }

        // ✅ Upload Proof - POST
        [HttpPost("UploadProof/{paymentId}")]
        [Authorize]
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

            await _hub.Clients.Group("Admins").SendAsync("PaymentProofUploaded", new
            {
                PaymentId = paymentId,
                CaseId = (int?)null,
                UserId = userId,
                ProofPath = proofPath,
                Time = DateTime.UtcNow
            });

            TempData["Message"] = "Proof uploaded. Waiting for admin verification.";
            return RedirectToAction(nameof(MyPayments));
        }

        // ✅ My Payments
        [HttpGet("MyPayments")]
        [Authorize]
        public async Task<IActionResult> MyPayments()
        {
            var userId = _userManager.GetUserId(User)!;
            var payments = await _paymentService.GetMyPaymentsAsync(userId);
            return View(payments);
        }
    }
}
