using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DonationManagementSystem.Application.Payments.Models
{
    public class UploadProofRequest
    {
        public int PaymentId { get; set; }
        public string UserId { get; set; } = null!;
        public string ProofPath { get; set; } = null!;
    }
}
