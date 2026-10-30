using Microsoft.AspNetCore.Mvc;
using Noor.Core.Dtos;
using Noor.Infrastructure.Services;

namespace Noor.Api.Controllers;

[ApiController]
[Route("api")]
public class CatalogController : ControllerBase
{
    private readonly CatalogService _catalog;

    public CatalogController(CatalogService catalog) => _catalog = catalog;

    [HttpGet("home")]
    public async Task<ActionResult<HomeContentDto>> Home() => Ok(await _catalog.GetHomeContentAsync());

    [HttpGet("categories")]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> Categories()
        => Ok(await _catalog.GetCategoriesAsync());
}
