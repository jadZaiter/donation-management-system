namespace DonationManagementSystem.Application.DonationCases.Dtos
{
    /// <summary>
    /// Search filter parameters for DonationCases
    /// </summary>
    public class DonationCaseSearchDto
    {
        public string? Keyword { get; set; }
        public int? CategoryId { get; set; }
        public List<int> TagIds { get; set; } = new();
        public decimal? MinGoal { get; set; }
        public decimal? MaxGoal { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string SortBy { get; set; } = "newest";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }

    /// <summary>
    /// Donation case card DTO for search results
    /// </summary>
    public class DonationCaseCardDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal TargetAmount { get; set; }
        public decimal CollectedAmount { get; set; }
        public int DonorsCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ImagePath { get; set; }
        public CategoryDto? Category { get; set; }
        public List<TagDto> Tags { get; set; } = new();
    }

    /// <summary>
    /// Category DTO
    /// </summary>
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Slug { get; set; } = "";
    }

    /// <summary>
    /// Tag DTO
    /// </summary>
    public class TagDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Slug { get; set; } = "";
    }

    /// <summary>
    /// Paginated search results - Main DTO
    /// </summary>
    public class DonationCaseSearchResultDto
    {
        public List<DonationCaseCardDto> Cases { get; set; } = new();
        public List<DonationCaseCardDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
        public List<CategoryDto> Categories { get; set; } = new();
        public List<TagDto> Tags { get; set; } = new();
        public DonationCaseSearchDto? CurrentFilters { get; set; }
    }

    /// <summary>
    /// Alias for backward compatibility
    /// </summary>
    public class PaginatedDonationCaseDto : DonationCaseSearchResultDto
    {
    }
}