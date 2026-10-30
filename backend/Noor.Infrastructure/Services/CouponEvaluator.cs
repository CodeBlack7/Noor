using Noor.Core.Entities;

namespace Noor.Infrastructure.Services;

public record CouponEvaluation(bool IsValid, string? Error, decimal Discount);

public static class CouponEvaluator
{
    public static CouponEvaluation Evaluate(Coupon? coupon, decimal subtotal, DateTime now)
    {
        if (coupon is null) return new CouponEvaluation(false, "Coupon not found.", 0);
        if (!coupon.IsActive) return new CouponEvaluation(false, "This coupon is not active.", 0);
        if (coupon.StartsAt.HasValue && coupon.StartsAt.Value > now)
            return new CouponEvaluation(false, "This coupon is not available yet.", 0);
        if (coupon.EndsAt.HasValue && coupon.EndsAt.Value < now)
            return new CouponEvaluation(false, "This coupon has expired.", 0);
        if (coupon.MaxRedemptions.HasValue && coupon.TimesRedeemed >= coupon.MaxRedemptions.Value)
            return new CouponEvaluation(false, "This coupon has reached its redemption limit.", 0);
        if (subtotal < coupon.MinOrderAmount)
            return new CouponEvaluation(false, $"This coupon requires a minimum subtotal of ₹{coupon.MinOrderAmount:0.00}.", 0);

        var discount = coupon.DiscountType == CouponType.Percentage
            ? subtotal * coupon.Value / 100m
            : coupon.Value;

        discount = Math.Clamp(discount, 0, subtotal);
        return new CouponEvaluation(true, null, Math.Round(discount, 2, MidpointRounding.AwayFromZero));
    }
}
