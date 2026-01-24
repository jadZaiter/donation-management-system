using System;
using System.Threading.Tasks;
using DonationManagementSystem.Application.DonationCases;
using DonationManagementSystem.Application.DonationCases.Models;
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
            var svc = new Mock<IDonationCaseService>();

            svc.Setup(s => s.GetByIdAsync(999))
               .ReturnsAsync((DonationCase?)null);

            var workflow = new DonationCaseWorkflow(svc.Object);

            // Act + Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => workflow.ApproveAsync(
                new ReviewDonationCaseRequest
                {
                    CaseId = 999,
                    AdminId = "admin",
                    Note = "ok"
                }));
        }

        [Fact]
        public async Task ApproveAsync_WhenValid_ShouldSetApprovedAndReviewFields_AndSave()
        {
            // Arrange
            var svc = new Mock<IDonationCaseService>();

            var c = new DonationCase
            {
                Id = 1,
                Title = "Test Case",
                Status = CaseStatus.Pending
            };

            svc.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(c);

            var workflow = new DonationCaseWorkflow(svc.Object);

            // Act
            await workflow.ApproveAsync(new ReviewDonationCaseRequest
            {
                CaseId = 1,
                AdminId = "admin1",
                Note = "approved"
            });

            // Assert
            Assert.Equal(CaseStatus.Approved, c.Status);
            Assert.Equal("admin1", c.ReviewedByUserId);
            Assert.NotNull(c.ReviewedAt);
            Assert.Equal("approved", c.AdminNote);

            svc.Verify(s => s.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task RejectAsync_WhenValid_ShouldSetRejectedAndReviewFields_AndSave()
        {
            // Arrange
            var svc = new Mock<IDonationCaseService>();

            var c = new DonationCase
            {
                Id = 2,
                Title = "Reject Case",
                Status = CaseStatus.Pending
            };

            svc.Setup(s => s.GetByIdAsync(2)).ReturnsAsync(c);

            var workflow = new DonationCaseWorkflow(svc.Object);

            // Act
            await workflow.RejectAsync(new ReviewDonationCaseRequest
            {
                CaseId = 2,
                AdminId = "admin2",
                Note = "not valid"
            });

            // Assert
            Assert.Equal(CaseStatus.Rejected, c.Status);
            Assert.Equal("admin2", c.ReviewedByUserId);
            Assert.NotNull(c.ReviewedAt);
            Assert.Equal("not valid", c.AdminNote);

            svc.Verify(s => s.SaveAsync(), Times.Once);
        }
    }
}
