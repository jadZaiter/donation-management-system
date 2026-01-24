using DonationManagementSystem.Domain.Entities;
using DonationManagementSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DonationManagementSystem.Web.Controllers.Api
{
    [ApiController]
    [Route("api/donation-cases")]
    public class DonationCasesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public DonationCasesApiController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /api/donation-cases
        [HttpGet]
        public async Task<IActionResult> GetApprovedCases()
        {
            var list = await _db.DonationCases
                .AsNoTracking()
                .Where(c => c.Status == CaseStatus.Approved)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new
                {
                    c.Id,
                    c.Title,
                    c.Description,
                    c.TargetAmount,
                    c.ImagePath,
                    c.CreatedAt
                })
                .ToListAsync();

            return Ok(list);
        }

        // GET: /api/donation-cases/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCaseDetails(int id)
        {
            var item = await _db.DonationCases
                .AsNoTracking()
                .Include(c => c.Donations)
                .Include(c => c.Comments)
                .FirstOrDefaultAsync(c => c.Id == id && c.Status == CaseStatus.Approved);

            if (item == null) return NotFound();

            var collected = item.Donations.Sum(d => d.Amount);

            return Ok(new
            {
                item.Id,
                item.Title,
                item.Description,
                item.TargetAmount,
                CollectedAmount = collected,
                item.ImagePath,
                item.CreatedAt,
                Comments = item.Comments
                    .OrderByDescending(x => x.CreatedAt)
                    .Select(x => new
                    {
                        x.Id,
                        x.UserId,
                        x.Text,
                        x.CreatedAt
                    })
            });
        }
    }
}
