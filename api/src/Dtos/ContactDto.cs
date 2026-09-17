namespace TbtbChallenge.Api.Dtos;

public record ContactDto(
    int Id,
    int PatientId,
    string PatientName,
    int GestorId,
    string GestorName,
    DateOnly ContactDate,
    string Channel,
    string Result,
    string? Notes);
