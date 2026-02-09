namespace DonationManagementSystem.Domain.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;

        // Navigation
        public ICollection<DonationCase> DonationCases { get; set; } = new List<DonationCase>();
    }
}