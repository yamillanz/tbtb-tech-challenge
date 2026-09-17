using Microsoft.AspNetCore.Mvc;
using TbtbChallenge.Api.Services;

namespace TbtbChallenge.Api.Controllers;

[ApiController]
[Route("api/patients")]
public class PatientsController : ControllerBase
{
    private readonly CatalogService _service;

    public PatientsController(CatalogService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        return Ok(await _service.ListPatientsAsync(cancellationToken));
    }
}
