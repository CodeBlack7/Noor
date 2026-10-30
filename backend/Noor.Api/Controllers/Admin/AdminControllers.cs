using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Noor.Core.Common;
using Noor.Core.Dtos;
using Noor.Infrastructure.Services;

namespace Noor.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = "Admin")]
public class AdminDashboardController : ControllerBase
{
    private readonly AdminService _admin;
    public AdminDashboardController(AdminService admin) => _admin = admin;

    [HttpGet]
    public async Task<ActionResult<DashboardDto>> Get() => Ok(await _admin.GetDashboardAsync());
}

[ApiController]
[Route("api/admin/products")]
[Authorize(Roles = "Admin")]
public class AdminProductsController : ControllerBase
{
    private readonly AdminService _admin;
    public AdminProductsController(AdminService admin) => _admin = admin;

    [HttpGet]
    public async Task<ActionResult<PagedResult<AdminProductListItemDto>>> GetAll([FromQuery] AdminProductQuery query)
        => Ok(await _admin.GetProductsAsync(query));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDetailDto>> Get(int id) => Ok(await _admin.GetProductAsync(id));

    [HttpPost]
    public async Task<ActionResult<ProductDetailDto>> Create(ProductUpsertRequest request)
        => Ok(await _admin.CreateProductAsync(request));

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductDetailDto>> Update(int id, ProductUpsertRequest request)
        => Ok(await _admin.UpdateProductAsync(id, request));

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ServiceResult>> Delete(int id) => Ok(await _admin.DeleteProductAsync(id));
}

[ApiController]
[Route("api/admin/categories")]
[Authorize(Roles = "Admin")]
public class AdminCategoriesController : ControllerBase
{
    private readonly AdminService _admin;
    public AdminCategoriesController(AdminService admin) => _admin = admin;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminCategoryDto>>> GetAll()
        => Ok(await _admin.GetCategoriesAsync());

    [HttpPost]
    public async Task<ActionResult<AdminCategoryDto>> Create(CategoryUpsertRequest request)
        => Ok(await _admin.CreateCategoryAsync(request));

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AdminCategoryDto>> Update(int id, CategoryUpsertRequest request)
        => Ok(await _admin.UpdateCategoryAsync(id, request));

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ServiceResult>> Delete(int id)
        => Ok(await _admin.DeleteCategoryAsync(id));
}

[ApiController]
[Route("api/admin/orders")]
[Authorize(Roles = "Admin")]
public class AdminOrdersController : ControllerBase
{
    private readonly AdminService _admin;
    public AdminOrdersController(AdminService admin) => _admin = admin;

    [HttpGet]
    public async Task<ActionResult<PagedResult<OrderSummaryDto>>> GetAll([FromQuery] AdminOrderQuery query)
        => Ok(await _admin.GetOrdersAsync(query));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDetailDto>> Get(Guid id) => Ok(await _admin.GetOrderAsync(id));

    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult<OrderDetailDto>> UpdateStatus(Guid id, UpdateOrderStatusRequest request)
        => Ok(await _admin.UpdateOrderStatusAsync(id, request));
}

[ApiController]
[Route("api/admin/coupons")]
[Authorize(Roles = "Admin")]
public class AdminCouponsController : ControllerBase
{
    private readonly AdminService _admin;
    public AdminCouponsController(AdminService admin) => _admin = admin;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CouponDto>>> GetAll()
        => Ok(await _admin.GetCouponsAsync());

    [HttpPost]
    public async Task<ActionResult<CouponDto>> Create(CouponUpsertRequest request)
        => Ok(await _admin.CreateCouponAsync(request));

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CouponDto>> Update(int id, CouponUpsertRequest request)
        => Ok(await _admin.UpdateCouponAsync(id, request));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _admin.DeleteCouponAsync(id);
        return NoContent();
    }
}

[ApiController]
[Route("api/admin/customers")]
[Authorize(Roles = "Admin")]
public class AdminCustomersController : ControllerBase
{
    private readonly AdminService _admin;
    public AdminCustomersController(AdminService admin) => _admin = admin;

    [HttpGet]
    public async Task<ActionResult<PagedResult<AdminCustomerDto>>> GetAll([FromQuery] AdminCustomerQuery query)
        => Ok(await _admin.GetCustomersAsync(query));
}
