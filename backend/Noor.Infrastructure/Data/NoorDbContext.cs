using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Noor.Core.Entities;

namespace Noor.Infrastructure.Data;

public class NoorDbContext : DbContext
{
    public NoorDbContext(DbContextOptions<NoorDbContext> options) : base(options) { }

    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
    public DbSet<NewsletterSubscriber> NewsletterSubscribers => Set<NewsletterSubscriber>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>(b =>
        {
            b.HasIndex(u => u.Email).IsUnique();
            b.Property(u => u.Email).HasMaxLength(200).IsRequired();
            b.Property(u => u.FullName).HasMaxLength(120).IsRequired();
            b.Property(u => u.Role).HasMaxLength(20).IsRequired();
            b.Property(u => u.PasswordHash).HasMaxLength(500).IsRequired();
        });

        modelBuilder.Entity<RefreshToken>(b =>
        {
            b.HasIndex(t => t.TokenHash).IsUnique();
            b.Property(t => t.TokenHash).HasMaxLength(128).IsRequired();
            b.HasOne(t => t.User).WithMany(u => u.RefreshTokens).HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Address>(b =>
        {
            b.Property(a => a.Label).HasMaxLength(40);
            b.Property(a => a.FullName).HasMaxLength(120).IsRequired();
            b.Property(a => a.Phone).HasMaxLength(30).IsRequired();
            b.Property(a => a.Line1).HasMaxLength(200).IsRequired();
            b.Property(a => a.Line2).HasMaxLength(200);
            b.Property(a => a.City).HasMaxLength(100).IsRequired();
            b.Property(a => a.State).HasMaxLength(100).IsRequired();
            b.Property(a => a.PostalCode).HasMaxLength(20).IsRequired();
            b.Property(a => a.Country).HasMaxLength(100).IsRequired();
            b.HasOne(a => a.User).WithMany(u => u.Addresses).HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Category>(b =>
        {
            b.HasIndex(c => c.Slug).IsUnique();
            b.Property(c => c.Name).HasMaxLength(100).IsRequired();
            b.Property(c => c.Slug).HasMaxLength(120).IsRequired();
            b.Property(c => c.Description).HasMaxLength(600);
            b.Property(c => c.ImageUrl).HasMaxLength(500);
        });

        modelBuilder.Entity<Product>(b =>
        {
            b.HasIndex(p => p.Slug).IsUnique();
            b.Property(p => p.Name).HasMaxLength(150).IsRequired();
            b.Property(p => p.Slug).HasMaxLength(170).IsRequired();
            b.Property(p => p.ShortDescription).HasMaxLength(260).IsRequired();
            b.Property(p => p.Description).HasMaxLength(5000).IsRequired();
            b.Property(p => p.Fabric).HasMaxLength(120);
            b.Property(p => p.CareInstructions).HasMaxLength(1000);
            b.Property(p => p.Price).HasPrecision(18, 2);
            b.Property(p => p.CompareAtPrice).HasPrecision(18, 2);
            b.HasOne(p => p.Category).WithMany(c => c.Products).HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductImage>(b =>
        {
            b.Property(i => i.Url).HasMaxLength(500).IsRequired();
            b.Property(i => i.AltText).HasMaxLength(200);
            b.HasOne(i => i.Product).WithMany(p => p.Images).HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductVariant>(b =>
        {
            b.HasIndex(v => v.Sku).IsUnique();
            b.HasIndex(v => new { v.ProductId, v.Size, v.ColorName }).IsUnique();
            b.Property(v => v.Size).HasMaxLength(30).IsRequired();
            b.Property(v => v.ColorName).HasMaxLength(50).IsRequired();
            b.Property(v => v.ColorHex).HasMaxLength(7).IsRequired();
            b.Property(v => v.Sku).HasMaxLength(60).IsRequired();
            b.Property(v => v.PriceAdjustment).HasPrecision(18, 2);
            b.HasOne(v => v.Product).WithMany(p => p.Variants).HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Review>(b =>
        {
            b.HasIndex(r => new { r.ProductId, r.UserId }).IsUnique();
            b.Property(r => r.Title).HasMaxLength(120);
            b.Property(r => r.Comment).HasMaxLength(1500).IsRequired();
            b.HasOne(r => r.Product).WithMany(p => p.Reviews).HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<WishlistItem>(b =>
        {
            b.HasIndex(w => new { w.UserId, w.ProductId }).IsUnique();
            b.HasOne(w => w.User).WithMany(u => u.Wishlist).HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(w => w.Product).WithMany().HasForeignKey(w => w.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<NewsletterSubscriber>(b =>
        {
            b.HasIndex(n => n.Email).IsUnique();
            b.Property(n => n.Email).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<Cart>(b =>
        {
            b.HasIndex(c => c.UserId).IsUnique().HasFilter("[UserId] IS NOT NULL");
            b.HasIndex(c => c.GuestToken).IsUnique().HasFilter("[GuestToken] IS NOT NULL");
            b.Property(c => c.GuestToken).HasMaxLength(80);
            b.Property(c => c.CouponCode).HasMaxLength(40);
            b.HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CartItem>(b =>
        {
            b.HasIndex(i => new { i.CartId, i.ProductVariantId }).IsUnique();
            b.HasOne(i => i.Cart).WithMany(c => c.Items).HasForeignKey(i => i.CartId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(i => i.ProductVariant).WithMany().HasForeignKey(i => i.ProductVariantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Coupon>(b =>
        {
            b.HasIndex(c => c.Code).IsUnique();
            b.Property(c => c.Code).HasMaxLength(40).IsRequired();
            b.Property(c => c.Description).HasMaxLength(300);
            b.Property(c => c.Value).HasPrecision(18, 2);
            b.Property(c => c.MinOrderAmount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Order>(b =>
        {
            b.HasIndex(o => o.OrderNumber).IsUnique();
            b.Property(o => o.OrderNumber).HasMaxLength(40).IsRequired();
            b.Property(o => o.Email).HasMaxLength(200).IsRequired();
            b.Property(o => o.Subtotal).HasPrecision(18, 2);
            b.Property(o => o.DiscountTotal).HasPrecision(18, 2);
            b.Property(o => o.ShippingTotal).HasPrecision(18, 2);
            b.Property(o => o.Total).HasPrecision(18, 2);
            b.Property(o => o.CouponCode).HasMaxLength(40);
            b.Property(o => o.ShippingFullName).HasMaxLength(120).IsRequired();
            b.Property(o => o.ShippingPhone).HasMaxLength(30).IsRequired();
            b.Property(o => o.ShippingLine1).HasMaxLength(200).IsRequired();
            b.Property(o => o.ShippingLine2).HasMaxLength(200);
            b.Property(o => o.ShippingCity).HasMaxLength(100).IsRequired();
            b.Property(o => o.ShippingState).HasMaxLength(100).IsRequired();
            b.Property(o => o.ShippingPostalCode).HasMaxLength(20).IsRequired();
            b.Property(o => o.ShippingCountry).HasMaxLength(100).IsRequired();
            b.Property(o => o.Carrier).HasMaxLength(80);
            b.Property(o => o.TrackingNumber).HasMaxLength(120);
            b.HasOne(o => o.User).WithMany(u => u.Orders).HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderItem>(b =>
        {
            b.Property(i => i.ProductName).HasMaxLength(150).IsRequired();
            b.Property(i => i.ProductSlug).HasMaxLength(170).IsRequired();
            b.Property(i => i.ImageUrl).HasMaxLength(500);
            b.Property(i => i.Sku).HasMaxLength(60).IsRequired();
            b.Property(i => i.Size).HasMaxLength(30).IsRequired();
            b.Property(i => i.ColorName).HasMaxLength(50).IsRequired();
            b.Property(i => i.UnitPrice).HasPrecision(18, 2);
            b.Property(i => i.LineTotal).HasPrecision(18, 2);
            b.HasOne(i => i.Order).WithMany(o => o.Items).HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(i => i.Product).WithMany().HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
