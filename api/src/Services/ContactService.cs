using Microsoft.EntityFrameworkCore;
using TbtbChallenge.Api.Data;
using TbtbChallenge.Api.Dtos;
using TbtbChallenge.Api.Entities;
using TbtbChallenge.Api.Exceptions;

namespace TbtbChallenge.Api.Services;

public class ContactService
{
    private readonly TbtbChallengeDbContext _context;

    public ContactService(TbtbChallengeDbContext context)
    {
        _context = context;
    }

    public async Task<ContactDto> CreateContactAsync(CreateContactRequest request, CancellationToken cancellationToken)
    {
        ContactRules.ValidateChannel(request.Channel);
        ContactRules.ValidateResult(request.Result);
        ContactRules.ValidateContactDate(request.ContactDate);

        var patient = await _context.Patients
            .SingleOrDefaultAsync(p => p.Id == request.PatientId, cancellationToken)
            ?? throw new KeyNotFoundException($"El paciente {request.PatientId} no existe.");

        var gestor = await _context.Gestors
            .SingleOrDefaultAsync(g => g.Id == request.GestorId, cancellationToken)
            ?? throw new KeyNotFoundException($"El gestor {request.GestorId} no existe.");

        var contact = new Contact
        {
            PatientId = patient.Id,
            GestorId = gestor.Id,
            ContactDate = request.ContactDate,
            Channel = request.Channel,
            Result = request.Result,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.Contacts.Add(contact);
        await _context.SaveChangesAsync(cancellationToken);

        return new ContactDto(
            contact.Id,
            patient.Id,
            patient.Name,
            gestor.Id,
            gestor.Name,
            contact.ContactDate,
            contact.Channel,
            contact.Result,
            contact.Notes);
    }

    public async Task<ContactPageDto> ListContactsOfMonthAsync(string? month, int? gestorId, string? city, string? day, int page, int pageSize, CancellationToken cancellationToken)
    {
        if (page < 1)
        {
            throw new ValidationException("page", "La página debe ser mayor o igual a 1.");
        }

        if (pageSize < 1 || pageSize > 100)
        {
            throw new ValidationException("pageSize", "El tamaño de página debe estar entre 1 y 100.");
        }

        var selectedDay = ParseDay(day);

        var startOfMonth = StartOfMonthFrom(month);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        var query = _context.Contacts
            .Include(c => c.Patient)
            .Include(c => c.Gestor)
            .Where(c => c.ContactDate >= startOfMonth && c.ContactDate <= endOfMonth);

        if (gestorId is not null)
        {
            query = query.Where(c => c.GestorId == gestorId);
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            query = query.Where(c => c.Patient.City == city);
        }

        var contacts = await query.ToListAsync(cancellationToken);

        var contactIds = contacts.Select(c => c.Id).ToList();

        var amendments = await _context.ContactAmendments
            .Where(a => contactIds.Contains(a.ContactId))
            .OrderBy(a => a.Id)
            .ToListAsync(cancellationToken);

        var amendmentsByContact = amendments
            .GroupBy(a => a.ContactId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var resolved = new List<ContactListItemDto>(contacts.Count);
        foreach (var contact in contacts)
        {
            var current = ResolveCurrentValues(contact, amendmentsByContact.GetValueOrDefault(contact.Id, []));
            resolved.Add(new ContactListItemDto(
                contact.Id,
                contact.Patient.Name,
                contact.Gestor.Name,
                current.ContactDate,
                current.Channel,
                current.Result,
                current.Notes));
        }


        var dayCounts = resolved
            .GroupBy(c => c.ContactDate)
            .ToDictionary(g => g.Key.ToString("yyyy-MM-dd"), g => g.Count());

        var selected = selectedDay is null
            ? resolved
            : resolved.Where(c => c.ContactDate == selectedDay);

        var ordered = selected
            .OrderBy(c => c.ContactDate).ThenBy(c => c.Id)
            .ToList();

        var total = ordered.Count;
        var totalPages = (int)Math.Ceiling(total / (double)pageSize);

        return new ContactPageDto(
            ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            total,
            page,
            pageSize,
            totalPages,
            dayCounts);
    }

    private static DateOnly? ParseDay(string? day)
    {
        if (string.IsNullOrWhiteSpace(day))
        {
            return null;
        }

        if (!DateOnly.TryParseExact(day, "yyyy-MM-dd", out var parsed))
        {
            throw new ValidationException("day", "El día debe tener el formato YYYY-MM-DD.");
        }

        return parsed;
    }

    private static Contact ResolveCurrentValues(Contact contact, IReadOnlyList<ContactAmendment> amendments)
    {
        var channel = contact.Channel;
        var result = contact.Result;
        var contactDate = contact.ContactDate;
        var notes = contact.Notes;

        foreach (var amendment in amendments)
        {
            if (amendment.NewChannel is not null)
            {
                channel = amendment.NewChannel;
            }

            if (amendment.NewResult is not null)
            {
                result = amendment.NewResult;
            }

            if (amendment.NewContactDate is not null)
            {
                contactDate = amendment.NewContactDate.Value;
            }

            if (amendment.NewNotes is not null)
            {
                notes = amendment.NewNotes;
            }
        }

        return new Contact
        {
            Id = contact.Id,
            PatientId = contact.PatientId,
            GestorId = contact.GestorId,
            ContactDate = contactDate,
            Channel = channel,
            Result = result,
            Notes = notes
        };
    }

    private static DateOnly StartOfMonthFrom(string? month)
    {
        if (string.IsNullOrWhiteSpace(month))
        {
            return ContactRules.StartOfCurrentProgramMonth();
        }

        if (!DateOnly.TryParseExact(month, "yyyy-MM", out var parsed))
        {
            throw new ValidationException("month", "El mes debe tener el formato YYYY-MM.");
        }

        return new DateOnly(parsed.Year, parsed.Month, 1);
    }
}
