using System.ComponentModel.DataAnnotations;
using Noor.Core.Entities;

namespace Noor.Core.Dtos;

public class AdminProductListItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public int TotalStock { get; set; }
    public int VariantCount { get; set; }
    public int SoldCount { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ImageInput
{
    [Required, StringLength(500)]
    public string Url { get; set; } = string.Empty;

    [StringLength(200)]
    public string? AltText { get; set; }
}

public class VariantInput
{
    public int? Id { get; set; }

    [Required, StringLength(30)]
    public string Size { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string ColorName { get; set; } = string.Empty;

    [Required, RegularExpression("^#([0-9a-fA-F]{6})$", ErrorMessage = "Color must be a hex value like #1c1b19.")]
    public string ColorHex { get; set; } = "#1c1b19";

    [Required, StringLength(60)]
    public string Sku { get; set; } = string.Empty;

    [Range(0, 100000)]
    public int StockQuantity { get; set; }

    [Range(0, 100000)]
    public decimal PriceAdjustment { get; set; }
}

public class ProductUpsertRequest
{
    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(160)]
    public string? Slug { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    [Required, StringLength(260)]
    public string ShortDescription { get; set; } = string.Empty;

    [Required, StringLength(5000)]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, 1000000)]
    public decimal Price { get; set; }

    [Range(0.01, 1000000)]
    public decimal? CompareAtPrice { get; set; }

    [Required, StringLength(120)]
    public string Fabric { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? CareInstructions { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }

    public List<ImageInput> Images { get; set; } = new();
    public List<VariantInput> Variants { get; set; } = new();
}

public class AdminCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public int ProductCount { get; set; }
}

public class CategoryUpsertRequest
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(120)]
    public string? Slug { get; set; }

    [StringLength(600)]
    public string? Description { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    [Range(0, 1000)]
    public int SortOrder { get; set; }
}

public class CouponDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CouponType DiscountType { get; set; }
    public decimal Value { get; set; }
    public decimal MinOrderAmount { get; set; }
    public int? MaxRedemptions { get; set; }
    public int TimesRedeemed { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public bool IsActive { get; set; }
    public bool IsLive { get; set; }
}

public class CouponUpsertRequest
{
    [Required, StringLength(40, MinimumLength = 2)]
    public string Code { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Description { get; set; }

    public CouponType DiscountType { get; set; } = CouponType.Percentage;

    [Range(0.01, 100000)]
    public decimal Value { get; set; }

    [Range(0, 1000000)]
    public decimal MinOrderAmount { get; set; }

    [Range(1, 100000)]
    public int? MaxRedemptions { get; set; }

    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public bool IsActive { get; set; } = true;
}

public class DashboardDto
{
    public decimal TotalRevenue { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public int TotalOrders { get; set; }
    public int OrdersThisMonth { get; set; }
    public int PendingOrders { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalProducts { get; set; }
    public int LowStockCount { get; set; }
    public int NewsletterSubscribers { get; set; }
    public IReadOnlyList<OrderSummaryDto> RecentOrders { get; set; } = Array.Empty<OrderSummaryDto>();
    public IReadOnlyList<TopProductDto> TopProducts { get; set; } = Array.Empty<TopProductDto>();
    public IReadOnlyList<SalesPointDto> SalesByDay { get; set; } = Array.Empty<SalesPointDto>();
    public IReadOnlyList<LowStockDto> LowStockVariants { get; set; } = Array.Empty<LowStockDto>();
}

public class SalesPointDto
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int Orders { get; set; }
}

public class TopProductDto
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int UnitsSold { get; set; }
    public decimal Revenue { get; set; }
}

public class LowStockDto
{
    public int VariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string ColorName { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
}

public class AdminCustomerDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public bool IsActive { get; set; }
    public bool MarketingOptIn { get; set; }
    public int OrderCount { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public class UploadResponse
{
    public string Url { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}

public class AdminOrderQuery
{
    public string? Search { get; set; }
    public OrderStatus? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class AdminProductQuery
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class AdminCustomerQuery
{
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
