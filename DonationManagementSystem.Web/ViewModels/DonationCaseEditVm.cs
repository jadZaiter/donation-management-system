using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace DonationManagementSystem.Web.ViewModels
{
    public class DonationCaseEditVm
    {
        public int Id { get; set; }

        [Required, StringLength(80)]
        public string Title { get; set; } = "";

        [Required, StringLength(3000)]
        public string Description { get; set; } = "";

        [Range(1, 100000000)]
        public decimal TargetAmount { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Please select at least one tag")]
        public List<int> TagIds { get; set; } = new();

        // Optional image
        public IFormFile? ImageFile { get; set; }

        public string? CurrentImagePath { get; set; }
    }
}