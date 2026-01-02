using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DonationManagementSystem.Domain.Entities
{
    public class Comment
    {
        public int Id { get; set; }

        public int DonationCaseId { get; set; }
        public DonationCase? DonationCase { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
