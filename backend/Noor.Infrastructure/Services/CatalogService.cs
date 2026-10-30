using Microsoft.EntityFrameworkCore;
using Noor.Core.Common;
using Noor.Core.Dtos;
using Noor.Core.Entities;
using Noor.Infrastructure.Data;
using Noor.Infrastructure.Helpers;

namespace Noor.Infrastructure.Services;

public class CatalogService
{
    private readonly NoorDbContext _db;

    public CatalogService(NoorDbContext db) => _db = db;

    public async Task<PagedResult<ProductSummaryDto>> GetProductsAsync(ProductQuery query)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 60);
        var products = BuildProductQuery();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            products = products.Where(p =>
                EF.Functions.Like(p.Name, $"%{term}%") ||
                EF.Functions.Like(p.ShortDescription, $"%{term}%") ||
                EF.Functions.Like(p.Description, $"%{term}%") ||
                EF.Functions.Like(p.Fabric, $"%{term}%"));
        }

        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            var category = query.Category.Trim().ToLowerInvariant();
            products = products.Where(p => p.Category.Slug == category);
        }

        if (query.MinPrice.HasValue) products = products.Where(p => p.Price >= query.MinPrice.Value);
        if (query.MaxPrice.HasValue) products = products.Where(p => p.Price <= query.MaxPrice.Value);
        if (!string.IsNullOrWhiteSpace(query.Size))
        {
            var size = query.Size.Trim();
            products = products.Where(p => p.Variants.Any(v => v.Size == size));
        }
        if (!string.IsNullOrWhiteSpace(query.Color))
        {
            var color = query.Color.Trim().ToLowerInvariant();
            products = products.Where(p => p.Variants.Any(v => v.ColorName.ToLower() == color));
        }
        if (query.Featured.HasValue && query.Featured.Value)
        {
            products = products.Where(p => p.IsFeatured);
        }

        products = (query.Sort ?? "featured").ToLowerInvariant() switch
        {
            "newest" => products.OrderByDescending(p => p.CreatedAt),
            "price-asc" => products.OrderBy(p => p.Price),
            "price-desc" => products.OrderByDescending(p => p.Price),
            "rating" => products.OrderByDescending(p => p.Reviews.Where(r => r.IsApproved).Average(r => (double?)r.Rating) ?? 0)
                .ThenByDescending(p => p.Reviews.Count(r => r.IsApproved)),
            "name" => products.OrderBy(p => p.Name),
            _ => products.OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.CreatedAt)
        };

        var total = await products.CountAsync();
        var items = await products
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<ProductSummaryDto>
        {
            Items = items.Select(p => DtoMapper.ToSummary(p)).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<ProductDetailDto> GetProductBySlugAsync(string slug, bool includeRelated = true)
    {
        var product = await BuildProductQuery()
            .Include(p => p.Reviews).ThenInclude(r => r.User)
            .FirstOrDefaultAsync(p => p.Slug == slug)
            ?? throw new NotFoundException("Product not found.");

        var dto = DtoMapper.ToDetail(product);

        if (includeRelated)
        {
            var related = await BuildProductQuery()
                .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id)
                .OrderByDescending(p => p.IsFeatured)
                .ThenByDescending(p => p.CreatedAt)
                .Take(4)
                .ToListAsync();
            dto.RelatedProducts = related.Select(p => DtoMapper.ToSummary(p)).ToList();
        }

        return dto;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync()
    {
        var categories = await _db.Categories.AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
            .Select(c => new
            {
                Category = c,
                ProductCount = c.Products.Count(p => p.IsActive)
            })
            .ToListAsync();

        return categories.Select(x => DtoMapper.ToCategoryDto(x.Category, x.ProductCount)).ToList();
    }

    public async Task<ProductFacetsDto> GetFacetsAsync()
    {
        var products = _db.Products.AsNoTracking().Where(p => p.IsActive);

        var priceRange = await products
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Min = g.Min(p => p.Price),
                Max = g.Max(p => p.Price)
            })
            .FirstOrDefaultAsync();

        var sizes = await _db.ProductVariants.AsNoTracking()
            .Where(v => v.Product.IsActive)
            .Select(v => v.Size)
            .Distinct()
            .ToListAsync();

        var colors = await _db.ProductVariants.AsNoTracking()
            .Where(v => v.Product.IsActive)
            .GroupBy(v => new { v.ColorName, v.ColorHex })
            .Select(g => new ColorSwatchDto { Name = g.Key.ColorName, Hex = g.Key.ColorHex })
            .ToListAsync();

        return new ProductFacetsDto
        {
            MinPrice = priceRange?.Min ?? 0,
            MaxPrice = priceRange?.Max ?? 0,
            Sizes = SortSizes(sizes),
            Colors = colors.OrderBy(c => c.Name).ToList()
        };
    }

    private static List<string> SortSizes(IEnumerable<string> sizes)
    {
        static int Rank(string size) => size.Trim().ToUpperInvariant() switch
        {
            "XS" => 0,
            "S" => 1,
            "M" => 2,
            "L" => 3,
            "XL" => 4,
            "XXL" => 5,
            "S/M" => 6,
            "L/XL" => 7,
            "ONE SIZE" => 99,
            _ => 50
        };

        return sizes
            .OrderBy(Rank)
            .ThenBy(s => s, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<HomeContentDto> GetHomeContentAsync()
    {
        var categories = await GetCategoriesAsync();

        var featured = await BuildProductQuery()
            .Where(p => p.IsFeatured)
            .OrderByDescending(p => p.CreatedAt)
            .Take(8)
            .ToListAsync();

        var newArrivals = await BuildProductQuery()
            .OrderByDescending(p => p.CreatedAt)
            .Take(8)
            .ToListAsync();

        var bestsellerIds = await _db.OrderItems.AsNoTracking()
            .Where(i => i.ProductId != null &&
                        i.Order.Status != OrderStatus.Cancelled &&
                        i.Order.Status != OrderStatus.Refunded)
            .GroupBy(i => i.ProductId!.Value)
            .OrderByDescending(g => g.Sum(i => i.Quantity))
            .Select(g => g.Key)
            .Take(8)
            .ToListAsync();

        var bestsellerProducts = await BuildProductQuery()
            .Where(p => bestsellerIds.Contains(p.Id))
            .ToListAsync();
        var byId = bestsellerProducts.ToDictionary(p => p.Id);
        var bestsellers = bestsellerIds.Where(byId.ContainsKey).Select(id => byId[id]).ToList();

        if (bestsellers.Count < 4)
        {
            var fill = newArrivals.Where(p => bestsellers.All(b => b.Id != p.Id)).Take(4 - bestsellers.Count);
            bestsellers.AddRange(fill);
        }

        return new HomeContentDto
        {
            Categories = categories,
            Featured = featured.Select(p => DtoMapper.ToSummary(p)).ToList(),
            NewArrivals = newArrivals.Select(p => DtoMapper.ToSummary(p)).ToList(),
            Bestsellers = bestsellers.Select(p => DtoMapper.ToSummary(p)).ToList()
        };
    }

    public async Task<ReviewListDto> GetReviewsAsync(int productId)
    {
        var product = await _db.Products.AsNoTracking()
            .Where(p => p.Id == productId)
            .Select(p => new
            {
                p.Id,
                Average = p.Reviews.Where(r => r.IsApproved).Average(r => (double?)r.Rating) ?? 0,
                Count = p.Reviews.Count(r => r.IsApproved)
            })
            .FirstOrDefaultAsync() ?? throw new NotFoundException("Product not found.");

        var reviews = await _db.Reviews.AsNoTracking()
            .Include(r => r.User)
            .Where(r => r.ProductId == productId && r.IsApproved)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return new ReviewListDto
        {
            AverageRating = Math.Round(product.Average, 1),
            ReviewCount = product.Count,
            Items = reviews.Select(DtoMapper.ToReviewDto).ToList()
        };
    }

    public async Task<ReviewDto> AddReviewAsync(Guid userId, int productId, CreateReviewRequest request)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId && p.IsActive)
                      ?? throw new NotFoundException("Product not found.");
        var alreadyReviewed = await _db.Reviews.AnyAsync(r => r.ProductId == product.Id && r.UserId == userId);
        if (alreadyReviewed)
        {
            throw new ConflictException("You have already reviewed this product.");
        }

        var review = new Review
        {
            ProductId = product.Id,
            UserId = userId,
            Rating = request.Rating,
            Title = string.IsNullOrWhiteSpace(request.Title) ? null : request.Title.Trim(),
            Comment = request.Comment.Trim(),
            CreatedAt = DateTime.UtcNow,
            IsApproved = true
        };
        _db.Reviews.Add(review);
        await _db.SaveChangesAsync();

        var saved = await _db.Reviews.AsNoTracking().Include(r => r.User).FirstAsync(r => r.Id == review.Id);
        return DtoMapper.ToReviewDto(saved);
    }

    private IQueryable<Product> BuildProductQuery() =>
        _db.Products.AsNoTracking()
            .AsSplitQuery()
            .Where(p => p.IsActive)
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .Include(p => p.Reviews);
}
