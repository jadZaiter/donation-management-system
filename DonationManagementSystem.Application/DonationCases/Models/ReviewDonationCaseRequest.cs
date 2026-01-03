using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DonationManagementSystem.Application.DonationCases.Models
{
    public class ReviewDonationCaseRequest
    {
        public int CaseId { get; set; }
        public string AdminId { get; set; } = null!;
        public string? Note { get; set; }
    }
}
