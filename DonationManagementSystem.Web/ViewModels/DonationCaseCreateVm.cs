using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace DonationManagementSystem.Web.ViewModels
{
    public class DonationCaseCreateVm
    {
        [Required, StringLength(80)]
        public string Title { get; set; } = "";

        [Required, StringLength(3000)]
        public string Description { get; set; } = "";

        [Range(1, 100000000)]
        public decimal TargetAmount { get; set; }

        // Optional image
        public IFormFile? ImageFile { get; set; }
    }
}
