using DonationManagementSystem.Application.Payments.Models;
using DonationManagementSystem.Application.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DonationManagementSystem.Web.Controllers.Api
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentsApiController : ControllerBase
    {
        private readonly PaymentWorkflow _paymentWorkflow;
        private readonly UserManager<IdentityUser> _userManager;

        public PaymentsApiController(PaymentWorkflow paymentWorkflow, UserManager<IdentityUser> userManager)
        {
            _paymentWorkflow = paymentWorkflow;
            _userManager = userManager;
        }

        // POST: /api/payments/start
        // body: { "caseId": 1, "amount": 50 }
        [Authorize]
        [HttpPost("start")]
        public async Task<IActionResult> Start([FromBody] StartPaymentApiRequest req)
        {
            if (req.Amount <= 0) return BadRequest("Amount must be > 0.");

            var userId = _userManager.GetUserId(User)!;

            var paymentId = await _paymentWorkflow.StartBankTransferAsync(new CreatePaymentRequest
            {
                CaseId = req.CaseId,
                Amount = req.Amount,
                UserId = userId
            });

            return Ok(new { paymentId });
        }

        // POST: /api/payments/{paymentId}/proof
        // body: { "proofPath": "/uploads/proofs/xxx.png" }
        [Authorize]
        [HttpPost("{paymentId:int}/proof")]
        public async Task<IActionResult> UploadProof(int paymentId, [FromBody] UploadProofApiRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.ProofPath))
                return BadRequest("ProofPath is required.");

            var userId = _userManager.GetUserId(User)!;

            await _paymentWorkflow.UploadProofAsync(new UploadProofRequest
            {
                PaymentId = paymentId,
                UserId = userId,
                ProofPath = req.ProofPath
            });

            return Ok(new { message = "Proof uploaded." });
        }

        // ADMIN: POST /api/payments/{paymentId}/approve
        [Authorize(Roles = "Admin")]
        [HttpPost("{paymentId:int}/approve")]
        public async Task<IActionResult> Approve(int paymentId, [FromBody] ReviewApiRequest req)
        {
            var adminId = User.Identity?.Name ?? "admin";

            await _paymentWorkflow.ApproveAsync(new ReviewPaymentRequest
            {
                PaymentId = paymentId,
                AdminId = adminId,
                Note = req.Note
            });

            return Ok(new { message = "Payment approved." });
        }

        // ADMIN: POST /api/payments/{paymentId}/reject
        [Authorize(Roles = "Admin")]
        [HttpPost("{paymentId:int}/reject")]
        public async Task<IActionResult> Reject(int paymentId, [FromBody] ReviewApiRequest req)
        {
            var adminId = User.Identity?.Name ?? "admin";

            await _paymentWorkflow.RejectAsync(new ReviewPaymentRequest
            {
                PaymentId = paymentId,
                AdminId = adminId,
                Note = req.Note
            });

            return Ok(new { message = "Payment rejected." });
        }
    }

    // Small API request DTOs (keep inside same file for simplicity)
    public class StartPaymentApiRequest
    {
        public int CaseId { get; set; }
        public decimal Amount { get; set; }
    }

    public class UploadProofApiRequest
    {
        public string ProofPath { get; set; } = "";
    }

    public class ReviewApiRequest
    {
        public string? Note { get; set; }
    }
}
