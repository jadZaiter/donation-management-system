using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DonationManagementSystem.Application.Payments.Models
{
    public class ReviewPaymentRequest
    {
        public int PaymentId { get; set; }
        public string AdminId { get; set; } = null!;
        public string? Note { get; set; }
    }
}

