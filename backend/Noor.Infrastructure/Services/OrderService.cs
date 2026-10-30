using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Noor.Core.Common;
using Noor.Core.Dtos;
using Noor.Core.Entities;
using Noor.Core.Options;
using Noor.Infrastructure.Data;
using Noor.Infrastructure.Helpers;

namespace Noor.Infrastructure.Services;

public class OrderService
{
    private readonly NoorDbContext _db;
    private readonly StoreOptions _store;
    private readonly CartService _cartService;

    public OrderService(NoorDbContext db, IOptions<StoreOptions> store, CartService cartService)
    {
        _db = db;
        _store = store.Value;
        _cartService = cartService;
    }

    public async Task<OrderDetailDto> PlaceOrderAsync(Guid userId, CheckoutRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId)
                   ?? throw new AuthenticationException("Please sign in to place an order.");
        var address = await _db.Addresses.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.AddressId && a.UserId == userId)
            ?? throw new NotFoundException("Select a valid shipping address.");

        if (!string.Equals(address.Country.Trim(), "India", StringComparison.OrdinalIgnoreCase))
        {
            throw new ValidationFailedException("Noor currently delivers within India only. Please select an Indian address.");
        }

        if (request.PaymentMethod != PaymentMethod.CashOnDelivery)
        {
            throw new ValidationFailedException("Cash on delivery is the only payment method available at checkout.");
        }

        var cart = await _cartService.GetCartEntityForCheckoutAsync(userId);
        if (cart.Items.Count == 0)
        {
            throw new ValidationFailedException("Your cart is empty.");
        }

        foreach (var item in cart.Items)
        {
            if (!item.ProductVariant.Product.IsActive)
            {
                throw new ValidationFailedException($"{item.ProductVariant.Product.Name} is no longer available.");
            }
            if (item.Quantity > item.ProductVariant.StockQuantity)
            {
                throw new ValidationFailedException(
                    $"Only {item.ProductVariant.StockQuantity} of {item.ProductVariant.Product.Name} ({item.ProductVariant.Size}, {item.ProductVariant.ColorName}) remain.");
            }
        }

        var subtotal = Money.Round(cart.Items.Sum(i => CartService.UnitPrice(i) * i.Quantity));
        decimal discount = 0;
        Coupon? coupon = null;

        if (!string.IsNullOrWhiteSpace(cart.CouponCode))
        {
            coupon = await _db.Coupons.FirstOrDefaultAsync(c => c.Code == cart.CouponCode.ToUpper());
            var evaluation = CouponEvaluator.Evaluate(coupon, subtotal, DateTime.UtcNow);
            if (!evaluation.IsValid)
            {
                throw new ValidationFailedException(evaluation.Error ?? "The applied coupon is not valid.");
            }
            discount = evaluation.Discount;
        }

        var shipping = _cartService.CalculateShipping(subtotal - discount, request.ShippingMethod);
        var total = Money.Round(subtotal - discount + shipping);

        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(),
            UserId = user.Id,
            Email = user.Email,
            Status = OrderStatus.Pending,
            PaymentStatus = PaymentStatus.Pending,
            PaymentMethod = PaymentMethod.CashOnDelivery,
            ShippingMethod = request.ShippingMethod,
            PaymentReference = null,
            Subtotal = subtotal,
            DiscountTotal = discount,
            ShippingTotal = shipping,
            Total = total,
            CouponCode = coupon?.Code,
            ShippingFullName = address.FullName,
            ShippingPhone = address.Phone,
            ShippingLine1 = address.Line1,
            ShippingLine2 = address.Line2,
            ShippingCity = address.City,
            ShippingState = address.State,
            ShippingPostalCode = address.PostalCode,
            ShippingCountry = address.Country,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        foreach (var item in cart.Items)
        {
            var variant = item.ProductVariant;
            var unitPrice = Money.Round(variant.Product.Price + variant.PriceAdjustment);
            order.Items.Add(new OrderItem
            {
                ProductId = variant.ProductId,
                ProductVariantId = variant.Id,
                ProductName = variant.Product.Name,
                ProductSlug = variant.Product.Slug,
                ImageUrl = variant.Product.Images.OrderBy(i => i.SortOrder).FirstOrDefault()?.Url,
                Sku = variant.Sku,
                Size = variant.Size,
                ColorName = variant.ColorName,
                UnitPrice = unitPrice,
                Quantity = item.Quantity,
                LineTotal = Money.Round(unitPrice * item.Quantity)
            });
            variant.StockQuantity -= item.Quantity;
        }

        if (coupon is not null)
        {
            coupon.TimesRedeemed++;
        }

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            _db.Orders.Add(order);
            _db.Carts.Remove(cart);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        return await GetOrderAsync(userId, order.Id);
    }

    public async Task<IReadOnlyList<OrderSummaryDto>> GetMyOrdersAsync(Guid userId)
    {
        var orders = await _db.Orders.AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
        return orders.Select(DtoMapper.ToOrderSummary).ToList();
    }

    public async Task<OrderDetailDto> GetOrderAsync(Guid userId, Guid orderId)
    {
        var order = await _db.Orders.AsNoTracking()
            .Include(o => o.Items)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId)
            ?? throw new NotFoundException("Order not found.");
        return DtoMapper.ToOrderDetail(order);
    }

    public async Task<OrderDetailDto> CancelOrderAsync(Guid userId, Guid orderId)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId)
            ?? throw new NotFoundException("Order not found.");

        if (order.Status is OrderStatus.Shipped or OrderStatus.Delivered or OrderStatus.Cancelled or OrderStatus.Refunded)
        {
            throw new ValidationFailedException("This order can no longer be cancelled online. Please contact care.");
        }

        await RestockAsync(order);
        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;
        if (order.PaymentStatus == PaymentStatus.Paid)
        {
            order.PaymentStatus = PaymentStatus.Refunded;
        }
        if (!string.IsNullOrWhiteSpace(order.CouponCode))
        {
            var coupon = await _db.Coupons.FirstOrDefaultAsync(c => c.Code == order.CouponCode);
            if (coupon is not null && coupon.TimesRedeemed > 0) coupon.TimesRedeemed--;
        }

        await _db.SaveChangesAsync();
        return DtoMapper.ToOrderDetail(order);
    }

    public async Task RestockAsync(Order order)
    {
        var variantIds = order.Items.Where(i => i.ProductVariantId.HasValue)
            .Select(i => i.ProductVariantId!.Value).Distinct().ToList();
        var variants = await _db.ProductVariants.Where(v => variantIds.Contains(v.Id)).ToListAsync();
        foreach (var item in order.Items)
        {
            var variant = variants.FirstOrDefault(v => v.Id == item.ProductVariantId);
            if (variant is not null) variant.StockQuantity += item.Quantity;
        }
    }

    private static string GenerateOrderNumber() =>
        $"NR-{DateTime.UtcNow:yyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
}
