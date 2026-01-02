using DonationManagementSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DonationManagementSystem.Web.Controllers
{
    [Authorize]
    public class DonationsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public DonationsController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> MyDonations()
        {
            var userId = _userManager.GetUserId(User);

            var donations = await _db.Donations
                .Include(d => d.DonationCase)
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.DonatedAt)
                .ToListAsync();

            return View(donations);
        }
    }
}
