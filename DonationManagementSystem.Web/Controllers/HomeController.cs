using DonationManagementSystem.Domain.Entities;
using DonationManagementSystem.Infrastructure.Data;
using DonationManagementSystem.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DonationManagementSystem.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var approved = await _db.DonationCases
                .Where(c => c.Status == CaseStatus.Approved)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new DonationCaseCardVm
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    TargetAmount = c.TargetAmount,
                    CreatedAt = c.CreatedAt,
                    CollectedAmount = c.Donations.Sum(d => (decimal?)d.Amount) ?? 0,
                    DonorsCount = c.Donations.Select(d => d.UserId).Distinct().Count()
                })
                .ToListAsync();

            var vm = new HomeIndexViewModel
            {
                Featured = approved.FirstOrDefault(),
                Others = approved.Skip(1).ToList()
            };

            return View(vm);
        }
    }
}
