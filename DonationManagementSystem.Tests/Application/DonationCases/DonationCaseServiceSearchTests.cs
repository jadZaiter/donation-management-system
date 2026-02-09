using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DonationManagementSystem.Application.DonationCases;
using DonationManagementSystem.Application.DonationCases.Dtos;
using DonationManagementSystem.Domain.Entities;
using DonationManagementSystem.Infrastructure.Data;
using DonationManagementSystem.Infrastructure.Repositories;
using DonationManagementSystem.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DonationManagementSystem.Tests.Application.DonationCases
{
    public class DonationCaseServiceSearchTests
    {
        private ApplicationDbContext CreateInMemoryDb()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        private void SeedTestData(ApplicationDbContext db)
        {
            var categories = new[]
            {
                new Category { Id = 1, Name = "Medical", Slug = "medical" },
                new Category { Id = 2, Name = "Education", Slug = "education" }
            };

            var tags = new[]
            {
                new Tag { Id = 1, Name = "Urgent", Slug = "urgent" },
                new Tag { Id = 2, Name = "Verified", Slug = "verified" }
            };

            var cases = new[]
            {
                new DonationCase
                {
                    Id = 1,
                    Title = "Emergency Surgery Needed",
                    Description = "Need urgent surgery for heart condition",
                    TargetAmount = 5000m,
                    CategoryId = 1,
                    Status = CaseStatus.Approved,
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    CreatedByUserId = "user1"
                },
                new DonationCase
                {
                    Id = 2,
                    Title = "College Scholarship Fund",
                    Description = "Scholarship for talented student",
                    TargetAmount = 2000m,
                    CategoryId = 2,
                    Status = CaseStatus.Approved,
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    CreatedByUserId = "user2"
                }
            };

            db.Categories.AddRange(categories);
            db.Tags.AddRange(tags);
            db.DonationCases.AddRange(cases);
            db.SaveChanges();
        }

        [Fact]
        public async Task SearchAsync_WithKeyword_ReturnsMatchingCases()
        {
            // Arrange
            var db = CreateInMemoryDb();
            SeedTestData(db);
            var uow = new UnitOfWork(db);
            var service = new DonationCaseService(uow, db);

            var filters = new DonationCaseSearchDto
            {
                Keyword = "Surgery"
            };

            // Act
            var result = await service.SearchAsync(filters);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Cases);
            Assert.Equal("Emergency Surgery Needed", result.Cases[0].Title);
        }

        [Fact]
        public async Task SearchAsync_WithCategoryFilter_ReturnsOnlyMatchingCategory()
        {
            // Arrange
            var db = CreateInMemoryDb();
            SeedTestData(db);
            var uow = new UnitOfWork(db);
            var service = new DonationCaseService(uow, db);

            var filters = new DonationCaseSearchDto
            {
                CategoryId = 1
            };

            // Act
            var result = await service.SearchAsync(filters);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Cases);
            Assert.Equal(1, result.Cases[0].Category?.Id);
        }

        [Fact]
        public async Task SearchAsync_WithGoalRange_ReturnsOnlyInRange()
        {
            // Arrange
            var db = CreateInMemoryDb();
            SeedTestData(db);
            var uow = new UnitOfWork(db);
            var service = new DonationCaseService(uow, db);

            var filters = new DonationCaseSearchDto
            {
                MinGoal = 1000m,
                MaxGoal = 3000m
            };

            // Act
            var result = await service.SearchAsync(filters);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Cases);
            Assert.Equal("College Scholarship Fund", result.Cases[0].Title);
        }

        [Fact]
        public async Task SearchAsync_WithPagination_ReturnsPaginatedResults()
        {
            // Arrange
            var db = CreateInMemoryDb();
            SeedTestData(db);
            var uow = new UnitOfWork(db);
            var service = new DonationCaseService(uow, db);

            var filters = new DonationCaseSearchDto
            {
                PageNumber = 1,
                PageSize = 1
            };

            // Act
            var result = await service.SearchAsync(filters);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Cases);
            Assert.Equal(2, result.TotalCount);
            Assert.True(result.HasNextPage);
        }

        [Fact]
        public async Task SearchAsync_NoFilters_ReturnsAllApprovedCases()
        {
            // Arrange
            var db = CreateInMemoryDb();
            SeedTestData(db);
            var uow = new UnitOfWork(db);
            var service = new DonationCaseService(uow, db);

            var filters = new DonationCaseSearchDto();

            // Act
            var result = await service.SearchAsync(filters);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Cases.Count);
            Assert.Equal(2, result.TotalCount);
        }
    }
}