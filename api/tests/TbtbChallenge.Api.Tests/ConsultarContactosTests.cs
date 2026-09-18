using Microsoft.EntityFrameworkCore;
using TbtbChallenge.Api.Data;
using TbtbChallenge.Api.Dtos;
using TbtbChallenge.Api.Entities;
using TbtbChallenge.Api.Exceptions;
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
            var withBothFilters = await contactService.ListContactsOfMonthAsync(null, gestorA.Id, "Bogotá", null, 1, 100, CancellationToken.None);
            var matchingIds = withBothFilters.Items.Select(c => c.Id).ToList();
            Assert.Contains(matchingContact.Id, matchingIds);
            Assert.DoesNotContain(otherCityContact.Id, matchingIds);
            Assert.DoesNotContain(otherGestorContact.Id, matchingIds);

            var withoutFilters = await contactService.ListContactsOfMonthAsync(null, null, null, null, 1, 100, CancellationToken.None);
            Assert.Contains(matchingContact.Id, withoutFilters.Items.Select(c => c.Id));
            Assert.Contains(otherCityContact.Id, withoutFilters.Items.Select(c => c.Id));
            Assert.Contains(otherGestorContact.Id, withoutFilters.Items.Select(c => c.Id));

            var onlyGestor = await contactService.ListContactsOfMonthAsync(null, gestorA.Id, null, null, 1, 100, CancellationToken.None);
            Assert.Contains(matchingContact.Id, onlyGestor.Items.Select(c => c.Id));
            Assert.DoesNotContain(otherGestorContact.Id, onlyGestor.Items.Select(c => c.Id));
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

    [Fact]
    public async Task ConsultarContactos_CuandoSePideLaSegundaPagina_SoloSeMuestranLosDelRangoSolicitado()
    {
        using var context = CreateContext();

        var gestor = new Gestor { Name = "Gestor CA-P1" };
        var patient = new Patient
        {
            Name = "Paciente CA-P1",
            DocumentType = "CC",
            DocumentNumber = $"CAP1-{Guid.NewGuid():N}",
            Phone = "+573000000004",
            City = "Bogotá",
            TreatmentStartDate = new DateOnly(2026, 8, 1),
            Status = "activo",
            CreatedAt = DateTime.UtcNow
        };

        context.Gestors.Add(gestor);
        context.Patients.Add(patient);
        await context.SaveChangesAsync();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var contactService = new ContactService(context);
        var createdIds = new List<int>();

        for (var i = 0; i < 12; i++)
        {
            var contact = await contactService.CreateContactAsync(new CreateContactRequest(patient.Id, gestor.Id, today, "llamada", "contestado", null), CancellationToken.None);
            createdIds.Add(contact.Id);
        }

        try
        {
            var secondPage = await contactService.ListContactsOfMonthAsync(null, gestor.Id, null, null, 2, 10, CancellationToken.None);

            Assert.Equal(12, secondPage.Total);
            Assert.Equal(2, secondPage.TotalPages);
            Assert.Equal(2, secondPage.Items.Count);
            Assert.Equal(createdIds.Skip(10).ToList(), secondPage.Items.Select(c => c.Id).ToList());

            await Assert.ThrowsAsync<ValidationException>(() => contactService.ListContactsOfMonthAsync(null, gestor.Id, null, null, 0, 10, CancellationToken.None));
            await Assert.ThrowsAsync<ValidationException>(() => contactService.ListContactsOfMonthAsync(null, gestor.Id, null, null, 1, 101, CancellationToken.None));
        }
        finally
        {
            context.Contacts.RemoveRange(context.Contacts.Where(c => c.PatientId == patient.Id));
            context.Patients.Remove(patient);
            context.Gestors.Remove(gestor);
            await context.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task ConsultarContactos_CuandoElDiaNoTieneFormatoValido_LaConsultaSeRechaza()
    {
        using var context = CreateContext();
        var contactService = new ContactService(context);

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => contactService.ListContactsOfMonthAsync(null, null, null, "ayer", 1, 20, CancellationToken.None));

        Assert.Equal("day", exception.Field);
    }

    [Fact]
    public async Task ConsultarContactos_CuandoSeFiltraPorDia_MuestraElContactoEnSuFechaVigenteYConservaLosConteosDelMes()
    {
        using var context = CreateContext();

        var gestor = new Gestor { Name = "Gestor CA-P2" };
        var patient = new Patient
        {
            Name = "Paciente CA-P2",
            DocumentType = "CC",
            DocumentNumber = $"CAP2-{Guid.NewGuid():N}",
            Phone = "+573000000005",
            City = "Bogotá",
            TreatmentStartDate = new DateOnly(2026, 8, 1),
            Status = "activo",
            CreatedAt = DateTime.UtcNow
        };

        context.Gestors.Add(gestor);
        context.Patients.Add(patient);
        await context.SaveChangesAsync();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var movedDay = today.Day == DateTime.DaysInMonth(today.Year, today.Month) ? today.AddDays(-1) : today.AddDays(1);
        var contactService = new ContactService(context);

        var contact = await contactService.CreateContactAsync(new CreateContactRequest(patient.Id, gestor.Id, today, "llamada", "buzón", null), CancellationToken.None);

        try
        {
            var amendmentService = new ContactAmendmentService(context);
            await amendmentService.CreateAmendmentAsync(contact.Id, new CreateAmendmentRequest(gestor.Id, "Se reagenda al día siguiente", null, "reagendado", movedDay, null), CancellationToken.None);

            var pageOfDay = await contactService.ListContactsOfMonthAsync(null, gestor.Id, null, movedDay.ToString("yyyy-MM-dd"), 1, 20, CancellationToken.None);

            Assert.Equal(1, pageOfDay.Total);
            Assert.Equal(contact.Id, pageOfDay.Items.Single().Id);
            Assert.Equal(movedDay, pageOfDay.Items.Single().ContactDate);

            Assert.Equal(1, pageOfDay.DayCounts[movedDay.ToString("yyyy-MM-dd")]);
            Assert.DoesNotContain(today.ToString("yyyy-MM-dd"), pageOfDay.DayCounts.Keys);

            var persisted = await context.Contacts.SingleAsync(c => c.Id == contact.Id, CancellationToken.None);
            Assert.Equal(today, persisted.ContactDate);
        }
        finally
        {
            context.ContactAmendments.RemoveRange(context.ContactAmendments.Where(a => a.ContactId == contact.Id));
            context.Contacts.RemoveRange(context.Contacts.Where(c => c.PatientId == patient.Id));
            context.Patients.Remove(patient);
            context.Gestors.Remove(gestor);
            await context.SaveChangesAsync();
        }
    }
}
