using Microsoft.EntityFrameworkCore;
using TbtbChallenge.Api.Data;
using TbtbChallenge.Api.Dtos;
using TbtbChallenge.Api.Entities;
using TbtbChallenge.Api.Services;

namespace TbtbChallenge.Api.Tests;

[Collection("SqlServer")]
public class RegistrarContactoTests
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
    public async Task RegistrarContacto_CuandoElGestorCompletaElFormulario_ElContactoQuedaAsociadoAlPaciente()
    {
        using var context = CreateContext();

        var patient = new Patient
        {
            Name = "Paciente CA-2",
            DocumentType = "CC",
            DocumentNumber = $"CA2-{Guid.NewGuid():N}",
            Phone = "+573000000001",
            City = "Bogotá",
            TreatmentStartDate = new DateOnly(2026, 8, 1),
            Status = "activo",
            CreatedAt = DateTime.UtcNow
        };

        var gestor = new Gestor
        {
            Name = "Gestor CA-2"
        };

        context.Patients.Add(patient);
        context.Gestors.Add(gestor);
        await context.SaveChangesAsync();

        try
        {
            var servicio = new ContactService(context);
            var request = new CreateContactRequest(
                patient.Id,
                gestor.Id,
                DateOnly.FromDateTime(DateTime.UtcNow),
                "whatsapp",
                "contestado",
                "Prueba del criterio CA-2");

            var contact = await servicio.CreateContactAsync(request, CancellationToken.None);

            Assert.Equal(patient.Id, contact.PatientId);
            Assert.Equal(gestor.Id, contact.GestorId);
            Assert.Equal(request.ContactDate, contact.ContactDate);
            Assert.Equal(request.Channel, contact.Channel);
            Assert.Equal(request.Result, contact.Result);

            var persisted = await context.Contacts.SingleAsync(c => c.Id == contact.Id);
            Assert.Equal(patient.Id, persisted.PatientId);
            Assert.Equal(gestor.Id, persisted.GestorId);
            Assert.Equal(request.ContactDate, persisted.ContactDate);
            Assert.Equal(request.Channel, persisted.Channel);
            Assert.Equal(request.Result, persisted.Result);
        }
        finally
        {
            context.Contacts.RemoveRange(context.Contacts.Where(c => c.PatientId == patient.Id));
            context.Patients.Remove(patient);
            context.Gestors.Remove(gestor);
            await context.SaveChangesAsync();
        }
    }
}
