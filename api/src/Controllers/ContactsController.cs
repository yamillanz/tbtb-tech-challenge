using Microsoft.AspNetCore.Mvc;
using TbtbChallenge.Api.Dtos;
using TbtbChallenge.Api.Services;

namespace TbtbChallenge.Api.Controllers;

[ApiController]
[Route("api/contacts")]
public class ContactsController : ControllerBase
{
    private readonly ContactService _service;

    public ContactsController(ContactService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateContactRequest request, CancellationToken cancellationToken)
    {
        var contact = await _service.CreateContactAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, contact);
    }
}
