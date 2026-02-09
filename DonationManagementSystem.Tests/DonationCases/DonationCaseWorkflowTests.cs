using System;
using System.Threading.Tasks;
using DonationManagementSystem.Application.Common.Interfaces;
using DonationManagementSystem.Application.DonationCases;
using DonationManagementSystem.Domain.Entities;
using Moq;
using Xunit;

namespace DonationManagementSystem.Tests.DonationCases
{
    public class DonationCaseWorkflowTests
    {
        [Fact]
        public async Task ApproveAsync_WhenCaseNotFound_ShouldThrow()
        {
            // Arrange
            var uow = new Mock<IUnitOfWork>();
            var notificationService = new Mock<INotificationService>();

            uow.Setup(u => u.DonationCases.GetByIdAsync(999))
               .ReturnsAsync((DonationCase?)null);

            var workflow = new DonationCaseWorkflow(uow.Object, notificationService.Object);

            // Act + Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => workflow.ApproveAsync(999, "admin", "ok"));
        }

        [Fact]
        public async Task ApproveAsync_WhenValid_ShouldSetApprovedAndReviewFields_AndNotify()
        {
            // Arrange
            var uow = new Mock<IUnitOfWork>();
            var notificationService = new Mock<INotificationService>();

            var c = new DonationCase
            {
                Id = 1,
                Title = "Test Case",
                CreatedByUserId = "user1",
                Status = CaseStatus.Pending
            };

            uow.Setup(u => u.DonationCases.GetByIdAsync(1)).ReturnsAsync(c);

            var workflow = new DonationCaseWorkflow(uow.Object, notificationService.Object);

            // Act
            await workflow.ApproveAsync(1, "admin1", "approved");

            // Assert
            Assert.Equal(CaseStatus.Approved, c.Status);
            Assert.Equal("admin1", c.ReviewedByUserId);
            Assert.NotNull(c.ReviewedAt);
            Assert.Equal("approved", c.AdminNote);

            uow.Verify(u => u.SaveChangesAsync(default), Times.Once);
            notificationService.Verify(n => n.CreateForUserAsync(
                "user1",
                "Case Approved",
                It.IsAny<string>(),
                It.IsAny<string>(),
                NotificationType.CaseApproved), Times.Once);
        }

        [Fact]
        public async Task RejectAsync_WhenValid_ShouldSetRejectedAndReviewFields_AndNotify()
        {
            // Arrange
            var uow = new Mock<IUnitOfWork>();
            var notificationService = new Mock<INotificationService>();

            var c = new DonationCase
            {
                Id = 2,
                Title = "Reject Case",
                CreatedByUserId = "user2",
                Status = CaseStatus.Pending
            };

            uow.Setup(u => u.DonationCases.GetByIdAsync(2)).ReturnsAsync(c);

            var workflow = new DonationCaseWorkflow(uow.Object, notificationService.Object);

            // Act
            await workflow.RejectAsync(2, "admin2", "not valid");

            // Assert
            Assert.Equal(CaseStatus.Rejected, c.Status);
            Assert.Equal("admin2", c.ReviewedByUserId);
            Assert.NotNull(c.ReviewedAt);
            Assert.Equal("not valid", c.AdminNote);

            uow.Verify(u => u.SaveChangesAsync(default), Times.Once);
            notificationService.Verify(n => n.CreateForUserAsync(
                "user2",
                "Case Rejected",
                It.IsAny<string>(),
                It.IsAny<string>(),
                NotificationType.CaseRejected), Times.Once);
        }
    }
}
