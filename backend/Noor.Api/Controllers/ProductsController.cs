using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Noor.Api.Json;
using Noor.Core.Common;
using Noor.Core.Dtos;
using Noor.Infrastructure.Services;

namespace Noor.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly CatalogService _catalog;

    public ProductsController(CatalogService catalog) => _catalog = catalog;

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductSummaryDto>>> GetAll([FromQuery] ProductQuery query)
        => Ok(await _catalog.GetProductsAsync(query));

    [HttpGet("facets")]
    public async Task<ActionResult<ProductFacetsDto>> GetFacets()
        => Ok(await _catalog.GetFacetsAsync());

    [HttpGet("{slug}")]
    public async Task<ActionResult<ProductDetailDto>> GetBySlug(string slug)
        => Ok(await _catalog.GetProductBySlugAsync(slug));

    [HttpGet("{id:int}/reviews")]
    public async Task<ActionResult<ReviewListDto>> GetReviews(int id)
        => Ok(await _catalog.GetReviewsAsync(id));

    [HttpPost("{id:int}/reviews")]
    [Authorize]
    public async Task<ActionResult<ReviewDto>> AddReview(int id, CreateReviewRequest request)
        => Ok(await _catalog.AddReviewAsync(User.GetUserId(), id, request));
}
