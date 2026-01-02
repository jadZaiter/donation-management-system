using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DonationManagementSystem.Domain.Entities
{
    public class Donation
    {
        public int Id { get; set; }

        public int DonationCaseId { get; set; }
        public DonationCase? DonationCase { get; set; }

        public string UserId { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public DateTime DonatedAt { get; set; } = DateTime.UtcNow;
    }
}
