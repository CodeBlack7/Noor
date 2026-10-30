using System.ComponentModel.DataAnnotations;
using Noor.Core.Entities;

namespace Noor.Core.Dtos;

public class CheckoutRequest
{
    [Range(1, int.MaxValue)]
    public int AddressId { get; set; }

    public ShippingMethod ShippingMethod { get; set; } = ShippingMethod.Standard;

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;

    [StringLength(600)]
    public string? Notes { get; set; }
}

public class OrderAddressDto
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

public class OrderSummaryDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal Total { get; set; }
    public int ItemCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? FirstItemImage { get; set; }
    public string? FirstItemName { get; set; }
}

public class OrderItemDto
{
    public int Id { get; set; }
    public int? ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSlug { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string ColorName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}

public class OrderDetailDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public ShippingMethod ShippingMethod { get; set; }
    public string? PaymentReference { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal ShippingTotal { get; set; }
    public decimal Total { get; set; }
    public string? CouponCode { get; set; }
    public OrderAddressDto ShippingAddress { get; set; } = new();
    public string? Notes { get; set; }
    public string? Carrier { get; set; }
    public string? TrackingNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public IReadOnlyList<OrderItemDto> Items { get; set; } = Array.Empty<OrderItemDto>();
}

public class UpdateOrderStatusRequest
{
    public OrderStatus Status { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }

    [StringLength(80)]
    public string? Carrier { get; set; }

    [StringLength(120)]
    public string? TrackingNumber { get; set; }
}
