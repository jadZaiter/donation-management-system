using System;
using System.Threading.Tasks;
using DonationManagementSystem.Application.Common.Interfaces;
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
            var uow = new Mock<IUnitOfWork>();
            var notificationService = new Mock<INotificationService>();
            
            var workflow = new PaymentWorkflow(uow.Object, notificationService.Object);

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
        public async Task StartBankTransferAsync_WhenCaseNotFound_ShouldThrow()
        {
            // Arrange
            var uow = new Mock<IUnitOfWork>();
            var notificationService = new Mock<INotificationService>();

            uow.Setup(u => u.DonationCases.GetByIdAsync(1))
               .ReturnsAsync((Domain.Entities.DonationCase?)null);

            var workflow = new PaymentWorkflow(uow.Object, notificationService.Object);

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
            var uow = new Mock<IUnitOfWork>();
            var notificationService = new Mock<INotificationService>();

            var donationCase = new Domain.Entities.DonationCase
            {
                Id = 1,
                TargetAmount = 100
            };

            uow.Setup(u => u.DonationCases.GetByIdAsync(1)).ReturnsAsync(donationCase);

            var createdPayment = new Domain.Entities.Payment { Id = 777 };
            uow.Setup(u => u.Payments.AddAsync(It.IsAny<Domain.Entities.Payment>()))
               .Returns(Task.CompletedTask);

            var workflow = new PaymentWorkflow(uow.Object, notificationService.Object);

            var req = new CreatePaymentRequest
            {
                CaseId = 1,
                UserId = "u1",
                Amount = 50
            };

            // Act
            var paymentId = await workflow.StartBankTransferAsync(req);

            // Assert
            Assert.True(paymentId > 0);
            uow.Verify(u => u.Payments.AddAsync(It.IsAny<Domain.Entities.Payment>()), Times.Once);
            uow.Verify(u => u.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task UploadProofAsync_WhenValid_ShouldUpdatePaymentAndNotify()
        {
            // Arrange
            var uow = new Mock<IUnitOfWork>();
            var notificationService = new Mock<INotificationService>();

            var payment = new Domain.Entities.Payment
            {
                Id = 10,
                UserId = "u1",
                DonationCaseId = 1
            };

            var donationCase = new Domain.Entities.DonationCase
            {
                Id = 1,
                Title = "Test Case"
            };

            uow.Setup(u => u.Payments.GetByIdAsync(10)).ReturnsAsync(payment);
            uow.Setup(u => u.DonationCases.GetByIdAsync(1)).ReturnsAsync(donationCase);

            var workflow = new PaymentWorkflow(uow.Object, notificationService.Object);

            var req = new UploadProofRequest
            {
                PaymentId = 10,
                UserId = "u1",
                ProofPath = "/uploads/proofs/x.png"
            };

            // Act
            await workflow.UploadProofAsync(req);

            // Assert
            uow.Verify(u => u.Payments.Update(It.IsAny<Domain.Entities.Payment>()), Times.Once);
            uow.Verify(u => u.SaveChangesAsync(default), Times.Once);
            notificationService.Verify(n => n.CreateForAdminsAsync(
                "Payment Proof Uploaded",
                It.IsAny<string>(),
                It.IsAny<string>(),
                Domain.Entities.NotificationType.ProofUploaded), Times.Once);
        }
    }
}
