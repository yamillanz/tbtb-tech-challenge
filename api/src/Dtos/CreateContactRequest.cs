namespace TbtbChallenge.Api.Dtos;

public record CreateContactRequest(
    int PatientId,
    int GestorId,
    DateOnly ContactDate,
    string Channel,
    string Result,
    string? Notes);
