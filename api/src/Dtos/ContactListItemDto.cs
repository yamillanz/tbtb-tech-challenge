namespace TbtbChallenge.Api.Dtos;

public record ContactListItemDto(
    int Id,
    string PatientName,
    string GestorName,
    DateOnly ContactDate,
    string Channel,
    string Result,
    string? Notes);
