using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Noor.Api.Json;
using Noor.Core.Common;
using Noor.Core.Dtos;
using Noor.Infrastructure.Services;

namespace Noor.Api.Controllers;

[ApiController]
[Route("api/addresses")]
[Authorize]
public class AddressesController : ControllerBase
{
    private readonly AccountService _account;

    public AddressesController(AccountService account) => _account = account;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AddressDto>>> GetAll()
        => Ok(await _account.GetAddressesAsync(User.GetUserId()));

    [HttpPost]
    public async Task<ActionResult<AddressDto>> Create(SaveAddressRequest request)
        => Ok(await _account.CreateAddressAsync(User.GetUserId(), request));

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AddressDto>> Update(int id, SaveAddressRequest request)
        => Ok(await _account.UpdateAddressAsync(User.GetUserId(), id, request));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _account.DeleteAddressAsync(User.GetUserId(), id);
        return NoContent();
    }

    [HttpPost("{id:int}/default")]
    public async Task<IActionResult> SetDefault(int id)
    {
        await _account.SetDefaultAddressAsync(User.GetUserId(), id);
        return NoContent();
    }
}

[ApiController]
[Route("api/wishlist")]
[Authorize]
public class WishlistController : ControllerBase
{
    private readonly AccountService _account;

    public WishlistController(AccountService account) => _account = account;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<WishlistItemDto>>> GetAll()
        => Ok(await _account.GetWishlistAsync(User.GetUserId()));

    [HttpPost("{productId:int}")]
    public async Task<ActionResult<WishlistToggleResponse>> Toggle(int productId)
        => Ok(await _account.ToggleWishlistAsync(User.GetUserId(), productId));
}

[ApiController]
[Route("api/newsletter")]
public class NewsletterController : ControllerBase
{
    private readonly AccountService _account;

    public NewsletterController(AccountService account) => _account = account;

    [HttpPost]
    public async Task<ActionResult<ServiceResult>> Subscribe(NewsletterRequest request)
        => Ok(await _account.SubscribeNewsletterAsync(request.Email));
}
