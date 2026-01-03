using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DonationManagementSystem.Domain.Entities
{
    public enum PaymentStatus
    {
        Pending = 0,
        ProofUploaded = 1,
        Approved = 2,
        Rejected = 3
    }

    public enum PaymentMethod
    {
        BankTransfer = 0
    }

    public class Payment
    {
        public int Id { get; set; }

        public int DonationCaseId { get; set; }
        public DonationCase DonationCase { get; set; } = null!;

        public string UserId { get; set; } = null!;

        public decimal Amount { get; set; }

        public PaymentMethod Method { get; set; } = PaymentMethod.BankTransfer;
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        public string? ProofPath { get; set; } // /uploads/proofs/xxx.png

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewedByUserId { get; set; } // admin user id
        public string? AdminNote { get; set; }
    }
}