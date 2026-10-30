using System.ComponentModel.DataAnnotations;

namespace Noor.Core.Dtos;

public class CartDto
{
    public Guid Id { get; set; }
    public IReadOnlyList<CartItemDto> Items { get; set; } = Array.Empty<CartItemDto>();
    public decimal Subtotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal ShippingEstimate { get; set; }
    public decimal Total { get; set; }
    public decimal FreeShippingRemaining { get; set; }
    public string? CouponCode { get; set; }
    public int ItemCount { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CartItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSlug { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int VariantId { get; set; }
    public string Size { get; set; } = string.Empty;
    public string ColorName { get; set; } = string.Empty;
    public string ColorHex { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public int StockQuantity { get; set; }
    public decimal LineTotal { get; set; }
}

public class AddCartItemRequest
{
    [Range(1, int.MaxValue)]
    public int VariantId { get; set; }

    [Range(1, 20)]
    public int Quantity { get; set; } = 1;
}

public class UpdateCartItemRequest
{
    [Range(0, 20)]
    public int Quantity { get; set; }
}

public class ApplyCouponRequest
{
    [Required, StringLength(40)]
    public string Code { get; set; } = string.Empty;
}

public class MergeCartRequest
{
    [Required, StringLength(80)]
    public string GuestKey { get; set; } = string.Empty;
}
