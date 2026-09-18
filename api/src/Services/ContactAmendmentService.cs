using Microsoft.EntityFrameworkCore;
using TbtbChallenge.Api.Data;
using TbtbChallenge.Api.Dtos;
using TbtbChallenge.Api.Entities;
using TbtbChallenge.Api.Exceptions;

namespace TbtbChallenge.Api.Services;

public class ContactAmendmentService
{
    private readonly TbtbChallengeDbContext _context;

    public ContactAmendmentService(TbtbChallengeDbContext context)
    {
        _context = context;
    }

    public async Task<ContactDto> CreateAmendmentAsync(int contactId, CreateAmendmentRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            throw new ValidationException("reason", "El motivo de la corrección es obligatorio.");
        }

        var contact = await _context.Contacts
            .Include(c => c.Patient)
            .Include(c => c.Gestor)
            .SingleOrDefaultAsync(c => c.Id == contactId, cancellationToken)
            ?? throw new KeyNotFoundException($"El contacto {contactId} no existe.");

        var gestor = await _context.Gestors
            .SingleOrDefaultAsync(g => g.Id == request.GestorId, cancellationToken)
            ?? throw new KeyNotFoundException($"El gestor {request.GestorId} no existe.");

        var current = ApplyRequestedChanges(contact, request);

        var amendment = new ContactAmendment
        {
            ContactId = contact.Id,
            Reason = request.Reason.Trim(),
            AmendedByGestorId = gestor.Id,
            OldChannel = request.Channel is null ? null : contact.Channel,
            NewChannel = request.Channel,
            OldResult = request.Result is null ? null : contact.Result,
            NewResult = request.Result,
            OldContactDate = request.ContactDate is null ? null : contact.ContactDate,
            NewContactDate = request.ContactDate,
            OldNotes = request.Notes is null ? null : contact.Notes,
            NewNotes = request.Notes,
            AmendedAt = DateTime.UtcNow
        };

        _context.ContactAmendments.Add(amendment);
        await _context.SaveChangesAsync(cancellationToken);

        return new ContactDto(
            contact.Id,
            contact.Patient.Id,
            contact.Patient.Name,
            contact.Gestor.Id,
            contact.Gestor.Name,
            current.ContactDate,
            current.Channel,
            current.Result,
            current.Notes);
    }

    private static Contact ApplyRequestedChanges(Contact contact, CreateAmendmentRequest request)
    {
        if (request.Channel is null && request.Result is null && request.ContactDate is null && request.Notes is null)
        {
            throw new ValidationException("amendment", "Debe corregir al menos un campo (canal, resultado, fecha o notas).");
        }

        var current = new Contact
        {
            Id = contact.Id,
            Channel = contact.Channel,
            Result = contact.Result,
            ContactDate = contact.ContactDate,
            Notes = contact.Notes
        };

        if (request.Channel is not null)
        {
            ContactRules.ValidateChannel(request.Channel);
            current.Channel = request.Channel;
        }

        if (request.Result is not null)
        {
            ContactRules.ValidateResult(request.Result);
            current.Result = request.Result;
        }

        if (request.ContactDate is not null)
        {
            ContactRules.ValidateContactDate(request.ContactDate.Value);
            current.ContactDate = request.ContactDate.Value;
        }

        if (request.Notes is not null)
        {
            current.Notes = request.Notes;
        }

        return current;
    }

    public async Task<IReadOnlyList<AmendmentDto>> ListAmendmentsAsync(int contactId, CancellationToken cancellationToken)
    {
        var contactExists = await _context.Contacts.AnyAsync(c => c.Id == contactId, cancellationToken);
        if (!contactExists)
        {
            throw new KeyNotFoundException($"El contacto {contactId} no existe.");
        }

        return await _context.ContactAmendments
            .Where(a => a.ContactId == contactId)
            .OrderBy(a => a.Id)
            .Select(a => new AmendmentDto(
                a.Id,
                a.ContactId,
                a.Reason,
                a.AmendedByGestor.Name,
                a.AmendedAt,
                a.OldChannel,
                a.NewChannel,
                a.OldResult,
                a.NewResult,
                a.OldContactDate,
                a.NewContactDate,
                a.OldNotes,
                a.NewNotes))
            .ToListAsync(cancellationToken);
    }
}
