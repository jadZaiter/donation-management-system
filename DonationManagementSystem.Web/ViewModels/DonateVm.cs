using System.ComponentModel.DataAnnotations;

namespace DonationManagementSystem.Web.ViewModels
{
    public class DonateVm
    {
        [Required]
        public int CaseId { get; set; }

        [Required]
        [Range(1, 100000000, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }
    }
}
