using DonationManagementSystem.Application.Common.Interfaces;
using DonationManagementSystem.Application.DonationCases;
using DonationManagementSystem.Application.DonationCases.Dtos;
using DonationManagementSystem.Domain.Entities;
using DonationManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DonationManagementSystem.Infrastructure.Services
{
    public class DonationCaseService : IDonationCaseService
    {
        private readonly IUnitOfWork _uow;
        private readonly ApplicationDbContext _db;

        public DonationCaseService(IUnitOfWork uow, ApplicationDbContext db)
        {
            _uow = uow;
            _db = db;
        }

        public async Task<DonationCase?> GetByIdAsync(int id)
        {
            return await _uow.DonationCases.Query()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task ApproveAsync(int caseId, string adminUserId, string? note)
        {
            var donationCase = await _uow.DonationCases.Query()
                .FirstOrDefaultAsync(c => c.Id == caseId);

            if (donationCase == null)
                throw new InvalidOperationException("Case not found.");

            donationCase.Status = CaseStatus.Approved;
            donationCase.ReviewedAt = DateTime.UtcNow;
            donationCase.ReviewedByUserId = adminUserId;
            donationCase.AdminNote = note;

            _uow.DonationCases.Update(donationCase);
            await _uow.SaveChangesAsync();
        }

        public async Task RejectAsync(int caseId, string adminUserId, string? note)
        {
            var donationCase = await _uow.DonationCases.Query()
                .FirstOrDefaultAsync(c => c.Id == caseId);

            if (donationCase == null)
                throw new InvalidOperationException("Case not found.");

            donationCase.Status = CaseStatus.Rejected;
            donationCase.ReviewedAt = DateTime.UtcNow;
            donationCase.ReviewedByUserId = adminUserId;
            donationCase.AdminNote = note;

            _uow.DonationCases.Update(donationCase);
            await _uow.SaveChangesAsync();
        }

        public async Task SaveAsync()
        {
            await _uow.SaveChangesAsync();
        }

        // ✅ Advanced search with pagination
        public async Task<PaginatedDonationCaseDto> SearchAsync(DonationCaseSearchDto filters)
        {
            // ✅ Start with base query (approved cases only)
            var baseQuery = _db.DonationCases
                .AsNoTracking()
                .Where(c => c.Status == CaseStatus.Approved)
                .Include(c => c.Category)
                .Include(c => c.DonationCaseTags)
                .ThenInclude(dct => dct.Tag)
                .AsQueryable();

            // ✅ Apply ALL filters before ordering
            var query = ApplyFilters(baseQuery, filters);

            // ✅ Apply Sorting
            query = ApplySorting(query, filters);

            // ✅ Get total count before pagination
            var totalCount = await query.CountAsync();

            // ✅ Apply Pagination
            var cases = await query
                .Skip((filters.PageNumber - 1) * filters.PageSize)
                .Take(filters.PageSize)
                .Select(c => new DonationCaseCardDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    TargetAmount = c.TargetAmount,
                    CreatedAt = c.CreatedAt,
                    ImagePath = c.ImagePath,
                    Category = new CategoryDto
                    {
                        Id = c.Category!.Id,
                        Name = c.Category.Name,
                        Slug = c.Category.Slug
                    },
                    Tags = c.DonationCaseTags
                        .Select(dct => new TagDto
                        {
                            Id = dct.Tag.Id,
                            Name = dct.Tag.Name,
                            Slug = dct.Tag.Slug
                        })
                        .ToList(),
                    CollectedAmount = _db.Payments
                        .Where(p => p.DonationCaseId == c.Id && p.Status == PaymentStatus.Approved)
                        .Sum(p => (decimal?)p.Amount) ?? 0m,
                    DonorsCount = _db.Payments
                        .Where(p => p.DonationCaseId == c.Id && p.Status == PaymentStatus.Approved)
                        .Select(p => p.UserId)
                        .Distinct()
                        .Count()
                })
                .ToListAsync();

            // ✅ Get filter options
            var categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
            var tags = await _db.Tags.OrderBy(t => t.Name).ToListAsync();

            var result = new PaginatedDonationCaseDto
            {
                Cases = cases,
                TotalCount = totalCount,
                PageNumber = filters.PageNumber,
                PageSize = filters.PageSize,
                Categories = categories.Select(c => new CategoryDto 
                { 
                    Id = c.Id, 
                    Name = c.Name, 
                    Slug = c.Slug 
                }).ToList(),
                Tags = tags.Select(t => new TagDto 
                { 
                    Id = t.Id, 
                    Name = t.Name, 
                    Slug = t.Slug 
                }).ToList(),
                CurrentFilters = filters
            };

            // ✅ Populate Items alias for view compatibility
            result.Items = result.Cases;

            return result;
        }

        // ✅ Helper: Apply all filters
        private IQueryable<DonationCase> ApplyFilters(IQueryable<DonationCase> query, DonationCaseSearchDto filters)
        {
            if (!string.IsNullOrWhiteSpace(filters.Keyword))
            {
                var keyword = filters.Keyword.ToLower();
                query = query.Where(c => 
                    c.Title.ToLower().Contains(keyword) || 
                    c.Description.ToLower().Contains(keyword));
            }

            if (filters.CategoryId.HasValue && filters.CategoryId > 0)
            {
                query = query.Where(c => c.CategoryId == filters.CategoryId);
            }

            if (filters.TagIds.Any())
            {
                query = query.Where(c => c.DonationCaseTags.Any(dct => filters.TagIds.Contains(dct.TagId)));
            }

            if (filters.MinGoal.HasValue && filters.MinGoal > 0)
            {
                query = query.Where(c => c.TargetAmount >= filters.MinGoal);
            }

            if (filters.MaxGoal.HasValue && filters.MaxGoal > 0)
            {
                query = query.Where(c => c.TargetAmount <= filters.MaxGoal);
            }

            if (filters.DateFrom.HasValue)
            {
                query = query.Where(c => c.CreatedAt >= filters.DateFrom);
            }

            if (filters.DateTo.HasValue)
            {
                var endOfDay = filters.DateTo.Value.AddDays(1);
                query = query.Where(c => c.CreatedAt < endOfDay);
            }

            return query;
        }

        // ✅ Helper: Apply sorting
        private IQueryable<DonationCase> ApplySorting(IQueryable<DonationCase> query, DonationCaseSearchDto filters)
        {
            return filters.SortBy?.ToLower() switch
            {
                "oldest" => query.OrderBy(c => c.CreatedAt),
                "highest-goal" => query.OrderByDescending(c => c.TargetAmount),
                "most-funded" => query.OrderByDescending(c => 
                    _db.Payments.Where(d => d.DonationCaseId == c.Id && d.Status == PaymentStatus.Approved).Sum(d => (decimal?)d.Amount) ?? 0m),
                "urgent-first" => query
                    .OrderByDescending(c => c.DonationCaseTags.Any(dct => dct.Tag.Slug == "urgent"))
                    .ThenByDescending(c => c.CreatedAt),
                _ => query.OrderByDescending(c => c.CreatedAt)
            };
        }
    }
}
