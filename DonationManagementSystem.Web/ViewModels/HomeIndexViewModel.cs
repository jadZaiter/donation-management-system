using DonationManagementSystem.Application.DonationCases.Dtos;
using DonationManagementSystem.Domain.Entities;

namespace DonationManagementSystem.Web.ViewModels
{
    /// <summary>
    /// Home page view model - displays featured and recent cases with category/tag filters
    /// </summary>
    public class HomeIndexViewModel
    {
        public List<CaseWithAmountDto> FeaturedCases { get; set; } = new(); // ✅ RENAME from Featured
        public List<Category> Categories { get; set; } = new(); // ✅ ADD THIS
        public List<CaseWithAmountDto> Others { get; set; } = new(); // ✅ ADD THIS
    }

    public class CaseWithAmountDto
    {
        public DonationCase Case { get; set; } = null!; // ✅ Use null! to suppress warning
        public decimal Collected { get; set; }
        public int DonorCount { get; set; }
        public int ProgressPercent { get; set; }
    }
}
