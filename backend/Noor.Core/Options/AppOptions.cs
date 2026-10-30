namespace Noor.Core.Options;

public class JwtOptions
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "Noor";
    public string Audience { get; set; } = "Noor.Web";
    public int AccessTokenMinutes { get; set; } = 60;
    public int RefreshTokenDays { get; set; } = 14;
}

public class StoreOptions
{
    public string Currency { get; set; } = "INR";
    public decimal FreeShippingThreshold { get; set; } = 1999m;
    public decimal StandardShippingCost { get; set; } = 99m;
    public decimal ExpressShippingCost { get; set; } = 249m;
    public decimal TaxRate { get; set; } = 0m;
    public string SupportEmail { get; set; } = "care@noor.store";
}
