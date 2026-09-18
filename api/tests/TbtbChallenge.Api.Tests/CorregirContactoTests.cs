using Microsoft.EntityFrameworkCore;
using TbtbChallenge.Api.Data;
using TbtbChallenge.Api.Dtos;
using TbtbChallenge.Api.Entities;
using TbtbChallenge.Api.Exceptions;
using TbtbChallenge.Api.Services;

namespace TbtbChallenge.Api.Tests;

[Collection("SqlServer")]
public class CorregirContactoTests
{
    private static TbtbChallengeDbContext CreateContext()
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__TbtbDatabase")
            ?? throw new InvalidOperationException("Define la variable de entorno ConnectionStrings__TbtbDatabase para ejecutar las pruebas contra SQL Server.");

        var options = new DbContextOptionsBuilder<TbtbChallengeDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new TbtbChallengeDbContext(options);
    }

    private static async Task<(Patient patient, Gestor gestor, Contact contact)> CreateArrangementAsync(TbtbChallengeDbContext context)
    {
        var patient = new Patient
        {
            Name = "Paciente CA-3",
            DocumentType = "CC",
            DocumentNumber = $"CA3-{Guid.NewGuid():N}",
            Phone = "+573000000002",
            City = "Bogotá",
            TreatmentStartDate = new DateOnly(2026, 8, 1),
            Status = "activo",
            CreatedAt = DateTime.UtcNow
        };

        var gestor = new Gestor
        {
            Name = "Gestor CA-3"
        };

        context.Patients.Add(patient);
        context.Gestors.Add(gestor);
        await context.SaveChangesAsync();

        var contactService = new ContactService(context);
        var contact = await contactService.CreateContactAsync(
            new CreateContactRequest(
                patient.Id,
                gestor.Id,
                DateOnly.FromDateTime(DateTime.UtcNow),
                "llamada",
                "contestado",
                "Registrado con error para la prueba"),
            CancellationToken.None);

        return (patient, gestor, context.Contacts.Single(c => c.Id == contact.Id));
    }

    [Fact]
    public async Task CorregirContacto_CuandoSeCorrigeUnContacto_ElOriginalNoCambiaYElReporteMuestraElValorVigente()
    {
        using var context = CreateContext();
        var (patient, gestor, contact) = await CreateArrangementAsync(context);

        var correctingGestor = new Gestor { Name = "Corrector CA-3" };
        context.Gestors.Add(correctingGestor);
        await context.SaveChangesAsync();

        try
        {
            var amendmentService = new ContactAmendmentService(context);
            var request = new CreateAmendmentRequest(
                correctingGestor.Id,
                "Se registró sin respuesta por error; el patient respondió",
                null,
                "no contesta",
                null,
                null);

            var current = await amendmentService.CreateAmendmentAsync(contact.Id, request, CancellationToken.None);

            Assert.Equal("no contesta", current.Result);

            var original = await context.Contacts.AsNoTracking().SingleAsync(c => c.Id == contact.Id);
            Assert.Equal("contestado", original.Result);
            Assert.Equal("llamada", original.Channel);

            var amendment = await context.ContactAmendments.SingleAsync(a => a.ContactId == contact.Id);
            Assert.Equal(request.Reason, amendment.Reason);
            Assert.Equal(correctingGestor.Id, amendment.AmendedByGestorId);
            Assert.Equal("contestado", amendment.OldResult);
            Assert.Equal("no contesta", amendment.NewResult);

            var listService = new ContactService(context);
            var contactsOfMonth = await listService.ListContactsOfMonthAsync(
                contact.ContactDate.ToString("yyyy-MM"),
                null,
                null,
                CancellationToken.None);

            var contactInList = contactsOfMonth.Single(c => c.Id == contact.Id);
            Assert.Equal("no contesta", contactInList.Result);
            Assert.Equal("llamada", contactInList.Channel);
        }
        finally
        {
            context.ContactAmendments.RemoveRange(context.ContactAmendments.Where(a => a.ContactId == contact.Id));
            context.Contacts.Remove(contact);
            context.Patients.Remove(patient);
            context.Gestors.Remove(gestor);
            context.Gestors.Remove(correctingGestor);
            await context.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task CorregirContacto_CuandoElMotivoEstaVacio_LaEnmiendaNoSeRegistra()
    {
        using var context = CreateContext();
        var (patient, gestor, contact) = await CreateArrangementAsync(context);

        try
        {
            var amendmentService = new ContactAmendmentService(context);
            var request = new CreateAmendmentRequest(gestor.Id, "", null, "buzón", null, null);

            var exception = await Assert.ThrowsAsync<ValidationException>(
                () => amendmentService.CreateAmendmentAsync(contact.Id, request, CancellationToken.None));

            Assert.Equal("reason", exception.Field);
            Assert.Empty(await context.ContactAmendments.Where(a => a.ContactId == contact.Id).ToListAsync());
        }
        finally
        {
            context.Contacts.Remove(contact);
            context.Patients.Remove(patient);
            context.Gestors.Remove(gestor);
            await context.SaveChangesAsync();
        }
    }
}
