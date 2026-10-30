using Noor.Core.Dtos;
using Noor.Core.Entities;

namespace Noor.Infrastructure.Services;

public static class DtoMapper
{
    public static UserDto ToUserDto(ApplicationUser user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Email = user.Email,
        Phone = user.Phone,
        Role = user.Role,
        MarketingOptIn = user.MarketingOptIn,
        CreatedAt = user.CreatedAt
    };

    public static CategoryDto ToCategoryDto(Category category, int productCount = 0) => new()
    {
        Id = category.Id,
        Name = category.Name,
        Slug = category.Slug,
        Description = category.Description,
        ImageUrl = category.ImageUrl,
        ProductCount = productCount
    };

    public static ProductSummaryDto ToSummary(Product product, DateTime? now = null)
    {
        var current = now ?? DateTime.UtcNow;
        var variants = product.Variants.OrderBy(v => v.Size).ToList();
        var colors = product.Variants
            .GroupBy(v => v.ColorName)
            .Select(g => new ColorSwatchDto { Name = g.Key, Hex = g.First().ColorHex })
            .ToList();

        return new ProductSummaryDto
        {
            Id = product.Id,
            Name = product.Name,
            Slug = product.Slug,
            ShortDescription = product.ShortDescription,
            Price = product.Price,
            CompareAtPrice = product.CompareAtPrice,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty,
            CategorySlug = product.Category?.Slug ?? string.Empty,
            PrimaryImageUrl = product.Images.OrderBy(i => i.SortOrder).FirstOrDefault()?.Url,
            IsFeatured = product.IsFeatured,
            IsNew = product.CreatedAt >= current.AddDays(-30),
            InStock = variants.Any(v => v.StockQuantity > 0),
            Colors = colors,
            Sizes = variants.Select(v => v.Size).Distinct().ToList(),
            AverageRating = product.Reviews.Where(r => r.IsApproved).Select(r => (double?)r.Rating).Average() ?? 0,
            ReviewCount = product.Reviews.Count(r => r.IsApproved)
        };
    }

    public static ProductDetailDto ToDetail(Product product)
    {
        var summary = ToSummary(product);
        var reviews = product.Reviews
            .Where(r => r.IsApproved)
            .OrderByDescending(r => r.CreatedAt)
            .ToList();

        return new ProductDetailDto
        {
            Id = summary.Id,
            Name = summary.Name,
            Slug = summary.Slug,
            ShortDescription = summary.ShortDescription,
            Description = product.Description,
            Price = product.Price,
            CompareAtPrice = product.CompareAtPrice,
            CategoryId = summary.CategoryId,
            CategoryName = summary.CategoryName,
            CategorySlug = summary.CategorySlug,
            Fabric = product.Fabric,
            CareInstructions = product.CareInstructions,
            IsActive = product.IsActive,
            IsFeatured = product.IsFeatured,
            InStock = summary.InStock,
            Images = product.Images.OrderBy(i => i.SortOrder)
                .Select(i => new ProductImageDto { Id = i.Id, Url = i.Url, AltText = i.AltText }).ToList(),
            Variants = product.Variants.OrderBy(v => v.Size).ThenBy(v => v.ColorName)
                .Select(v => new ProductVariantDto
                {
                    Id = v.Id,
                    Size = v.Size,
                    ColorName = v.ColorName,
                    ColorHex = v.ColorHex,
                    Sku = v.Sku,
                    StockQuantity = v.StockQuantity,
                    Price = product.Price + v.PriceAdjustment,
                    PriceAdjustment = v.PriceAdjustment
                }).ToList(),
            Colors = summary.Colors,
            Sizes = summary.Sizes,
            AverageRating = summary.AverageRating,
            ReviewCount = summary.ReviewCount,
            Reviews = reviews.Take(8).Select(ToReviewDto).ToList()
        };
    }

