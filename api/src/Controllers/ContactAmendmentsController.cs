using Microsoft.AspNetCore.Mvc;
using TbtbChallenge.Api.Dtos;
using TbtbChallenge.Api.Services;

namespace TbtbChallenge.Api.Controllers;

[ApiController]
[Route("api/contacts/{contactId:int}/amendments")]
public class ContactAmendmentsController : ControllerBase
{
    private readonly ContactAmendmentService _service;

    public ContactAmendmentsController(ContactAmendmentService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create(int contactId, [FromBody] CreateAmendmentRequest request, CancellationToken cancellationToken)
    {
        var contact = await _service.CreateAmendmentAsync(contactId, request, cancellationToken);
        return Ok(contact);
    }

    [HttpGet]
    public async Task<IActionResult> List(int contactId, CancellationToken cancellationToken)
    {
        var amendments = await _service.ListAmendmentsAsync(contactId, cancellationToken);
        return Ok(amendments);
    }
}
