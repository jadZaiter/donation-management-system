using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DonationManagementSystem.Application.Payments.Models
{
    public class CreatePaymentRequest
    {
        public int CaseId { get; set; }
        public decimal Amount { get; set; }
        public string UserId { get; set; } = null!;
    }
}

