namespace DonationManagementSystem.Domain.Entities
{
    public class DonationCaseTag
    {
        public int DonationCaseId { get; set; }
        public DonationCase? DonationCase { get; set; }

        public int TagId { get; set; }
        public Tag? Tag { get; set; }
    }
}