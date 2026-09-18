using Microsoft.EntityFrameworkCore;
using TbtbChallenge.Api.Data;
using TbtbChallenge.Api.Dtos;
using TbtbChallenge.Api.Entities;
using TbtbChallenge.Api.Services;

namespace TbtbChallenge.Api.Tests;

[Collection("SqlServer")]
public class ConsultarContactosTests
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

    [Fact]
    public async Task ConsultarContactos_CuandoSeFiltraPorGestorYCiudad_SoloSeMuestranLosQueCumplenAmbosFiltros()
    {
        using var context = CreateContext();

        var gestorA = new Gestor { Name = "Gestor CA-4 A" };
        var gestorB = new Gestor { Name = "Gestor CA-4 B" };
        var patientBogota = new Patient
        {
            Name = "Paciente CA-4 Bogotá",
            DocumentType = "CC",
            DocumentNumber = $"CA4-{Guid.NewGuid():N}",
            Phone = "+573000000003",
            City = "Bogotá",
            TreatmentStartDate = new DateOnly(2026, 8, 1),
            Status = "activo",
            CreatedAt = DateTime.UtcNow
        };
        var patientLima = new Patient
        {
            Name = "Paciente CA-4 Lima",
            DocumentType = "CC",
            DocumentNumber = $"CA4-{Guid.NewGuid():N}",
            Phone = "+519300000003",
            City = "Lima",
            TreatmentStartDate = new DateOnly(2026, 8, 1),
            Status = "activo",
            CreatedAt = DateTime.UtcNow
        };

        context.Gestors.Add(gestorA);
        context.Gestors.Add(gestorB);
        context.Patients.Add(patientBogota);
        context.Patients.Add(patientLima);
        await context.SaveChangesAsync();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var contactService = new ContactService(context);

        var matchingContact = await contactService.CreateContactAsync(new CreateContactRequest(patientBogota.Id, gestorA.Id, today, "llamada", "contestado", null), CancellationToken.None);
        var otherCityContact = await contactService.CreateContactAsync(new CreateContactRequest(patientLima.Id, gestorA.Id, today, "whatsapp", "contestado", null), CancellationToken.None);
        var otherGestorContact = await contactService.CreateContactAsync(new CreateContactRequest(patientBogota.Id, gestorB.Id, today, "correo", "contestado", null), CancellationToken.None);

        try
        {
            var withBothFilters = await contactService.ListContactsOfMonthAsync(null, gestorA.Id, "Bogotá", CancellationToken.None);
            var matchingIds = withBothFilters.Select(c => c.Id).ToList();
            Assert.Contains(matchingContact.Id, matchingIds);
            Assert.DoesNotContain(otherCityContact.Id, matchingIds);
            Assert.DoesNotContain(otherGestorContact.Id, matchingIds);

            var withoutFilters = await contactService.ListContactsOfMonthAsync(null, null, null, CancellationToken.None);
            Assert.Contains(matchingContact.Id, withoutFilters.Select(c => c.Id));
            Assert.Contains(otherCityContact.Id, withoutFilters.Select(c => c.Id));
            Assert.Contains(otherGestorContact.Id, withoutFilters.Select(c => c.Id));

            var onlyGestor = await contactService.ListContactsOfMonthAsync(null, gestorA.Id, null, CancellationToken.None);
            Assert.Contains(matchingContact.Id, onlyGestor.Select(c => c.Id));
            Assert.DoesNotContain(otherGestorContact.Id, onlyGestor.Select(c => c.Id));
        }
        finally
        {
            context.Contacts.RemoveRange(context.Contacts.Where(c => c.PatientId == patientBogota.Id || c.PatientId == patientLima.Id));
            context.Patients.Remove(patientBogota);
            context.Patients.Remove(patientLima);
            context.Gestors.Remove(gestorA);
            context.Gestors.Remove(gestorB);
            await context.SaveChangesAsync();
        }
    }
}
