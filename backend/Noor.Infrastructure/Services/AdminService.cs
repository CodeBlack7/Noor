using Microsoft.EntityFrameworkCore;
using Noor.Core.Common;
using Noor.Core.Dtos;
using Noor.Core.Entities;
using Noor.Infrastructure.Data;
using Noor.Infrastructure.Helpers;

namespace Noor.Infrastructure.Services;

public class AdminService
{
    private readonly NoorDbContext _db;
    private readonly OrderService _orderService;

    public AdminService(NoorDbContext db, OrderService orderService)
    {
        _db = db;
        _orderService = orderService;
    }

    public async Task<DashboardDto> GetDashboardAsync()
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startDay = now.Date.AddDays(-6);

        var paidOrders = _db.Orders.AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Paid &&
                        o.Status != OrderStatus.Cancelled &&
                        o.Status != OrderStatus.Refunded);

        var totalRevenue = await paidOrders.SumAsync(o => (decimal?)o.Total) ?? 0;
        var revenueThisMonth = await paidOrders.Where(o => o.CreatedAt >= monthStart)
            .SumAsync(o => (decimal?)o.Total) ?? 0;

        var salesRaw = await paidOrders
            .Where(o => o.CreatedAt >= startDay)
            .Select(o => new { o.CreatedAt, o.Total })
            .ToListAsync();

        var recentOrders = await _db.Orders.AsNoTracking()
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .Take(6)
            .ToListAsync();

        var topRaw = await _db.OrderItems.AsNoTracking()
            .Where(i => i.Order.PaymentStatus == PaymentStatus.Paid &&
                        i.Order.Status != OrderStatus.Cancelled &&
                        i.Order.Status != OrderStatus.Refunded &&
                        i.ProductId != null)
            .GroupBy(i => i.ProductId!.Value)
            .Select(g => new
            {
                ProductId = g.Key,
                UnitsSold = g.Sum(x => x.Quantity),
                Revenue = g.Sum(x => x.LineTotal),
                Name = g.OrderByDescending(x => x.Quantity).Select(x => x.ProductName).FirstOrDefault(),
                ImageUrl = g.OrderByDescending(x => x.Quantity).Select(x => x.ImageUrl).FirstOrDefault()
            })
            .OrderByDescending(x => x.Revenue)
            .Take(5)
            .ToListAsync();

        var topProducts = topRaw.Select(x => new TopProductDto
        {
            ProductId = x.ProductId,
            Name = x.Name ?? "Product",
            ImageUrl = x.ImageUrl,
            UnitsSold = x.UnitsSold,
            Revenue = x.Revenue
        }).ToList();

        var lowStock = await _db.ProductVariants.AsNoTracking()
            .Include(v => v.Product)
            .Where(v => v.StockQuantity <= 5 && v.Product.IsActive)
            .OrderBy(v => v.StockQuantity)
            .Take(8)
            .Select(v => new LowStockDto
            {
                VariantId = v.Id,
                ProductName = v.Product.Name,
                Sku = v.Sku,
                Size = v.Size,
                ColorName = v.ColorName,
                StockQuantity = v.StockQuantity
            })
            .ToListAsync();

        var lowStockCount = await _db.ProductVariants.AsNoTracking()
            .CountAsync(v => v.StockQuantity <= 5 && v.Product.IsActive);

        var salesByDay = Enumerable.Range(0, 7).Select(offset =>
        {
            var date = startDay.AddDays(offset);
            var dayOrders = salesRaw.Where(o => o.CreatedAt.Date == date).ToList();
            return new SalesPointDto
            {
                Date = date,
                Revenue = dayOrders.Sum(o => o.Total),
                Orders = dayOrders.Count
            };
        }).ToList();

        return new DashboardDto
        {
            TotalRevenue = totalRevenue,
            RevenueThisMonth = revenueThisMonth,
            TotalOrders = await _db.Orders.CountAsync(),
            OrdersThisMonth = await _db.Orders.CountAsync(o => o.CreatedAt >= monthStart),
            PendingOrders = await _db.Orders.CountAsync(o =>
                o.Status == OrderStatus.Pending || o.Status == OrderStatus.Confirmed || o.Status == OrderStatus.Processing),
            TotalCustomers = await _db.Users.CountAsync(u => u.Role == Roles.Customer),
            TotalProducts = await _db.Products.CountAsync(p => p.IsActive),
            LowStockCount = lowStockCount,
            NewsletterSubscribers = await _db.NewsletterSubscribers.CountAsync(n => n.IsActive),
            RecentOrders = recentOrders.Select(DtoMapper.ToOrderSummary).ToList(),
            TopProducts = topProducts,
            SalesByDay = salesByDay,
            LowStockVariants = lowStock
        };
    }

    public async Task<PagedResult<AdminProductListItemDto>> GetProductsAsync(AdminProductQuery query)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var products = _db.Products.AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            products = products.Where(p => EF.Functions.Like(p.Name, $"%{term}%") ||
                                           p.Variants.Any(v => EF.Functions.Like(v.Sku, $"%{term}%")));
        }
        if (query.CategoryId.HasValue) products = products.Where(p => p.CategoryId == query.CategoryId);
        if (query.IsActive.HasValue) products = products.Where(p => p.IsActive == query.IsActive);

        products = products.OrderByDescending(p => p.UpdatedAt);
        var total = await products.CountAsync();
        var pageProducts = await products.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        var ids = pageProducts.Select(p => p.Id).ToList();

        var sold = await _db.OrderItems.AsNoTracking()
            .Where(i => i.ProductId != null && ids.Contains(i.ProductId.Value) &&
                        i.Order.Status != OrderStatus.Cancelled && i.Order.Status != OrderStatus.Refunded)
            .GroupBy(i => i.ProductId!.Value)
            .Select(g => new { ProductId = g.Key, Quantity = g.Sum(i => i.Quantity) })
            .ToDictionaryAsync(x => x.ProductId, x => x.Quantity);

        return new PagedResult<AdminProductListItemDto>
        {
            Items = pageProducts.Select(p => new AdminProductListItemDto
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                CategoryName = p.Category.Name,
                Price = p.Price,
                CompareAtPrice = p.CompareAtPrice,
                ImageUrl = p.Images.OrderBy(i => i.SortOrder).FirstOrDefault()?.Url,
                IsActive = p.IsActive,
                IsFeatured = p.IsFeatured,
                TotalStock = p.Variants.Sum(v => v.StockQuantity),
                VariantCount = p.Variants.Count,
                SoldCount = sold.GetValueOrDefault(p.Id),
                UpdatedAt = p.UpdatedAt
            }).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<ProductDetailDto> GetProductAsync(int id)
    {
        var product = await _db.Products.AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .Include(p => p.Reviews)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundException("Product not found.");
        return DtoMapper.ToDetail(product);
    }

    public async Task<ProductDetailDto> CreateProductAsync(ProductUpsertRequest request)
    {
        ValidateProductRequest(request);
        var categoryExists = await _db.Categories.AnyAsync(c => c.Id == request.CategoryId);
        if (!categoryExists) throw new ValidationFailedException("Choose a valid category.");

        var slug = Slugifier.UniqueSlug(request.Slug ?? request.Name,
            s => _db.Products.Any(p => p.Slug == s));
        await EnsureSkusAvailableAsync(request.Variants, null);

        var product = new Product
        {
            Name = request.Name.Trim(),
            Slug = slug,
            CategoryId = request.CategoryId,
            ShortDescription = request.ShortDescription.Trim(),
            Description = request.Description.Trim(),
            Price = request.Price,
            CompareAtPrice = request.CompareAtPrice,
            Fabric = request.Fabric.Trim(),
            CareInstructions = string.IsNullOrWhiteSpace(request.CareInstructions) ? null : request.CareInstructions.Trim(),
            IsActive = request.IsActive,
            IsFeatured = request.IsFeatured,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        for (var i = 0; i < request.Images.Count; i++)
        {
            product.Images.Add(new ProductImage
            {
                Url = request.Images[i].Url.Trim(),
                AltText = request.Images[i].AltText?.Trim(),
                SortOrder = i
            });
        }

        foreach (var variant in request.Variants)
        {
            product.Variants.Add(ToVariant(variant));
        }

        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return await GetProductAsync(product.Id);
    }

    public async Task<ProductDetailDto> UpdateProductAsync(int id, ProductUpsertRequest request)
    {
        ValidateProductRequest(request);
        var product = await _db.Products
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundException("Product not found.");

        if (!await _db.Categories.AnyAsync(c => c.Id == request.CategoryId))
            throw new ValidationFailedException("Choose a valid category.");
        await EnsureSkusAvailableAsync(request.Variants, id);

        product.Name = request.Name.Trim();
        product.Slug = Slugifier.UniqueSlug(request.Slug ?? request.Name,
            s => _db.Products.Any(p => p.Slug == s && p.Id != id));
        product.CategoryId = request.CategoryId;
        product.ShortDescription = request.ShortDescription.Trim();
        product.Description = request.Description.Trim();
        product.Price = request.Price;
        product.CompareAtPrice = request.CompareAtPrice;
        product.Fabric = request.Fabric.Trim();
        product.CareInstructions = string.IsNullOrWhiteSpace(request.CareInstructions) ? null : request.CareInstructions.Trim();
        product.IsActive = request.IsActive;
        product.IsFeatured = request.IsFeatured;
        product.UpdatedAt = DateTime.UtcNow;

        _db.ProductImages.RemoveRange(product.Images);
        product.Images.Clear();
        for (var i = 0; i < request.Images.Count; i++)
        {
            product.Images.Add(new ProductImage
            {
                ProductId = product.Id,
                Url = request.Images[i].Url.Trim(),
                AltText = request.Images[i].AltText?.Trim(),
                SortOrder = i
            });
        }

        var incomingIds = request.Variants.Where(v => v.Id.HasValue).Select(v => v.Id!.Value).ToHashSet();
        var toRemove = product.Variants.Where(v => !incomingIds.Contains(v.Id)).ToList();
        _db.ProductVariants.RemoveRange(toRemove);

        foreach (var input in request.Variants)
        {
            if (input.Id.HasValue)
            {
                var existing = product.Variants.FirstOrDefault(v => v.Id == input.Id.Value);
                if (existing is null) continue;
                existing.Size = input.Size.Trim();
                existing.ColorName = input.ColorName.Trim();
                existing.ColorHex = input.ColorHex;
                existing.Sku = input.Sku.Trim().ToUpperInvariant();
                existing.StockQuantity = input.StockQuantity;
                existing.PriceAdjustment = input.PriceAdjustment;
            }
            else
            {
                var variant = ToVariant(input);
                variant.ProductId = product.Id;
                product.Variants.Add(variant);
            }
        }

        await _db.SaveChangesAsync();
        return await GetProductAsync(product.Id);
    }

    public async Task<ServiceResult> DeleteProductAsync(int id)
    {
        var product = await _db.Products.Include(p => p.Variants).FirstOrDefaultAsync(p => p.Id == id)
                      ?? throw new NotFoundException("Product not found.");
        var hasOrders = await _db.OrderItems.AnyAsync(i => i.ProductId == id);
        if (hasOrders)
        {
            product.IsActive = false;
            product.IsFeatured = false;
            product.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ServiceResult.Ok("This product has order history, so it was archived instead of deleted.");
        }

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
        return ServiceResult.Ok("Product deleted.");
    }

    public async Task<IReadOnlyList<AdminCategoryDto>> GetCategoriesAsync()
    {
        return await _db.Categories.AsNoTracking()
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
            .Select(c => new AdminCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                IsActive = c.IsActive,
                SortOrder = c.SortOrder,
                ProductCount = c.Products.Count
            })
            .ToListAsync();
    }

    public async Task<AdminCategoryDto> CreateCategoryAsync(CategoryUpsertRequest request)
    {
        var slug = Slugifier.UniqueSlug(request.Slug ?? request.Name,
            s => _db.Categories.Any(c => c.Slug == s), "collection");
        var category = new Category
        {
            Name = request.Name.Trim(),
            Slug = slug,
            Description = request.Description?.Trim(),
            ImageUrl = request.ImageUrl?.Trim(),
            IsActive = request.IsActive,
            SortOrder = request.SortOrder
        };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return (await GetCategoriesAsync()).First(c => c.Id == category.Id);
    }

    public async Task<AdminCategoryDto> UpdateCategoryAsync(int id, CategoryUpsertRequest request)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id)
                       ?? throw new NotFoundException("Collection not found.");
        category.Name = request.Name.Trim();
        category.Slug = Slugifier.UniqueSlug(request.Slug ?? request.Name,
            s => _db.Categories.Any(c => c.Slug == s && c.Id != id), "collection");
        category.Description = request.Description?.Trim();
        category.ImageUrl = request.ImageUrl?.Trim();
        category.IsActive = request.IsActive;
        category.SortOrder = request.SortOrder;
        await _db.SaveChangesAsync();
        return (await GetCategoriesAsync()).First(c => c.Id == category.Id);
    }

    public async Task<ServiceResult> DeleteCategoryAsync(int id)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id)
                       ?? throw new NotFoundException("Collection not found.");
        if (await _db.Products.AnyAsync(p => p.CategoryId == id))
        {
            throw new ConflictException("This collection still has products. Move or delete them first.");
        }
        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        return ServiceResult.Ok("Collection deleted.");
    }

    public async Task<PagedResult<OrderSummaryDto>> GetOrdersAsync(AdminOrderQuery query)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var orders = _db.Orders.AsNoTracking().Include(o => o.Items).AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            orders = orders.Where(o => EF.Functions.Like(o.OrderNumber, $"%{term}%") ||
                                       EF.Functions.Like(o.Email, $"%{term}%") ||
                                       EF.Functions.Like(o.ShippingFullName, $"%{term}%"));
        }
        if (query.Status.HasValue) orders = orders.Where(o => o.Status == query.Status);

        var total = await orders.CountAsync();
        var items = await orders.OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<OrderSummaryDto>
        {
            Items = items.Select(DtoMapper.ToOrderSummary).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<OrderDetailDto> GetOrderAsync(Guid id)
    {
        var order = await _db.Orders.AsNoTracking()
            .Include(o => o.Items)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == id)
            ?? throw new NotFoundException("Order not found.");
        return DtoMapper.ToOrderDetail(order);
    }

    public async Task<OrderDetailDto> UpdateOrderStatusAsync(Guid id, UpdateOrderStatusRequest request)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == id)
            ?? throw new NotFoundException("Order not found.");

        if (order.Status is OrderStatus.Cancelled or OrderStatus.Refunded &&
            request.Status is not (OrderStatus.Cancelled or OrderStatus.Refunded))
        {
            throw new ConflictException("A cancelled or refunded order cannot be reopened.");
        }

        if (request.Status == OrderStatus.Cancelled && order.Status != OrderStatus.Cancelled)
        {
            await _orderService.RestockAsync(order);
            if (order.PaymentStatus == PaymentStatus.Paid)
            {
                order.PaymentStatus = PaymentStatus.Refunded;
            }
        }

        order.Status = request.Status;
        if (request.PaymentStatus.HasValue)
        {
            order.PaymentStatus = request.PaymentStatus.Value;
        }
        order.Carrier = string.IsNullOrWhiteSpace(request.Carrier) ? order.Carrier : request.Carrier.Trim();
        order.TrackingNumber = string.IsNullOrWhiteSpace(request.TrackingNumber) ? order.TrackingNumber : request.TrackingNumber.Trim();
        order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return DtoMapper.ToOrderDetail(order);
    }

    public async Task<IReadOnlyList<CouponDto>> GetCouponsAsync()
    {
        var now = DateTime.UtcNow;
        var coupons = await _db.Coupons.AsNoTracking().OrderByDescending(c => c.Id).ToListAsync();
        return coupons.Select(c => new CouponDto
        {
            Id = c.Id,
            Code = c.Code,
            Description = c.Description,
            DiscountType = c.DiscountType,
            Value = c.Value,
            MinOrderAmount = c.MinOrderAmount,
            MaxRedemptions = c.MaxRedemptions,
            TimesRedeemed = c.TimesRedeemed,
            StartsAt = c.StartsAt,
            EndsAt = c.EndsAt,
            IsActive = c.IsActive,
            IsLive = c.IsActive &&
                     (!c.StartsAt.HasValue || c.StartsAt <= now) &&
                     (!c.EndsAt.HasValue || c.EndsAt >= now) &&
                     (!c.MaxRedemptions.HasValue || c.TimesRedeemed < c.MaxRedemptions)
        }).ToList();
    }

    public async Task<CouponDto> CreateCouponAsync(CouponUpsertRequest request)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        if (await _db.Coupons.AnyAsync(c => c.Code == code))
            throw new ConflictException("A coupon with this code already exists.");
        ValidateCoupon(request);
        var coupon = new Coupon
        {
            Code = code,
            Description = request.Description?.Trim(),
            DiscountType = request.DiscountType,
            Value = request.Value,
            MinOrderAmount = request.MinOrderAmount,
            MaxRedemptions = request.MaxRedemptions,
            StartsAt = request.StartsAt,
            EndsAt = request.EndsAt,
            IsActive = request.IsActive
        };
        _db.Coupons.Add(coupon);
        await _db.SaveChangesAsync();
        return (await GetCouponsAsync()).First(c => c.Id == coupon.Id);
    }

    public async Task<CouponDto> UpdateCouponAsync(int id, CouponUpsertRequest request)
    {
        var coupon = await _db.Coupons.FirstOrDefaultAsync(c => c.Id == id)
                     ?? throw new NotFoundException("Coupon not found.");
        var code = request.Code.Trim().ToUpperInvariant();
        if (await _db.Coupons.AnyAsync(c => c.Code == code && c.Id != id))
            throw new ConflictException("A coupon with this code already exists.");
        ValidateCoupon(request);
        coupon.Code = code;
        coupon.Description = request.Description?.Trim();
        coupon.DiscountType = request.DiscountType;
        coupon.Value = request.Value;
        coupon.MinOrderAmount = request.MinOrderAmount;
        coupon.MaxRedemptions = request.MaxRedemptions;
        coupon.StartsAt = request.StartsAt;
        coupon.EndsAt = request.EndsAt;
        coupon.IsActive = request.IsActive;
        await _db.SaveChangesAsync();
        return (await GetCouponsAsync()).First(c => c.Id == coupon.Id);
    }

    public async Task DeleteCouponAsync(int id)
    {
        var coupon = await _db.Coupons.FirstOrDefaultAsync(c => c.Id == id)
                     ?? throw new NotFoundException("Coupon not found.");
        _db.Coupons.Remove(coupon);
        await _db.SaveChangesAsync();
    }

    public async Task<PagedResult<AdminCustomerDto>> GetCustomersAsync(AdminCustomerQuery query)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var users = _db.Users.AsNoTracking().Where(u => u.Role == Roles.Customer).AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            users = users.Where(u => EF.Functions.Like(u.FullName, $"%{term}%") ||
                                     EF.Functions.Like(u.Email, $"%{term}%"));
        }

        var total = await users.CountAsync();
        var items = await users.OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(u => new AdminCustomerDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Phone = u.Phone,
                IsActive = u.IsActive,
                MarketingOptIn = u.MarketingOptIn,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt,
                OrderCount = u.Orders.Count,
                TotalSpent = u.Orders
                    .Where(o => o.Status != OrderStatus.Cancelled && o.Status != OrderStatus.Refunded)
                    .Sum(o => (decimal?)o.Total) ?? 0
            })
            .ToListAsync();

        return new PagedResult<AdminCustomerDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    private static void ValidateProductRequest(ProductUpsertRequest request)
    {
        if (request.Images.Count == 0)
            throw new ValidationFailedException("Add at least one product image.");
        if (request.Variants.Count == 0)
            throw new ValidationFailedException("Add at least one size and colour variant.");
        if (request.CompareAtPrice.HasValue && request.CompareAtPrice <= request.Price)
            throw new ValidationFailedException("Compare-at price must be higher than the selling price.");

        var duplicateSku = request.Variants.GroupBy(v => v.Sku.Trim(), StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(g => g.Count() > 1);
        if (duplicateSku is not null) throw new ValidationFailedException($"SKU '{duplicateSku.Key}' is used more than once.");

        var duplicateVariant = request.Variants
            .GroupBy(v => $"{v.Size.Trim().ToLowerInvariant()}|{v.ColorName.Trim().ToLowerInvariant()}")
            .FirstOrDefault(g => g.Count() > 1);
        if (duplicateVariant is not null)
            throw new ValidationFailedException("Each size and colour combination must be unique.");
    }

    private async Task EnsureSkusAvailableAsync(List<VariantInput> variants, int? productId)
    {
        var skus = variants.Select(v => v.Sku.Trim().ToUpperInvariant()).ToList();
        var conflicts = await _db.ProductVariants.AsNoTracking()
            .Where(v => skus.Contains(v.Sku) && (!productId.HasValue || v.ProductId != productId.Value))
            .Select(v => v.Sku)
            .ToListAsync();
        if (conflicts.Count > 0)
        {
            throw new ConflictException($"These SKUs are already in use: {string.Join(", ", conflicts)}.");
        }
    }

    private static ProductVariant ToVariant(VariantInput input) => new()
    {
        Size = input.Size.Trim(),
        ColorName = input.ColorName.Trim(),
        ColorHex = input.ColorHex,
        Sku = input.Sku.Trim().ToUpperInvariant(),
        StockQuantity = input.StockQuantity,
        PriceAdjustment = input.PriceAdjustment
    };

    private static void ValidateCoupon(CouponUpsertRequest request)
    {
        if (request.DiscountType == CouponType.Percentage && request.Value > 100)
            throw new ValidationFailedException("A percentage coupon cannot exceed 100%.");
        if (request.StartsAt.HasValue && request.EndsAt.HasValue && request.EndsAt <= request.StartsAt)
            throw new ValidationFailedException("The coupon end date must be after the start date.");
    }
}
