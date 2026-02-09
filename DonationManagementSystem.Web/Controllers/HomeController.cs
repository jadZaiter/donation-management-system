using DonationManagementSystem.Application.DonationCases;
using DonationManagementSystem.Application.DonationCases.Dtos;
using DonationManagementSystem.Domain.Entities;
using DonationManagementSystem.Infrastructure.Data;
using DonationManagementSystem.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DonationManagementSystem.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDonationCaseService _caseService;
        private readonly ApplicationDbContext _db;

        public HomeController(IDonationCaseService caseService, ApplicationDbContext db)
        {
            _caseService = caseService;
            _db = db;
        }

        // ? Home page - display approved cases with collected amounts
        public async Task<IActionResult> Index()
        {
            var cases = await _db.DonationCases
                .AsNoTracking()
                .Where(c => c.Status == CaseStatus.Approved)
                .OrderByDescending(c => c.CreatedAt)
                .Take(6)
                .ToListAsync();

            var featuredCases = new List<CaseWithAmountDto>();

            foreach (var c in cases)
            {
                var collected = await _db.Payments
                    .AsNoTracking()
                    .Where(p => p.DonationCaseId == c.Id && p.Status == PaymentStatus.Approved)
                    .SumAsync(p => (decimal?)p.Amount) ?? 0m;

                var donorCount = await _db.Payments
                    .AsNoTracking()
                    .Where(p => p.DonationCaseId == c.Id && p.Status == PaymentStatus.Approved)
                    .Select(p => p.UserId)
                    .Distinct()
                    .CountAsync();

                var progressPercent = c.TargetAmount > 0
                    ? (int)Math.Min(100, Math.Round((collected / c.TargetAmount * 100)))
                    : 0;

                featuredCases.Add(new CaseWithAmountDto
                {
                    Case = c,
                    Collected = collected,
                    DonorCount = donorCount,
                    ProgressPercent = progressPercent
                });
            }

            // ? Load categories
            var categories = await _db.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();

            // ? Load other cases (beyond featured 6)
            var otherCases = await _db.DonationCases
                .AsNoTracking()
                .Where(c => c.Status == CaseStatus.Approved)
                .OrderByDescending(c => c.CreatedAt)
                .Skip(6)
                .Take(6)
                .ToListAsync();

            var otherCasesList = new List<CaseWithAmountDto>();
            foreach (var c in otherCases)
            {
                var collected = await _db.Payments
                    .AsNoTracking()
                    .Where(p => p.DonationCaseId == c.Id && p.Status == PaymentStatus.Approved)
                    .SumAsync(p => (decimal?)p.Amount) ?? 0m;

                var donorCount = await _db.Payments
                    .AsNoTracking()
                    .Where(p => p.DonationCaseId == c.Id && p.Status == PaymentStatus.Approved)
                    .Select(p => p.UserId)
                    .Distinct()
                    .CountAsync();

                var progressPercent = c.TargetAmount > 0
                    ? (int)Math.Min(100, Math.Round((collected / c.TargetAmount * 100)))
                    : 0;

                otherCasesList.Add(new CaseWithAmountDto
                {
                    Case = c,
                    Collected = collected,
                    DonorCount = donorCount,
                    ProgressPercent = progressPercent
                });
            }

            return View(new HomeIndexViewModel
            {
                FeaturedCases = featuredCases,
                Categories = categories,
                Others = otherCasesList
            });
        }

        // ? Advanced search page
        public async Task<IActionResult> Search(DonationCaseSearchDto filters)
        {
            // ? Validate pagination
            if (filters.PageNumber < 1) filters.PageNumber = 1;
            if (filters.PageSize < 1 || filters.PageSize > 100) filters.PageSize = 12;

            // ? Use search service
            var result = await _caseService.SearchAsync(filters);

            return View(result);
        }
    }
}
