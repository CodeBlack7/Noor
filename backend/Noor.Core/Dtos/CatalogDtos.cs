using System.ComponentModel.DataAnnotations;

namespace Noor.Core.Dtos;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int ProductCount { get; set; }
}

public class ProductImageDto
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? AltText { get; set; }
}

public class ColorSwatchDto
{
    public string Name { get; set; } = string.Empty;
    public string Hex { get; set; } = "#1c1b19";
}

public class ProductVariantDto
{
    public int Id { get; set; }
    public string Size { get; set; } = string.Empty;
    public string ColorName { get; set; } = string.Empty;
    public string ColorHex { get; set; } = "#1c1b19";
    public string Sku { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public decimal Price { get; set; }
    public decimal PriceAdjustment { get; set; }
    public bool InStock => StockQuantity > 0;
}

public class ProductSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategorySlug { get; set; } = string.Empty;
    public string? PrimaryImageUrl { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsNew { get; set; }
    public bool InStock { get; set; }
    public IReadOnlyList<ColorSwatchDto> Colors { get; set; } = Array.Empty<ColorSwatchDto>();
    public IReadOnlyList<string> Sizes { get; set; } = Array.Empty<string>();
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
}

public class ProductDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategorySlug { get; set; } = string.Empty;
    public string Fabric { get; set; } = string.Empty;
    public string? CareInstructions { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public bool InStock { get; set; }
    public IReadOnlyList<ProductImageDto> Images { get; set; } = Array.Empty<ProductImageDto>();
    public IReadOnlyList<ProductVariantDto> Variants { get; set; } = Array.Empty<ProductVariantDto>();
    public IReadOnlyList<ColorSwatchDto> Colors { get; set; } = Array.Empty<ColorSwatchDto>();
    public IReadOnlyList<string> Sizes { get; set; } = Array.Empty<string>();
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public IReadOnlyList<ReviewDto> Reviews { get; set; } = Array.Empty<ReviewDto>();
    public IReadOnlyList<ProductSummaryDto> RelatedProducts { get; set; } = Array.Empty<ProductSummaryDto>();
}

public class ReviewDto
{
    public int Id { get; set; }
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ReviewListDto
{
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public IReadOnlyList<ReviewDto> Items { get; set; } = Array.Empty<ReviewDto>();
}

public class CreateReviewRequest
{
    [Range(1, 5)]
    public int Rating { get; set; }

    [StringLength(120)]
    public string? Title { get; set; }

    [Required, StringLength(1500, MinimumLength = 3)]
    public string Comment { get; set; } = string.Empty;
}

public class ProductQuery
{
    public string? Search { get; set; }
    public string? Category { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Size { get; set; }
    public string? Color { get; set; }
    public bool? Featured { get; set; }
    public string? Sort { get; set; } = "featured";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}

public class ProductFacetsDto
{
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public IReadOnlyList<string> Sizes { get; set; } = Array.Empty<string>();
    public IReadOnlyList<ColorSwatchDto> Colors { get; set; } = Array.Empty<ColorSwatchDto>();
}

public class HomeContentDto
{
    public IReadOnlyList<CategoryDto> Categories { get; set; } = Array.Empty<CategoryDto>();
    public IReadOnlyList<ProductSummaryDto> Featured { get; set; } = Array.Empty<ProductSummaryDto>();
    public IReadOnlyList<ProductSummaryDto> NewArrivals { get; set; } = Array.Empty<ProductSummaryDto>();
    public IReadOnlyList<ProductSummaryDto> Bestsellers { get; set; } = Array.Empty<ProductSummaryDto>();
}

public class NewsletterRequest
{
    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = string.Empty;
}
