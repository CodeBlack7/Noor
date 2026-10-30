using Microsoft.EntityFrameworkCore;
using Noor.Core.Common;
using Noor.Core.Dtos;
using Noor.Core.Entities;
using Noor.Infrastructure.Data;

namespace Noor.Infrastructure.Services;

public class AccountService
{
    private readonly NoorDbContext _db;

    public AccountService(NoorDbContext db) => _db = db;

    public async Task<IReadOnlyList<AddressDto>> GetAddressesAsync(Guid userId)
    {
        var addresses = await _db.Addresses.AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault).ThenByDescending(a => a.CreatedAt)
            .ToListAsync();
        return addresses.Select(DtoMapper.ToAddressDto).ToList();
    }

    public async Task<AddressDto> CreateAddressAsync(Guid userId, SaveAddressRequest request)
    {
        var hasAddresses = await _db.Addresses.AnyAsync(a => a.UserId == userId);
        var makeDefault = request.IsDefault || !hasAddresses;

        if (makeDefault)
        {
            var existing = await _db.Addresses.Where(a => a.UserId == userId && a.IsDefault).ToListAsync();
            foreach (var address in existing) address.IsDefault = false;
        }

        var entity = new Address { UserId = userId, IsDefault = makeDefault };
        Apply(entity, request);
        _db.Addresses.Add(entity);
        await _db.SaveChangesAsync();
        return DtoMapper.ToAddressDto(entity);
    }

    public async Task<AddressDto> UpdateAddressAsync(Guid userId, int id, SaveAddressRequest request)
    {
        var entity = await _db.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId)
                     ?? throw new NotFoundException("Address not found.");

        if (request.IsDefault && !entity.IsDefault)
        {
            var existing = await _db.Addresses.Where(a => a.UserId == userId && a.IsDefault).ToListAsync();
            foreach (var address in existing) address.IsDefault = false;
        }

        Apply(entity, request);
        await _db.SaveChangesAsync();
        return DtoMapper.ToAddressDto(entity);
    }

    public async Task DeleteAddressAsync(Guid userId, int id)
    {
        var entity = await _db.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId)
                     ?? throw new NotFoundException("Address not found.");
        var wasDefault = entity.IsDefault;
        _db.Addresses.Remove(entity);
        await _db.SaveChangesAsync();

        if (wasDefault)
        {
            var next = await _db.Addresses.Where(a => a.UserId == userId).OrderByDescending(a => a.CreatedAt).FirstOrDefaultAsync();
            if (next is not null)
            {
                next.IsDefault = true;
                await _db.SaveChangesAsync();
            }
        }
    }

    public async Task SetDefaultAddressAsync(Guid userId, int id)
    {
        var addresses = await _db.Addresses.Where(a => a.UserId == userId).ToListAsync();
        var target = addresses.FirstOrDefault(a => a.Id == id) ?? throw new NotFoundException("Address not found.");
        foreach (var address in addresses) address.IsDefault = address.Id == target.Id;
        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<WishlistItemDto>> GetWishlistAsync(Guid userId)
    {
        var items = await _db.WishlistItems.AsNoTracking()
            .Include(w => w.Product).ThenInclude(p => p.Category)
            .Include(w => w.Product).ThenInclude(p => p.Images)
            .Include(w => w.Product).ThenInclude(p => p.Variants)
            .Include(w => w.Product).ThenInclude(p => p.Reviews)
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();

        return items.Select(w => new WishlistItemDto
        {
            Product = DtoMapper.ToSummary(w.Product),
            AddedAt = w.CreatedAt
        }).ToList();
    }

    public async Task<WishlistToggleResponse> ToggleWishlistAsync(Guid userId, int productId)
    {
        if (!await _db.Products.AnyAsync(p => p.Id == productId && p.IsActive))
        {
            throw new NotFoundException("Product not found.");
        }

        var existing = await _db.WishlistItems.FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);
        if (existing is not null)
        {
            _db.WishlistItems.Remove(existing);
            await _db.SaveChangesAsync();
            return new WishlistToggleResponse { InWishlist = false, Message = "Removed from your wishlist." };
        }

        _db.WishlistItems.Add(new WishlistItem { UserId = userId, ProductId = productId });
        await _db.SaveChangesAsync();
        return new WishlistToggleResponse { InWishlist = true, Message = "Saved to your wishlist." };
    }

    public async Task<ServiceResult> SubscribeNewsletterAsync(string email)
    {
        var normalized = email.Trim().ToLowerInvariant();
        var existing = await _db.NewsletterSubscribers.FirstOrDefaultAsync(n => n.Email == normalized);
        if (existing is not null)
        {
            if (!existing.IsActive)
            {
                existing.IsActive = true;
                await _db.SaveChangesAsync();
            }
            return ServiceResult.Ok("You're already on the list. Welcome back.");
        }

        _db.NewsletterSubscribers.Add(new NewsletterSubscriber { Email = normalized });
        await _db.SaveChangesAsync();
        return ServiceResult.Ok("Welcome to the Noor circle.");
    }

    private static void Apply(Address entity, SaveAddressRequest request)
    {
        if (!string.Equals(request.Country.Trim(), "India", StringComparison.OrdinalIgnoreCase))
        {
            throw new ValidationFailedException("Noor currently delivers within India only.");
        }

        entity.Label = request.Label.Trim();
        entity.FullName = request.FullName.Trim();
        entity.Phone = request.Phone.Trim();
        entity.Line1 = request.Line1.Trim();
        entity.Line2 = string.IsNullOrWhiteSpace(request.Line2) ? null : request.Line2.Trim();
        entity.City = request.City.Trim();
        entity.State = request.State.Trim();
        entity.PostalCode = request.PostalCode.Trim();
        entity.Country = request.Country.Trim();
        entity.IsDefault = request.IsDefault || entity.IsDefault;
    }
}
