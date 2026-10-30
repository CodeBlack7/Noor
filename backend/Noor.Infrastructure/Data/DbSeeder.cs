using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Noor.Core.Entities;
using Noor.Core.Options;
using Noor.Core.Security;
using Noor.Infrastructure.Data;
using Noor.Infrastructure.Helpers;

namespace Noor.Infrastructure.Data;

public class DbSeeder
{
    private readonly NoorDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly StoreOptions _store;

    public DbSeeder(NoorDbContext db, IPasswordHasher passwordHasher, IOptions<StoreOptions> store)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _store = store.Value;
    }

    public async Task SeedAsync()
    {
        var now = DateTime.UtcNow;

        if (!await _db.Users.AnyAsync())
        {
            await SeedUsersAsync(now);
        }

        if (!await _db.Categories.AnyAsync())
        {
            await SeedCatalogAsync(now);
        }

        if (!await _db.Coupons.AnyAsync())
        {
            await SeedCouponsAsync(now);
        }

        if (!await _db.Reviews.AnyAsync())
        {
            await SeedReviewsAsync(now);
        }

        if (!await _db.Orders.AnyAsync())
        {
            await SeedOrdersAsync(now);
        }

        if (!await _db.NewsletterSubscribers.AnyAsync())
        {
            _db.NewsletterSubscribers.AddRange(
                new NewsletterSubscriber { Email = "sara.khan@example.com", CreatedAt = now.AddDays(-28) },
                new NewsletterSubscriber { Email = "leila.haddad@example.com", CreatedAt = now.AddDays(-12) },
                new NewsletterSubscriber { Email = "noor.fan@example.com", CreatedAt = now.AddDays(-3) }
            );
            await _db.SaveChangesAsync();
        }
    }

    private async Task SeedUsersAsync(DateTime now)
    {
        var admin = new ApplicationUser
        {
            FullName = "Noor Atelier",
            Email = "admin@noor.store",
            PasswordHash = _passwordHasher.Hash("Admin@12345"),
            Role = Roles.Admin,
            CreatedAt = now.AddMonths(-8)
        };
        var demo = new ApplicationUser
        {
            FullName = "Amina Yusuf",
            Email = "demo@noor.store",
            Phone = "+91 98490 14820",
            PasswordHash = _passwordHasher.Hash("Demo@12345"),
            IsActive = true,
            MarketingOptIn = true,
            CreatedAt = now.AddMonths(-5),
            LastLoginAt = now.AddHours(-5)
        };
        var sara = new ApplicationUser
        {
            FullName = "Sara Khan",
            Email = "sara@noor.store",
            Phone = "+91 98490 16277",
            PasswordHash = _passwordHasher.Hash("Sara@12345"),
            MarketingOptIn = true,
            CreatedAt = now.AddMonths(-3),
            LastLoginAt = now.AddDays(-2)
        };
        var leila = new ApplicationUser
        {
            FullName = "Leila Haddad",
            Email = "leila@noor.store",
            Phone = "+91 98490 18441",
            PasswordHash = _passwordHasher.Hash("Leila@12345"),
            MarketingOptIn = true,
            CreatedAt = now.AddMonths(-2),
            LastLoginAt = now.AddDays(-6)
        };
        var yasmin = new ApplicationUser
        {
            FullName = "Yasmin Rahman",
            Email = "yasmin@noor.store",
            Phone = "+91 98490 19320",
            PasswordHash = _passwordHasher.Hash("Yasmin@12345"),
            CreatedAt = now.AddDays(-18)
        };

        _db.Users.AddRange(admin, demo, sara, leila, yasmin);
        await _db.SaveChangesAsync();

        _db.Addresses.AddRange(
            new Address
            {
                UserId = demo.Id, Label = "Home", FullName = demo.FullName, Phone = demo.Phone ?? "+91 98490 14820",
                Line1 = "12 Banjara Hills Road No. 3", Line2 = "Apartment 4B", City = "Hyderabad", State = "Telangana",
                PostalCode = "500034", Country = "India", IsDefault = true
            },
            new Address
            {
                UserId = demo.Id, Label = "Work", FullName = demo.FullName, Phone = demo.Phone ?? "+91 98490 14820",
                Line1 = "Plot 18, HITEC City", Line2 = "Level 7, Avasa Workspace", City = "Hyderabad", State = "Telangana",
                PostalCode = "500081", Country = "India"
            },
            new Address
            {
                UserId = sara.Id, Label = "Home", FullName = sara.FullName, Phone = sara.Phone ?? "+91 98490 16277",
                Line1 = "24 Marine Lines", City = "Mumbai", State = "Maharashtra",
                PostalCode = "400002", Country = "India", IsDefault = true
            },
            new Address
            {
                UserId = leila.Id, Label = "Home", FullName = leila.FullName, Phone = leila.Phone ?? "+91 98490 18441",
                Line1 = "7 Marine Drive", City = "Kochi", State = "Kerala",
                PostalCode = "682031", Country = "India", IsDefault = true
            }
        );
        await _db.SaveChangesAsync();
    }

    private async Task SeedCatalogAsync(DateTime now)
    {
        var categories = new List<Category>
        {
            new() { Name = "Burqas", Slug = "burqas", SortOrder = 1, IsActive = true,
                Description = "Considered silhouettes in breathable, beautifully draping fabrics.",
                ImageUrl = "/images/categories/burqas.svg" },
            new() { Name = "Abayas", Slug = "abayas", SortOrder = 2, IsActive = true,
                Description = "Fluid layering pieces for every day and every occasion.",
                ImageUrl = "/images/categories/abayas.svg" },
            new() { Name = "Hijabs & Scarves", Slug = "hijabs", SortOrder = 3, IsActive = true,
                Description = "Featherlight fabrics in a palette made to be lived in.",
                ImageUrl = "/images/categories/hijabs.svg" },
            new() { Name = "Niqabs", Slug = "niqabs", SortOrder = 4, IsActive = true,
                Description = "Soft, considered coverage with an effortless finish.",
                ImageUrl = "/images/categories/niqabs.svg" },
            new() { Name = "Prayer Wear", Slug = "prayer-wear", SortOrder = 5, IsActive = true,
                Description = "Quiet rituals, made more comfortable.",
                ImageUrl = "/images/categories/prayer-wear.svg" },
            new() { Name = "Accessories", Slug = "accessories", SortOrder = 6, IsActive = true,
                Description = "The small details that complete the look.",
                ImageUrl = "/images/categories/accessories.svg" }
        };
        _db.Categories.AddRange(categories);
        await _db.SaveChangesAsync();

        var burqaColors = new List<(string, string)>
        {
            ("Midnight", "#1c1b19"), ("Espresso", "#4a3528"), ("Slate", "#56606a")
        };
        var sandColors = new List<(string, string)>
        {
            ("Sand", "#c8b49a"), ("Olive", "#6b6b4f"), ("Midnight", "#1c1b19")
        };
        var abayaSizes = new List<string> { "XS", "S", "M", "L", "XL" };
        var burqaSizes = new List<string> { "52", "54", "56", "58" };
        var oneSize = new List<string> { "One Size" };

        var products = new List<Product>();
        var skuCounter = 100;

        Product Add(string name, string categorySlug, decimal price, decimal? compareAt, bool featured,
            string shortDescription, string fabric, string description, List<(string, string)> colors,
            List<string> sizes, string care, int sortSeed)
        {
            var category = categories.First(c => c.Slug == categorySlug);
            var slug = Slugifier.Slugify(name);
            var product = new Product
            {
                Name = name,
                Slug = slug,
                CategoryId = category.Id,
                Price = price,
                CompareAtPrice = compareAt,
                ShortDescription = shortDescription,
                Description = description,
                Fabric = fabric,
                CareInstructions = care,
                IsFeatured = featured,
                IsActive = true,
                CreatedAt = now.AddDays(-(sortSeed * 3 + 4)),
                UpdatedAt = now.AddDays(-(sortSeed % 5)),
                Images =
                {
                    new ProductImage { Url = $"/images/products/{slug}-1.svg", AltText = $"{name} editorial view", SortOrder = 0 },
                    new ProductImage { Url = $"/images/products/{slug}-2.svg", AltText = $"{name} detail view", SortOrder = 1 }
                }
            };

            skuCounter++;
            var code = categorySlug switch
            {
                "burqas" => "BUR",
                "abayas" => "ABY",
                "hijabs" => "HJB",
                "niqabs" => "NIQ",
                "prayer-wear" => "PRY",
                _ => "ACC"
            };
            var colorIndex = 0;
            foreach (var (colorName, colorHex) in colors)
            {
                colorIndex++;
                var sizeIndex = 0;
                foreach (var size in sizes)
                {
                    sizeIndex++;
                    var stock = (sortSeed * 7 + colorIndex * 5 + sizeIndex * 3) % 24 + 4;
                    if (sortSeed % 6 == 1 && colorIndex == colors.Count && sizeIndex == sizes.Count) stock = 0;
                    if (sortSeed % 7 == 2 && colorIndex == 1 && sizeIndex == 1) stock = 2;
                    product.Variants.Add(new ProductVariant
                    {
                        Size = size,
                        ColorName = colorName,
                        ColorHex = colorHex,
                        Sku = $"NR-{code}-{skuCounter:000}-{colorIndex}{sizeIndex}",
                        StockQuantity = stock,
                        PriceAdjustment = 0
                    });
                }
            }

            return product;
        }

        products.Add(Add("Layla Everyday Burqa", "burqas", 4499m, 6499m, true,
            "Our quiet everyday essential, cut for graceful movement.",
            "Nida crepe",
            "The Layla is the burqa you reach for without thinking. A clean, considered line falls from the shoulders into a soft, fluid drape, with a generous cut that layers beautifully. Finished with a concealed front opening and a gently shaped hood.",
            burqaColors, burqaSizes, "Hand wash cold or dry clean. Cool iron on reverse.", 1));
        products.Add(Add("Amara Pleated Burqa", "burqas", 6999m, 8999m, false,
            "Fine sunray pleats bring texture and light to a classic silhouette.",
            "Pleated soft crepe",
            "Cut from a featherweight pleated crepe, the Amara moves with a beautiful fluidity. Narrow sunray pleats catch the light while keeping coverage effortless, and the relaxed shape drapes softly over the body.",
            burqaColors, burqaSizes, "Dry clean recommended. Steam gently to refresh pleats.", 2));
        products.Add(Add("Noor Signature Embroidered Burqa", "burqas", 13999m, 16999m, true,
            "Our atelier piece, hand-finished with tonal botanical embroidery.",
            "Premium Nida crepe",
            "The Signature is an heirloom in the making. Each piece is cut from our finest Nida crepe and finished by hand with a delicate tonal vine along the cuffs and hood. An elevated burqa for gatherings, milestones, and moments worth remembering.",
            burqaColors, burqaSizes, "Dry clean only. Store folded with tissue.", 3));
        products.Add(Add("Safiya Two-Piece Burqa", "burqas", 6499m, null, true,
            "A practical two-piece set with an easy pull-on hood.",
            "Cotton-blend crepe",
            "Designed for busy mornings, the Safiya pairs a pull-on hood with a separate flowing skirt. The cotton-blend fabric is breathable and forgiving, while the two-piece construction makes it simple to wear and care for.",
            sandColors, burqaSizes, "Machine wash cold, gentle cycle. Hang to dry.", 4));
        products.Add(Add("Hana Travel Burqa", "burqas", 5999m, 7499m, false,
            "Light, packable, and made for the journey.",
            "Featherweight poly-crepe",
            "The Hana folds down small and shakes out beautifully. Its featherweight crepe resists creasing, making it an easy companion for travel or long days. A simple, roomy cut keeps things comfortable from airport to arrival.",
            burqaColors, burqaSizes, "Machine wash cold in a laundry bag. Hang to dry.", 5));
        products.Add(Add("Maryam Occasion Burqa", "burqas", 13999m, 17999m, true,
            "A floor-skimming occasion piece with a whisper-soft finish.",
            "Silk-touch crepe",
            "For the days that deserve a little more. The Maryam is cut longer and fuller, with a subtle sheen that photographs beautifully. A softly structured hood and delicate drape make it our most requested piece for weddings and Eid.",
            new List<(string, string)> { ("Midnight", "#1c1b19"), ("Deep Plum", "#4a293f"), ("Mocha", "#6f5545") },
            burqaSizes, "Dry clean only.", 6));

        products.Add(Add("Zahra Flowing Abaya", "abayas", 7999m, 9999m, true,
            "A best-selling abaya with a beautifully fluid fall.",
            "Premium Nida",
            "The Zahra is our definition of an everyday abaya: clean at the shoulder, generous through the body, and finished with a soft, fluid hem. An easy layer over everything you already love.",
            burqaColors, abayaSizes, "Hand wash cold or dry clean.", 7));
        products.Add(Add("Inaya Linen Abaya", "abayas", 8999m, null, false,
            "Airy washed linen for warm days and slow afternoons.",
            "Washed European linen",
            "Cut from washed linen with a beautiful natural texture, the Inaya feels like a deep breath. Dropped shoulders and side seam pockets keep it relaxed and useful, while the open front makes layering effortless.",
            sandColors, abayaSizes, "Machine wash cold, line dry. Linen softens with wear.", 8));
        products.Add(Add("Soraya Belted Abaya", "abayas", 8499m, 10999m, true,
            "An elegant belted shape with an adjustable waist tie.",
            "Crepe suiting",
            "A softly tailored abaya with a self-tie belt that lets you shape the waist. The Soraya works beautifully for dinners and events, and looks just as considered with flats on a weekday.",
            new List<(string, string)> { ("Espresso", "#4a3528"), ("Olive", "#6b6b4f"), ("Black", "#151515") },
            abayaSizes, "Dry clean recommended.", 9));
        products.Add(Add("Dalia Kimono Abaya", "abayas", 6999m, null, false,
            "A wide-sleeve kimono layer with a relaxed, airy line.",
            "Cotton-modal blend",
            "The Dalia is an easy, open abaya with wide kimono sleeves and a softly curved hem. Cut from a breathable cotton-modal blend that drapes without clinging, it is the layer you will keep by the door.",
            sandColors, oneSize, "Machine wash cold, gentle. Hang to dry.", 10));

        products.Add(Add("Classic Modal Hijab", "hijabs", 799m, null, false,
            "The soft, everyday hijab that stays put beautifully.",
            "Modal jersey",
            "Our signature modal hijab is soft enough for all-day wear and substantial enough to hold its shape. Finished with a delicate hand-rolled edge. One size.",
            new List<(string, string)> { ("Sand", "#c8b49a"), ("Midnight", "#1c1b19"), ("Rosewood", "#8a5a55"), ("Sage", "#8b927c") },
            oneSize, "Machine wash cold in a laundry bag. Hang to dry.", 11));
        products.Add(Add("Crinkled Chiffon Hijab", "hijabs", 999m, 1299m, false,
            "Airy crinkle texture with a soft, weightless drape.",
            "Crinkled chiffon",
            "A light-as-air chiffon with a permanent crinkle that looks effortless and travels beautifully. Layer it loose or wrap it close; the texture does the work.",
            new List<(string, string)> { ("Ivory", "#efe8db"), ("Mocha", "#6f5545"), ("Dusty Blue", "#7a8b97"), ("Black", "#151515") },
            oneSize, "Hand wash cold. Do not wring. Hang to dry.", 12));
        products.Add(Add("Silk Blend Hijab", "hijabs", 1499m, 1899m, true,
            "A luminous silk blend for the moments that call for it.",
            "Silk-modal blend",
            "Lustrous without being fussy, this silk-blend hijab has a beautiful weight and a subtle sheen. A refined finishing touch for celebrations or simply a Tuesday that deserves better.",
            new List<(string, string)> { ("Champagne", "#d8c5a1"), ("Olive", "#6b6b4f"), ("Plum", "#4a293f"), ("Midnight", "#1c1b19") },
            oneSize, "Dry clean or hand wash cold. Cool iron.", 13));
        products.Add(Add("Jersey Everyday Hijab", "hijabs", 599m, null, false,
            "Stretch jersey. Zero fuss. All-day comfort.",
            "Cotton jersey",
            "An easy stretch-jersey hijab that slips on in seconds and stays comfortably in place. The kind you buy in three colours and live in.",
            new List<(string, string)> { ("Black", "#151515"), ("Charcoal", "#3d3d3d"), ("Oat", "#cdbfa8"), ("Mauve", "#9b7d7a") },
            oneSize, "Machine wash warm. Tumble dry low.", 14));

        products.Add(Add("Soft Touch Niqab", "niqabs", 499m, null, false,
            "Lightweight, soft coverage with a gentle elastic fit.",
            "Soft-touch jersey",
            "A comfortable everyday niqab in a soft-touch jersey that sits gently and breathes easily. Designed with a clean finish and an easy pull-on fit.",
            new List<(string, string)> { ("Black", "#151515"), ("Midnight", "#1c1b19"), ("Mocha", "#6f5545") },
            oneSize, "Machine wash cold, gentle. Hang to dry.", 15));
        products.Add(Add("Tie-Back Niqab", "niqabs", 699m, 899m, false,
            "Adjustable tie-back design for a personalised fit.",
            "Chiffon georgette",
            "A softly structured niqab with adjustable ties at the back, so you can find your perfect fit. Cut from a breathable georgette with a beautifully clean edge.",
            new List<(string, string)> { ("Black", "#151515"), ("Espresso", "#4a3528") },
            oneSize, "Hand wash cold. Hang to dry.", 16));

        products.Add(Add("One-Piece Prayer Dress", "prayer-wear", 1999m, 2499m, true,
            "A soft, one-piece prayer dress with an attached scarf.",
            "Brushed cotton jersey",
            "Everything you need for prayer in one considered piece. The attached scarf, roomy cut, and soft brushed jersey make it easy to slip on and comfortable to wear. Folds neatly for travel.",
            new List<(string, string)> { ("Mist", "#b9c0b8"), ("Sand", "#c8b49a"), ("Charcoal", "#3d3d3d") },
            new List<string> { "S/M", "L/XL" }, "Machine wash cold. Tumble dry low.", 17));
        products.Add(Add("Travel Prayer Mat", "prayer-wear", 1299m, null, false,
            "A lightweight prayer mat that folds into its own pouch.",
            "Microfiber suede",
            "A beautifully simple travel mat with a soft suede touch and a non-slip base. Folds into its own slim pouch, so it can live in your bag or car without a second thought.",
            new List<(string, string)> { ("Sage", "#8b927c"), ("Clay", "#a9836f") },
            oneSize, "Spot clean. Air dry fully before folding.", 18));

        products.Add(Add("Magnetic Hijab Pins", "accessories", 349m, null, false,
            "Six pairs of discreet, secure magnetic pins.",
            "Gold-tone finish",
            "A set of six pairs of strong magnetic pins in a soft gold tone. The secure hold is gentle on delicate fabrics, with no pin holes and no fuss. Presented in a small linen pouch.",
            new List<(string, string)> { ("Gold", "#c1a166"), ("Brushed Silver", "#a9abad") },
            oneSize, "Wipe with a soft dry cloth.", 19));
        products.Add(Add("Under-Cap Set", "accessories", 499m, null, false,
            "Two breathable under-caps for a smooth, secure base.",
            "Cotton bamboo blend",
            "A set of two soft under-caps in neutral tones. The breathable cotton-bamboo blend keeps hair neatly contained and gives your hijab a smooth, secure base.",
            new List<(string, string)> { ("Black / Oat", "#7b7265") },
            oneSize, "Machine wash cold. Hang to dry.", 20));

        _db.Products.AddRange(products);
        await _db.SaveChangesAsync();
    }

    private async Task SeedCouponsAsync(DateTime now)
    {
        _db.Coupons.AddRange(
            new Coupon
            {
                Code = "WELCOME10", Description = "10% off your first order",
                DiscountType = CouponType.Percentage, Value = 10, MinOrderAmount = 0,
                StartsAt = now.AddDays(-60), EndsAt = now.AddMonths(8), IsActive = true
            },
            new Coupon
            {
                Code = "NOOR500", Description = "₹500 off orders over ₹4,999",
                DiscountType = CouponType.FixedAmount, Value = 500, MinOrderAmount = 4999,
                StartsAt = now.AddDays(-30), EndsAt = now.AddMonths(4), IsActive = true
            },
            new Coupon
            {
                Code = "EID20", Description = "20% off the occasion edit",
                DiscountType = CouponType.Percentage, Value = 20, MinOrderAmount = 4999,
                StartsAt = now.AddDays(-10), EndsAt = now.AddDays(45), IsActive = true,
                MaxRedemptions = 500
            });
        await _db.SaveChangesAsync();
    }

    private async Task SeedReviewsAsync(DateTime now)
    {
        var users = await _db.Users.Where(u => u.Role == Roles.Customer).OrderBy(u => u.CreatedAt).ToListAsync();
        var products = await _db.Products.OrderBy(p => p.Id).ToListAsync();
        if (users.Count == 0 || products.Count == 0) return;

        var comments = new[]
        {
            ("Beautifully made", "The fabric is even lovelier in person. It drapes beautifully and the finish is so considered."),
            ("My new everyday piece", "I have worn this three times in the first week. Light, comfortable, and exactly the length I hoped for."),
            ("Quietly luxurious", "The quality is exceptional. It feels special without ever feeling fussy."),
            ("Perfect for travel", "Packed beautifully and came out looking fresh. The coverage and weight are just right."),
            ("So soft", "Gentle on the skin and easy to style. I will be ordering another colour."),
            ("Worth it", "A considered purchase that I know I will keep for years. The stitching and presentation are lovely."),
            ("Subtle and elegant", "Exactly as pictured. The colour is rich and calm, and the fit is generous."),
        };

        var reviews = new List<Review>();
        for (var i = 0; i < products.Count; i++)
        {
            var product = products[i];
            var reviewerCount = i % 3 == 0 ? 2 : 1;
            for (var r = 0; r < reviewerCount; r++)
            {
                var user = users[(i + r) % users.Count];
                var comment = comments[(i + r) % comments.Length];
                reviews.Add(new Review
                {
                    ProductId = product.Id,
                    UserId = user.Id,
                    Rating = (i + r) % 7 == 3 ? 4 : 5,
                    Title = comment.Item1,
                    Comment = comment.Item2,
                    CreatedAt = now.AddDays(-(i * 2 + r * 4 + 2)),
                    IsApproved = true
                });
            }
        }

        _db.Reviews.AddRange(reviews);
        await _db.SaveChangesAsync();
    }

    private async Task SeedOrdersAsync(DateTime now)
    {
        var demo = await _db.Users.FirstOrDefaultAsync(u => u.Email == "demo@noor.store");
        var sara = await _db.Users.FirstOrDefaultAsync(u => u.Email == "sara@noor.store");
        var leila = await _db.Users.FirstOrDefaultAsync(u => u.Email == "leila@noor.store");
        if (demo is null || sara is null || leila is null) return;

        var demoAddress = await _db.Addresses.FirstAsync(a => a.UserId == demo.Id && a.Label == "Home");
        var saraAddress = await _db.Addresses.FirstAsync(a => a.UserId == sara.Id);
        var leilaAddress = await _db.Addresses.FirstAsync(a => a.UserId == leila.Id);

        var products = await _db.Products.Include(p => p.Images).Include(p => p.Variants).ToListAsync();
        Product P(string slug) => products.First(p => p.Slug == slug);

        void AddOrder(string number, ApplicationUser user, Address address, OrderStatus status,
            PaymentStatus paymentStatus, ShippingMethod shippingMethod, int daysAgo,
            params (string Slug, string Size, string Color, int Qty)[] specs)
        {
            var order = new Order
            {
                OrderNumber = number,
                UserId = user.Id,
                Email = user.Email,
                Status = status,
                PaymentStatus = paymentStatus,
                PaymentMethod = PaymentMethod.CashOnDelivery,
                ShippingMethod = shippingMethod,
                PaymentReference = null,
                ShippingFullName = address.FullName,
                ShippingPhone = address.Phone,
                ShippingLine1 = address.Line1,
                ShippingLine2 = address.Line2,
                ShippingCity = address.City,
                ShippingState = address.State,
                ShippingPostalCode = address.PostalCode,
                ShippingCountry = address.Country,
                CreatedAt = now.AddDays(-daysAgo),
                UpdatedAt = now.AddDays(-daysAgo).AddHours(6)
            };

            foreach (var spec in specs)
            {
                var product = P(spec.Slug);
                var variant = product.Variants.First(v => v.Size == spec.Size && v.ColorName == spec.Color);
                var unit = product.Price + variant.PriceAdjustment;
                order.Items.Add(new OrderItem
                {
                    ProductId = product.Id,
                    ProductVariantId = variant.Id,
                    ProductName = product.Name,
                    ProductSlug = product.Slug,
                    ImageUrl = product.Images.OrderBy(i => i.SortOrder).FirstOrDefault()?.Url,
                    Sku = variant.Sku,
                    Size = variant.Size,
                    ColorName = variant.ColorName,
                    UnitPrice = unit,
                    Quantity = spec.Qty,
                    LineTotal = unit * spec.Qty
                });
            }

            order.Subtotal = order.Items.Sum(i => i.LineTotal);
            order.ShippingTotal = shippingMethod == ShippingMethod.Express
                ? _store.ExpressShippingCost
                : order.Subtotal >= _store.FreeShippingThreshold ? 0 : _store.StandardShippingCost;
            order.Total = order.Subtotal + order.ShippingTotal;
            if (status == OrderStatus.Shipped || status == OrderStatus.Delivered)
            {
                order.Carrier = "Noor Express";
                order.TrackingNumber = $"NRX{now.AddDays(-daysAgo):MMdd}{order.Id.ToString("N")[..5].ToUpperInvariant()}";
            }
            _db.Orders.Add(order);
        }

        AddOrder("NR-260906-A1B2C3", demo, demoAddress, OrderStatus.Delivered, PaymentStatus.Paid,
            ShippingMethod.Express, 52,
            ("noor-signature-embroidered-burqa", "54", "Midnight", 1),
            ("silk-blend-hijab", "One Size", "Champagne", 1));
        AddOrder("NR-260922-D4E5F6", demo, demoAddress, OrderStatus.Shipped, PaymentStatus.Pending,
            ShippingMethod.Standard, 36,
            ("zahra-flowing-abaya", "M", "Midnight", 1));
        AddOrder("NR-261010-G7H8I9", sara, saraAddress, OrderStatus.Processing, PaymentStatus.Pending,
            ShippingMethod.Standard, 19,
            ("amara-pleated-burqa", "56", "Espresso", 1),
            ("classic-modal-hijab", "One Size", "Rosewood", 1));
        AddOrder("NR-261015-M4N5O6", demo, demoAddress, OrderStatus.Confirmed, PaymentStatus.Pending,
            ShippingMethod.Express, 14,
            ("maryam-occasion-burqa", "56", "Deep Plum", 1));
        AddOrder("NR-261021-J1K2L3", leila, leilaAddress, OrderStatus.Pending, PaymentStatus.Pending,
            ShippingMethod.Standard, 8,
            ("layla-everyday-burqa", "52", "Slate", 1));
        AddOrder("NR-261024-P7Q8R9", sara, saraAddress, OrderStatus.Delivered, PaymentStatus.Paid,
            ShippingMethod.Standard, 5,
            ("one-piece-prayer-dress", "S/M", "Sand", 1),
            ("travel-prayer-mat", "One Size", "Sage", 1));
        AddOrder("NR-261026-S1T2U3", leila, leilaAddress, OrderStatus.Processing, PaymentStatus.Pending,
            ShippingMethod.Standard, 3,
            ("safiya-two-piece-burqa", "54", "Olive", 1),
            ("magnetic-hijab-pins", "One Size", "Gold", 1));
        AddOrder("NR-261027-V4W5X6", demo, demoAddress, OrderStatus.Confirmed, PaymentStatus.Pending,
            ShippingMethod.Standard, 2,
            ("inaya-linen-abaya", "S", "Sand", 1));
        AddOrder("NR-261028-Y7Z8A9", sara, saraAddress, OrderStatus.Pending, PaymentStatus.Pending,
            ShippingMethod.Standard, 1,
            ("soraya-belted-abaya", "M", "Olive", 1));

        await _db.SaveChangesAsync();
    }
}
