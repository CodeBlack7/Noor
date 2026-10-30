using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Noor.Api.Json;
using Noor.Core.Dtos;
using Noor.Infrastructure.Services;

namespace Noor.Api.Controllers;

[ApiController]
[Route("api/cart")]
public class CartController : ControllerBase
{
    private readonly CartService _cart;

    public CartController(CartService cart) => _cart = cart;

    private Guid? UserId => User.Identity?.IsAuthenticated == true ? User.GetUserId() : null;
    private string? GuestKey => Request.Headers["X-Cart-Key"].FirstOrDefault();

    [HttpGet]
    public async Task<ActionResult<CartDto>> Get() => Ok(await _cart.GetCartAsync(UserId, GuestKey));

    [HttpPost("items")]
    public async Task<ActionResult<CartDto>> AddItem(AddCartItemRequest request)
        => Ok(await _cart.AddItemAsync(UserId, GuestKey, request));

    [HttpPut("items/{itemId:int}")]
    public async Task<ActionResult<CartDto>> UpdateItem(int itemId, UpdateCartItemRequest request)
        => Ok(await _cart.UpdateItemAsync(UserId, GuestKey, itemId, request));

    [HttpDelete("items/{itemId:int}")]
    public async Task<ActionResult<CartDto>> RemoveItem(int itemId)
        => Ok(await _cart.RemoveItemAsync(UserId, GuestKey, itemId));

    [HttpPost("coupon")]
    public async Task<ActionResult<CartDto>> ApplyCoupon(ApplyCouponRequest request)
        => Ok(await _cart.ApplyCouponAsync(UserId, GuestKey, request));

    [HttpDelete("coupon")]
    public async Task<ActionResult<CartDto>> RemoveCoupon()
        => Ok(await _cart.RemoveCouponAsync(UserId, GuestKey));

    [HttpPost("merge")]
    [Authorize]
    public async Task<ActionResult<CartDto>> Merge(MergeCartRequest request)
        => Ok(await _cart.MergeGuestCartAsync(User.GetUserId(), request.GuestKey));
}
