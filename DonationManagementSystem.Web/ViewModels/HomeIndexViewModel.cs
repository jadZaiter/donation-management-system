namespace DonationManagementSystem.Web.ViewModels
{
    public class HomeIndexViewModel
    {
        public DonationCaseCardVm? Featured { get; set; }
        public List<DonationCaseCardVm> Others { get; set; } = new();
    }

    public class DonationCaseCardVm
    {
        public string? ImagePath { get; set; }
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal TargetAmount { get; set; }
        public decimal CollectedAmount { get; set; }
        public int DonorsCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
