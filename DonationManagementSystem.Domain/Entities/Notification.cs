namespace DonationManagementSystem.Domain.Entities
{
    public enum NotificationType
    {
        CaseSubmitted = 0,
        ProofUploaded = 1,
        CaseApproved = 2,
        CaseRejected = 3,
        PaymentApproved = 4,
        PaymentRejected = 5,
        CommentAdded = 6
    }

    public class Notification
    {
        public int Id { get; set; }
        
        public string UserId { get; set; } = string.Empty; // Identity user ID
        
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Link { get; set; } // Link to related resource (e.g., /DonationCases/Details/5)
        
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}