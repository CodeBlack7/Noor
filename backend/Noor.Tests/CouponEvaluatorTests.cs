using Noor.Core.Entities;
using Noor.Infrastructure.Services;

namespace Noor.Tests;

public class CouponEvaluatorTests
{
    private static readonly DateTime Now = new(2026, 10, 30, 12, 0, 0, DateTimeKind.Utc);

    private static Coupon Coupon(
        CouponType type = CouponType.Percentage,
        decimal value = 10m,
        decimal minimum = 0m,
        bool active = true,
        DateTime? startsAt = null,
        DateTime? endsAt = null,
        int? maxRedemptions = null,
        int timesRedeemed = 0) => new()
    {
        Code = "TEST",
        DiscountType = type,
        Value = value,
        MinOrderAmount = minimum,
        IsActive = active,
        StartsAt = startsAt,
        EndsAt = endsAt,
        MaxRedemptions = maxRedemptions,
        TimesRedeemed = timesRedeemed
    };

    [Fact]
    public void PercentageCoupon_ComputesAndRoundsDiscount()
    {
        var evaluation = CouponEvaluator.Evaluate(Coupon(value: 12.5m), 219.99m, Now);

        Assert.True(evaluation.IsValid);
        Assert.Equal(27.50m, evaluation.Discount);
    }

    [Fact]
    public void FixedCoupon_NeverExceedsSubtotal()
    {
        var evaluation = CouponEvaluator.Evaluate(
            Coupon(CouponType.FixedAmount, value: 50m), 30m, Now);

        Assert.True(evaluation.IsValid);
        Assert.Equal(30m, evaluation.Discount);
    }

    [Fact]
    public void MinimumOrder_RejectsSmallerBaskets()
    {
        var evaluation = CouponEvaluator.Evaluate(Coupon(minimum: 4999m), 4998.99m, Now);

        Assert.False(evaluation.IsValid);
        Assert.Contains("₹4999.00", evaluation.Error);
    }

    [Fact]
    public void Schedule_RejectsCouponsOutsideWindow()
    {
        var notYet = CouponEvaluator.Evaluate(
            Coupon(startsAt: Now.AddDays(1)), 100m, Now);
        var expired = CouponEvaluator.Evaluate(
            Coupon(endsAt: Now.AddDays(-1)), 100m, Now);

        Assert.False(notYet.IsValid);
        Assert.False(expired.IsValid);
        Assert.Contains("not available yet", notYet.Error);
        Assert.Contains("expired", expired.Error);
    }

    [Fact]
    public void InactiveAndExhaustedCoupons_AreRejected()
    {
        var inactive = CouponEvaluator.Evaluate(Coupon(active: false), 100m, Now);
        var exhausted = CouponEvaluator.Evaluate(
            Coupon(maxRedemptions: 10, timesRedeemed: 10), 100m, Now);

        Assert.False(inactive.IsValid);
        Assert.False(exhausted.IsValid);
        Assert.Contains("redemption limit", exhausted.Error);
    }

    [Fact]
    public void MissingCoupon_IsRejected()
    {
        var evaluation = CouponEvaluator.Evaluate(null, 100m, Now);

        Assert.False(evaluation.IsValid);
        Assert.Equal(0m, evaluation.Discount);
    }
}
