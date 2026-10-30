using System.ComponentModel.DataAnnotations;

namespace Noor.Core.Dtos;

public class AddressDto
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}

public class SaveAddressRequest
{
    [Required, StringLength(40)]
    public string Label { get; set; } = "Home";

    [Required, StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required, Phone, StringLength(30)]
    public string Phone { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Line1 { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Line2 { get; set; }

    [Required, StringLength(100)]
    public string City { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string State { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string PostalCode { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Country { get; set; } = "India";

    public bool IsDefault { get; set; }
}

public class WishlistItemDto
{
    public ProductSummaryDto Product { get; set; } = new();
    public DateTime AddedAt { get; set; }
}

public class WishlistToggleResponse
{
    public bool InWishlist { get; set; }
    public string Message { get; set; } = string.Empty;
}
