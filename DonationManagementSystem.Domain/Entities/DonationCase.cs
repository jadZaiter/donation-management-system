using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DonationManagementSystem.Domain.Entities
{

    public enum CaseStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }
    public class DonationCase
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public decimal TargetAmount { get; set; }

        public CaseStatus Status { get; set; } = CaseStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? ImagePath { get; set; }
        // who submitted it (Identity user id)
        public string CreatedByUserId { get; set; } = string.Empty;

        // Navigation
        public ICollection<Donation> Donations { get; set; } = new List<Donation>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
