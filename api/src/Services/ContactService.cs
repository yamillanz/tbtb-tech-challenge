using Microsoft.EntityFrameworkCore;
using TbtbChallenge.Api.Data;
using TbtbChallenge.Api.Dtos;
using TbtbChallenge.Api.Entities;
using TbtbChallenge.Api.Exceptions;

namespace TbtbChallenge.Api.Services;

public class ContactService
{
    private static readonly string[] Channels = ["llamada", "whatsapp", "correo"];

    private static readonly string[] Results = ["contestado", "no contesta", "buzón", "número equivocado", "reagendado", "otro"];

    private readonly TbtbChallengeDbContext _context;

    public ContactService(TbtbChallengeDbContext context)
    {
        _context = context;
    }

    public async Task<ContactDto> CreateContactAsync(CreateContactRequest request, CancellationToken cancellationToken)
    {
        ValidateChannel(request.Channel);
        ValidateResult(request.Result);
        ValidateContactDate(request.ContactDate);

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

    private static void ValidateChannel(string channel)
    {
        if (!Channels.Contains(channel))
        {
            throw new ValidationException("channel", $"El canal debe ser uno de: {string.Join(", ", Channels)}.");
        }
    }

    private static void ValidateResult(string result)
    {
        if (!Results.Contains(result))
        {
            throw new ValidationException("result", $"El resultado debe ser uno de: {string.Join(", ", Results)}.");
        }
    }

    private static void ValidateContactDate(DateOnly date)
    {
        var today = ProgramToday();
        var startOfMonth = new DateOnly(today.Year, today.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        if (date < startOfMonth || date > endOfMonth)
        {
            throw new ValidationException("contactDate", $"La fecha del contacto debe estar dentro del mes en curso ({startOfMonth:yyyy-MM-dd} a {endOfMonth:yyyy-MM-dd}).");
        }
    }

    private static DateOnly ProgramToday()
    {
        return DateOnly.FromDateTime(DateTime.UtcNow.AddHours(-5));
    }
}
