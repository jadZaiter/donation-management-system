namespace DonationManagementSystem.Domain.Entities
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;

        // Navigation
        public ICollection<DonationCaseTag> DonationCaseTags { get; set; } = new List<DonationCaseTag>();
    }
}