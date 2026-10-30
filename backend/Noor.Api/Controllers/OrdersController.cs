using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Noor.Api.Json;
using Noor.Core.Dtos;
using Noor.Infrastructure.Services;

namespace Noor.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orders;

    public OrdersController(OrderService orders) => _orders = orders;

    [HttpPost]
    public async Task<ActionResult<OrderDetailDto>> PlaceOrder(CheckoutRequest request)
        => Ok(await _orders.PlaceOrderAsync(User.GetUserId(), request));

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderSummaryDto>>> MyOrders()
        => Ok(await _orders.GetMyOrdersAsync(User.GetUserId()));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDetailDto>> GetOrder(Guid id)
        => Ok(await _orders.GetOrderAsync(User.GetUserId(), id));

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<OrderDetailDto>> Cancel(Guid id)
        => Ok(await _orders.CancelOrderAsync(User.GetUserId(), id));
}
