using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Noor.Core.Common;
using Noor.Core.Dtos;
using Noor.Core.Entities;
using Noor.Core.Options;
using Noor.Infrastructure.Data;
using Noor.Infrastructure.Helpers;

namespace Noor.Infrastructure.Services;

public class CartService
{
    private readonly NoorDbContext _db;
    private readonly StoreOptions _store;

    public CartService(NoorDbContext db, IOptions<StoreOptions> store)
    {
        _db = db;
        _store = store.Value;
    }

    public async Task<CartDto> GetCartAsync(Guid? userId, string? guestKey)
    {
        if (userId is null && string.IsNullOrWhiteSpace(guestKey))
        {
            return new CartDto();
        }

        var cart = await FindCartQuery(userId, guestKey).FirstOrDefaultAsync();
        if (cart is null)
        {
            if (userId is null) return new CartDto();
            cart = await CreateCartAsync(userId, null);
        }

        return BuildDto(cart);
    }

    public async Task<CartDto> AddItemAsync(Guid? userId, string? guestKey, AddCartItemRequest request)
    {
        var cart = await GetOrCreateCartAsync(userId, guestKey, create: true)
                   ?? throw new ValidationFailedException("A cart key is required for guest checkout.");

        var variant = await _db.ProductVariants
            .Include(v => v.Product).ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(v => v.Id == request.VariantId)
            ?? throw new NotFoundException("That product option could not be found.");

        if (!variant.Product.IsActive)
        {
            throw new ValidationFailedException("This product is no longer available.");
        }
        if (variant.StockQuantity < 1)
        {
            throw new ValidationFailedException("This option is sold out.");
        }

        var existing = cart.Items.FirstOrDefault(i => i.ProductVariantId == variant.Id);
        var targetQuantity = request.Quantity + (existing?.Quantity ?? 0);
        if (targetQuantity > variant.StockQuantity)
        {
            throw new ValidationFailedException($"Only {variant.StockQuantity} left in this option.");
        }
        if (targetQuantity > 20)
        {
            throw new ValidationFailedException("You can add up to 20 units of one option.");
        }

        if (existing is not null)
        {
            existing.Quantity = targetQuantity;
        }
        else
        {
            cart.Items.Add(new CartItem
            {
                ProductVariantId = variant.Id,
                Quantity = request.Quantity,
                AddedAt = DateTime.UtcNow
            });
        }

        cart.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return await ReloadAndBuildAsync(cart.Id);
    }

    public async Task<CartDto> UpdateItemAsync(Guid? userId, string? guestKey, int itemId, UpdateCartItemRequest request)
    {
        var cart = await GetOrCreateCartAsync(userId, guestKey, create: false)
                   ?? throw new NotFoundException("Cart not found.");
        var item = cart.Items.FirstOrDefault(i => i.Id == itemId)
                   ?? throw new NotFoundException("Cart item not found.");

        if (request.Quantity <= 0)
        {
            _db.CartItems.Remove(item);
        }
        else
        {
            if (request.Quantity > item.ProductVariant.StockQuantity)
            {
                throw new ValidationFailedException($"Only {item.ProductVariant.StockQuantity} left in this option.");
            }
            item.Quantity = request.Quantity;
        }

        cart.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return await ReloadAndBuildAsync(cart.Id);
    }

    public async Task<CartDto> RemoveItemAsync(Guid? userId, string? guestKey, int itemId)
    {
        var cart = await GetOrCreateCartAsync(userId, guestKey, create: false)
                   ?? throw new NotFoundException("Cart not found.");
        var item = cart.Items.FirstOrDefault(i => i.Id == itemId)
                   ?? throw new NotFoundException("Cart item not found.");
        _db.CartItems.Remove(item);
        cart.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return await ReloadAndBuildAsync(cart.Id);
    }

    public async Task<CartDto> ApplyCouponAsync(Guid? userId, string? guestKey, ApplyCouponRequest request)
    {
        var cart = await GetOrCreateCartAsync(userId, guestKey, create: true)
                   ?? throw new ValidationFailedException("A cart key is required.");
        var code = request.Code.Trim().ToUpperInvariant();
        var coupon = await _db.Coupons.FirstOrDefaultAsync(c => c.Code == code.ToUpper());
        var subtotal = cart.Items.Sum(i => UnitPrice(i) * i.Quantity);
        var evaluation = CouponEvaluator.Evaluate(coupon, subtotal, DateTime.UtcNow);
        if (!evaluation.IsValid)
        {
            throw new ValidationFailedException(evaluation.Error ?? "This coupon is not valid.");
        }

        cart.CouponCode = coupon!.Code;
        cart.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return await ReloadAndBuildAsync(cart.Id);
    }

    public async Task<CartDto> RemoveCouponAsync(Guid? userId, string? guestKey)
    {
        var cart = await GetOrCreateCartAsync(userId, guestKey, create: false)
                   ?? throw new NotFoundException("Cart not found.");
        cart.CouponCode = null;
        cart.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return await ReloadAndBuildAsync(cart.Id);
    }

