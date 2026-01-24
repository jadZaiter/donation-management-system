using System;
using System.Threading.Tasks;
using DonationManagementSystem.Application.Payments;
using DonationManagementSystem.Application.Payments.Models;
using Moq;
using Xunit;

namespace DonationManagementSystem.Tests.Payments
{
    public class PaymentWorkflowTests
    {
        [Fact]
        public async Task StartBankTransferAsync_AmountLessOrEqualZero_ShouldThrow()
        {
            // Arrange
            var payments = new Mock<IPaymentService>();
            var workflow = new PaymentWorkflow(payments.Object);

            var req = new CreatePaymentRequest
            {
                CaseId = 1,
                UserId = "u1",
                Amount = 0
            };

            // Act + Assert
            await Assert.ThrowsAsync<ArgumentException>(() => workflow.StartBankTransferAsync(req));
        }

        [Fact]
        public async Task StartBankTransferAsync_WhenCollectedReachedTarget_ShouldThrow()
        {
            // Arrange
            var payments = new Mock<IPaymentService>();

            payments.Setup(p => p.GetTargetAmountAsync(1)).ReturnsAsync(100);
            payments.Setup(p => p.GetCollectedAmountAsync(1)).ReturnsAsync(100);

            var workflow = new PaymentWorkflow(payments.Object);

            var req = new CreatePaymentRequest
            {
                CaseId = 1,
                UserId = "u1",
                Amount = 50
            };

            // Act + Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => workflow.StartBankTransferAsync(req));
        }

        [Fact]
        public async Task StartBankTransferAsync_WhenValid_ShouldCreatePayment_AndReturnPaymentId()
        {
            // Arrange
            var payments = new Mock<IPaymentService>();

            payments.Setup(p => p.GetTargetAmountAsync(1)).ReturnsAsync(100);
            payments.Setup(p => p.GetCollectedAmountAsync(1)).ReturnsAsync(20);

            payments.Setup(p => p.CreatePaymentAsync(1, "u1", 50))
                    .ReturnsAsync(777);

            var workflow = new PaymentWorkflow(payments.Object);

            var req = new CreatePaymentRequest
            {
                CaseId = 1,
                UserId = "u1",
                Amount = 50
            };

            // Act
            var paymentId = await workflow.StartBankTransferAsync(req);

            // Assert
            Assert.Equal(777, paymentId);
            payments.Verify(p => p.CreatePaymentAsync(1, "u1", 50), Times.Once);
        }

        [Fact]
        public async Task UploadProofAsync_WhenProofPathMissing_ShouldThrow()
        {
            // Arrange
            var payments = new Mock<IPaymentService>();
            var workflow = new PaymentWorkflow(payments.Object);

            // Act + Assert
            await Assert.ThrowsAsync<ArgumentException>(() => workflow.UploadProofAsync(new UploadProofRequest
            {
                PaymentId = 10,
                UserId = "u1",
                ProofPath = "" // missing
            }));
        }

        [Fact]
        public async Task UploadProofAsync_WhenValid_ShouldCallService()
        {
            // Arrange
            var payments = new Mock<IPaymentService>();
            var workflow = new PaymentWorkflow(payments.Object);

            var req = new UploadProofRequest
            {
                PaymentId = 10,
                UserId = "u1",
                ProofPath = "/uploads/proofs/x.png"
            };

            // Act
            await workflow.UploadProofAsync(req);

            // Assert
            payments.Verify(p => p.UploadProofAsync(10, "u1", "/uploads/proofs/x.png"), Times.Once);
        }
    }
}