    public static ReviewDto ToReviewDto(Review review)
    {
        var parts = (review.User?.FullName ?? string.Empty)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var author = parts.Length switch
        {
            0 => "Noor customer",
            1 => parts[0],
            _ => $"{parts[0]} {parts[^1][0]}."
        };

        return new ReviewDto
        {
            Id = review.Id,
            Rating = review.Rating,
            Title = review.Title,
            Comment = review.Comment,
            AuthorName = author,
            CreatedAt = review.CreatedAt
        };
    }

    public static AddressDto ToAddressDto(Address address) => new()
    {
        Id = address.Id,
        Label = address.Label,
        FullName = address.FullName,
        Phone = address.Phone,
        Line1 = address.Line1,
        Line2 = address.Line2,
        City = address.City,
        State = address.State,
        PostalCode = address.PostalCode,
        Country = address.Country,
        IsDefault = address.IsDefault
    };

    public static CartItemDto ToCartItemDto(CartItem item)
    {
        var variant = item.ProductVariant;
        var product = variant.Product;
        return new CartItemDto
        {
            Id = item.Id,
            ProductId = product.Id,
            ProductName = product.Name,
            ProductSlug = product.Slug,
            ImageUrl = product.Images.OrderBy(i => i.SortOrder).FirstOrDefault()?.Url,
            VariantId = variant.Id,
            Size = variant.Size,
            ColorName = variant.ColorName,
            ColorHex = variant.ColorHex,
            Sku = variant.Sku,
            UnitPrice = product.Price + variant.PriceAdjustment,
            Quantity = item.Quantity,
            StockQuantity = variant.StockQuantity,
            LineTotal = (product.Price + variant.PriceAdjustment) * item.Quantity
        };
    }

    public static OrderSummaryDto ToOrderSummary(Order order) => new()
    {
        Id = order.Id,
        OrderNumber = order.OrderNumber,
        Status = order.Status,
        PaymentStatus = order.PaymentStatus,
        PaymentMethod = order.PaymentMethod,
        Total = order.Total,
        ItemCount = order.Items.Sum(i => i.Quantity),
        CreatedAt = order.CreatedAt,
        FirstItemImage = order.Items.FirstOrDefault()?.ImageUrl,
        FirstItemName = order.Items.FirstOrDefault()?.ProductName
    };

    public static OrderDetailDto ToOrderDetail(Order order) => new()
    {
        Id = order.Id,
        OrderNumber = order.OrderNumber,
        Email = order.Email,
        CustomerName = order.User?.FullName ?? order.ShippingFullName,
        Status = order.Status,
        PaymentStatus = order.PaymentStatus,
        PaymentMethod = order.PaymentMethod,
        ShippingMethod = order.ShippingMethod,
        PaymentReference = order.PaymentReference,
        Subtotal = order.Subtotal,
        DiscountTotal = order.DiscountTotal,
        ShippingTotal = order.ShippingTotal,
        Total = order.Total,
        CouponCode = order.CouponCode,
        ShippingAddress = new OrderAddressDto
        {
            FullName = order.ShippingFullName,
            Phone = order.ShippingPhone,
            Line1 = order.ShippingLine1,
            Line2 = order.ShippingLine2,
            City = order.ShippingCity,
            State = order.ShippingState,
            PostalCode = order.ShippingPostalCode,
            Country = order.ShippingCountry
        },
        Notes = order.Notes,
        Carrier = order.Carrier,
        TrackingNumber = order.TrackingNumber,
        CreatedAt = order.CreatedAt,
        UpdatedAt = order.UpdatedAt,
        Items = order.Items.Select(i => new OrderItemDto
        {
            Id = i.Id,
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            ProductSlug = i.ProductSlug,
            ImageUrl = i.ImageUrl,
            Sku = i.Sku,
            Size = i.Size,
            ColorName = i.ColorName,
            UnitPrice = i.UnitPrice,
            Quantity = i.Quantity,
            LineTotal = i.LineTotal
        }).ToList()
    };
}
