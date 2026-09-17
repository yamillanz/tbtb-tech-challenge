using Microsoft.AspNetCore.Mvc;
using TbtbChallenge.Api.Services;

namespace TbtbChallenge.Api.Controllers;

[ApiController]
[Route("api/gestors")]
public class GestorsController : ControllerBase
{
    private readonly CatalogService _service;

    public GestorsController(CatalogService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        return Ok(await _service.ListGestorsAsync(cancellationToken));
    }
}