    public async Task<CartDto> MergeGuestCartAsync(Guid userId, string guestKey)
    {
        var userCart = await FindCartQuery(userId, null).FirstOrDefaultAsync();
        if (userCart is null)
        {
            userCart = await CreateCartAsync(userId, null);
        }

        var guestCart = await FindCartQuery(null, guestKey).FirstOrDefaultAsync();
        if (guestCart is null || guestCart.Id == userCart.Id)
        {
            return BuildDto(userCart);
        }

        foreach (var guestItem in guestCart.Items.ToList())
        {
            var existing = userCart.Items.FirstOrDefault(i => i.ProductVariantId == guestItem.ProductVariantId);
            var stock = guestItem.ProductVariant.StockQuantity;
            if (existing is null)
            {
                userCart.Items.Add(new CartItem
                {
                    ProductVariantId = guestItem.ProductVariantId,
                    Quantity = Math.Min(guestItem.Quantity, stock)
                });
            }
            else
            {
                existing.Quantity = Math.Min(existing.Quantity + guestItem.Quantity, Math.Max(stock, existing.Quantity));
            }
        }

        if (string.IsNullOrWhiteSpace(userCart.CouponCode) && !string.IsNullOrWhiteSpace(guestCart.CouponCode))
        {
            userCart.CouponCode = guestCart.CouponCode;
        }

        userCart.UpdatedAt = DateTime.UtcNow;
        _db.Carts.Remove(guestCart);
        await _db.SaveChangesAsync();
        return await ReloadAndBuildAsync(userCart.Id);
    }

    public async Task<Cart> GetCartEntityForCheckoutAsync(Guid userId)
    {
        return await _db.Carts
            .Include(c => c.Items).ThenInclude(i => i.ProductVariant).ThenInclude(v => v.Product).ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(c => c.UserId == userId)
            ?? throw new ValidationFailedException("Your cart is empty.");
    }

    public static decimal UnitPrice(CartItem item) => item.ProductVariant.Product.Price + item.ProductVariant.PriceAdjustment;

    public decimal CalculateShipping(decimal subtotalAfterDiscount, ShippingMethod method = ShippingMethod.Standard)
    {
        if (method == ShippingMethod.Express) return _store.ExpressShippingCost;
        return subtotalAfterDiscount >= _store.FreeShippingThreshold ? 0 : _store.StandardShippingCost;
    }

    private IQueryable<Cart> FindCartQuery(Guid? userId, string? guestKey)
    {
        var query = _db.Carts
            .Include(c => c.Items).ThenInclude(i => i.ProductVariant).ThenInclude(v => v.Product).ThenInclude(p => p.Images)
            .AsQueryable();

        if (userId.HasValue)
        {
            return query.Where(c => c.UserId == userId.Value);
        }

        var key = guestKey?.Trim();
        return query.Where(c => c.GuestToken == key);
    }

    private async Task<Cart?> GetOrCreateCartAsync(Guid? userId, string? guestKey, bool create)
    {
        if (userId is null && string.IsNullOrWhiteSpace(guestKey)) return null;
        var cart = await FindCartQuery(userId, guestKey).FirstOrDefaultAsync();
        if (cart is null && create) cart = await CreateCartAsync(userId, guestKey);
        return cart;
    }

    private async Task<Cart> CreateCartAsync(Guid? userId, string? guestKey)
    {
        var cart = new Cart
        {
            UserId = userId,
            GuestToken = userId is null ? guestKey?.Trim() : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Carts.Add(cart);
        await _db.SaveChangesAsync();
        return cart;
    }

    private async Task<CartDto> ReloadAndBuildAsync(Guid cartId)
    {
        var cart = await _db.Carts
            .Include(c => c.Items).ThenInclude(i => i.ProductVariant).ThenInclude(v => v.Product).ThenInclude(p => p.Images)
            .AsNoTracking()
            .FirstAsync(c => c.Id == cartId);
        return BuildDto(cart);
    }

    private CartDto BuildDto(Cart cart)
    {
        var items = cart.Items.OrderBy(i => i.AddedAt).Select(DtoMapper.ToCartItemDto).ToList();
        var subtotal = items.Sum(i => i.LineTotal);

        decimal discount = 0;
        if (!string.IsNullOrWhiteSpace(cart.CouponCode))
        {
            var coupon = _db.Coupons.AsNoTracking().FirstOrDefault(c => c.Code == cart.CouponCode.ToUpper());
            var evaluation = CouponEvaluator.Evaluate(coupon, subtotal, DateTime.UtcNow);
            if (evaluation.IsValid) discount = evaluation.Discount;
        }

        var afterDiscount = subtotal - discount;
        var shipping = items.Count == 0 ? 0 : CalculateShipping(afterDiscount);
        var freeShippingRemaining = items.Count == 0
            ? _store.FreeShippingThreshold
            : Math.Max(0, _store.FreeShippingThreshold - afterDiscount);

        return new CartDto
        {
            Id = cart.Id,
            Items = items,
            Subtotal = Money.Round(subtotal),
            DiscountTotal = Money.Round(discount),
            ShippingEstimate = Money.Round(shipping),
            Total = Money.Round(afterDiscount + shipping),
            FreeShippingRemaining = Money.Round(freeShippingRemaining),
            CouponCode = cart.CouponCode,
            ItemCount = items.Sum(i => i.Quantity),
            UpdatedAt = cart.UpdatedAt
        };
    }
}
